using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ZeroFormatter;

namespace DataBaseType
{
    [Union(typeof(FieldInt), typeof(FieldDouble), typeof(FieldChar))]
    public abstract class Field
    {
        [UnionKey]
        public abstract DataType Type { get; }
    }

    [ZeroFormattable]
    public class FieldInt : Field
    {
        public override DataType Type => DataType.INT;

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
        public override DataType Type => DataType.DOUBLE;

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
        public override DataType Type => DataType.CHAR;

        [Index(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Свойства не должны возвращать массивы", Justification = "<Ожидание>")]
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
        [Index(0)]
        public virtual List<string> Name { get; set; }

        [Index(1)]
        public virtual DataType DataType { get; set; }

        [Index(2)]
        public virtual double? DataParam { get; set; }

        [Index(3)]
        public virtual List<string> Constrains { get; set; }

        [Index(4)]
        public virtual int Size { get; set; }

        [Index(5)]
        public virtual NullSpecOpt TypeState { get; set; }

        public Column() { }

        public Column(List<string> name) => Name = name;

        public Column(List<string> name, DataType dataType, double? dataParam, List<string> constrains, NullSpecOpt typeState)
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
                case DataType.INT:
                    try
                    {
                        var val = Convert.ToInt32(data);
                        return new OperationResult<Field>(OperationExecutionState.performed, new FieldInt { Value = val });
                    }
                    catch
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldException(Name, DataType.ToString(), data));
                    }
                case DataType.DOUBLE:
                    try
                    {
                        var val = Convert.ToDouble(data, new NumberFormatInfo { NumberDecimalSeparator = "." });
                        return new OperationResult<Field>(OperationExecutionState.performed, new FieldDouble { Value = val });
                    }
                    catch
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldException(Name, DataType.ToString(), data));
                    }
                case DataType.CHAR:
                    return new OperationResult<Field>(OperationExecutionState.performed, new FieldChar(data, (int)DataParam));

            }

            return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldException(Name, DataType.ToString(), data));
        }
    }

    [Serializable]
    [ZeroFormattable]
    public class TableMetaInf
    {
        [Index(0)]
        public virtual List<string> Name { get; set; }

        [Index(1)]
        public virtual Dictionary<string, Column> ColumnPool { get; set; }

        [Index(2)]
        public virtual int SizeInBytes { get; set; }

        public TableMetaInf() { }

        public string GetFullName()
        {
            var sb = new StringBuilder();
            foreach (var n in Name)
            {
                sb.Append(n);
            }
            return sb.ToString();
        }
        public TableMetaInf(List<string> name) => Name = name;
    }

    [Serializable]
    public class Table
    {
        public IEnumerable<Field[]> TableData { get; set; }

        public TableMetaInf TableMetaInf { get; set; }

        public Table()
        { }

        public Table(List<string> name) => TableMetaInf = new TableMetaInf(name);

        public Table(TableMetaInf tableMetaInf) => TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));

        public Table(IEnumerable<Field[]> tableData, TableMetaInf tableMetaInf)
        {
            TableData = tableData ?? throw new ArgumentNullException(nameof(tableData));
            TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));
        }

        public OperationResult<Table> AddColumn(Column column)
        {
            TableMetaInf.ColumnPool ??= new Dictionary<string, Column>();
            if (!TableMetaInf.ColumnPool.ContainsKey(column?.Name.ToString()))
            {
                TableMetaInf.ColumnPool.Add(column.Name.ToString(), column);
            }
            else
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnAlreadyExistException(column.Name.ToString(), TableMetaInf.Name.ToString()));
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
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnNotExistException(ColumName, TableMetaInf.Name.ToString()));
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
            if (strs is null)
            {
                throw new ArgumentNullException(nameof(strs));
            }

            var row = new Field[TableMetaInf.ColumnPool.Count];
            var i = 0;

            foreach (var col in TableMetaInf.ColumnPool)
            {
                var result = col.Value.CreateField(strs[i]);
                if (result.State != OperationExecutionState.performed)
                {
                    return new OperationResult<Field[]>(OperationExecutionState.failed, null, result.OperationException);
                }
                row[i] = result.Result;
                i++;
            }

            return new OperationResult<Field[]>(OperationExecutionState.performed, row);
        }
        public OperationResult<Field[]> CreateDefaultRow()
        {
            var row = new Field[TableMetaInf.ColumnPool.Count];
            var i = 0;

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
