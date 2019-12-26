using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using ProtoBuf;

using ZeroFormatter;

namespace DataBaseType
{
    [ProtoContract]
    [Union(typeof(FieldInt), typeof(FieldDouble), typeof(FieldChar))]
    public abstract class Field
    {
        [ProtoMember(1)]
        [UnionKey]
        public abstract DataType Type { get; }

        public static OperationResult<bool> Compare (Field a, Field b)
        {
            if (a.Type != b.Type)
            {
                return new OperationResult<bool>(ExecutionState.failed, false);
            }
            switch (a.Type)
            {
                case DataType.INT:
                    return new OperationResult<bool>(ExecutionState.performed, ((FieldInt)a).Value == ((FieldInt)b).Value);
                case DataType.DOUBLE:
                    return new OperationResult<bool>(ExecutionState.performed, ((FieldDouble)a).Value == ((FieldDouble)b).Value);
                case DataType.CHAR:
                    return new OperationResult<bool>(ExecutionState.performed, ((FieldChar)a).Value == ((FieldChar)b).Value);
                default:
                    break;
            }
            return new OperationResult<bool>(ExecutionState.failed, false);
        }
    }

    [ProtoContract]
    [ProtoInclude(10, typeof(FieldInt))]
    [ProtoInclude(11, typeof(FieldDouble))]
    [ProtoInclude(12, typeof(FieldChar))]
    [ZeroFormattable]
    public class FieldInt : Field
    {
        [ProtoMember(1)]
        public override DataType Type => DataType.INT;

        [ProtoMember(2)]
        [Index(0)]
        public virtual int Value { get; set; }

        public FieldInt ()
        {
        }
        public FieldInt (int val) => Value = val;

        public override string ToString () => Value.ToString();
    }

    [ProtoContract]
    [ZeroFormattable]
    public class FieldDouble : Field
    {
        [ProtoMember(1)]
        public override DataType Type => DataType.DOUBLE;

        [ProtoMember(2)]
        [Index(0)]
        public virtual double Value { get; set; }

        public FieldDouble ()
        {

        }
        public FieldDouble (double val) => Value = val;

        public override string ToString () => Value.ToString();
    }

    [ProtoContract]
    [ZeroFormattable]
    public class FieldChar : Field
    {
        [ProtoMember(1)]
        public override DataType Type => DataType.CHAR;

        [ProtoMember(2)]
        [Index(0)]
        public virtual byte[] ValueBytes { get; set; }

        [ProtoMember(3)]
        [IgnoreFormat]
        public string Value => Encoding.UTF8.GetString(ValueBytes, 0, ValueBytes.Length);

        public FieldChar ()
        {
        }

        public FieldChar (string val, int size)
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

        public override string ToString () => Value.ToString();
    }

    [ProtoContract]
    [ZeroFormattable]
    public class Row
    {
        [ProtoMember(1)]
        [Index(0)]
        public virtual Field[] Fields { get; set; }

        [ProtoMember(2)]
        [Index(1)]
        public virtual long TrStart { get; set; }

        [ProtoMember(3)]
        [Index(2)]
        public virtual long TrEnd { get; set; }
        [IgnoreFormat]
        public long FilePtrBlock { get; set; } = -1;
        [IgnoreFormat]
        public long InBlockPos { get; set; } = -1;
        public Row ()
        {

        }
        public Row (Field[] fields)
        {
            Fields = fields;
            TrStart = -1;
            TrEnd = long.MaxValue;
        }
        public Row SetTrEnd (long trEnd)
        {
            TrEnd = trEnd;
            return this;
        }
    }

    [ProtoContract]
    [ZeroFormattable]
    public class Column
    {
        [ProtoMember(1)]
        [Index(0)]
        public virtual Id Name { get; set; }

        [ProtoMember(2)]
        [Index(1)]
        public virtual DataType DataType { get; set; }

        [ProtoMember(3)]
        [Index(2)]
        public virtual double? DataParam { get; set; }

        [ProtoMember(4)]
        [Index(3)]
        public virtual List<string> Constrains { get; set; }

        [ProtoMember(5)]
        [Index(4)]
        public virtual int Size { get; set; }

        [ProtoMember(6)]
        [Index(5)]
        public virtual NullSpecOpt TypeState { get; set; }

        public Column () { }

        public Column (Id name) => Name = name;
        public Column (Column col)
        {
            if (col == null)
            {
                throw new NullReferenceException();
            }
            Name = col.Name;
            DataType = col.DataType;
            DataParam = col.DataParam;
            Constrains = col.Constrains;
            TypeState = col.TypeState;
        }
        public Column (Id name, DataType dataType, double? dataParam, List<string> constrains, NullSpecOpt typeState)
        {
            Name = name;
            DataType = dataType;
            DataParam = dataParam;
            Constrains = constrains;
            TypeState = typeState;
        }

