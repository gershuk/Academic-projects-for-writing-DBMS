using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using DataBaseEngine;

using DataBaseErrors;
using ZeroFormatter;

namespace DataBaseTable
{
    public enum ColumnDataType
    {
        INT,
        DOUBLE,
        CHAR,
    }

    public enum NullSpecOpt
    {
        Null,
        NotNull,
        Empty
    }
    
    [Union(typeof(FieldInt), typeof(FieldDouble),typeof(FieldChar))]
    public abstract class Field
    {
        [UnionKey]
        public abstract ColumnDataType Type { get; }
        public Field() { }
    }

    [ZeroFormattable]
    public class FieldInt : Field
    {
        public override ColumnDataType Type
        {
            get
            {
                return ColumnDataType.INT;
            }
        }

        [Index(0)]
        public virtual int Value { get; set; }

        public FieldInt()
        {
        }

        public override string ToString() => Value.ToString();
    }

    [ZeroFormattable]
    public class FieldDouble : Field
    {
        public override ColumnDataType Type
        {
            get
            {
                return ColumnDataType.DOUBLE;
            }
        }
        [Index(0)]
        public virtual double Value { get; set; }
        public FieldDouble()
        {

        }

        public override string ToString() => Value.ToString();
    }

    [ZeroFormattable]
    public class FieldChar : Field
    {
        public override ColumnDataType Type
        {
            get
            {
                return ColumnDataType.CHAR;
            }
        }
        [Index(0)]
        public virtual byte[] ValueBytes { get; set; }
        [IgnoreFormat]
        public string Value => Encoding.UTF8.GetString(ValueBytes, 0, ValueBytes.Length);

        public FieldChar()
        {
        }
        public FieldChar(string val, int size)
        {
            ValueBytes = new byte[size];
            var buf = System.Text.Encoding.UTF8.GetBytes(val);
            for (var i = 0; i < buf.Length && i < size; i++)
            {
                ValueBytes[i] = buf[i];
            }
            for (var i = buf.Length; i < size; i++)
            {
                ValueBytes[i] = System.Text.Encoding.ASCII.GetBytes(" ")[0];
            }
        }
        public override string ToString() => Value.ToString();
    }
    [ZeroFormattable]
    public class Column
    {
        [Index(0)] public virtual string Name { get; set; }
        [Index(1)] public virtual ColumnDataType DataType { get; set; }
        [Index(2)] public virtual int DataParam { get; set; }
        [Index(3)] public virtual List<string> Constrains { get; set; }
        [Index(4)] public virtual int Size { get; set; }
        [Index(5)] public virtual NullSpecOpt TypeState { get; set; }

        public Column() { }

        public Column(string name) => Name = name;

        public Column(string name, ColumnDataType dataType, int dataParam, List<string> constrains, NullSpecOpt typeState)
        {
            Name = name;
            DataType = dataType;
            DataParam = dataParam;
            Constrains = constrains;
            TypeState = typeState;
        }

