using System;

namespace DataBaseErrors
{
    class ParsingRequestError : Exception
    {
        public ParsingRequestError (string message,string errorLocation)
            : base($"{message} {errorLocation}")
        { }

        public override string ToString() => this.Message;
    }
}
