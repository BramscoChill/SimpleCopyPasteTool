using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SimpleCopyPasteTool.Model;

namespace SimpleCopyPasteTool.Includes
{
    public static class GeneralHelper
    {
        private static readonly Random _random = new Random(DateTime.Now.Year + DateTime.Now.DayOfYear + DateTime.Now.Month + DateTime.Now.Millisecond + 578);
        public static bool IsAnyKeyPressed()
        {
            var allPossibleKeys = Enum.GetValues(typeof(Key));
            bool results = false;
            foreach (var currentKey in allPossibleKeys)
            {
                Key key = (Key)currentKey;
                if (key != Key.None)
                    if (Keyboard.IsKeyDown((Key)currentKey)) { results = true; break; }
            }
            return results;
        }
        public static List<int> GetKeysPressed()
        {
            List<int> keysPressed = new List<int>();

            var allPossibleKeys = Enum.GetValues(typeof(Key));
            bool results = false;
            foreach (var currentKey in allPossibleKeys)
            {
                Key key = (Key)currentKey;
                if (key != Key.None)
                    if (Keyboard.IsKeyDown((Key) currentKey))
                    {
                        keysPressed.Add((int)key);
                    }
            }
            return keysPressed;
        }

        public static int CheckColumn(int[,] matrix, int column)
        {
            int[] data = new int[matrix.GetLength(0)];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = matrix[i, column];
            }
            var hist = data.GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Num = g.Key, Dupes = g.Count() - 1 })
                .Where(h => h.Dupes > 0);

            return hist.Count() > 0 ? hist.Sum(h => h.Dupes) : 0;
        }

        public static bool AreHotkeysEqual(Tuple<int, int> hotkey1, Tuple<int, int> hotkey2)
        {
            //if they are all zero, they are not the same, because 0 == NONE
            if (hotkey1.Item1 == 0 && hotkey1.Equals(hotkey2))
                return false;

            //if there are not all 0
            // compare the rows, if they are both 0, they are the same, compare the rest
            bool row1Equal = ((hotkey1.Item1 == 0 && hotkey2.Item1 == 0) || (hotkey1.Item1 == hotkey2.Item1));
            bool row2Equal = ((hotkey1.Item2 == 0 && hotkey2.Item2 == 0) || (hotkey1.Item2 == hotkey2.Item2));

            
            return row1Equal && row2Equal;
        }
        public static BitmapImage GetRandomImage
        {
            get
            {
                string[] images =
                {
                    "Images/universe1.gif",
                    "Images/universe2.gif",
                    "Images/universe3.gif",
                    "Images/universe7.gif",
                };
                var id = _random.Next(0, images.Length);
                var bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + images[id]));
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        public static BitmapSource BitmapImageFromFile(string filepath)
        {
            var bi = new BitmapImage();

            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                bi.BeginInit();
                bi.StreamSource = fs;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
            }

            bi.Freeze(); //Important to freeze it, otherwise it will still have minor leaks

            return bi;
        }

        public static void ShowMessageBox(string text, MessageBoxImage type, Window window = null)
        {
            string title = type == MessageBoxImage.Error ? "ERROR" : "Informatie";
            if (Application.Current.Dispatcher.CheckAccess())
            {
                //Application.Current.MainWindow
                MessageBox.Show(window ?? Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) ?? Application.Current.MainWindow, text, title, MessageBoxButton.OK, type);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    MessageBox.Show(window ?? Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) ?? Application.Current.MainWindow, text, title, MessageBoxButton.OK, type);
                }));
            }
        }

        public static bool AreHotKeyConfigItemsValid(HotkeyConfigItem[] hotkeyConfigItems, bool showErrorMessage = false)
        {
            bool isValid = true;
            //compares all hotkey configurations with each other, they cannot be te same
            for (var i = 0; i < hotkeyConfigItems.Length; i++)
            {
                for (var j = 0; j < hotkeyConfigItems.Length; j++)
                {
                    if (i != j)
                    {
                        bool areEqual = hotkeyConfigItems[i].AreHotkeysEqual(hotkeyConfigItems[j]);
                        if (areEqual)
                        {
                            if (showErrorMessage)
                                GeneralHelper.ShowMessageBox($"Sneltoets {(i + 1)} mag niet gelijk zijn aan sneltoets {(j + 1)}!", MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
            }
            return isValid;
        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                if (Directory.Exists(dirPath) == false)
                    Directory.CreateDirectory(dirPath);

                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                { }
                return true;
            }
            catch  //(Exception ex)
            {
                //MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }
    }
    public static class GenericCopier<T>    //deep copy a list
    {
        public static T DeepCopy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}