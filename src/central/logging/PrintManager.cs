namespace VMRuntime.Logging
{
    public static class PrintManager
    {
        private static IPrintSystem _instance = new ConsoleLogSystem();
        public static IPrintSystem Current => _instance;

        public static void SetPrintSystem(IPrintSystem system)
        {
            _instance = system ?? new ConsoleLogSystem();
        }
    }
}