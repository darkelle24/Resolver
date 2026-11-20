namespace VMRuntime.Logging
{
    public static class LogManager
    {
        private static ILogSystem _instance = new ConsoleLogSystem();
        public static ILogSystem Current => _instance;

        public static void SetLogSystem(ILogSystem system)
        {
            _instance = system ?? new ConsoleLogSystem();
        }
    }
}