        public OperationResult<Field> CreateField (dynamic data)
        {

            switch (DataType)
            {
                case DataType.INT:
                    try
                    {
                        var val = Convert.ToInt32(data);
                        return new OperationResult<Field>(ExecutionState.performed, new FieldInt { Value = val });
                    }
                    catch (FormatException)
                    {
                        return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
                    }
                case DataType.DOUBLE:
                    try
                    {
                        var val = Convert.ToDouble(data, new NumberFormatInfo { NumberDecimalSeparator = "." });
                        return new OperationResult<Field>(ExecutionState.performed, new FieldDouble { Value = val });
                    }
                    catch (FormatException)
                    {
                        return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
                    }
                case DataType.CHAR:
                    return new OperationResult<Field>(ExecutionState.performed, new FieldChar(data, (int)DataParam));

            }

            return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
        }
        public OperationResult<Field> CreateField (string data)
        {
            switch (DataType)
            {
                case DataType.INT:
                    try
                    {
                        var val = Convert.ToInt32(data);
                        return new OperationResult<Field>(ExecutionState.performed, new FieldInt { Value = val });
                    }
                    catch (FormatException)
                    {
                        return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
                    }
                case DataType.DOUBLE:
                    try
                    {
                        var val = Convert.ToDouble(data, new NumberFormatInfo { NumberDecimalSeparator = "." });
                        return new OperationResult<Field>(ExecutionState.performed, new FieldDouble { Value = val });
                    }
                    catch (FormatException)
                    {
                        return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
                    }
                case DataType.CHAR:
                    return new OperationResult<Field>(ExecutionState.performed, new FieldChar(data, (int)DataParam));

            }

            return new OperationResult<Field>(ExecutionState.failed, null, new CastFieldError(Name.ToString(), DataType.ToString(), data));
        }
    }

    [ProtoContract]
    [ZeroFormattable]
    public class TableMetaInf
    {
        [ProtoMember(1)]
        [Index(0)]
        public virtual Id Name { get; set; }

        [ProtoMember(2)]
        [Index(1)]
        public virtual List<Column> ColumnPool { get; set; }

        [ProtoMember(3)]
        [Index(2)]
        public virtual int SizeInBytes { get; set; }

        [ProtoMember(4)]
        [Index(3)]
        public virtual long CreatedTrId { get; set; }
        [ProtoMember(5)]
        [Index(4)]
        public virtual long DeletedTrId { get; set; }
        [ProtoMember(6)]
        [Index(5)]
        public virtual bool Versioning { get; set; } = false;

        public TableMetaInf () { }

        public string GetFullName () => Name.ToString();

        public TableMetaInf (Id name) => Name = name;
    }

    [ProtoContract]
    public class Table
    {
        [ProtoMember(1)]
        public IEnumerable<Row> TableData { get; set; }

        [ProtoMember(2)]
        public TableMetaInf TableMetaInf { get; set; }

        public Table ()
        { }

        public Table (Id name) => TableMetaInf = new TableMetaInf(name);

        public Table (TableMetaInf tableMetaInf) => TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));

        public Table (IEnumerable<Row> tableData, TableMetaInf tableMetaInf)
        {
            TableData = tableData ?? throw new ArgumentNullException(nameof(tableData));
            TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));
        }
        private static string GetFullName (List<string> Name)
        {
            _ = Name ?? throw new ArgumentNullException(nameof(Name));
            var sb = new StringBuilder();
            foreach (var n in Name)
            {
                sb.Append(n);
            }
            return sb.ToString();
        }
        public OperationResult<Table> AddColumn (Column column)
        {
            TableMetaInf.ColumnPool ??= new List<Column>();
            if (column == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(column)));
            }
            if (TableMetaInf.ColumnPool.FindIndex((Column c) => c.Name.ToString() == column.Name.ToString()) < 0)
            {
                TableMetaInf.ColumnPool.Add(column);
            }
            else
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ColumnAlreadyExistError(column.Name.ToString(), TableMetaInf.Name.ToString()));
            }

            return new OperationResult<Table>(ExecutionState.performed, this);
        }


        public override string ToString () => TableData == null ?
            ShowCreateTable().Result : ShowDataTable().Result;

        public OperationResult<string> ShowDataTable ()//To do Сделать красивый вывод таблички
        {
            using var sw = new StringWriter();

            sw.Write("\n");

            foreach (var key in TableMetaInf.ColumnPool)
            {
                var column = key;
                sw.Write("{0,-15} ", column.Name);
            }
            sw.Write("{0,-15} ", "TrStart");
            sw.Write("{0,-15} ", "TrEnd");
            sw.Write("\n");

            foreach (var row in TableData)
            {
                foreach (var field in row.Fields)
                {
                    sw.Write("{0,-15} ", field == null ? "NULL" : field.ToString().Trim(' '));
                }
                sw.Write("{0,-15} ", row.TrStart);
                sw.Write("{0,-15} ", row.TrEnd == long.MaxValue ? "inf" : "" + row.TrEnd);
                sw.Write("\n");
            }

            return new OperationResult<string>(ExecutionState.performed, sw.ToString());
        }



        public OperationResult<Row> CreateRowFormStr (string[] strs)
        {
            if (strs is null)
            {
                throw new ArgumentNullException(nameof(strs));
            }

            var row = new Field[TableMetaInf.ColumnPool.Count];
            var i = 0;

            foreach (var col in TableMetaInf.ColumnPool)
            {
                var result = col.CreateField(strs[i]);
                if (result.State != ExecutionState.performed)
                {
                    return new OperationResult<Row>(ExecutionState.failed, null, result.OperationError);
                }
                row[i] = result.Result;
                i++;
            }

            return new OperationResult<Row>(ExecutionState.performed, new Row(row));
        }
        public OperationResult<Row> CreateDefaultRow ()
        {
            var row = new Field[TableMetaInf.ColumnPool.Count];
            var i = 0;

            foreach (var col in TableMetaInf.ColumnPool)
            {
                row[i] = col.CreateField("0").Result;
                i++;
            }
            return new OperationResult<Row>(ExecutionState.performed, new Row(row));
        }

        public OperationResult<string> ShowCreateTable ()
        {
            using var sw = new StringWriter();

            sw.Write("CREATE TABLE {0} (", TableMetaInf.Name);

            foreach (var key in TableMetaInf.ColumnPool)
            {
                var column = key;
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

            return new OperationResult<string>(ExecutionState.performed, str);
        }
    }
}
