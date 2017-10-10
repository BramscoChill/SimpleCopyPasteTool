using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace SimpleCopyPasteTool.Includes
{

    internal class GlobalHotKeyWinApi
    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public sealed class GlobalHotKey : IDisposable
    {
        public event Action<GlobalHotKey> HotKeyPressed;

        private readonly int _id;
        private int _configId;
        private bool _isKeyRegistered;
        readonly IntPtr _handle;

        public GlobalHotKey(ModifierKeys modifierKeys, Keys key, Window window, int configId)
            : this(modifierKeys, key, new WindowInteropHelper(window), configId)
        {
            Contract.Requires(window != null);
        }

        public GlobalHotKey(ModifierKeys modifierKeys, Keys key, WindowInteropHelper window, int configId)
            : this(modifierKeys, key, window.Handle, configId)
        {
            Contract.Requires(window != null);
        }

        public GlobalHotKey(ModifierKeys modifierKeys, Keys key, IntPtr windowHandle, int configId)
        {
            Contract.Requires(modifierKeys != ModifierKeys.None || key != Keys.None);
            Contract.Requires(windowHandle != IntPtr.Zero);

            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            _handle = windowHandle;
            _configId = configId;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        ~GlobalHotKey()
        {
            Dispose();
        }

        public Keys Key { get; private set; }

        public ModifierKeys KeyModifier { get; private set; }

        public int ConfigId
        {
            get => _configId;
            set => _configId = value;
        }

        public void RegisterHotKey()
        {
            if (Key == Keys.None)
                return;
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = GlobalHotKeyWinApi.RegisterHotKey(_handle, _id, KeyModifier, Key);
            if (!_isKeyRegistered)
                throw new ApplicationException("Hotkey already in use");
        }

        public void UnregisterHotKey()
        {
            _isKeyRegistered = !GlobalHotKeyWinApi.UnregisterHotKey(_handle, _id);
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            UnregisterHotKey();
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == GlobalHotKeyWinApi.WmHotKey
                    && (int)(msg.wParam) == _id)
                {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }

        private void OnHotKeyPressed()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this);
        }
    }
}