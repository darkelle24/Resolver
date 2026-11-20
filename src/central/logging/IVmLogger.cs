using VMRuntime.Diagnostics;

namespace VMRuntime.Logging;

/// <summary>Receives structured failures at compilation and execution boundaries.</summary>
public interface IVmLogger
{
    /// <summary>Records one failure with source and execution context.</summary>
    void Log(VmError error);
}

/// <summary>Silent logger used unless the host supplies a logging destination.</summary>
public sealed class NullVmLogger : IVmLogger
{
    /// <summary>Shared stateless logger.</summary>
    public static NullVmLogger Instance { get; } = new();
    /// <summary>Accepts a diagnostic without writing it.</summary>
    public void Log(VmError error) { }
}

/// <summary>Thread-safe text logger; the caller retains ownership of the writer.</summary>
public sealed class TextWriterVmLogger : IVmLogger
{
    private readonly TextWriter _writer;
    private readonly object _gate = new();
    /// <summary>Creates a structured logger for the supplied writer.</summary>
    public TextWriterVmLogger(TextWriter writer)
        => _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    /// <summary>Writes one complete JSON diagnostic.</summary>
    public void Log(VmError error)
    {
        lock (_gate) { _writer.WriteLine(error.ToJson(false)); _writer.Flush(); }
    }
}

/// <summary>Standalone fallback for the legacy logging and printing interfaces.</summary>
public sealed class ConsoleLogSystem : ILogSystem, IPrintSystem
{
    /// <summary>Writes information to standard output.</summary>
    public void Log(string message) => Console.WriteLine(message);
    /// <summary>Writes a warning to standard error.</summary>
    public void LogWarning(string message) => Console.Error.WriteLine(message);
    /// <summary>Writes an error to standard error.</summary>
    public void LogError(string message) => Console.Error.WriteLine(message);
    /// <summary>Writes text to standard output.</summary>
    public void Print(string message) => Log(message);
    /// <summary>Writes a warning.</summary>
    public void PrintWarning(string message) => LogWarning(message);
    /// <summary>Writes an error.</summary>
    public void PrintError(string message) => LogError(message);
}
