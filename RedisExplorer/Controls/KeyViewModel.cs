﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;

using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;
using StackExchange.Redis;
using RedisKey = RedisExplorer.Models.RedisKey;

namespace RedisExplorer.Controls
{
    [Export(typeof(KeyViewModel))]
    public class KeyViewModel : Conductor<IValueItem>.Collection.OneActive, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>
    {
        #region Members

        private RedisKey item;

        private readonly IEventAggregator eventAggregator;

        private bool resetValue;

        private bool hasSelected;
        private string keyNameTextBox;
        private DateTime? ttlDateTimePicker;
        private RedisType selectedType;

        private KeyStringViewModel keyStringViewModel { get; set; }
        public KeySetViewModel keySetViewModel { get; set; }
        public KeyListViewModel keyListViewModel { get; set; }
        public KeyHashViewModel keyHashViewModel { get; set; }
        public KeySortedSetViewModel keySortedSetViewModel { get; set; }
        
        #endregion

        #region Properties

        public KeyStringViewModel KeyStringViewModel
        {
            get
            {
                return keyStringViewModel;
            }
            set
            {
                keyStringViewModel = value;
                NotifyOfPropertyChange(() => KeyStringViewModel);
            }
        }

        public KeySetViewModel KeySetViewModel 
        {
            get
            {
                return keySetViewModel;
            }
            set
            {
                keySetViewModel = value;
                NotifyOfPropertyChange(() => KeySetViewModel);
            }
        }

        public KeyListViewModel KeyListViewModel
        {
            get
            {
                return keyListViewModel;
            }
            set
            {
                keyListViewModel = value;
                NotifyOfPropertyChange(() => KeyListViewModel);
            }
        }

        public KeyHashViewModel KeyHashViewModel
        {
            get
            {
                return keyHashViewModel;
            }
            set
            {
                keyHashViewModel = value;
                NotifyOfPropertyChange(() => KeyHashViewModel);
            }
        }

        public KeySortedSetViewModel KeySortedSetViewModel
        {
            get
            {
                return keySortedSetViewModel;
            }
            set
            {
                keySortedSetViewModel = value;
                NotifyOfPropertyChange(() => KeySortedSetViewModel);
            }
        }

        public bool HasSelected
        {
            get
            {
                return hasSelected;
            }
            set
            {
                hasSelected = value;
                NotifyOfPropertyChange(() => HasSelected);
            }
        }

        public string KeyNameTextBox
        {
            get
            {
                return keyNameTextBox;
            }
            set
            {
                keyNameTextBox = value;
                NotifyOfPropertyChange(() => KeyNameTextBox);
            }
        }

        
        public DateTime? TTLDateTimePicker
        {
            get
            {
                return ttlDateTimePicker;
            }
            set
            {
                ttlDateTimePicker = value;
                NotifyOfPropertyChange(() => TTLDateTimePicker);
            }
        }

        public RedisType SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;

                ChangeDisplayType();

