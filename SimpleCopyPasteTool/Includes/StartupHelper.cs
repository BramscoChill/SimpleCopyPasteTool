using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security.RightsManagement;
using System.Windows;
using Microsoft.Win32;

namespace SimpleCopyPasteTool.Includes
{
    public static class StartupHelper
    {

        /// <summary>
        /// Execute this at startup to check the install location
        /// </summary>
        public static void CheckProcessesAndKillNotFromInstallPath()
        {
            try
            {
                //get all the processes of the simplecopypastetool
                var allProcceses = Process.GetProcesses().Where(p => p.ProcessName.Contains(Constants.ASSEMBLY_TITLE));
                foreach (Process process in allProcceses)
                {
                    string fullPath = process.MainModule.FileName;
                    //checks if the executing path is not on the location it should be, the install dir
                    if (StringUtils.NormalizePath(fullPath) != StringUtils.NormalizePath(Constants.APPLICATION_FULL_PATH))
                    {
                        process.Kill();
                        process.WaitForExit();
                        //File.Delete(fullPath);
                        //MessageBox.Show(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
//                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
//                Environment.Exit(0);
            }
        }
        public static void InstallApplication()
        {
            var installStatus = CheckApplicationInstalled().Data;
            //MessageBox.Show(installStatus.ToString());
            switch (installStatus)
            {
                case Constants.ApplicationInstallationStatus.NotInstalled:
                case Constants.ApplicationInstallationStatus.CurrentVersionHigher:
                    GeneralHelper.ShowMessageBox($"De applicatie wordt geinstalleer onder: \"{Constants.INSTALL_FULLPATH}\". Er wordt een icoon op het bureublad geplaatst, start de applicatie de volgende keer vanaf daar! (nu wordt dit automatisch gedaan)", MessageBoxImage.Information);
                    //copy to the desired location
                    File.Copy(Constants.APPLICATION_FULL_PATH, Constants.INSTALL_FULLPATH, true);
                    //starts it
                    Process.Start(Constants.INSTALL_FULLPATH);
                    break;
                case Constants.ApplicationInstallationStatus.InstalledBudExecutedFromOtherLocation:
                    GeneralHelper.ShowMessageBox($"Start de applicatie de volgende keer vanaf het bureaublad of vanaf: \"{Constants.INSTALL_FULLPATH}\". Nu wordt dit automatisch gedaan.", MessageBoxImage.Information);
                    //starts it
                    Process.Start(Constants.INSTALL_FULLPATH);
                    break;
                case Constants.ApplicationInstallationStatus.Installed:
                    //all good
                    CheckProcessesAndKillNotFromInstallPath();
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    CreateShortcutIconAtPath(desktopPath);
                    break;
            }
        }
        public static Result<Constants.ApplicationInstallationStatus> CheckApplicationInstalled()
        {
            Result<Constants.ApplicationInstallationStatus> result = new Result<Constants.ApplicationInstallationStatus>();

            string expectedInstallPath = Constants.INSTALL_FULLPATH;
            string executingPath = Constants.APPLICATION_FULL_PATH;

            if(expectedInstallPath.IsNullOrEmpty())
                throw new RightsManagementException("No write rights at disk");

            if (File.Exists(expectedInstallPath))
            {
                //installed and executed from instllation location
                if (executingPath.Equals(expectedInstallPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = Constants.ApplicationInstallationStatus.Installed;
                    result.Succeeded = true;
                }
                else //executed from other location, bud exists at install location
                {
                    var installLocation = System.Diagnostics.FileVersionInfo.GetVersionInfo(expectedInstallPath);
                    var currentLocation = System.Diagnostics.FileVersionInfo.GetVersionInfo(executingPath);
                    var installVersion = new Version(installLocation.ProductVersion);
                    var currentlVersion = new Version(currentLocation.ProductVersion);
                    if (installVersion == currentlVersion)
                    {
                        result.Data = Constants.ApplicationInstallationStatus.InstalledBudExecutedFromOtherLocation;
                        //MessageBox.Show($"executingPath: {executingPath}, expectedInstallPath: {expectedInstallPath}");
                    }
//                    else if (installVersion > currentlVersion) //cannot be possible
//                    {
//                        result.Data = Constants.ApplicationInstallationStatus.CurrentVersionLower;
//                    }
                    else
                    {
                        result.Data = Constants.ApplicationInstallationStatus.CurrentVersionHigher;
                    }
                }
                
            }
            else
            {
                result.Data = Constants.ApplicationInstallationStatus.NotInstalled;
            }
            return result;
        }
        public static bool CheckAddedToStartup(bool automaticallyAdd)
        {
            //it needs to be at an static location
            if (CheckApplicationInstalled().Data != Constants.ApplicationInstallationStatus.Installed)
                return false;

            bool isAlreadyAdded = false;
//            bool canAddInRegistry = CanWriteKey(Constants.WINDOWS_REGISTER_STARTUP_LOCATION);
//
//            //checks if we have permissions for the startup location in the registry
//            if (canAddInRegistry)
//            {
//                try
//                {
//                    RegistryKey regKey = Registry.CurrentUser.OpenSubKey(Constants.WINDOWS_REGISTER_STARTUP_LOCATION, true);
//                    string currentValue = regKey.GetValue(Constants.APP_REGISTER_NAME, string.Empty).ToString();
//
//                    if (currentValue.Equals(FULL_STARTUP_PATH, StringComparison.OrdinalIgnoreCase))
//                    {
//                        isAlreadyAdded = true;
//                    }
//                    else
//                    {
//                        isAlreadyAdded = false;
//                        if (automaticallyAdd)
//                        {
//                            regKey.SetValue(Constants.APP_REGISTER_NAME, "\"" + Constants.APPLICATION_FULL_PATH + "\"");
//                        }
//                    }
//                }
//                catch
//                {
//                    canAddInRegistry = false;
//                }
//            }

            //if the registry adding faills, try it in the startup folder
            try
            {
                //get the rifht startup folder and copies it
                Environment.SpecialFolder folder = (IsWindowsVistaOrHigher() ? Environment.SpecialFolder.CommonStartup : Environment.SpecialFolder.Startup);
                var startupPath = Environment.GetFolderPath(folder);
                string foundInStartupFolderLocation = null;

                foreach (string fullFilePath in Directory.EnumerateFiles(startupPath))
                {
                    if (Path.GetExtension(fullFilePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        string targetFullPath = GetShortcutTargetFile(fullFilePath);
                        //if it is this lnk file
                        if (targetFullPath.IsNullOrEmpty() == false && targetFullPath.Equals(Constants.APPLICATION_FULL_PATH, StringComparison.OrdinalIgnoreCase))
                        {
                            foundInStartupFolderLocation = fullFilePath;
                            isAlreadyAdded = true;
                            break;
                        }
                    }
                    else //maybe somehow the exe file is in there, bud not really possible
                    {
                        var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(fullFilePath);
                        if (info != null && info.InternalName.IsNullOrEmpty() == false)
                        {
                            bool foundInStartupFolder = info.InternalName.Equals(Constants.ASSEMBLY_TITLE, StringComparison.OrdinalIgnoreCase);
                            if (foundInStartupFolder)
                            {
                                foundInStartupFolderLocation = fullFilePath;
                                isAlreadyAdded = true;
                                break;
                            }
                        }
                    }
                }

                if (automaticallyAdd)
                {
                    //remove old one
                    if (foundInStartupFolderLocation.IsNullOrEmpty() == false)
                    {
                        File.Delete(foundInStartupFolderLocation);
                    }

                    CreateShortcutIconAtPath(startupPath);
                }
            }
            catch
            {
                isAlreadyAdded = false;
            }
            
            return isAlreadyAdded;
        }

        public static void RemoveFromStartup()
        {
//            try
//            {
//                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(Constants.WINDOWS_REGISTER_STARTUP_LOCATION, true);
//                string currentValue = regKey.GetValue(Constants.APP_REGISTER_NAME, string.Empty).ToString();
//                regKey.DeleteValue(Constants.APP_REGISTER_NAME, false);
//            }
//            catch { }

            try
            {
                Environment.SpecialFolder folder = (IsWindowsVistaOrHigher() ? Environment.SpecialFolder.CommonStartup : Environment.SpecialFolder.Startup);
                var startupPath = Environment.GetFolderPath(folder);

                string foundInStartupFolderLocation = null;

                foreach (string fullFilePath in Directory.EnumerateFiles(startupPath))
                {
                    if (Path.GetExtension(fullFilePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        string targetFullPath = GetShortcutTargetFile(fullFilePath);
                        //if it is this lnk file
                        if (targetFullPath.IsNullOrEmpty() == false && targetFullPath.Equals(Constants.APPLICATION_FULL_PATH, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(fullFilePath);
                        }
                    }
                    else //maybe somehow the exe file is in there, bud not really possible
                    {
                        var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(fullFilePath);
                        if (info != null && info.InternalName.IsNullOrEmpty() == false)
                        {
                            bool foundInStartupFolder = info.InternalName.Equals(Constants.ASSEMBLY_TITLE, StringComparison.OrdinalIgnoreCase);
                            if (foundInStartupFolder)
                            {
                                File.Delete(fullFilePath);
                                break;
                            }
                        }
                    }
                }
            } catch { }
        }
        public static string GetShortcutTargetFile(string shortcutFullPath)
        {
            string path = null;
            using (ShellLink shellLink = new ShellLink(shortcutFullPath))
            {
                path = shellLink.TargetPath;
            }
            return path;
        }
        /// <summary>
        /// ONLY USE when application is installed
        /// </summary>
        public static void CreateShortcutIconAtPath(string directoryPath)
        {
            string fullAppPath = Constants.APPLICATION_FULL_PATH;
            string iconPath = Path.Combine(Path.GetDirectoryName(fullAppPath), "icon.ico");
            var shortcutLocation = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(fullAppPath) + ".lnk");

            if (File.Exists(iconPath) == false)
            {
                //copy icon
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleCopyPasteTool.Resources.Images.icon.ico");
                FileStream fileStream = new FileStream(iconPath, FileMode.CreateNew);
                for (int i = 0; i < stream.Length; i++)
                    fileStream.WriteByte((byte)stream.ReadByte());
                fileStream.Close();
            }
            
            if (File.Exists(shortcutLocation))
            {
                //if the existing icon path is not the same as the current application install path (happens with older versions)
                string targetFullPath = GetShortcutTargetFile(shortcutLocation);
                if (StringUtils.NormalizePath(targetFullPath) != StringUtils.NormalizePath(Constants.INSTALL_FULLPATH))
                    File.Delete(shortcutLocation);
            }

            if (File.Exists(shortcutLocation) == false)
            { 
                //create the shortcut
                using (ShellLink shortcut = new ShellLink())
                {
                    shortcut.TargetPath = fullAppPath;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(fullAppPath);
                    shortcut.Description = Constants.ASSEMBLY_TITLE;
                    shortcut.WindowStyle = ShellLink.SW.SW_SHOWNORMAL;
                    shortcut.IconPath = iconPath;
                    shortcut.IconIndex = 0;
                    shortcut.Save(shortcutLocation);
                }
            }
        }
        private static bool IsWindowsVistaOrHigher()
        {
            OperatingSystem os = Environment.OSVersion;
            return ((os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6));
        }
        private static bool IsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
                return (new WindowsPrincipal(identity)).IsInRole(WindowsBuiltInRole.Administrator);

            return false;
        }
        public static bool HavePermissionsOnKey(RegistryPermissionAccess accessLevel, string key)
        {
            try
            {
                RegistryPermission r = new RegistryPermission(accessLevel, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        public static bool CanWriteKey(string key)
        {
            try
            {
                RegistryPermission r = new RegistryPermission(RegistryPermissionAccess.Write, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        public static bool CanReadKey(string key)
        {
            try
            {
                RegistryPermission r = new RegistryPermission(RegistryPermissionAccess.Read, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }
    }
}