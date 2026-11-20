using System.Reflection;
using VMRuntime.Logging;

namespace VMRuntime;

/// <summary>Legacy Godot initialization; the standalone VmEngine requires no global initialization.</summary>
public static class Initializer
{
    private static bool _initialized;
    private static readonly object Gate = new();
    /// <summary>Registers default Godot methods and logging once. Additional game assemblies can register explicitly.</summary>
    public static void Initialize()
    {
        lock (Gate)
        {
            if (_initialized) return;
            DefaultExposedMethods.RegisterDefaults();
            ExposedMethodRegistry.InitializeFromAttributes(Assembly.GetExecutingAssembly());
            PrintManager.SetPrintSystem(new GodotPrintSystem());
            LogManager.SetLogSystem(new GodotLogSystem());
            _initialized = true;
        }
    }
}
