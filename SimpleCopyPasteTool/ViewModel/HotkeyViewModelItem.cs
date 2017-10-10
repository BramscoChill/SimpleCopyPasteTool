using System;
using System.Windows;
using System.Windows.Input;
using SimpleCopyPasteTool.Core;
using SimpleCopyPasteTool.Core.WindowsInput;
using SimpleCopyPasteTool.Includes;

namespace SimpleCopyPasteTool.ViewModel
{
    public delegate void UpdateStatusBarEvent(string text);

    public class HotkeyViewModelItem : ViewModelBase
    {
        public UpdateStatusBarEvent UpdateStatusBarEventHandler;

        #region members
        private int _id;
        private RelayCommand _showHideTextCommand;
        private RelayCommand _copyToClipboardTextCommand;
        #endregion members

        #region constructors
        public HotkeyViewModelItem(int id)
        {
            _id = id;
        }
        #endregion constructors

        #region properties
        public bool ShowText
        {
            get
            {
                if (_id < Config.Instance.HotkeyConfigItems.Length)
                {
                    return Config.Instance.HotkeyConfigItems[_id].ShowText;
                }
                return true;
            }
            set
            {
                if (_id < Config.Instance.HotkeyConfigItems.Length)
                {
                    Config.Instance.HotkeyConfigItems[_id].ShowText = value;
                    Config.Instance.Save();
                }
                RaisePropertyChanged("Text");
                RaisePropertyChanged("ShowText");
            }
        }
        public string Text
        {
            get
            {
                if (_id < Config.Instance.HotkeyConfigItems.Length)
                {
                    if (Config.Instance.HotkeyConfigItems[_id].Text == null)
                        return string.Empty;
                    else if (ShowText)
                        return Config.Instance.HotkeyConfigItems[_id].Text;
                    else
                        return new String('*', Config.Instance.HotkeyConfigItems[_id].Text.Length);
                }
                return String.Empty;
            }
            set
            {
                if (_id < Config.Instance.HotkeyConfigItems.Length)
                {
                    Config.Instance.HotkeyConfigItems[_id].Text = value;
                    Config.Instance.Save();
                    RaisePropertyChanged("Text");
                }
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        public string IdText
        {
            get => (_id+1).ToString();
        }
        #endregion properties

        #region RelayCommands availability
        private bool ShowHideTextEnabled
        {
            get { return true; }
        }
        #endregion RelayCommands availability

        #region RelayCommands
        public ICommand ShowHideTextCommand
        {
            get
            {
                if (_showHideTextCommand == null)
                {
                    _showHideTextCommand = new RelayCommand(param => ShowHideText(), param => ShowHideTextEnabled == true);
                }

                return _showHideTextCommand;
            }
        }
        public ICommand MainActionTextCommand
        {
            get
            {
                if (_copyToClipboardTextCommand == null)
                {
                    //no need to paste when you press the copy button, it makes no sense
                    _copyToClipboardTextCommand = new RelayCommand(param => MainAction(false));
                }

                return _copyToClipboardTextCommand;
            }
        }
        #endregion RelayCommands

        #region RelayCommand functions
        private void ShowHideText()
        {
            ShowText = !ShowText;
        }
        public void MainAction(bool alsoPaste)
        {
            Clipboard.SetText(Config.Instance.HotkeyConfigItems[_id].Text);

            if (alsoPaste && Config.Instance.UseDirectPaste)
            {

                if (string.IsNullOrEmpty(Config.Instance.HotkeyConfigItems[_id].Text) == false)
                    InputSimulator.Instance.Keyboard.TextEntry(Config.Instance.HotkeyConfigItems[_id].Text);
                UpdateStatusBar((_id + 1) + " is meteen geplakt en in het klembord gezet!");
            }
            else
            {
                UpdateStatusBar((_id + 1) + " is gekopieerd naar het klembord!");
            }
        }
        private void UpdateStatusBar(string text)
        {
            if (UpdateStatusBarEventHandler != null)
                UpdateStatusBarEventHandler(text);
        }
        #endregion RelayCommand functions
    }
}