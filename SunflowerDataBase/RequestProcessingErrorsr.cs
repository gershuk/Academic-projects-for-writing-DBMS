using System;

namespace SunflowerDB
{
    internal class ParsingRequestError : Exception
    {
        public ParsingRequestError(string message, string errorLocation)
            : base($"{message} {errorLocation}")
        { }

        public override string ToString() => Message;
    }
}
