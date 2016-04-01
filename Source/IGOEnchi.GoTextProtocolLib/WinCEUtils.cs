using System;
using System.IO;
using System.Reflection;

namespace IGoEnchi
{
    public static class WinCEUtils
    {
        public static string PathTo(string fileName)
        {
            return GetCurrentDirectory() + @"\" + fileName;
        }

        public static string GetCurrentDirectory()
        {
            return
                Environment.OSVersion.Platform == PlatformID.WinCE
                    ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                    : Directory.GetCurrentDirectory();
        }
    }
}