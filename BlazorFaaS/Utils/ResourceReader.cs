// ======================================
//  From Chris Sainity
//  Blazor Spread. LHTV
// ======================================
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BlazorFaaS.Client.Utils
{
    public static class ResourceReader
    {
        public static string Read(string name)
        {
            try {
                var assembly = Assembly.GetExecutingAssembly();
                string resourcePath = name;
                // Format is something like: "{Namespace}.{Folder}.{filename}.{Extension}"
                resourcePath = assembly.GetManifestResourceNames().Single(_ => _.EndsWith(name));
                // deseralize
                using var stream = assembly.GetManifestResourceStream(resourcePath);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch {
                // Unexpected. Missing resorce 
            }
            return "";
        }
    }
}
