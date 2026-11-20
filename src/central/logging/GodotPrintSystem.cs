using Godot;

namespace VMRuntime.Logging
{
    public class GodotPrintSystem : IPrintSystem
    {
        public void Print(string message) => GD.Print(message);
        public void PrintWarning(string message) => GD.PushWarning(message);
        public void PrintError(string message) => GD.PushError(message);
    }
}