        public OperationResult<Field> CreateField(string data)
        {
            switch (DataType)
            {
                case ColumnDataType.INT:
                    try
                    {
                        var val = Convert.ToInt32(data);
                        return new OperationResult<Field>(OperationExecutionState.performed, new FieldInt { Value = val });
                    }
                    catch
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
                    }
                case ColumnDataType.DOUBLE:
                    try
                    {
                        var val = Convert.ToDouble(data, new NumberFormatInfo { NumberDecimalSeparator = "." });
                        return new OperationResult<Field>(OperationExecutionState.performed, new FieldDouble { Value = val });
                    }
                    catch
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
                    }
                case ColumnDataType.CHAR:
                    return new OperationResult<Field>(OperationExecutionState.performed, new FieldChar(data, DataParam));

            }
            return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
        }
    }
    [ZeroFormattable]
    public class TableMetaInf
    {
        [Index(0)] public virtual string Name { get; set; }
        [Index(1)] public virtual Dictionary<string, Column> ColumnPool { get; set; }
        [Index(2)] public virtual int SizeInBytes { get; protected set; }

        public TableMetaInf() { }

        public TableMetaInf(string name) => Name = name;
    }

    public class Table
    {
        public IEnumerable<Field[]> TableData { get; set; }
        public TableMetaInf TableMetaInf { get; set; }

        public Table()
        { }

        public Table(string name) => TableMetaInf = new TableMetaInf(name);

        public Table(TableMetaInf tableMetaInf) => TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));

        public Table(IEnumerable<Field[]> tableData, TableMetaInf tableMetaInf)
        {
            TableData = tableData ?? throw new ArgumentNullException(nameof(tableData));
            TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));
        }

        public OperationResult<Table> AddColumn(Column column)
        {
            TableMetaInf.ColumnPool ??= new Dictionary<string, Column>();
            if (!TableMetaInf.ColumnPool.ContainsKey(column.Name))
            {
                TableMetaInf.ColumnPool.Add(column.Name, column);
            }
            else
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnAlreadyExistExeption(column.Name, TableMetaInf.Name));
            }

            return new OperationResult<Table>(OperationExecutionState.performed, this);
        }

        public OperationResult<Table> DeleteColumn(string ColumName)
        {

            TableMetaInf.ColumnPool ??= new Dictionary<string, Column>();
            if (TableMetaInf.ColumnPool.ContainsKey(ColumName))
            {
                TableMetaInf.ColumnPool.Remove(ColumName);
            }
            else
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnNotExistExeption(ColumName, TableMetaInf.Name));
            }
            return new OperationResult<Table>(OperationExecutionState.performed, this);
        }

        public override string ToString() => TableData == null ? ShowCreateTable().Result : ShowDataTable().Result;

        public OperationResult<string> ShowDataTable()
        {
            using var sw = new StringWriter();

            sw.Write("\n");

            foreach (var key in TableMetaInf.ColumnPool)
            {
                var column = key.Value;
                sw.Write("{0} ", column.Name);
            }

            sw.Write("\n");

            foreach (var row in TableData)
            {
                foreach (var field in row)
                {
                    sw.Write("{0} ", field.ToString());
                }
                sw.Write("\n");
            }

            return new OperationResult<string>(OperationExecutionState.performed, sw.ToString());
        }
        public OperationResult<Field[]> CreateRowFormStr(string[] strs)
        {
            var row = new Field[TableMetaInf.ColumnPool.Count];
            int i = 0;
            foreach (var col in TableMetaInf.ColumnPool)
            {
                var result = col.Value.CreateField(strs[i]);
                if (result.State != OperationExecutionState.performed)
                {
                    return new OperationResult<Field[]>(OperationExecutionState.failed, null,result.OperationException);
                }
                row[i] = result.Result;
                i++;
            }
            return new OperationResult<Field[]>(OperationExecutionState.performed, row);
        }
        public OperationResult<Field[]> CreateDefaultRow()
        {
            var row = new Field[TableMetaInf.ColumnPool.Count];
            int i = 0;
            foreach (var col in TableMetaInf.ColumnPool)
            {
                row[i] = col.Value.CreateField("0").Result;
                i++;
            }
            return new OperationResult<Field[]>(OperationExecutionState.performed, row);
        }
        public OperationResult<string> ShowCreateTable()
        {
            using var sw = new StringWriter();

            sw.Write("CREATE TABLE {0} (", TableMetaInf.Name);

            foreach (var key in TableMetaInf.ColumnPool)
            {
                var column = key.Value;
                sw.Write($"{column.Name} {column.DataType} ({column.DataParam})");

                foreach (var key2 in column.Constrains)
                {
                    sw.Write($" {key2}");
                }

                sw.Write(",");
            }

            var str = sw.ToString();
            str = str.TrimEnd(new char[] { ',' });
            str += ");";

            return new OperationResult<string>(OperationExecutionState.performed, str);
        }
    }
}
