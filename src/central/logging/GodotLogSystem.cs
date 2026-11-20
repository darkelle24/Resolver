using System;
using System.IO;
using Godot;

namespace VMRuntime.Logging
{
    public class GodotLogSystem : ILogSystem
    {
        private readonly string _logFilePath = "user://vm_log.txt";

        public void Log(string message)
        {
            GD.Print(message);
            WriteToFile($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            GD.PushWarning(message);
            WriteToFile($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            GD.PushError(message);
            WriteToFile($"[ERROR] {message}");
        }

        private void WriteToFile(string line)
        {
            string fullPath = ProjectSettings.GlobalizePath(_logFilePath);
            using (var writer = File.AppendText(fullPath))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {line}");
            }
        }
    }
}