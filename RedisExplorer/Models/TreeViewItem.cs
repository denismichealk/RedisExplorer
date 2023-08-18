﻿using System.Collections.ObjectModel;

using Caliburn.Micro;

using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Properties;

namespace RedisExplorer.Models
{
    /// <summary>
    /// Tree View Item view Model
    /// </summary>
    /// <see cref="http://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode"/>
    public class TreeViewItem : PropertyChangedBase, ITreeViewItemViewModel
    {
        #region Members

        static readonly TreeViewItem DummyChild = new TreeViewItem();

        private readonly IEventAggregator eventAggregator;

        bool isExpanded;
        bool isSelected;

        #endregion // Members

        #region Properties

        public virtual string Display { get; set; }

        public TreeViewItem Parent { get; }

        public ObservableCollection<TreeViewItem> Children { get; }

        /// <summary>
        /// Property to indicate if has children - not including dummy children.
        /// </summary>
        public bool HasChildren => Children.Count > 1 || Children.Count == 1 && Children[0] != DummyChild;

        public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value.Equals(isExpanded)) return;
                
                isExpanded = value;

                if (isExpanded && Parent != null)
                {
                    Parent.IsExpanded = true;
                }

                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    LoadChildren();
                }

                if (isExpanded && HasChildren)
                {
                    eventAggregator.PublishOnUIThreadAsync(new TreeItemExpandedMessage { SelectedItem = this });
                }

                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        public  virtual  bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value.Equals(isSelected)) return;

                isSelected = value;
                if (Settings.Default.OneClick && !IsExpanded && HasChildren)
                {
                    IsExpanded = true;
                }
                
                if (isSelected)
                {
                     eventAggregator.PublishOnUIThreadAsync(new TreeItemSelectedMessage { SelectedItem = this });
                }

                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        #endregion // Properties

        #region Constructors

        protected TreeViewItem(TreeViewItem parent, bool lazyLoadChildren, IEventAggregator eventAggregator)
        {
            this.Parent = parent;
            this.eventAggregator = eventAggregator;

            isSelected = false;
            isExpanded = false;

            Children = new ObservableCollection<TreeViewItem>();

            if (lazyLoadChildren)
            {
                Children.Add(DummyChild);
            }
        }

        // This is used to create the DummyChild instance.
        private TreeViewItem()
        {
        }

        #endregion // Constructors

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }
    }
}
