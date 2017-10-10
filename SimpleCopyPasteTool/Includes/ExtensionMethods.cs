using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace SimpleCopyPasteTool.Includes
{
    public static class ExtensionMethods
    {
        public static ObservableCollection<T> ToObservableCollection<T> (this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ObservableCollection<T>(source);
        }
        public static KeyValuePair<int, string> ToKeyValuePair_Key(this int source)
        {
            string value = source < 1 ? Constants.NONE_SELECTION_DEFAULT : ((Key)source).ToString();
            return new KeyValuePair<int, string>(source, value);
        }
        public static KeyValuePair<int, string> ToKeyValuePair_ModifierKeys(this int source)
        {
            string value = source < 1 ? Constants.NONE_SELECTION_DEFAULT : ((ModifierKeys) source).ToString();
            return new KeyValuePair<int, string>(source, value);
        }
        public static Keys ToKeys(this Key source)
        {
            return (Keys)KeyInterop.VirtualKeyFromKey(source);
        }
        public static Keys ConvertToKeys(params Key[] source)
        {
            Keys keys = Keys.None;
            foreach (Key key in source)
            {
                keys = keys | (Keys)KeyInterop.VirtualKeyFromKey(key);
            }
            return keys;
        }
        public static ModifierKeys? ToModifierKeys(this Keys source)
        {
            ModifierKeys? output = null;
            if (source == Keys.Control || source == Keys.LControlKey || source == Keys.RControlKey)
            {
                output = ModifierKeys.Control;
            }
            else if (source == Keys.Alt || source == Keys.LMenu || source == Keys.RMenu)
            {
                output = ModifierKeys.Alt;
            }
            else if (source == Keys.Shift || source == Keys.LShiftKey || source == Keys.RShiftKey)
            {
                output = ModifierKeys.Shift;
            }
            else if (source == Keys.LWin || source == Keys.RWin)
            {
                output = ModifierKeys.Windows;
            }
            return output;
        }
    }

    public class Result
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class Result<T> where T : new ()
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

}