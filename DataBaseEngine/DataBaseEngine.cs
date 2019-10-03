using System;
using System.Collections.Generic;
using DataBaseTable;
using Newtonsoft.Json;
using System.IO;

namespace DataBaseEngine
{

    public enum OperationExecutionState
    {
        notProcessed,
        parserError,
        failed,
        performed
    }


    public class OperationResult<T>
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
        OperationResult<string> CreateTable(string name);
        OperationResult<string> CreateTable(TableMetaInf metaInf);

        OperationResult<string> DeleteColumnFromTable(string tableName, string ColumnName);

        OperationResult<string> AddColumnToTable(string tableName, Column column);

        OperationResult<TableData> GetTableData(string name);
        OperationResult<TableMetaInf> GetTableMetaInf(string name);

        OperationResult<string> DeleteTable(string name);
        OperationResult<string> ShowCreateTable(string name);
    }

    public interface IDataBaseEngineDuty
    {
        OperationResult<string> LoadTablePool(string path);
        OperationResult<string> SaveTablePool(string path);
    }

    public class SimpleDataBaseEngine : IDataBaseEngineDuty, IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }

        public OperationResult<string> AddColumnToTable(string tableName, Column column) => throw new NotImplementedException();
        public OperationResult<string> CreateTable(string name) => throw new NotImplementedException();
        public OperationResult<string> CreateTable(TableMetaInf metaInf) => throw new NotImplementedException();
        public OperationResult<string> DeleteColumnFromTable(string tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<string> DeleteTable(string name) => throw new NotImplementedException();
        public OperationResult<TableData> GetTableData(string name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInf(string name) => throw new NotImplementedException();
        public OperationResult<string> LoadTablePool(string path) => throw new NotImplementedException();
        public OperationResult<string> SaveTablePool(string path) => throw new NotImplementedException();
        public OperationResult<string> ShowCreateTable(string name) => throw new NotImplementedException();
    }

    public class DataBaseEngineMain : IDataBaseEngineDuty, IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }
        const string fileMark = "DATA_BASE_FILE";
        public DataBaseEngineMain()
        {
            TablePool = new Dictionary<string, Table>();
        }

        public DataBaseEngineMain(string path)
        {
            LoadTablePool(path);
        }

        public OperationResult<string> AddColumnToTable(string tableName, Column column)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error DeleteColumnFromTable, Table named {0} doesn't exist", tableName);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }

            return TablePool[tableName].AddColumn(column);
        }

        public OperationResult<string> CreateTable(string name)
        {
            if (TablePool.ContainsKey(name))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error CreateTable, Table with name {0} already exist.", name);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            else
            {
                TablePool.Add(name, new Table(name));
            }

            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<string> CreateTable(TableMetaInf metaInf)
        {
            if (TablePool.ContainsKey(metaInf.Name))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error CreateTable, Table with name {0} already exist.", metaInf.Name);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            else
            {
                TablePool.Add(metaInf.Name, new Table(metaInf));
            }

            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<string> DeleteColumnFromTable(string tableName, string ColumnName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error DeleteColumnFromTable, Table named {0} doesn't exist", tableName);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            return TablePool[tableName].DeleteColumn(ColumnName);
        }

        public OperationResult<string> DeleteTable(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error DeleteTable, Table named {0} doesn't exist", tableName);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            TablePool.Remove(tableName);
            return new OperationResult<string>(OperationExecutionState.performed, "ok");
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

        public OperationResult<string> LoadTablePool(string path)
        {
            if (!File.Exists(path))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error LoadTablePool, File named {0} doesn't exist ", path);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            using (var sr = new StreamReader(path))
            {
                if (sr.ReadLine() != fileMark)
                {
                    using (var sw = new StringWriter())
                    {
                        sw.WriteLine("Error LoadTablePool, File named {0} doesn't contain 'file mark' '{1}'  ", path, fileMark);
                        return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                    }
                }
                TablePool = new Dictionary<string, Table>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var table = new Table();
                    table.LoadTableMetaInf(line);
                    TablePool.Add(table.TableMetaInf.Name, table);
                }
            }
            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<string> SaveTablePool(string path)
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
            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<string> ShowCreateTable(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error DeleteTable, Table named {0} doesn't exist", tableName);
                    return new OperationResult<string>(OperationExecutionState.failed, sw.ToString());
                }
            }
            using (var sw = new StringWriter())
            {
                var table = TablePool[tableName];
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
