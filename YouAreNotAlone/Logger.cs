using System;
using System.IO;

namespace YouAreNotAlone
{
    public static class Logger
    {
        private static string filePath = @"YouAreNotAlone.log";

        public static void Init()
        {
            if (Main.NoLog) return;

            File.WriteAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "YouAreNotAlone started. (" + GTA.Game.Version.ToString() + ")\n");
        }

        public static void Write(string s, string name)
        {
            if (Main.NoLog) return;

            if (name == "") File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + " " + name + "\n");
            else File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + " (" + name + ")\n");
        }

        public static void Error(string s, string name)
        {
            if (Main.NoLog) return;

            if (name == "") File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] " + s + " " + name + "\n");
            else File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] " + s + " (" + name + ")\n");
        }
    }
}