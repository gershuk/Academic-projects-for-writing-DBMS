using System;
using System.Collections.Generic;
using System.Text;
using DataBaseEngine;
using Newtonsoft.Json;
using System.IO;
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
        IMAGE,
        TEXT,
        NTEXT
    }

    public class Column
    {
        public Column() { }
        public Column(string name) => Name = name;

        public Column(string name, ColumnDataType dataType, int dataParam, List<string> constrains)
        {
            Name = name;
            DataType = dataType;
            DataParam = dataParam;
            Constrains = constrains;
        }
        public string Name { get; set; }
        public ColumnDataType DataType { get; set; }
        public int DataParam { get; set;}
        public List<string> Constrains { get; set; }
        public int Size { get; set; }
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
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            return new OperationResult<string>(OperationExecutionState.performed,"ok");
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

        public OperationExecutionState LoadTableData(string data) => throw new NotImplementedException();
        public OperationExecutionState SerializeTableData() => throw new NotImplementedException();

        public OperationExecutionState LoadTableMetaInf(string data)
        {
            TableMetaInf = JsonConvert.DeserializeObject<TableMetaInf>(data);
            return OperationExecutionState.performed;
        }

        public OperationResult<string> SerializeTableMetaInf() => new OperationResult<string>(OperationExecutionState.performed, JsonConvert.SerializeObject(TableMetaInf));

    }
}
