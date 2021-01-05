using System;

namespace Hairibar.NaughtyExtensions.Editor
{
    public sealed class InvalidConversionException : Exception
    {
        public InvalidConversionException(string message) : base(message)
        {
        }

        public InvalidConversionException()
        {
        }
    }
}
