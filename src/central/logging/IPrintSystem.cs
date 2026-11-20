namespace VMRuntime.Logging
{
    public interface IPrintSystem
    {
        void Print(string message);
        void PrintWarning(string message);
        void PrintError(string message);
    }
}