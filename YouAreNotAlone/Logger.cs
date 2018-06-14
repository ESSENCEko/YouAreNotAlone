using System;
using System.IO;

namespace YouAreNotAlone
{
    public static class Logger
    {
        private readonly static string filePath = @"YouAreNotAlone.log";

        public static void Init()
        {
            File.WriteAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Initializing. (" + GTA.Game.Version.ToString() + ")\n");
        }

        public static void ForceWrite(string s, string name)
        {
            if (name == "") File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + " " + name + "\n");
            else File.AppendAllText(filePath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + " (" + name + ")\n");
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