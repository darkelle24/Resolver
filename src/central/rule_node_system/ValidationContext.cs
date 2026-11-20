using System.Collections.Generic;

namespace VM.RuleNodeSystem
{
    public class ValidationContext
    {
        private readonly List<ValidationError> _errors = new();
        public IReadOnlyList<ValidationError> Errors => _errors;

        public void AddError(string path, string message)
            => _errors.Add(new ValidationError(path, message));

        public bool HasErrors => _errors.Count > 0;
    }

    public record ValidationError(string Path, string Message);
}