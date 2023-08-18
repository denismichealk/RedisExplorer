﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    public class KeySetViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>, IKeyValue<BindableCollection<NumberedStringWrapper>>, IValueItem, IHandle<RedisKeyReloadMessage>
    {
        private BindableCollection<NumberedStringWrapper> keyValue;

        public BindableCollection<NumberedStringWrapper> KeyValue
        {
            get
            {
                return keyValue;
            }
            set
            {
                keyValue = value ?? new BindableCollection<NumberedStringWrapper>();
                NotifyOfPropertyChange(() => KeyValue);
            }
        }

        public KeySetViewModel(IEventAggregator eventAggregator)
        {
            var eAggregator = eventAggregator;
            eAggregator.Subscribe(this);
        }

        #region Message handlers

        public async Task HandleAsync(TreeItemSelectedMessage message,CancellationToken ct)
        {
            if (message?.SelectedItem is RedisKeySet && !message.SelectedItem.HasChildren)
            {
                DisplayValue((RedisKeySet)message.SelectedItem);
            }
        }

        public async Task HandleAsync(AddKeyMessage message, CancellationToken ct)
        {
            KeyValue = new BindableCollection<NumberedStringWrapper>();
        }

        public async Task HandleAsync(RedisKeyReloadMessage message, CancellationToken ct)
        {
            var redisKeySet = message.Item as RedisKeySet;
            if (redisKeySet != null)
            {
                DisplayValue(redisKeySet);
            }
        }

        #endregion

        #region Private

        private void DisplayValue(RedisKeySet item)
        {
            if (item != null)
            {
                var value = item.KeyValue;

                KeyValue = new BindableCollection<NumberedStringWrapper>(value.Select((itemvalue, index) => new NumberedStringWrapper { RowNumber = index + 1, Item = itemvalue }));
            }
        }

        #endregion
    }
}
