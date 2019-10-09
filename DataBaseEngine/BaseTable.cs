using System;
using System.Collections.Generic;
using System.IO;

using DataBaseEngine;

using DataBaseErrors;

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
    public abstract class Field
    {

    }
    public class FieldInt : Field
    {
        public int Value { get; set; }
    }
    public class FieldDouble : Field
    {
        public double Value { get; set; }
    }
    public class FieldChar : Field
    {
        public string Value { get; set; }
    }
    public class FieldVarChar : Field
    { 
        public string Value { get; set; }
    }
    public class FieldDate : Field
    {
        public int Value { get; set; }
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

        public OperationResult<string> AddColumn(Column column)
        {
            ColumnPool = ColumnPool ?? new Dictionary<string, Column>();
            if (!ColumnPool.ContainsKey(column.Name))
            {
                ColumnPool.Add(column.Name, column);
            }
            else
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error AddColumn, Column with name {0} alredy exist in Table {1}", column.Name, Name);
                    return new OperationResult<string>(OperationExecutionState.failed, null, new ColumnAlreadyExistExeption(column.Name, Name));
                }
            }
            return new OperationResult<string>(OperationExecutionState.performed, "");
        }

        public OperationResult<string> DeleteColumn(string ColumName)
        {
            ColumnPool = ColumnPool ?? new Dictionary<string, Column>();
            if (ColumnPool.ContainsKey(ColumName))
            {
                ColumnPool.Remove(ColumName);
            }
            else
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error DeleteColumn, Column with name {0} not exist in Table {1}", ColumName, Name);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public string Name { get; set; }
        public Dictionary<string, Column> ColumnPool { get; set; }
        public int SizeInBytes { get; }
    }

    public class TableData
    {

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

        public OperationResult<string> DeleteColumn(string ColumName) => TableMetaInf.DeleteColumn(ColumName);

        public OperationResult<string> AddColumn(Column column) => TableMetaInf.AddColumn(column);
    }
}
