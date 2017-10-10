using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SimpleCopyPasteTool.Core.HookManager;
using SimpleCopyPasteTool.Includes;
using SimpleCopyPasteTool.ViewModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;

namespace SimpleCopyPasteTool.View
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        private OptionsViewModel viewModel;
        private GlobalEventProvider globalEventProvider;
        private KeyEventHandler globalKeyUpEventHandler;
        private bool hasHotkeyInputSelected = false;

        private Key textKey = Key.None;
        private ModifierKeys textModifierKeys = ModifierKeys.None;

        public OptionsView()
        {
            InitializeComponent();

            viewModel = mainGrid.DataContext as OptionsViewModel;
            globalEventProvider = new GlobalEventProvider();
            //makes sure all up keys are parsed, before resetting with the debounced down key event
            globalKeyUpEventHandler = globalEventProvider.CreateDebounceKeyEventHandler(Global_OnKeyUp, TimeSpan.FromMilliseconds(300));

            AttachGlobalKeyListener();
        }

        private void Global_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Key key = KeyInterop.KeyFromVirtualKey((int) e.KeyCode);
            if (hasHotkeyInputSelected && IsKeyValid((int)key))
            {
                ModifierKeys? currentModifierKeys = e.KeyCode.ToModifierKeys();
                if (currentModifierKeys != null)
                {
                    textModifierKeys = textModifierKeys | currentModifierKeys.Value;
                }
                else
                {
                    textKey = key;
                }
                //Console.WriteLine("DOWN - downkeys: " + (pressedKeys == null ? string.Empty : string.Join(",", pressedKeys.Select(x => x.ToString()))));
            }
        }
        private void Global_OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (textKey != Key.None && textModifierKeys != ModifierKeys.None)
            {
                SetCurrentHotkeyToOptions();
            }

            textKey = Key.None;
            textModifierKeys = ModifierKeys.None;
            //Console.WriteLine("UP - downkeys: " + (pressedKeys == null ? string.Empty : string.Join(",", pressedKeys.Select(x => x.ToString()))));
        }
        private void Text1_GotFocus(object sender, RoutedEventArgs e)
        {
            hasHotkeyInputSelected = true;
            AttachGlobalKeyListener();
        }

        private void Text1_LostFocus(object sender, RoutedEventArgs e)
        {
            hasHotkeyInputSelected = false;
            DeAttachGlobalKeyListener();
        }
        

        private bool IsKeyValid(int key)
        {
            //Key values
            return key > 0 && ((key >= 34 && key <= 69) || //AZ TM 10
                   (key >= 74 && key <= 83) || //numpad keys
                   (key >= 84 && key <= 89) || //Add, Separator, Subtract, Decimal, Divide
                   (key >= 90 && key <= 113) || //F1 tm F22
                   (key >= 116 && key <= 121) || //Shift, control, alt, 
                   (key >= 70 && key <= 71)); //win
        }
        private void AttachGlobalKeyListener()
        {
            textKey = Key.None;
            textModifierKeys = ModifierKeys.None;
            globalEventProvider.KeyDown += Global_OnKeyDown;
            globalEventProvider.KeyUp += globalKeyUpEventHandler;
        }

        private void DeAttachGlobalKeyListener()
        {
            textKey = Key.None;
            textModifierKeys = ModifierKeys.None;
            globalEventProvider.KeyDown -= Global_OnKeyDown;
            globalEventProvider.KeyUp -= globalKeyUpEventHandler;
        }

        private void SetCurrentHotkeyToOptions()
        {
            //sets the current keys to the options model
            if (hasHotkeyInputSelected)
            {
                if ((int)textModifierKeys != viewModel.TextModifierKeys)
                {
                    viewModel.TextModifierKeys = (int) textModifierKeys;
                }
                if ((int)textKey != viewModel.TextKey)
                {
                    viewModel.TextKey = (int)textKey;
                }
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            DeAttachGlobalKeyListener();

            base.OnClosing(e);
        }
        
    }
}