                NotifyOfPropertyChange(() => SelectedType);
            }
        }

        

        #endregion

        public KeyViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);

            KeyStringViewModel = new KeyStringViewModel(eventAggregator);
            KeyStringViewModel.ConductWith(this);

            KeySetViewModel = new KeySetViewModel(eventAggregator);
            KeySetViewModel.ConductWith(this);
                
            KeyListViewModel = new KeyListViewModel(eventAggregator);
            KeyListViewModel.ConductWith(this);

            KeyHashViewModel = new KeyHashViewModel(eventAggregator);
            KeyHashViewModel.ConductWith(this);

            KeySortedSetViewModel = new KeySortedSetViewModel(eventAggregator);
            KeySortedSetViewModel.ConductWith(this);

            Items.Add(KeyStringViewModel);
            Items.Add(KeySetViewModel);
            Items.Add(KeyListViewModel);
            Items.Add(KeyHashViewModel);
            Items.Add(KeySortedSetViewModel);

            //ActivateItem(KeyStringViewModel);

            SetDefault();
        }

        public void SetDefault()
        {
            SelectedType = RedisType.String;
            TTLDateTimePicker = null;
        }

        public IEnumerable<RedisType> RedisTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(RedisType)).Cast<RedisType>();
            }
        }

        #region Button Actions

        public void SaveButton()
        {
            if (item == null)
            {
                return;
            }
            item.KeyName = keyNameTextBox;
            item.KeyType = SelectedType;

            if (TTLDateTimePicker.HasValue)
            {
                item.TTL = new TimeSpan((TTLDateTimePicker.Value - DateTime.Now).Ticks);
            }

            switch (SelectedType)
            {
                case RedisType.String:
                    UpdateKeyValue<string, string>(x => x);
                    break;
                // UpdateItemInTree<string>(); // Not sure needed this before. non ref type?
                case RedisType.Set:
                case RedisType.List:
                    UpdateKeyValue<BindableCollection<NumberedStringWrapper>, List<string>>(x => x.Select(y => y.Item).ToList());
                    UpdateItemInTree<List<string>>();
                    break;
                case RedisType.Hash:
                    UpdateKeyValue<BindableCollection<HashWrapper>, Dictionary<string, string>>(x => x.Where(y => !string.IsNullOrEmpty(y.Key)).ToDictionary(y => y.Key, y => y.Value));
                    UpdateItemInTree<Dictionary<string, string>>(); // hmm
                    break;
                case RedisType.SortedSet:
                    UpdateKeyValue<BindableCollection<ScoreWrapper>, List<SortedSetEntry>>(x => x.Select(y => new SortedSetEntry(y.Item, y.Score)).ToList());
                    UpdateItemInTree<List<SortedSetEntry>>(); // hmm
                    break;
            }

            if (item.Save())
            {
                item.NotifyOfSave();
            }
        }

        private void UpdateKeyValue<DisplayType, BaseType>(Func<DisplayType, BaseType> selector)
        {
            var value = ((IKeyValue<DisplayType>)ActiveItem).KeyValue;
            if (value != null)
            {
                ((IKeyValue<BaseType>) item).KeyValue = selector(value); 
            }
        }

        private void UpdateItemInTree<T>()
        {
            dynamic treeitem = item.Parent.Children.FirstOrDefault(x => x.IsSelected);
            if (treeitem != null)
            {
                treeitem.KeyValue = ((IKeyValue<T>)item).KeyValue;
            }
        }

        public void DeleteButton()
        {
            if (item == null)
            {
                return;
            }
            item.Delete();
        }

        public void ReloadButton()
        {
            if (item == null)
            {
                return;
            }
            item.Reload();
            DisplayItem(item);
        }

        public void ClearButton()
        {
            TTLDateTimePicker = null;
        }

        public void OneHourButton()
        {
            TTLDateTimePicker = DateTime.Now.AddHours(1);
        }

        public void TwentyFourHoursButton()
        {
            TTLDateTimePicker = DateTime.Now.AddHours(24);
        }

        #endregion

        #region Message Handlers

        public void Handle(TreeItemSelectedMessage message)
        {
            if (message != null && message.SelectedItem is RedisKey && !message.SelectedItem.HasChildren)
            {
                item = message.SelectedItem as RedisKey;

                resetValue = false;
                
                DisplayItem(item);
            }
        }

        

        public void Handle(AddKeyMessage message)
        {
            item = new RedisKeyString(message.ParentDatabase, eventAggregator) { KeyValue = string.Empty };
            //ActivateItem(KeyStringViewModel);
            SetDefault();
            KeyNameTextBox = message.KeyBase;
        }

        #endregion

        #region Private methods

        private void DisplayItem(RedisKey item)
        {
            if (item != null)
            {
                KeyNameTextBox = item.KeyName;
                SelectedType = item.KeyType;

                var ttl = item.TTL;
                if (ttl.HasValue)
                {
                    TTLDateTimePicker = DateTime.Now + ttl.Value;
                }
                else
                {
                    TTLDateTimePicker = null;
                }
            }
        }

        private void ChangeDisplayType()
        {
            if (item != null)
            {
                var redisTypeViewModelMap = new Dictionary<RedisType, IValueItem>
                {
                    {RedisType.String, KeyStringViewModel},
                    {RedisType.Set, KeySetViewModel},
                    {RedisType.List, KeyListViewModel},
                    {RedisType.Hash, KeyHashViewModel},
                    {RedisType.SortedSet, KeySortedSetViewModel}
                };

                if (!Maps.RedisTypeKeyMap.ContainsKey(selectedType))
                {
                    selectedType = RedisType.String;
                }

                var olditem = this.item;

                item = (RedisKey)Activator.CreateInstance(Maps.RedisTypeKeyMap[selectedType], this.item.Parent, eventAggregator);
                if (item != null)
                {
                    item.KeyName = olditem.KeyName;
                    item.KeyType = selectedType;
                    item.TTL = olditem.TTL;
                    item.Display = olditem.KeyName;

                    //var valuetype = Activator.CreateInstance(Maps.RedisTypeValueTypeMap[selectedType]);
                    //newitem.KeyValue = valuetype;
                    //item = newitem;

                    ActivateItem(redisTypeViewModelMap[selectedType]);
                }

                resetValue = true; // If this is true, try to reset the value when displaying new type.
            }
        }

        #endregion
    }
}
