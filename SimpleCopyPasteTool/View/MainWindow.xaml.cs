using SimpleCopyPasteTool.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SimpleCopyPasteTool.Core;
using SimpleCopyPasteTool.Includes;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace SimpleCopyPasteTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer resizeTimer;
        private GlobalHotKey[] globalHotkeyHooks;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private MenuItem menuItemShowHide;
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            StartupHelper.InstallApplication();

            //set title
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title = "Simple-Copy-Past-Tool v" + version.ToString();

            //get the viewmodel
            viewModel = mainGrid.DataContext as MainWindowViewModel;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            viewModel.ReloadSettingsEvent += ReloadSettingsEventAction;

            //save the last known window size
            resizeTimer = new DispatcherTimer();
            resizeTimer.Tick += new EventHandler(resizingDone);
            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 600);
            resizeTimer.Stop();
            
            this.SizeChanged += OnSizeChanged;
            this.Closing += OnClosing;
            this.Loaded += OnLOaded;

            //load window site settings
            if (Config.Instance.MainWindowHeight > 100 && Config.Instance.MainWindowWidth > 100)
            {
                this.Width = Convert.ToDouble(Config.Instance.MainWindowWidth);
                this.Height = Convert.ToDouble(Config.Instance.MainWindowHeight);
            }
        }

        private void DispatcherOnUnhandledException(object o, DispatcherUnhandledExceptionEventArgs e)
        {
            GeneralHelper.ShowMessageBox(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace, MessageBoxImage.Error, this);
        }

        #region events
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width > 0 && e.PreviousSize.Height > 0)
            {
                resizeTimer.Stop();
                resizeTimer.Start();
            }

