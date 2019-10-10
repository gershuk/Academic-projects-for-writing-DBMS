using System;
using System.Collections.Generic;
using System.IO;

using DataBaseErrors;

using DataBaseTable;

using Newtonsoft.Json;
using System.Linq;

namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
    }

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
        public Exception OperationException { get; set; }
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result, Exception opException = null)
        {
            State = state;
            Result = result;
            OperationException = opException;
        }
    }

    public interface IDataBaseEngineFunction
    {
        OperationResult<Table> CreateTable(string name);

        OperationResult<Table> DeleteColumnFromTable(string tableName, string ColumnName);

        OperationResult<Table> AddColumnToTable(string tableName, Column column);

        OperationResult<TableData> GetTableData(string name);
        OperationResult<Table> GetTable(string name);
        OperationResult<TableMetaInf> GetTableMetaInf(string name);

        OperationResult<Table> DeleteTable(string name);

        OperationResult<Table> Insert(string tableName, List<string> columnNames, List<List<string>> rows);
        OperationResult<Table> Select(List<string> tableName, List<Tuple<string, string>> columnNames);
        OperationResult<Table> Update(string tableName, Dictionary<string, string> row);
        OperationResult<Table> Delete(string tableName);
    }



    public class DataBaseEngineMain : IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }
        public EngineConfig EngineConfig { get; set; }
        public IDataStorage dataStorage;
        private const string _fileMarkEngineConfig = "ENGINE_CONFIG_FILE";
        private const string _DefPathToEngineConfig = "DataEngineConfig.json";

        public DataBaseEngineMain()
        {
            LoadEngine(_DefPathToEngineConfig);
        }

        public DataBaseEngineMain(string configPath)
        {
            LoadEngine(configPath);
        }

        private void LoadEngine(string configPath)
        {
            var result = LoadEngineConfig(configPath);

            if (result.State == OperationExecutionState.failed)
            {
                CreateDefaultEngineConfig(configPath);
            }
            dataStorage = new DataStorageInFiles(EngineConfig.Path);
            var resultLoad = dataStorage.LoadTablePoolMetaInf();

            if (resultLoad.State == OperationExecutionState.performed)
            {
                TablePool = resultLoad.Result;
            }
            else
            {
                TablePool = new Dictionary<string, Table>();
            }
        }
        private void CreateDefaultEngineConfig(string path)
        {
            EngineConfig = new EngineConfig
            {
                Path = "MainDataBase.db"
            };
            SaveEngineConfig(path);
        }

        private OperationResult<string> LoadEngineConfig(string path)
        {
            if (!File.Exists(path))
            {
                return new OperationResult<string>(OperationExecutionState.failed, "", new FileNotExistExeption(path));
            }

            using (var sr = new StreamReader(path))
            {
                if (sr.ReadLine() != _fileMarkEngineConfig)
                {
                    return new OperationResult<string>(OperationExecutionState.failed, "", new FileMarkNotExistExeption(path, _fileMarkEngineConfig));

                }
                EngineConfig = JsonConvert.DeserializeObject<EngineConfig>(sr.ReadToEnd());
            }

            return new OperationResult<string>(OperationExecutionState.performed, "Config loaded.");
        }

        private OperationResult<string> SaveEngineConfig(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine(_fileMarkEngineConfig);
                sw.Write(JsonConvert.SerializeObject(EngineConfig));
            }

            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<Table> AddColumnToTable(string tableName, Column column)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            return TablePool[tableName].AddColumn(column);
        }

        public OperationResult<Table> CreateTable(string name)
        {
            if (TablePool.ContainsKey(name))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableAlreadyExistExeption(name));
            }
            TablePool.Add(name, new Table(name));
            return new OperationResult<Table>(OperationExecutionState.performed, TablePool[name]);
        }

        public OperationResult<Table> DeleteColumnFromTable(string tableName, string ColumnName)
        {
            return !TablePool.ContainsKey(tableName)
                ? new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName))
                : TablePool[tableName].DeleteColumn(ColumnName);
        }

        public OperationResult<Table> DeleteTable(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            TablePool.Remove(tableName);
            return new OperationResult<Table>(OperationExecutionState.performed, null);
        }

        public OperationResult<TableData> GetTableData(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<TableData>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            return new OperationResult<TableData>(OperationExecutionState.performed, TablePool[tableName].TableData);
        }

        public OperationResult<TableMetaInf> GetTableMetaInf(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<TableMetaInf>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }

            return new OperationResult<TableMetaInf>(OperationExecutionState.performed, TablePool[tableName].TableMetaInf);
        }

        public OperationResult<string> Commit()
        {
            dataStorage.SaveTablePoolMetaInf(TablePool);
            foreach (var t in TablePool)
            {
                dataStorage.SaveTableData(t.Value);
                t.Value.TableData = null;
            }
            return new OperationResult<string>(OperationExecutionState.performed, "Commited");
        }
        public OperationResult<Table> GetTable(string name)
        {
            return !TablePool.ContainsKey(name)
                ? new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(name))
                : new OperationResult<Table>(OperationExecutionState.performed, TablePool[name]);
        }

        public OperationResult<Table> Insert(string tableName, List<string> columnNames, List<List<string>> rows)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            if(columnNames == null)
            {
                columnNames = new List<string>();
                columnNames.AddRange(TablePool[tableName].TableMetaInf.ColumnPool.Keys);
            }
            var table = TablePool[tableName];
            dataStorage.LoadTableData(table);
            table.TableData ??= new TableData { Rows = new List<Dictionary<string, Field>>()}; 
            foreach (var L1 in rows)
            {
                var row = new Dictionary<string, Field>();
                for (int i = 0; i < L1.Count; ++i)
                {
                    Field field;
                    if (!table.TableMetaInf.ColumnPool.ContainsKey(columnNames[i]))
                    {
                        return new OperationResult<Table>(OperationExecutionState.failed, null, new ColumnNotExistExeption(tableName, columnNames[i]));
                    }

                    var result = table.TableMetaInf.ColumnPool[columnNames[i]].CreateField(L1[i]);
                    if (result.State != OperationExecutionState.performed)
                    {
                        return new OperationResult<Table>(OperationExecutionState.failed, null, result.OperationException);
                    }
                    row.Add(columnNames[i], result.Result);
                }
                table.TableData.Rows.Add(row);
            }
            return new OperationResult<Table>(OperationExecutionState.performed, table);
        }
        /// <summary>
        /// Select statment
        /// </summary>
        /// <param name="tableNames">tableNames</param>
        /// <param name="columnNames">Item1 - tableName, Item2 - columnName. For *, Item2 == "*" then adding all colums from table Item1</param>
        /// <returns>New table</returns>
        public OperationResult<Table> Select(List<string> tableNames, List<Tuple<string, string>> columnNames)
        {
          
            foreach (var L in tableNames)
            {
                if (!TablePool.ContainsKey(L))
                {
                    return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(L));
                }
            }
            foreach (var L in columnNames)
            {
                var tableName = L.Item1 ?? tableNames[0];
                if (L.Item2 == "*") { 
                    foreach (var column in TablePool[tableName].TableMetaInf.ColumnPool)
                    {
                        columnNames.Add(new Tuple<string, string>(L.Item1,column.Key));
                    }
                }
            }
            columnNames.RemoveAll(item => item.Item2 == "*");
                if (columnNames[0].Item2 == "*")
            {
                columnNames.Clear();
                foreach (var L in tableNames)
                {
                    foreach (var d in TablePool[L].TableMetaInf.ColumnPool)
                    {
                        columnNames.Add(new Tuple<string, string>(L,d.Value.Name));
                    }
                }
            }

            var tableOut = new Table("tableOut");
            foreach (var d in columnNames)
            {
                tableOut.AddColumn(TablePool[d.Item1].TableMetaInf.ColumnPool[d.Item2]);
            }
            var tableData = new TableData();
            foreach (var L in tableNames)
            {
                dataStorage.LoadTableData(TablePool[L]);
                tableData.Rows = new List<Dictionary<string, Field>>();
                tableData.Rows.AddRange(TablePool[L].TableData.Rows);
                TablePool[L].TableData = null;
            }
            tableOut.TableData = tableData;
            return new OperationResult<Table>(OperationExecutionState.performed, tableOut);
        }

        public OperationResult<Table> Update(string tableName, Dictionary<string, string> row) {

            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            var table = TablePool[tableName];
            dataStorage.LoadTableData(table);
            foreach(var L in table.TableData.Rows)
            {
                foreach (var d in table.TableMetaInf.ColumnPool)
                {
                    var result = d.Value.CreateField(row[d.Key]);
                    if (result.State != OperationExecutionState.performed)
                    {
                        return new OperationResult<Table>(OperationExecutionState.failed, null, result.OperationException);
                    }
                    L[d.Key] = result.Result;
                }
            }
            return new OperationResult<Table>(OperationExecutionState.performed, table);
        }
        public OperationResult<Table> Delete(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(tableName));
            }
            var table = TablePool[tableName];
            dataStorage.LoadTableData(table);
            table.TableData.Rows.RemoveAll(item => true);
            return new OperationResult<Table>(OperationExecutionState.performed, table);
        }
    }

}
