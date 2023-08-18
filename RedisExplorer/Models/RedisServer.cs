﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using RedisExplorer.Messages;
using RedisExplorer.Properties;
using StackExchange.Redis;

namespace RedisExplorer.Models
{
    public class RedisServer : TreeViewItem, IHandle<FlushDbMessage>
    {
        private IEventAggregator eventAggregator { get; set; }

        private string connectionStr { get; set; }

        private IConnectionMultiplexer connection { get; set; }

        private bool isConnected { get; set; }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        protected IConnectionMultiplexer Connection
        {
            get
            {
                if (connection != null) { return connection; }
                try
                {
                    connection = ConnectionMultiplexer.Connect(connectionStr);
                    IsConnected = true;
                    return connection;
                }
                catch (RedisConnectionException rcException)
                {
                    eventAggregator.PublishOnUIThreadAsync(new ConnectionFailedMessage { ErrorMessage = rcException.Message });
                    return null;
                }
            }
        }

        public RedisServer(string displayName, string connectionString, IEventAggregator eventAggregator) : base(null, Settings.Default.LazyLoadServer, eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            Display = displayName;
            connectionStr = connectionString;
        }
        
        public IServer GetServer()
        {
            // TODO : this just gets first one
            return Connection?.GetEndPoints().Select(endpoint => Connection.GetServer(endpoint)).FirstOrDefault();
        }

        public IGrouping<string, KeyValuePair<string, string>>[] GetServerInfo()
        {
            return GetServer().Info();
        }

        public IDatabase GetDatabase(int dbnumber)
        {
            return Connection.GetDatabase(dbnumber);
        }

        protected override async void LoadChildren()
        {
            var server = GetServer();

            if (server != null)
            {
                var info = server.Info("keyspace");
                var databases = server.ConfigGet("databases");
                if (databases != null)
                {
                    int dbcounter;
                    if (int.TryParse(databases.First().Value, out dbcounter))
                    {
                        foreach (var dbnumber in Enumerable.Range(0, dbcounter))
                        {
                            var keycount = 0;
                            if (info != null && info.Length > 0)
                            {
                                var dbinfo = info[0].FirstOrDefault(x => x.Key == "db" + dbnumber);
                                if (!string.IsNullOrEmpty(dbinfo.Value))
                                {
                                    int.TryParse(dbinfo.Value.Split(',')[0].Split('=')[1], out keycount);
                                }
                            }
                            else
                            {
                                await eventAggregator.PublishOnUIThreadAsync(new InfoNotValidMessage());                                
                            }

                            var db = new RedisDatabase(this, dbnumber, eventAggregator, keycount);

                            Children.Add(db);
                        }
                    }
                }
            }
        }

        public async void Reload()
        {
            Children.Clear();

            await eventAggregator.PublishOnUIThreadAsync(new ServerReloadMessage { Name = Display });

            LoadChildren();
        }

        public async void Delete(RedisServer server)
        {
            if (Settings.Default.Servers != null)
            {
                var servers = new StringCollection();
                servers.AddRange(Settings.Default.Servers.Cast<string>().ToArray().Where(x => !x.StartsWith(server.Display + ";")).ToArray());
                Settings.Default.Servers = servers;
                Settings.Default.Save();

                await eventAggregator.PublishOnUIThreadAsync(new DeleteConnectionMessage());
            }
        }

        public async Task HandleAsync(FlushDbMessage message,CancellationToken ct)
        {
            Children.Clear();
            LoadChildren();
        }
    }
}
