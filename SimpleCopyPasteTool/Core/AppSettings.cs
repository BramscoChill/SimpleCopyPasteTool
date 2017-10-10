using System.IO;
using System.Web.Script.Serialization;
using SimpleCopyPasteTool.Includes;

namespace SimpleCopyPasteTool.Core
{
    public class AppSettings<T> where T : new()
    {
        public void Save(string fileName = null)
        {
            if (fileName.IsNullOrEmpty())
                fileName = Constants.DEFAULT_SETTINGS_FULLPATH;
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static void Save(T pSettings, string fileName = null)
        {
            if (fileName.IsNullOrEmpty())
                fileName = Constants.DEFAULT_SETTINGS_FULLPATH;
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(pSettings));
        }

        public static T Load(string fileName = null)
        {
            if (fileName.IsNullOrEmpty())
                fileName = Constants.DEFAULT_SETTINGS_FULLPATH;
            T t = new T();
            if (File.Exists(fileName))
                t = (new JavaScriptSerializer()).Deserialize<T>(File.ReadAllText(fileName));
            return t;
        }
    }
}