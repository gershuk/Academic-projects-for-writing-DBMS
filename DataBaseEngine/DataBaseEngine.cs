using System;
using System.Collections.Generic;
using DataBaseTable;
using Newtonsoft.Json;
using System.IO;

namespace DataBaseEngine
{
   
    public enum OperationExecutionState
    {
        failed,
        performed
    }


    public struct OperationResult<T>
    {
        public OperationExecutionState State { get; set; }
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result)
        {
            State = state;
            Result = result;
        }
    }

    public interface IDataBaseEngineFunction
    {
        OperationExecutionState CreateTable(string name);
        OperationExecutionState CreateTable(TableMetaInf metaInf);

        OperationExecutionState DeleteColumnFromTable(string tableName, string ColumnName);

        OperationExecutionState AddColumnToTable(string tableName, Column column);

        OperationResult<TableData> GetTableData(string name);
        OperationResult<TableMetaInf> GetTableMetaInf(string name);

        OperationExecutionState DeleteTable(string name);
    }

    public interface IDataBaseEngineDuty
    {
        OperationExecutionState LoadTablePool(string path);
        OperationExecutionState SaveTablePool(string path);
    }

    public class SimpleDataBaseEngine : IDataBaseEngineDuty, IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }

        public OperationExecutionState AddColumnToTable(string tableName, Column column) => throw new NotImplementedException();
        public OperationExecutionState CreateTable(string name) => throw new NotImplementedException();
        public OperationExecutionState CreateTable(TableMetaInf metaInf) => throw new NotImplementedException();
        public OperationExecutionState DeleteColumnFromTable(string tableName, string ColumnName) => throw new NotImplementedException();
        public OperationExecutionState DeleteTable(string name) => throw new NotImplementedException();
        public OperationResult<TableData> GetTableData(string name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInf(string name) => throw new NotImplementedException();
        public OperationExecutionState LoadTablePool(string path) => throw new NotImplementedException();
        public OperationExecutionState SaveTablePool(string path) => throw new NotImplementedException();
    }

    public class DataBase : IDataBaseEngineDuty, IDataBaseEngineFunction
    {

        public Dictionary<string, Table> TablePool { get; set; }
        public DataBase()
        {
            TablePool = new Dictionary<string, Table>();
        }
        public DataBase(string path)
        {
            LoadTablePool(path);
        }
        public OperationExecutionState AddColumnToTable(string tableName,Column column) {
            if (!TablePool.ContainsKey(tableName))
            {
                Console.WriteLine("Error DeleteColumnFromTable, Table named {0} doesn't exist", tableName);
                return OperationExecutionState.failed;
            }

            return TablePool[tableName].AddColumn(column);
        }
        public OperationExecutionState CreateTable(string name)
        {
            if (TablePool.ContainsKey(name))
            {
                Console.WriteLine("Error CreateTable, Table with name {0} already exist.", name);
                return OperationExecutionState.failed;
            }
            else
            {
                TablePool.Add(name, new Table(name));
            }

            return OperationExecutionState.performed;
        }
        public OperationExecutionState CreateTable(TableMetaInf metaInf)
        {
            if (TablePool.ContainsKey(metaInf.Name))
            {
                Console.WriteLine("Error CreateTable, Table with name {0} already exist.", metaInf.Name);
                return OperationExecutionState.failed;
            }
            else
            {
                TablePool.Add(metaInf.Name, new Table(metaInf));
            }

            return OperationExecutionState.performed;
        }
        public OperationExecutionState DeleteColumnFromTable(string tableName, string ColumnName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                Console.WriteLine("Error DeleteColumnFromTable, Table named {0} doesn't exist", tableName);
                return OperationExecutionState.failed;
            }
            return TablePool[tableName].DeleteColumn(ColumnName);
        }
        public OperationExecutionState DeleteTable(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                Console.WriteLine("Error DeleteTable, Table named {0} doesn't exist", tableName);
                return OperationExecutionState.failed;
            }
            TablePool.Remove(tableName);
            return OperationExecutionState.performed;
        }
        public OperationResult<TableData> GetTableData(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                Console.WriteLine("Error GetTableData, Table named {0} doesn't exist", tableName);
                return new OperationResult<TableData>(OperationExecutionState.failed, null);
            }
            return new OperationResult<TableData>(OperationExecutionState.performed, TablePool[tableName].TableData);
        }

        public OperationResult<TableMetaInf> GetTableMetaInf(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                Console.WriteLine("Error GetTableMetaInf, Table named {0} doesn't exist", tableName);
                return new OperationResult<TableMetaInf>(OperationExecutionState.failed, null);
            }
            return new OperationResult<TableMetaInf>(OperationExecutionState.performed, TablePool[tableName].TableMetaInf);
        }
        public OperationExecutionState LoadTablePool(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Error LoadTablePool, File named {0} doesn't exist ", path);
                return OperationExecutionState.failed;
            }
            using (var sr = new StreamReader(path))
            {
                if (sr.ReadLine() != fileMark)
                {
                    Console.WriteLine("Error LoadTablePool, File named {0} doesn't contain 'file mark' '{1}'  ", path, fileMark);
                    return OperationExecutionState.failed;
                }
                TablePool = new Dictionary<string, Table>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Table table = new Table();
                    table.LoadTableMetaInf(line);
                    TablePool.Add(table.TableMetaInf.Name,table);
                }
            }
            return OperationExecutionState.performed;
        }
        public OperationExecutionState SaveTablePool(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine(fileMark);
                foreach (var keyValue in TablePool)
                {
                    sw.Write(keyValue.Value.SerializeTableMetaInf().Result);
                    sw.WriteLine("");
                }
            }
            return OperationExecutionState.performed;
        }
        const string fileMark = "DATA_BASE_FILE";
    }

}
