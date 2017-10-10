using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleCopyPasteTool.Core;
using SimpleCopyPasteTool.Includes;

namespace SimpleCopyPasteTool.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private RelayCommand _quitAboutCommand;


        public AboutViewModel()
        {

        }
        

        #region properties
        public string MainText { get; set; }

        #endregion properties

        #region RelayCommands
        public ICommand QuitAboutCommand
        {
            get
            {
                if (_quitAboutCommand == null)
                {
                    _quitAboutCommand = new RelayCommand(param => QuitAbout(param));
                }

                return _quitAboutCommand;
            }
        }

        #endregion RelayCommands

        #region RelayCommand functions

        private void QuitAbout(object windowObjectParam)
        {
            ((Window)windowObjectParam).DialogResult = true;
            ((Window)windowObjectParam).Close();
        }
        #endregion RelayCommand functions

    }

    public class NegatingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return -((double)value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return +(double)value;
            }
            return value;
        }
    }
}