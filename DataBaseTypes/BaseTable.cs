using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ProtoBuf;

namespace DataBaseType
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(FieldInt))]
    [ProtoInclude(101, typeof(FieldDouble))]
    [ProtoInclude(102, typeof(FieldChar))]
    public abstract class Field
    {
        public abstract int MaxSize();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldInt : Field
    {
        public int Value { get; set; }

        public override int MaxSize() => sizeof(int);

        public override string ToString() => Value.ToString();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldDouble : Field
    {
        public double Value { get; set; }

        public override int MaxSize() => sizeof(double);

        public override string ToString() => Value.ToString();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldChar : Field
    {
        private readonly byte[] _value;

        public string Value => Encoding.UTF8.GetString(_value, 0, _value.Length);

        public FieldChar(string val, int size)
        {
            _value = System.Text.Encoding.UTF8.GetBytes(val);
            Array.Resize(ref _value, size);
        }

        public override int MaxSize() => _value.Length;

        public override string ToString() => Value.ToString();
    }

    public class Column
    {
        public List<string> Name { get; set; }
        public DataType DataType { get; set; }
        public double? DataParam { get; set; }
        public List<string> Constrains { get; set; }
        public int Size { get; set; }
        public NullSpecOpt TypeState { get; set; }

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

        public OperationResult<Field> CreateField(string data) => null;
    }

    public class TableMetaInf
    {
        public List<string> Name { get; set; }
        public Dictionary<string, Column> ColumnPool { get; set; }
        public int SizeInBytes { get; }

        public TableMetaInf() { }

        public TableMetaInf(List<string> name) => Name = name;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class TableData
    {
        public List<Dictionary<string, Field>> Rows { get; set; }
    }

    public class Table
    {
        public TableData TableData { get; set; }
        public TableMetaInf TableMetaInf { get; set; }

        public Table()
        { }

        public Table(List<string> name) => TableMetaInf = new TableMetaInf(name);

        public Table(TableMetaInf tableMetaInf) => TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));

        public Table(TableData tableData, TableMetaInf tableMetaInf)
        {
            TableData = tableData ?? throw new ArgumentNullException(nameof(tableData));
            TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));
        }

        public OperationResult<Table> AddColumn(Column column)
        {
            TableMetaInf.ColumnPool ??= new Dictionary<string, Column>();
            if (!TableMetaInf.ColumnPool.ContainsKey(column.Name.ToString()))
            {
                TableMetaInf.ColumnPool.Add(column.Name.ToString(), column);
            }
            else
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnAlreadyExistExeption(column.Name.ToString(), TableMetaInf.Name.ToString()));
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
                return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnNotExistExeption(ColumName, TableMetaInf.Name.ToString()));
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

            foreach (var row in TableData.Rows)
            {
                foreach (var field in row)
                {
                    sw.Write("{0} ", field.Value.ToString());
                }
                sw.Write("\n");
            }

            return new OperationResult<string>(OperationExecutionState.performed, sw.ToString());
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
