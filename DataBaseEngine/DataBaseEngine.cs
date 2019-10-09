using System;
using System.Collections.Generic;
using System.IO;

using DataBaseErrors;

using DataBaseTable;

using Newtonsoft.Json;

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
        OperationResult<Table> CreateTable(TableMetaInf metaInf);

        OperationResult<Table> DeleteColumnFromTable(string tableName, string ColumnName);

        OperationResult<Table> AddColumnToTable(string tableName, Column column);

        OperationResult<TableData> GetTableData(string name);
        OperationResult<Table> GetTable(string name);
        OperationResult<TableMetaInf> GetTableMetaInf(string name);

        OperationResult<Table> DeleteTable(string name);
    }



    public class DataBaseEngineMain : IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }
        public EngineConfig EngineConfig { get; set; }
        protected IDataStorage dataStorage;
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

        public OperationResult<Table> CreateTable(TableMetaInf metaInf)
        {
            if (TablePool.ContainsKey(metaInf.Name))
            {
                return new OperationResult<Table>(OperationExecutionState.failed, null, new TableAlreadyExistExeption(metaInf.Name));
            }
            TablePool.Add(metaInf.Name, new Table(metaInf));
            return new OperationResult<Table>(OperationExecutionState.performed, TablePool[metaInf.Name]);
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
            return new OperationResult<Table>(OperationExecutionState.performed, TablePool[tableName]);
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
            return new OperationResult<string>(OperationExecutionState.performed, "Commited");
        }
        public OperationResult<Table> GetTable(string name)
        {
            return !TablePool.ContainsKey(name)
                ? new OperationResult<Table>(OperationExecutionState.failed, null, new TableNotExistExeption(name))
                : new OperationResult<Table>(OperationExecutionState.performed, TablePool[name]);
        }
    }

}
