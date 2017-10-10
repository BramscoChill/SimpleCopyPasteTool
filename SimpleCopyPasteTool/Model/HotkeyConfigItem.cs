using System;
using SimpleCopyPasteTool.Includes;

namespace SimpleCopyPasteTool.Model
{
    public class HotkeyConfigItem : ICloneable
    {
        private string _text;
        private bool _showText;
        private int _textKey;
        private int _textModifierKeys;

        public string Text
        {
            get => _text.ToSigleNewline();
            set => _text = value.ToSigleNewline();
        }

        public bool ShowText
        {
            get => _showText;
            set => _showText = value;
        }

        public int TextKey
        {
            get => _textKey;
            set => _textKey = value;
        }

        public int TextModifierKeys
        {
            get => _textModifierKeys;
            set => _textModifierKeys = value;
        }

        public static HotkeyConfigItem Default()
        {
            return new HotkeyConfigItem() { ShowText = true, Text = string.Empty, TextKey = 0, TextModifierKeys = 0 };
        }

        public bool AreHotkeysEqual(HotkeyConfigItem comparer)
        {
            //if they are all zero, they are not the same, because 0 == NONE
            if (TextKey == 0 && comparer.TextKey == 0 && TextModifierKeys == 0 && comparer.TextModifierKeys == 0)
                return false;

            //if there are not all 0
            // compare the rows, if they are both 0, they are the same, compare the rest
            bool row1Equal = ((TextKey == 0 && comparer.TextKey == 0) || (TextKey == comparer.TextKey));
            bool row2Equal = ((TextModifierKeys == 0 && comparer.TextModifierKeys == 0) || (TextModifierKeys == comparer.TextModifierKeys));


            return row1Equal && row2Equal;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}