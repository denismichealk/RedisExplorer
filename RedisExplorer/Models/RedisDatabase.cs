﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using RedisExplorer.Messages;
using RedisExplorer.Properties;
using StackExchange.Redis;

namespace RedisExplorer.Models
{
    public class RedisDatabase : TreeViewItem
    {
        private IEventAggregator eventAggregator { get; set; }

        private RedisServer parent { get; set; }

        private int dbNumber { get; set; }

        private int maxKeys { get; set; }

        private string urnSeparator { get; set; }

        public RedisDatabase(RedisServer parent, int dbnumber, IEventAggregator eventAggregator) : base(parent, Settings.Default.LazyLoadDatabase, eventAggregator)
        {
            this.parent = parent;
            this.dbNumber = dbnumber;
            this.eventAggregator = eventAggregator;
            maxKeys = string.IsNullOrEmpty(Settings.Default.MaxKeys) ? 1000 : int.Parse(Settings.Default.MaxKeys);
            urnSeparator = string.IsNullOrEmpty(Settings.Default.UrnSeparator) ? ":" : Settings.Default.UrnSeparator;
        }

        public IDatabase GetDatabase()
        {
            return parent.GetDatabase(dbNumber);
        }

        protected override void LoadChildren()
        {
            var db = GetDatabase();
            if (db != null)
            {
                var keys = parent.GetServer().Keys(db.Database, "*", maxKeys);

                foreach (var key in keys)
                {
                    var parts = new Queue<string>(key.ToString().Split(new [] { urnSeparator }, StringSplitOptions.RemoveEmptyEntries));
                    if (parts.Count > 0)
                    {
                        AddChildren(this, parts, eventAggregator);
                    }
                }
            }
        }

        private static void AddChildren(TreeViewItem item, Queue<string> urn, IEventAggregator eventAggregator)
        {
            var keystr = urn.Dequeue();
            var key = item.Children.FirstOrDefault(x => x.Display == keystr);
            if (key == null)
            {
                key = new RedisKey(item, eventAggregator) { Display = keystr };
                item.Children.Add(key);
            }

            if (urn.Count > 0)
            {
                AddChildren(key, urn, eventAggregator);
            }
        }

        public void Flush(RedisDatabase database)
        {
            var s = parent.GetServer();
            s.FlushDatabase(dbNumber);

            Children.Clear();

            eventAggregator.PublishOnUIThread(new FlushDbMessage { dbNumber = dbNumber});
        }

        public void Add()
        {
            eventAggregator.PublishOnUIThread(new AddKeyMessage { ParentDatabase = this });
        }
    }
}
