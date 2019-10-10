using System;
using System.Collections.Generic;
using System.IO;

using DataBaseEngine;

using DataBaseErrors;
using ProtoBuf;

namespace DataBaseTable
{
    public enum ColumnDataType
    {
        DATETIME,
        INT,
        DOUBLE,
        CHAR,
        NCHAR,
        VARCHAR,
        NVARCHAR,
        TEXT,
        NTEXT
    }

    public enum NullSpecOpt
    {
        Null,
        NotNull,
        Empty
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(FieldInt))]
    [ProtoInclude(101, typeof(FieldDouble))]
    [ProtoInclude(102, typeof(FieldChar))]
    [ProtoInclude(103, typeof(FieldVarChar))]
    [ProtoInclude(104, typeof(FieldDate))]
    public abstract class Field
    {

    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldInt : Field
    {
        public int Value { get; set; }
        public override string ToString()
        {
            return "" + Value;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldDouble : Field
    {
        public double Value { get; set; }
        public override string ToString()
        {
            return "" + Value;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldChar : Field
    {
        public string Value { get; set; }
        public override string ToString()
        {
            return "" + Value;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldVarChar : Field
    {
        public string Value { get; set; }
        public override string ToString()
        {
            return "" + Value;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class FieldDate : Field
    {
        public int Value { get; set; }
        public override string ToString()
        {
            return "" + Value;
        }
    }

    public class Column
    {
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
                    catch (FormatException e)
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
                    }
                case ColumnDataType.DOUBLE:
                    try
                    {
                        var val = Convert.ToDouble(data);
                        return new OperationResult<Field>(OperationExecutionState.performed, new FieldDouble { Value = val });
                    }
                    catch (FormatException e)
                    {
                        return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
                    }
                case ColumnDataType.CHAR:
                    return new OperationResult<Field>(OperationExecutionState.performed, new FieldChar { Value = data });
                case ColumnDataType.NCHAR:
                case ColumnDataType.NTEXT:
                case ColumnDataType.TEXT:
                case ColumnDataType.VARCHAR:
                    return new OperationResult<Field>(OperationExecutionState.performed, new FieldVarChar { Value = data });

            }
            return new OperationResult<Field>(OperationExecutionState.failed, null, new CastFieldExeption(Name, DataType.ToString(), data));
        }
        public string Name { get; set; }
        public ColumnDataType DataType { get; set; }
        public int DataParam { get; set; }
        public List<string> Constrains { get; set; }
        public int Size { get; set; }
        public NullSpecOpt TypeState { get; set; }
    }

    public class TableMetaInf
    {
        public TableMetaInf() { }
        public TableMetaInf(string name) => Name = name;

        public string Name { get; set; }
        public Dictionary<string, Column> ColumnPool { get; set; }
        public int SizeInBytes { get; }
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
        {
        }

        public Table(string name) => TableMetaInf = new TableMetaInf(name);

        public Table(TableMetaInf tableMetaInf) => TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));

        public Table(TableData tableData, TableMetaInf tableMetaInf)
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
        public override string ToString()
        {
            return TableData == null ? ShowCreateTable().Result : ShowDataTable().Result;
        }
        public OperationResult<string> ShowDataTable()
        {
            using (var sw = new StringWriter())
            {
                var table = this;
               
                foreach (var key in table.TableMetaInf.ColumnPool)
                {
                    var column = key.Value;
                    sw.Write("{0} ", column.Name);
                }
                sw.Write("/n");
                foreach (var row in table.TableData.Rows)
                {
                    foreach (var field in row)
                    {
                        sw.Write("{0} ", field.ToString());
                    }
                    sw.Write("/n ");
                }
                var str = sw.ToString();
                return new OperationResult<string>(OperationExecutionState.performed, str);
            }
        }
        public OperationResult<string> ShowCreateTable()
        {
            using (var sw = new StringWriter())
            {
                var table = this;
                sw.Write("CREATE TABLE {0} (", table.TableMetaInf.Name);
                foreach (var key in table.TableMetaInf.ColumnPool)
                {
                    var column = key.Value;
                    sw.Write("{0} {1} ({2})", column.Name, column.DataType.ToString(), column.DataParam);
                    foreach (var key2 in column.Constrains)
                    {
                        sw.Write(" {0}", key2);
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
}
