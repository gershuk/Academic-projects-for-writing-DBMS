using System;
using System.Collections.Generic;

namespace DataBaseType
{
    public enum UnionKind
    {
        Empty,
        All
    }

    public enum JoinKind
    {
        Empty,
        Inner,
        Left,
        Right
    }

    
    public enum OperationExecutionState
    {
        notProcessed,
        parserError,
        failed,
        performed
    }

    public interface IOperationResult<T>
    {
        DBError OperationException { get; set; }
        T Result { get; set; }
        OperationExecutionState State { get; set; }
    }

    
    public class OperationResult<T> : IOperationResult<T>
    {
        public OperationExecutionState State { get; set; }
        public DBError OperationException { get; set; }
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result, DBError opException = null)
        {
            State = state;
            Result = result;
            OperationException = opException;
        }

        public override string ToString()
        {
            var result = "---------------------------------------\n";

            switch (State)
            {
                case OperationExecutionState.notProcessed:
                    result += "Not Processed\n";
                    break;
                case OperationExecutionState.parserError:
                    result += "Parser Error\n";
                    result += OperationException.Message + "\n";
                    break;
                case OperationExecutionState.failed:
                    result += "Failed\n";
                    result += OperationException.Message + "\n";
                    break;
                case OperationExecutionState.performed:
                    result += "Performed\n";
                    result += Result?.ToString() + "\n";
                    break;
            }

            result += "---------------------------------------\n";

            return result;
        }
    }

    public enum TransactionEndType
    {
        Commit,
        Rollback
    }

    public enum NullSpecOpt
    {
        Null,
        NotNull,
        Empty
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Идентификатор содержит имя типа", Justification = "<Ожидание>")]
    public enum DataType
    {
        DATETIME,
        INT,
        DOUBLE,
        CHAR,
        NCHAR,
        VARCHAR,
        NVARCHAR,
        IMAGE,
        TEXT,
        NTEXT
    }

    public class Variable
    {
        public dynamic Data { get; set; }
        public List<string> Name { get; set; }
    }

    public class ExpressionFunction
    {
        public Func<dynamic> CalcFunc { get; set; }
        public Dictionary<List<string>, Variable> Variables { get; set; }

        public ExpressionFunction()
        {
        }

        public ExpressionFunction(Func<dynamic> calcFunc, Dictionary<List<string>, Variable> variables)
        {
            CalcFunc = calcFunc ?? throw new ArgumentNullException(nameof(calcFunc));
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }
    }

    public class Assigment
    {
        public List<string> Id { get; set; }
        public ExpressionFunction EpressionFunction { get; set; }

        public Assigment()
        {
        }

        public Assigment(List<string> id, ExpressionFunction epressionFunction)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            EpressionFunction = epressionFunction ?? throw new ArgumentNullException(nameof(epressionFunction));
        }
    }
}
