using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    public class KeyListViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>, IKeyValue<BindableCollection<NumberedStringWrapper>>, IValueItem, IHandle<RedisKeyReloadMessage>
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

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            if (KeyValue == null)
            {
                keyValue = new BindableCollection<NumberedStringWrapper>();
            }
            base.OnActivateAsync( ct);
        }

        public KeyListViewModel(IEventAggregator eventAggregator)
        {
            var eAggregator = eventAggregator;
            eAggregator.Subscribe(this);
        }

        #region Message Handlers

        public async Task HandleAsync(TreeItemSelectedMessage message,CancellationToken ct)
        {
            if (message?.SelectedItem is RedisKeyList && !message.SelectedItem.HasChildren)
            {
                DisplayValue((RedisKeyList)message.SelectedItem);
            }
        }

        public async Task HandleAsync(AddKeyMessage message, CancellationToken ct)
        {
            KeyValue = new BindableCollection<NumberedStringWrapper>();
        }

        public async Task HandleAsync(RedisKeyReloadMessage message, CancellationToken ct)
        {
            var redisKeyList = message.Item as RedisKeyList;
            if (redisKeyList != null)
            {
                DisplayValue(redisKeyList);
            }
        }

        #endregion

        #region Private

        private void DisplayValue(RedisKeyList item)
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
