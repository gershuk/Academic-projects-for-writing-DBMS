using System;
using System.Collections.Generic;

using ProtoBuf;

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


    public enum ExecutionState
    {
        notProcessed,
        parserError,
        failed,
        performed
    }

    public interface IOperationResult<T>
    {
        DBError OperationError { get; set; }
        T Result { get; set; }
        ExecutionState State { get; set; }
    }

    [ProtoContract]
    public class OperationResult<T> : IOperationResult<T>
    {
        [ProtoMember(1)]
        public ExecutionState State { get; set; }

        [ProtoMember(2)]
        public DBError OperationError { get; set; }

        [ProtoMember(3)]
        public T Result { get; set; }

        public OperationResult ()
        {
            State = ExecutionState.notProcessed;
            OperationError = default;
            Result = default;
        }

        public OperationResult (ExecutionState state, T result, DBError opException = null)
        {
            State = state;
            Result = result;
            OperationError = opException;
        }

        public override string ToString ()
        {
            var result = "---------------------------------------\n";

            switch (State)
            {
                case ExecutionState.notProcessed:
                    result += "Not Processed\n";
                    break;
                case ExecutionState.parserError:
                    result += "Parser Error\n";
                    result += OperationError.Message + "\n";
                    break;
                case ExecutionState.failed:
                    result += "Failed\n";
                    result += OperationError.Message + "\n";
                    break;
                case ExecutionState.performed:
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

        public ExpressionFunction ()
        {
        }

        public ExpressionFunction (Func<dynamic> calcFunc, Dictionary<List<string>, Variable> variables)
        {
            CalcFunc = calcFunc ?? throw new ArgumentNullException(nameof(calcFunc));
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }
    }

    public class Assigment
    {
        public List<string> Id { get; set; }
        public ExpressionFunction EpressionFunction { get; set; }

        public Assigment ()
        {
        }

        public Assigment (List<string> id, ExpressionFunction epressionFunction)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            EpressionFunction = epressionFunction ?? throw new ArgumentNullException(nameof(epressionFunction));
        }
    }
}
