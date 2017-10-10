using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using SimpleCopyPasteTool.Core;
using SimpleCopyPasteTool.Core.WindowsInput;
using SimpleCopyPasteTool.Includes;
using SimpleCopyPasteTool.Model;
using SimpleCopyPasteTool.View;

namespace SimpleCopyPasteTool.ViewModel
{
    public delegate void ReloadSettingsEventHandler(Constants.ReloadKeyBindingsAction action);

    public class MainWindowViewModel : ViewModelBase
    {
        public ReloadSettingsEventHandler ReloadSettingsEvent;
        public ObservableCollection<HotkeyViewModelItem> HotkeyItemCollection { get; set; }

        private RelayCommand _openOptionsCommand;
        private RelayCommand _openAboutCommand;
        private RelayCommand _exitApplicationCommand;

        private string _statusLabelText = "-";

        private DispatcherTimer resetStatusBarTextTimer;

        public MainWindowViewModel()
        {
            resetStatusBarTextTimer = new System.Windows.Threading.DispatcherTimer();

            resetStatusBarTextTimer.Tick += new EventHandler(resetStatusBarTextTimer_Tick);
            resetStatusBarTextTimer.Interval = new TimeSpan(0, 0, 10);

            LoadOptions();
        }

        #region properties
        public string StatusLabelText
        {
            get => _statusLabelText;
            set
            {
                _statusLabelText = value;
                RaisePropertyChanged("StatusLabelText");
            }
        }
        #endregion properties



        #region RelayCommands
        public ICommand OpenOptionsCommand
        {
            get
            {
                if (_openOptionsCommand == null)
                {
                    _openOptionsCommand = new RelayCommand(param => OpenOptions());
                }

                return _openOptionsCommand;
            }
        }
        public ICommand OpenAboutCommand
        {
            get
            {
                if (_openAboutCommand == null)
                {
                    _openAboutCommand = new RelayCommand(param => OpenAbout());
                }

                return _openAboutCommand;
            }
        }
        public ICommand ExitApplicationCommand
        {
            get
            {
                if (_exitApplicationCommand == null)
                {
                    _exitApplicationCommand = new RelayCommand(param => Environment.Exit(0));
                }

                return _exitApplicationCommand;
            }
        }
        #endregion RelayCommands

        #region RelayCommand functions
        private void OpenOptions()
        {
            if (ReloadSettingsEvent != null)
                ReloadSettingsEvent.Invoke(Constants.ReloadKeyBindingsAction.Disable);

            var optionsDialog = new OptionsView();
            optionsDialog.Closed += delegate(object sender, EventArgs args)
            {
                if (optionsDialog.DialogResult == true)
                {
                    StatusLabelText = "Opties opgeslagen!";
                    resetStatusBarTextTimer.Start();

                    //reload the bindings
                    if(ReloadSettingsEvent != null)
                        ReloadSettingsEvent.Invoke(Constants.ReloadKeyBindingsAction.Reload);

                    LoadOptions();
                }
            };
            optionsDialog.ShowDialog();
        }
        private void OpenAbout()
        {
            var aboutDialog = new AboutView();
            aboutDialog.Closed += delegate (object sender, EventArgs args)
            {
                if (aboutDialog.DialogResult == true)
                {

                }
            };
            aboutDialog.ShowDialog();
        }
        #endregion RelayCommand functions

        private void resetStatusBarTextTimer_Tick(object sender, EventArgs e)
        {
            StatusLabelText = "-";
            resetStatusBarTextTimer.Stop();
        }

        private void LoadOptions()
        {
            //load options and the mvvm for the binding
            HotkeyItemCollection = new ObservableCollection<HotkeyViewModelItem>();
            for (int i = 0; i < Config.Instance.HotkeyConfigItems.Length; i++)
            {
                var hotkeyItem = new HotkeyViewModelItem(i);
                hotkeyItem.UpdateStatusBarEventHandler += UpdateStatusBar_Event;
                HotkeyItemCollection.Add(hotkeyItem);
            }
            RaisePropertyChanged("HotkeyItemCollection");
        }

        private void UpdateStatusBar_Event(string text)
        {
            StatusLabelText = text;
            resetStatusBarTextTimer.Start();
        }
    }
}