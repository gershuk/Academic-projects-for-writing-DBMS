using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

using ZeroFormatter;

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
        Right,
        Full
    }

#nullable enable
    public class VariableBorder : ICloneable
    {
        private dynamic? _leftBorder;
        private dynamic? _rightBorder;

        public bool StrictLeft { get; set; }
        public bool StrictRight { get; set; }

        public dynamic? LeftBorder
        {
            get => _leftBorder;
            set => _leftBorder = _leftBorder < value || _leftBorder == null ? value : _leftBorder;
        }

        public dynamic? RightBorder
        {
            get => _rightBorder;
            set => _rightBorder = _rightBorder > value || _rightBorder == null ? value : _rightBorder;
        }

        public VariableBorder (dynamic? leftBorder, dynamic? rightBorder, bool strictLeft, bool strictRight)
        {
            LeftBorder = leftBorder;
            RightBorder = rightBorder;
            StrictLeft = strictLeft;
            StrictRight = strictRight;
        }

        public VariableBorder ()
        {
            StrictLeft = true;
            StrictRight = true;
            LeftBorder = null;
            RightBorder = null;
        }

        public object Clone () => new VariableBorder(LeftBorder, RightBorder, StrictLeft, StrictRight);
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

    public delegate bool TimeSelectorDelegate (DateTime startTime, DateTime endTime);

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

    [ZeroFormattable]
    [ProtoContract]
    public class Id
    {
        [ProtoMember(1)]
        [Index(1)]
        public virtual List<string> SimpleIds { get; set; }

        public Id (List<string> simpleIds) => SimpleIds = simpleIds ?? throw new ArgumentNullException(nameof(simpleIds));

        public Id ()
        {

        }

        public override string ToString ()
        {
            var stringBuilder = new StringBuilder();

            foreach (var simpleId in SimpleIds)
            {
                stringBuilder.Append(simpleId + ".");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }
    }

    public class ExpressionFunction
    {
        public Func<Dictionary<string, dynamic>, dynamic> CalcFunc { get; private set; }
        public List<string> VariablesNames { get; set; }

        public ExpressionFunction (Func<Dictionary<string, dynamic>, dynamic> calcFunc, List<string> variablesNames)
        {
            CalcFunc = calcFunc ?? throw new ArgumentNullException(nameof(calcFunc));
            VariablesNames = variablesNames ?? throw new ArgumentNullException(nameof(variablesNames));
        }
    }

    public class Assigment
    {
        public Id Id { get; set; }
        public ExpressionFunction EpressionFunction { get; set; }

        public Assigment ()
        {
        }

        public Assigment (Id id, ExpressionFunction epressionFunction)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            EpressionFunction = epressionFunction ?? throw new ArgumentNullException(nameof(epressionFunction));
        }
    }
}