//
//            this.Width = e.NewSize.Width;
//            this.Height = e.NewSize.Height;
//
//            double xChange = 1, yChange = 1;
//
//            if (e.PreviousSize.Width != 0)
//                xChange = (e.NewSize.Width / e.PreviousSize.Width);
//
//            if (e.PreviousSize.Height != 0)
//                yChange = (e.NewSize.Height / e.PreviousSize.Height);
//
//            List<Panel> childrens = GeneralHelper.GetLogicalChildCollection<Panel>(this);
//
//            foreach (var fe in childrens)
//            {
//                //fe.
//                /*because I didn't want to resize the grid I'm having inside the canvas in this particular instance. (doing that from xaml) */
////                if (fe is Grid == false)
////                {
////                    fe.Height = fe.ActualHeight * yChange;
////                    fe.Width = fe.ActualWidth * xChange;
////                    
////                    Canvas.SetTop(fe, Canvas.GetTop(fe) * yChange);
////                    Canvas.SetLeft(fe, Canvas.GetLeft(fe) * xChange);
////
////                }
//            }
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            RemoveAllKeyBindings(mainGrid.DataContext as MainWindowViewModel);
        }
        private void OnLOaded(object sender, RoutedEventArgs e)
        {
            ReloadSettingsEventAction(Constants.ReloadKeyBindingsAction.Reload);

            //on load the first time, check if it is launched at startup, if it is, minimize it
            if (Config.Instance.UseLaunchAtStartup)
            {
                ToSystemTray();
            }
        }
        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                ToSystemTray();
            }
            else if (this.WindowState == WindowState.Normal)
            {
                FromSystemTray();
            }
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            PerformSystemTraySwitch();
        }
        private void HideScreenSystemTrayIconButton_Click(object sender, EventArgs e)
        {
            PerformSystemTraySwitch();
        }
        private void InputField_OnMouseLeave(object sender, MouseEventArgs e)
        {
            mainGrid.Focus();
        }
        #endregion events

        private void resizingDone(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            if (this.Width > 100 && this.Height > 100 && Convert.ToInt32(this.Width) != Config.Instance.MainWindowWidth && Convert.ToInt32(this.Height) != Config.Instance.MainWindowHeight)
            {
                Config.Instance.MainWindowWidth = Convert.ToInt32(this.Width);
                Config.Instance.MainWindowHeight = Convert.ToInt32(this.Height);
                Config.Instance.Save();
            }
        }
        private void ReloadSettingsEventAction(Constants.ReloadKeyBindingsAction action)
        {
            if (action == Constants.ReloadKeyBindingsAction.Reload)
            {
                ReloadKeyBindings(viewModel);
                ResetSystemTray();
                ResetStartup();
            }
            else if (action == Constants.ReloadKeyBindingsAction.Disable)
            {
                RemoveAllKeyBindings(viewModel);
            }
            GC.Collect();
        }
        private void ReloadKeyBindings(MainWindowViewModel viewModel)
        {
            try
            {
                RemoveAllKeyBindings(viewModel);
                globalHotkeyHooks = new GlobalHotKey[Config.Instance.HotkeyConfigItems.Length];

                if (GeneralHelper.AreHotKeyConfigItemsValid(Config.Instance.HotkeyConfigItems, false))
                {
                    for (var i = 0; i < Config.Instance.HotkeyConfigItems.Length; i++)
                    {
                        if (Config.Instance.HotkeyConfigItems[i].TextKey > 0 && Config.Instance.HotkeyConfigItems[i].TextModifierKeys > 0)
                        {
                            ModifierKeys modifierKeys = (ModifierKeys)Config.Instance.HotkeyConfigItems[i].TextModifierKeys;
                            Key mainKey = (Key)Config.Instance.HotkeyConfigItems[i].TextKey;

                            //use global registred hotkeys or local
                            if (Config.Instance.UseGlobalHotkeys)
                            {
                                globalHotkeyHooks[i] = new GlobalHotKey(modifierKeys, mainKey.ToKeys(), this, i);
                                globalHotkeyHooks[i].HotKeyPressed += (globalHotKey) => 
                                {
                                    viewModel.HotkeyItemCollection[globalHotKey.ConfigId].MainAction(true);
                                };
                            }
                            else
                            {
                                InputBinding ib = new InputBinding(viewModel.HotkeyItemCollection[i].MainActionTextCommand, new KeyGesture(mainKey, modifierKeys));
                                this.InputBindings.Add(ib);
                            }
                        }
                    }
                }
                else
                {
                    Config.Instance.ResetHotkeySettings();
                    RemoveAllKeyBindings(viewModel);
                    GeneralHelper.ShowMessageBox("De sneltoetsen konden niet geconfigureerd worden, de instellingen worden gereset!", MessageBoxImage.Error, this);
                }
            }
            catch (Exception e)
            {
                Config.Instance.ResetHotkeySettings();
                RemoveAllKeyBindings(viewModel);
                GeneralHelper.ShowMessageBox("Er is een fout opgetreden bij het laden van de hotkeys, ze worden gereset en allemaal verwijderd!", MessageBoxImage.Error, this);
            }
        }
        private void RemoveAllKeyBindings(MainWindowViewModel viewModel)
        {
            //remove all bindings
            List<InputBinding> inputBindingsToRemove = new List<InputBinding>();
            for (int i = 0; i < this.InputBindings.Count; i++)
            {
                inputBindingsToRemove.Add(this.InputBindings[i]);
            }
            foreach (InputBinding inputBinding in inputBindingsToRemove)
            {
                this.InputBindings.Remove(inputBinding);
            }

            if (globalHotkeyHooks != null)
            {
                for (var i = 0; i < globalHotkeyHooks.Length; i++)
                {
                    if (globalHotkeyHooks[i] != null)
                    {
                        globalHotkeyHooks[i].UnregisterHotKey();
                        globalHotkeyHooks[i].Dispose();
                        globalHotkeyHooks[i] = null;
                    }
                }
            }
            globalHotkeyHooks = null;
        }
        private void PerformSystemTraySwitch()
        {
            //switch from an to system tray
            if (this.WindowState == WindowState.Minimized)
            {
                FromSystemTray();
            }
            else if (this.WindowState == WindowState.Normal)
            {
                ToSystemTray();
            }
        }
        private void ToSystemTray()
        {
            this.StateChanged -= OnStateChanged;

            menuItemShowHide.Text = "Scherm tonen";
            this.Hide();
            ShowInTaskbar = false;
            this.WindowState = WindowState.Minimized;

            this.StateChanged += OnStateChanged;
        }
        private void FromSystemTray()
        {
            this.StateChanged -= OnStateChanged;

            menuItemShowHide.Text = "Scherm verbergen";
            this.Show();
            ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;

            this.StateChanged += OnStateChanged;
        }
        private void ResetSystemTray()
        {
            this.StateChanged -= OnStateChanged;
            notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;

            if (Config.Instance.UseMinimizeSystemTray)
            {
                //system tray icon handling
                var systemTrayContextMenu = new ContextMenu();
                var menuItemClose = new MenuItem("Afsluiten");
                menuItemShowHide = new MenuItem("Scherm verbergen");
                menuItemShowHide.Click += HideScreenSystemTrayIconButton_Click;
                menuItemClose.Click += (sender, args) => Environment.Exit(0);

                systemTrayContextMenu.MenuItems.Add(menuItemShowHide);
                systemTrayContextMenu.MenuItems.Add(menuItemClose);
                notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
                notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                notifyIcon.ContextMenu = systemTrayContextMenu;

                this.StateChanged += OnStateChanged;
                notifyIcon.Visible = true;
            }
            else
            {
                notifyIcon.ContextMenu = null;
                notifyIcon.Visible = false;
            }
        }
        /// <summary>
        /// Check if it needs to set the launch at startup from the config
        /// </summary>
        private void ResetStartup()
        {
            if (Config.Instance.UseLaunchAtStartup)
            {
                StartupHelper.CheckAddedToStartup(true);
            }
            else
            {
                StartupHelper.RemoveFromStartup();
            }
        }
    }
}
