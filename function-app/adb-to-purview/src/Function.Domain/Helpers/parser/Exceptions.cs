using System;

namespace Function.Domain.Helpers
{
    /// <summary>
    /// Exception throuwn when the current runo of the function cannot continue due to missing or invalid configuration.
    /// </summary>
    public class ConfigMismatchException : Exception
    {
        public ConfigMismatchException(string message) 
            : base(message) { }

        public ConfigMismatchException() { }

        public ConfigMismatchException(string message, Exception inner)
            : base(message, inner) { }

    }
    /// <summary>
    /// Exception thrown when the current run of the function cannot continue due to mismatched or missing data values.
    /// </summary>
    public class MissingCriticalDataException : Exception
    {
        public MissingCriticalDataException(string message) 
            : base(message) { }

        public MissingCriticalDataException() { }

        public MissingCriticalDataException(string message, Exception inner)
            : base(message, inner) { }
    }
}