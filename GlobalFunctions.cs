using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OjamajoBot
{
    public class GlobalFunctions
    {
        public static string getRandomFile(string path, string[] extensions)
        {
            string file = null;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var di = new DirectoryInfo(path);
                    var rgFiles = di.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
                    Random R = new Random();
                    file = rgFiles.ElementAt(R.Next(0, rgFiles.Count())).FullName;
                }
                // probably should only catch specific exceptions
                // throwable by the above methods.
                catch { }
            }
            return file;
        }

    }
}
