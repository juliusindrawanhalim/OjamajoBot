using Config;
using Newtonsoft.Json.Linq;
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

        public static JToken getPropertyValue(string directory,string property)
        {
            var val = JObject.Parse(File.ReadAllText($"{directory}"));
            JProperty optionProp = val.Property(property);
            return optionProp.Value;
            
        }

        public static string UppercaseFirst(string s){
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
