using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Caliburn.Micro;
using RedisExplorer.Controls;
using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;
using RedisExplorer.Properties;

namespace RedisExplorer
{
    [Export(typeof(AppViewModel))]
    public sealed class AppViewModel : Conductor<IDisplayPanel>.Collection.OneActive, IApp, IHandle<TreeItemSelectedMessage>, IHandle<TreeItemExpandedMessage>, IHandle<AddConnectionMessage>, IHandle<DeleteConnectionMessage>, IHandle<RedisKeyAddedMessage>, IHandle<RedisKeyUpdatedMessage>, IHandle<ConnectionFailedMessage>, IHandle<InfoNotValidMessage>, IHandle<ReloadKeyMessage>, IHandle<ServerReloadMessage>, IHandle<KeyDeletedMessage>, IHandle<DatabaseReloadMessage>, IHandle<AddKeyMessage>, IHandle<KeysDeletedMessage>
    {
        #region Private members

        private readonly IEventAggregator eventAggregator;

        private readonly IWindowManager windowManager;

        private string statusBarTextBlock;

        private BindableCollection<RedisServer> servers; 

        #endregion

        #region Properties

        public BindableCollection<RedisServer> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        #endregion

        public AppViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            DisplayName = "Redis Explorer";

            //SetTheme();

            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            Servers = new BindableCollection<RedisServer>();

            KeyViewModel = new KeyViewModel(eventAggregator);
            KeyViewModel.ConductWith(this);

            DefaultViewModel = new DefaultViewModel();
            DefaultViewModel.ConductWith(this);

            ServerViewModel = new ServerViewModel(eventAggregator);
            ServerViewModel.ConductWith(this);

            DatabaseViewModel = new DatabaseViewModel(eventAggregator);
            DatabaseViewModel.ConductWith(this);

             ActivateItemAsync(DefaultViewModel);


            LoadServers();
        }

        //private void SetTheme()
        //{
        //    ThemeManager.ChangeAppStyle(Application.Current,
        //                                ThemeManager.GetAccent(Settings.Default.Accent),
        //                                ThemeManager.GetAppTheme(Settings.Default.Theme));
        //}

        private void LoadServers()
        {
            Servers.Clear();
            if (Settings.Default.Servers != null)
            {
                foreach (var conn in from string connection in Settings.Default.Servers select new RedisConnection(connection) into server select new RedisServer(server.Name, server.Address + ",keepAlive = 180,allowAdmin=true", eventAggregator))
                {
                    Servers.Add(conn);
                }
            }
        }

        #region Properties

        public KeyViewModel KeyViewModel { get; set; }

        public DefaultViewModel DefaultViewModel { get; set; }

        public ServerViewModel ServerViewModel { get; set; }

        public DatabaseViewModel DatabaseViewModel { get; set; }

        public string StatusBarTextBlock
        {
            get
            {
                return statusBarTextBlock;
            }
            set
            {
                statusBarTextBlock = value;
                NotifyOfPropertyChange(() => StatusBarTextBlock);
            }
        }

        #endregion

        #region Menu

        public void Exit()
        {
            Application.Current.Shutdown();
        }


        public async Task AddServer()
        {
            dynamic settings = new ExpandoObject();
            settings.Width = 400;
            settings.Height = 275;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Add Server";

            await windowManager.ShowWindowAsync(new AddConnectionViewModel(eventAggregator), null, settings);    
        }

        public async Task Preferences()
        {
            dynamic settings = new ExpandoObject();
            settings.Width = 400;
            settings.Height = 550;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Preferences";

           await windowManager.ShowWindowAsync(new PreferencesViewModel(eventAggregator), null, settings);    
        }

        public async Task About()
        {
            await ActivateItemAsync(DefaultViewModel);
            StatusBarTextBlock = string.Empty;
        }

        #endregion

        public async Task HandleAsync(AddConnectionMessage message, CancellationToken ct)
        {
            StringCollection connections = Settings.Default.Servers ?? new StringCollection();

            var newconn = new RedisConnection
            {
                Name = message.Connection.Name,
                Address = message.Connection.Address,
                Port = message.Connection.Port
            };

            connections.Add(newconn.ToString());

            Settings.Default.Servers = connections;
            Settings.Default.Save();

            StatusBarTextBlock = "Connection Added : " + newconn.Name;

            LoadServers();
        }

        public async Task HandleAsync(DeleteConnectionMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Connection Deleted";
            LoadServers();
        }

        public async Task HandleAsync(RedisKeyAddedMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Added Key : " + message.Key.KeyName;
        }

        public async Task HandleAsync(ConnectionFailedMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Could not connect : " + message.ErrorMessage;
        }

        public async Task HandleAsync(InfoNotValidMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Could not query database counts.";
        }

        public async Task HandleAsync(RedisKeyUpdatedMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Updated Key : " + message.Key.KeyName;
        }

        public async Task HandleAsync(ReloadKeyMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Reloaded Key : " + message.Urn;
        }

        public async Task HandleAsync(ServerReloadMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Reload Server : " + message.Name;
        }

        public async Task HandleAsync(KeyDeletedMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Deleted Key : " + message.Key.KeyName;
        }

        public async Task HandleAsync(KeysDeletedMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Deleted " + message.Keys.Count + " Keys";
        }

        public async Task HandleAsync(DatabaseReloadMessage message, CancellationToken ct)
        {
            StatusBarTextBlock = "Reloaded Database : " + message.DbNumber;
        }

        public async Task HandleAsync(TreeItemSelectedMessage message,CancellationToken ct)
        {
            if (message.SelectedItem is RedisServer)
            {
                await ActivateItemAsync(ServerViewModel);
                StatusBarTextBlock = "Connecting to server : " + message.SelectedItem.Display;
            }
            else if (message.SelectedItem is RedisDatabase)
            {
                await ActivateItemAsync(DatabaseViewModel);
                StatusBarTextBlock = "Selected Database : " + message.SelectedItem.Display;
            }
            else
            {
                await ActivateItemAsync(KeyViewModel);
                StatusBarTextBlock = "Selected : " + message.SelectedItem.Display;
            }
        }

        public async Task HandleAsync(TreeItemExpandedMessage message,CancellationToken ct)
        {
            StatusBarTextBlock = "Expanded : " + message.SelectedItem.Display;
        }

        public async Task HandleAsync(AddKeyMessage message, CancellationToken ct)
        {
            if (!ActiveItem.Equals(KeyViewModel))
            {
                await ActivateItemAsync(KeyViewModel);
            }
        }
    }
}
