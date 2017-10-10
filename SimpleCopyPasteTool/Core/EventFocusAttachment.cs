using System.Windows;
using System.Windows.Controls;

namespace SimpleCopyPasteTool.Core
{
    public class EventFocusAttachment
    {
        public static Control GetElementToFocus(Control control)
        {
            return (Control)control.GetValue(ElementToFocusProperty);
        }

        public static void SetElementToFocus(Control control, Control value)
        {
            control.SetValue(ElementToFocusProperty, value);
        }

        public static readonly DependencyProperty ElementToFocusProperty =
            DependencyProperty.RegisterAttached("ElementToFocus", typeof(Control),
                typeof(EventFocusAttachment), new UIPropertyMetadata(null, ElementToFocusPropertyChanged));

        public static void ElementToFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            
            if (sender is Button)
            {
                var button = sender as Button;
                button.Click += (s, args) =>
                {
                    Control control = GetElementToFocus(button);
                    if (control != null)
                    {
                        control.Focus();
                        SelectLastLineInTextBox(control);
                    }
                };
            } else if (sender is ComboBox)
            {
                var combobox = sender as ComboBox;
                combobox.SelectionChanged += (o, args) =>
                {
                    Control control = GetElementToFocus(combobox);
                    if (control != null)
                    {
                        control.Focus();
                        SelectLastLineInTextBox(control);
                    }
                };
            }
        }

        private static void SelectLastLineInTextBox(Control control)
        {
            if (control is TextBox)
            {
                var textBox = control as TextBox;
                int len = textBox.Text.Length;
                textBox.Select(len,len);
            }
        }
    }
}