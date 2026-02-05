using System;
using System.IO;

namespace SoftOne.Soe.Util
{
    public static class IOUtil
    {
        public static string FileNameSafe(string source)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            { 
                source = source.Replace(c.ToString(), ""); 
            }

            source = source.Trim();

            if (string.IsNullOrEmpty(source))
                source = Guid.NewGuid().ToString();

            return source;
        }
    }
}
