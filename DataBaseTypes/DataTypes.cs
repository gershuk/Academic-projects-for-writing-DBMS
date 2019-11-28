using System;
using System.Collections.Generic;

namespace DataBaseType
{
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
