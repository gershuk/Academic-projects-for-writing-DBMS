using System;

namespace DBMS_Operation
{
    public enum OperationExecutionState
    {
        notProcessed,
        parserError,
        failed,
        performed
    }

    public interface IOperationResult<T>
    {
        Exception OperationException { get; set; }
        T Result { get; set; }
        OperationExecutionState State { get; set; }
    }

    public class OperationResult<T> : IOperationResult<T>
    {
        public OperationExecutionState State { get; set; }
        public Exception OperationException { get; set; }
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result, Exception opException = null)
        {
            State = state;
            Result = result;
            OperationException = opException;
        }
    }
}