using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using SimpleCopyPasteTool.Core;
using SimpleCopyPasteTool.Includes;
using SimpleCopyPasteTool.Model;

namespace SimpleCopyPasteTool.ViewModel
{
    public class OptionsViewModel : ViewModelBase
    {
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;

        private HotkeyConfigItem[] _hotkeyConfigItems;

        private bool _useGlobalHotkeys = true;
        private bool _useMinimizeSystemTray = true;
        private bool _useDirectPaste;
        private bool _useLaunchAtStartup;

        private ObservableCollection<HotKeyOptionsItem> _configurableHotkeys;
        private HotKeyOptionsItem _configurableHotkeySelection;
        private int _amountHotkeys = Convert.ToInt32(Constants.MIN_HOTKEYS);
        
        public OptionsViewModel()
        {
            LoadSettings();
        }

        #region properties

        public HotkeyConfigItem[] HotkeyConfigItems
        {
            get => _hotkeyConfigItems;
            set
            {
                _hotkeyConfigItems = value; 
                RaisePropertyChanged("HotkeyConfigItems");
            }
        }

        public string Text
        {
            get
            {
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                    return _hotkeyConfigItems[ConfigurableHotkeySelection.Id].Text;
                return string.Empty;
            }
            set
            {
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                {
                    _hotkeyConfigItems[ConfigurableHotkeySelection.Id].Text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }
        public bool UseGlobalHotkeys
        {
            get => _useGlobalHotkeys;
            set
            {
                _useGlobalHotkeys = value;
                RaisePropertyChanged("UseGlobalHotkeys");
            }
        }
        public bool UseMinimizeSystemTray
        {
            get => _useMinimizeSystemTray;
            set
            {
                _useMinimizeSystemTray = value;
                RaisePropertyChanged("UseMinimizeSystemTray");
            }
        }
        public bool UseDirectPaste
        {
            get => _useDirectPaste;
            set
            {
                _useDirectPaste = value;
                RaisePropertyChanged("UseDirectPaste");
            }
        }public bool UseLaunchAtStartup
        {
            get => _useLaunchAtStartup;
            set
            {
                _useLaunchAtStartup = value;
                RaisePropertyChanged("UseLaunchAtStartup");
            }
        }
        public bool CanSaveOptions { get { return IsValid(); } }
        public int TextKey
        {
            get
            {
                if(ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                    return _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextKey;
                return 0;
            }
            set
            {
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                {

                    var oldValue = _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextKey;
                    _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextKey = value;
                    if (IsValid(true) == false)
                    {
                        _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextKey = oldValue;
                    }
                    
                    RaisePropertyChanged("TextKey");
                    RaisePropertyChanged("TextKeyShortcutString");
                    RaisePropertyChanged("CanSaveOptions");
                }
            }
        }
        public int TextModifierKeys
        {
            get
            {
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                    return _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextModifierKeys;
                return 0;
            }
            set
            {
                //if there is an config item selected
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                {
                    //we keep the old value in case the hotkeys are invalid
                    var oldValue = _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextModifierKeys;
                    _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextModifierKeys = value;
                    if (IsValid(true) == false)
                    {
                        _hotkeyConfigItems[ConfigurableHotkeySelection.Id].TextModifierKeys = oldValue;
                    }
                    
                    RaisePropertyChanged("TextModifierKeys");
                    RaisePropertyChanged("TextKeyShortcutString");
                    RaisePropertyChanged("CanSaveOptions");
                }
            }
        }
        public string TextKeyShortcutString
        {
            get => TextModifierKeys > 0 && TextKey > 0 ? $"{((ModifierKeys)TextModifierKeys).ToString()} + {((Key)TextKey).ToString()}" : string.Empty;
        }
        public string LabelIdText
        {
            get
            {
                if (ConfigurableHotkeySelection != null && ConfigurableHotkeySelection.Id < _hotkeyConfigItems.Length)
                    return (ConfigurableHotkeySelection.Id + 1).ToString();
                return string.Empty;
            }
        }
        public decimal AmountHotkeys
        {
            get => (decimal)_amountHotkeys;
            set
            {
                //if the hotkeys are between the min and max amount
                if (value >= Constants.MIN_HOTKEYS && value <= Constants.MAX_HOTKEYS)
                {
                    //enlarges the array or shortens it
                    _amountHotkeys = Convert.ToInt32(value);
                    ReloadAmountHotkeys(_amountHotkeys);
                    RaisePropertyChanged("ConfigurableHotkeys");
                    RaisePropertyChanged("AmountHotkeys");
                }
            }
        }
        public ObservableCollection<HotKeyOptionsItem> ConfigurableHotkeys
        {
            get => _configurableHotkeys;
            set
            {
                _configurableHotkeys = value;
                RaisePropertyChanged("ConfigurableHotkeys");
            }
        }
        public HotKeyOptionsItem ConfigurableHotkeySelection
        {
            get => _configurableHotkeySelection;
            set
            {
                _configurableHotkeySelection = value;
                RaisePropertyChanged("ConfigurableHotkeySelection");
                RaisePropertyChanged("Text");
                RaisePropertyChanged("TextKeyShortcutString");
                RaisePropertyChanged("LabelIdText");
            }
        }
        #endregion properties

        #region RelayCommands
        public ICommand SaveOptionsCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => SaveOptions(param));
                }

                return _saveCommand;
            }
        }
        public ICommand CancelOptionsCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param => CancelSaveOptions(param));
                }

                return _cancelCommand;
            }
        }
        #endregion RelayCommands

        #region RelayCommand functions
        private void SaveOptions(object windowObjectParam)
        {
            Config.Instance.SaveAll(_hotkeyConfigItems, _useGlobalHotkeys, _useMinimizeSystemTray, _useDirectPaste, _useLaunchAtStartup);
            Config.Instance.Save();
            ((Window) windowObjectParam).DialogResult = true;
            ((Window)windowObjectParam).Close();
        }
        private void CancelSaveOptions(object windowObjectParam)
        {
            ((Window)windowObjectParam).DialogResult = false;
            ((Window)windowObjectParam).Close();
        }
        #endregion RelayCommand functions

        #region private functions
        private void LoadSettings()
        {
            HotkeyConfigItems = new HotkeyConfigItem[Config.Instance.HotkeyConfigItems.Length];
            for (var i = 0; i < Config.Instance.HotkeyConfigItems.Length; i++)
            {
                HotkeyConfigItems[i] = Config.Instance.HotkeyConfigItems[i].Clone() as HotkeyConfigItem;
            }

            UseGlobalHotkeys = Config.Instance.UseGlobalHotkeys;
            UseMinimizeSystemTray = Config.Instance.UseMinimizeSystemTray;
            UseDirectPaste = Config.Instance.UseDirectPaste;
            UseLaunchAtStartup = Config.Instance.UseLaunchAtStartup;
            _amountHotkeys = Config.Instance.HotkeyConfigItems.Length;
            ReloadAmountHotkeys(_amountHotkeys);
        }
        private void ReloadAmountHotkeys(int newSize)
        {
            int oldSize = _hotkeyConfigItems.Length;
            Array.Resize<HotkeyConfigItem>(ref _hotkeyConfigItems, Convert.ToInt32(newSize));
            if (newSize > oldSize)
            {
                for (int i = (newSize - (newSize - oldSize)); i < newSize; i++)
                {
                    _hotkeyConfigItems[i] = HotkeyConfigItem.Default();
                }
            }
            ConfigurableHotkeys = new ObservableCollection<HotKeyOptionsItem>();
            for (int i = 0; i < newSize; i++)
            {
                ConfigurableHotkeys.Add(new HotKeyOptionsItem(i, "Sneltoets " + (i + 1)));
            }
            //selects the first one
            ConfigurableHotkeySelection = ConfigurableHotkeys.FirstOrDefault();
        }
        private bool IsValid(bool showErrorMessage = false)
        {
            return GeneralHelper.AreHotKeyConfigItemsValid(HotkeyConfigItems, showErrorMessage);
        }
        #endregion private functions
    }
}