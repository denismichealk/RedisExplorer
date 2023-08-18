using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    public class KeySortedSetViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>, IKeyValue<BindableCollection<ScoreWrapper>>, IValueItem, IHandle<RedisKeyReloadMessage>
    {
        private BindableCollection<ScoreWrapper> keyValue;

        public BindableCollection<ScoreWrapper> KeyValue
        {
            get { return keyValue; }
            set
            {
                keyValue = value ?? new BindableCollection<ScoreWrapper>();
                NotifyOfPropertyChange(() => KeyValue);
            }
        }

        public KeySortedSetViewModel(IEventAggregator eventAggregator)
        {
            var eAggregator = eventAggregator;
            eAggregator.Subscribe(this);
        }
        
        #region Message handlers

        public async Task HandleAsync(TreeItemSelectedMessage message,CancellationToken ct)
        {
            if (message?.SelectedItem is RedisKeySortedSet && !message.SelectedItem.HasChildren)
            {
                DisplayValue((RedisKeySortedSet)message.SelectedItem);
            }
        }

        public async Task HandleAsync(AddKeyMessage message, CancellationToken ct)
        {
            KeyValue = new BindableCollection<ScoreWrapper>();
        }

        public async Task HandleAsync(RedisKeyReloadMessage message, CancellationToken ct)
        {
            var redisKeySortedSet = message.Item as RedisKeySortedSet;
            if (redisKeySortedSet != null)
            {
                DisplayValue(redisKeySortedSet);
            }
        }

        #endregion

        #region Private 

        private void DisplayValue(RedisKeySortedSet item)
        {
            if (item != null)
            {
                var value = item.KeyValue;

                KeyValue = new BindableCollection<ScoreWrapper>(value.Select((itemvalue, index) => new ScoreWrapper { RowNumber = index + 1, Item = itemvalue.Element, Score = itemvalue.Score }));
            }
        }

        #endregion
    }
}
