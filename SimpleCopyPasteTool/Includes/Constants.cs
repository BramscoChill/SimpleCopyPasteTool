using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleCopyPasteTool.Includes
{
    public static class Constants
    {
        public static readonly string DEFAULT_SETTINGS_FULLPATH = Path.Combine(ASSEMBLY_DIRECTORY, ASSEMBLY_TITLE + ".json");
        public const string NONE_SELECTION_DEFAULT = " - (geen) - ";
        public static readonly KeyValuePair<int, string> DEFAULT_EMPTY_HOTKEY = new KeyValuePair<int, string>(0, NONE_SELECTION_DEFAULT);
        public const decimal MAX_HOTKEYS = 25;
        public const decimal MIN_HOTKEYS = 1;
        public static string APP_REGISTER_NAME = "SimpleCopyPasteTool";
        public static string WINDOWS_REGISTER_STARTUP_LOCATION = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public static string APPLICATION_FULL_PATH = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
        public static string ASSEMBLY_DIRECTORY
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string _title;
        public static string ASSEMBLY_TITLE
        {
            get
            {
                if (_title == null)
                {
                    AssemblyTitleAttribute attributes = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false);
                    _title = attributes?.Title;
                }
                return _title;
            }
        }
        public static string PROGRAMFILESX86
        {
            get
            {
                if (8 == IntPtr.Size ||
                    (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                }

                return Environment.GetEnvironmentVariable("ProgramFiles");
            }
        }

        private static string _install_fulllpath;
        public static string INSTALL_FULLPATH
        {
            get
            {
                if (_install_fulllpath == null)
                {
                    var tempFolder = Path.Combine(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "TEMP"), ASSEMBLY_TITLE + Path.DirectorySeparatorChar + ASSEMBLY_TITLE + ".exe");
                    if (GeneralHelper.IsDirectoryWritable(Path.GetDirectoryName(tempFolder)))
                    {
                        //System.Windows.MessageBox.Show($" tempFolder: {tempFolder}");
                        _install_fulllpath = tempFolder;
                    }
                }
                return _install_fulllpath;
            }
        }
        public enum ReloadKeyBindingsAction
        {
            Reload,
            Disable,
        }

        public enum ApplicationInstallationStatus
        {
            NotInstalled = 0,
            CurrentVersionHigher,
            //CurrentVersionLower, //cannot be possible
            InstalledBudExecutedFromOtherLocation,
            Installed,
        }
    }
}