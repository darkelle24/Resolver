namespace VMRuntime.Logging
{
    public interface ILogSystem
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}