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
            set => _leftBorder = (value != null && (_leftBorder == null || _leftBorder < value)) ? value : _leftBorder;
        }

        public dynamic? RightBorder
        {
            get => _rightBorder;
            set => _rightBorder = (value != null && (_rightBorder == null || _rightBorder > value)) ? value : _rightBorder;
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

    public struct Varchar : IComparable<Varchar>, IEquatable<Varchar>
    {
        private string _charArray;
        private int? _hash;

        public string CharArray
        {
            get => _charArray;
            set
            {
                _hash = null;
                _charArray = value;
            }
        }

        public Varchar (string charArray)
        {
            _hash = 0;
            _charArray = charArray;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Укажите StringComparison", Justification = "<Ожидание>")]
        public int CompareTo (Varchar other) => CharArray.CompareTo(other.CharArray);

        public static bool operator > (Varchar operand1, Varchar operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator < (Varchar operand1, Varchar operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >= (Varchar operand1, Varchar operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <= (Varchar operand1, Varchar operand2) => operand1.CompareTo(operand2) <= 0;

        public override bool Equals (object obj) => obj is Varchar other ? other.CharArray == CharArray : false;

        public override int GetHashCode ()
        {
            const int p = 317;
            var pPow = 1;

            if (_hash == null)
            {
                for (var i = 0; i < _charArray.Length; i++)
                {
                    _hash = pPow * Convert.ToInt32(_charArray[i]);
                    pPow *= p;
                }
            }

            return _hash.GetValueOrDefault();
        }

        public static bool operator == (Varchar left, Varchar right) => left.Equals(right);

        public static bool operator != (Varchar left, Varchar right) => !(left == right);

        public bool Equals (Varchar other) => other._charArray == _charArray;
        public override string ToString () => (string)_charArray.Clone();

        public static explicit operator string (Varchar param) => (string)param._charArray.Clone();

        public static explicit operator Varchar (string param)
        {
            if (param == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return new Varchar((string)param.Clone());
            }
        }
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
        public Dictionary<string, VariableBorder> VariablesBorder { get; protected set; }

        public ExpressionFunction (Func<Dictionary<string, dynamic>, dynamic> calcFunc, List<string> variablesNames)
        {
            CalcFunc = calcFunc ?? throw new ArgumentNullException(nameof(calcFunc));
            VariablesNames = variablesNames ?? throw new ArgumentNullException(nameof(variablesNames));
        }

        public ExpressionFunction (Func<Dictionary<string, dynamic>, dynamic> calcFunc, List<string> variablesNames, Dictionary<string, VariableBorder> variablesBorder) : this(calcFunc, variablesNames) => VariablesBorder = variablesBorder ?? throw new ArgumentNullException(nameof(variablesBorder));
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
