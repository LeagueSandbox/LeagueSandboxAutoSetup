using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;

namespace League_Sandbox_Auto_Setup.Util
{
    public class Utility
    {
        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation, string description, String iconPath)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.WorkingDirectory = Path.GetDirectoryName(targetFileLocation);
            shortcut.Description = description;   // The description of the shortcut
            shortcut.IconLocation = iconPath;           // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            shortcut.Save();                                    // Save the shortcut
        }

        public static void WaitForFile(Action action, int timeoutMs = 300000)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs) // Default timeout is 5 mins
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException e)
                {
                    // access error
                    if (e.HResult != -2147024864)
                    {
                        throw;
                    }
                }
            }
            throw new Exception("Failed to perform action within timeout.");
        }
    }
}
