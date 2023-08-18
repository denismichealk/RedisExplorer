using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    public class KeyHashViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>, IKeyValue<BindableCollection<HashWrapper>>, IValueItem, IHandle<RedisKeyReloadMessage>
    {
        private BindableCollection<HashWrapper> keyValue;

        public BindableCollection<HashWrapper> KeyValue
        {
            get
            {
                return keyValue;
            }
            set
            {
                keyValue = value ?? new BindableCollection<HashWrapper>();
                NotifyOfPropertyChange(() => KeyValue);
            }
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            if (KeyValue == null)
            {
                KeyValue = new BindableCollection<HashWrapper>();
            }
            await base.OnActivateAsync(ct);
        }

        public KeyHashViewModel(IEventAggregator eventAggregator)
        {
            var eAggregator = eventAggregator;
            eAggregator.Subscribe(this);
        }

        #region Message Handlers

        public async Task HandleAsync(TreeItemSelectedMessage message, CancellationToken ct)
        {
            if (message?.SelectedItem is RedisKeyHash && !message.SelectedItem.HasChildren)
            {
                DisplayValue((RedisKeyHash)message.SelectedItem);
            }
        }

        public async Task HandleAsync(AddKeyMessage message, CancellationToken ct)
        {
            KeyValue = new BindableCollection<HashWrapper>();
        }

        public async Task HandleAsync(RedisKeyReloadMessage message, CancellationToken ct)
        {
            var redisKeyHash = message.Item as RedisKeyHash;
            if (redisKeyHash != null)
            {
                DisplayValue(redisKeyHash);
            }
        }

        #endregion 

        #region Private

        private void DisplayValue(RedisKeyHash item)
        {
            if (item != null)
            {
                var value = item.KeyValue;

                KeyValue = new BindableCollection<HashWrapper>(value.Select(x => new HashWrapper { Key = x.Key, Value = x.Value }));
            }
        }

        #endregion
    }
}
