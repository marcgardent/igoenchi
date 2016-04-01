using System;

namespace IGoEnchi
{
    public class IGSParseException : Exception
    {
        public IGSParseException()
        {
        }

        public IGSParseException(string message) : base(message)
        {
        }
    }
}