using System;
using System.IO;
using System.Web.Script.Serialization;
using SimpleCopyPasteTool.Includes;
using SimpleCopyPasteTool.Model;

namespace SimpleCopyPasteTool.Core
{
    public class Config : AppSettings<Config>
    {
        #region members
        private static Config _configInstance = null;
        private HotkeyConfigItem[] _hotkeyConfigItems = new HotkeyConfigItem[]{HotkeyConfigItem.Default(), };

        private int _mainWindowWidth;
        private int _mainWindowHeight;

        private bool _useGlobalHotkeys = true;
        private bool _useMinimizeSystemTray = true;
        private bool _useDirectPaste;
        private bool _useLaunchAtStartup;
        #endregion members

        #region constructors
        public static Config Instance
        {
            get
            {
                if (_configInstance == null)
                {
                    _configInstance = Config.Load();
                }

                return _configInstance;
            }
        }
        #endregion constructors

        #region properties
        public HotkeyConfigItem[] HotkeyConfigItems
        {
            get => _hotkeyConfigItems;
            set => _hotkeyConfigItems = value;
        }
        public int MainWindowWidth
        {
            get => _mainWindowWidth;
            set
            {
                _mainWindowWidth = value;
                //dont save in here, it sets the properties at startup and dan it had an loop gone wrong in it
            }
        }
        public int MainWindowHeight
        {
            get => _mainWindowHeight;
            set
            {
                _mainWindowHeight = value;
            }
        }

        public bool UseGlobalHotkeys
        {
            get => _useGlobalHotkeys;
            set
            {
                _useGlobalHotkeys = value;
            }
        }
        public bool UseMinimizeSystemTray
        {
            get => _useMinimizeSystemTray;
            set
            {
                _useMinimizeSystemTray = value;
            }
        }
        public bool UseDirectPaste
        {
            get => _useDirectPaste;
            set
            {
                _useDirectPaste = value;
            }
        }
        public bool UseLaunchAtStartup
        {
            get => _useLaunchAtStartup;
            set
            {
                _useLaunchAtStartup = value;
            }
        }
        #endregion properties

        #region public functions
        public void SaveAll(HotkeyConfigItem[] hotkeyConfigItems, bool useGlobalHotkeys = true, bool useMinimizeSystemTray = true, bool useDirectPaste = false, bool useLaunchAtStartup = false)
        {
            //the amount of hotkeys may not exceed the application wide limit
            if (hotkeyConfigItems != null && hotkeyConfigItems.Length >= Convert.ToInt32(Constants.MIN_HOTKEYS) && hotkeyConfigItems.Length <= Convert.ToInt32(Constants.MAX_HOTKEYS))
            {
                _hotkeyConfigItems = hotkeyConfigItems;
            }
            _useGlobalHotkeys = useGlobalHotkeys;
            _useMinimizeSystemTray = useMinimizeSystemTray;
            _useDirectPaste = useDirectPaste;
            _useLaunchAtStartup = useLaunchAtStartup;
            this.Save();
        }
        public void ResetHotkeySettings()
        {
            _hotkeyConfigItems = new HotkeyConfigItem[1] {HotkeyConfigItem.Default()};
            this.Save();
        }
        public void ResetAllSettings()
        {
            ResetHotkeySettings();
            _useGlobalHotkeys = true;
            _useMinimizeSystemTray = true;
            _useDirectPaste = false;
            _useLaunchAtStartup = false;
            this.Save();
        }
        #endregion public functions
    }
}