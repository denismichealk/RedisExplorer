﻿using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    public class KeyStringViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>, IValueItem, IKeyValue<string>, IHandle<RedisKeyReloadMessage>
    {
        private string keyValue;

        public string KeyValue
        {
            get
            {
                return keyValue;
            }
            set
            {
                keyValue = value;
                NotifyOfPropertyChange(() => KeyValue);
            }
        }

        public KeyStringViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        #region Message Handlers

        public void Handle(TreeItemSelectedMessage message)
        {
            if (message?.SelectedItem is RedisKeyString && !message.SelectedItem.HasChildren)
            {
                DisplayStringValue((RedisKeyString) message.SelectedItem);
            }
        }

        public void Handle(AddKeyMessage message)
        {
            KeyValue = string.Empty;
        }

        public void Handle(RedisKeyReloadMessage message)
        {
            var redisKeyString = message.Item as RedisKeyString;
            if (redisKeyString != null)
            {
                DisplayStringValue(redisKeyString);
            }
        }

        #endregion

        #region Private

        private void DisplayStringValue(RedisKeyString item)
        {
            if (item == null)
            {
                return;
            }

            var value = item.KeyValue;

            try
            {
                KeyValue = JObject.Parse(value).ToString(Formatting.Indented);
            }
            catch (JsonReaderException)
            {
                KeyValue = value;
            }
        }

        #endregion
    }
}
