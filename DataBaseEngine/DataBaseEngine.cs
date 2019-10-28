using System;
using System.Collections.Generic;
using System.IO;

using DataBaseErrors;

using DataBaseTable;

using Newtonsoft.Json;

using StorageEngine;

namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
        public int BlockSize { get; set; }
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

        OperationResult<Table> GetTable(string name);


        OperationResult<Table> DeleteTable(string name);

        OperationResult<Table> Insert(string tableName, List<string> columnNames, List<List<string>> rows);

        OperationResult<Table> Select(List<string> tableName, List<Tuple<string, string>> columnNames);

        OperationResult<Table> Update(string tableName, Dictionary<string, string> row);

        OperationResult<Table> Delete(string tableName);
    }



    public class DataBaseEngineMain : IDataBaseEngineFunction
    {
        private const string _fileMarkEngineConfig = "ENGINE_CONFIG_FILE";
        private const string _DefPathToEngineConfig = "DataEngineConfig.json";

        public EngineConfig EngineConfig { get; set; }
        public IDataStorage DataStorage { get; set; }


        public DataBaseEngineMain() => LoadEngine(_DefPathToEngineConfig);

        public DataBaseEngineMain(string configPath) => LoadEngine(configPath);

        private void LoadEngine(string configPath)
        {
            var result = LoadEngineConfig(configPath);

            if (result.State == OperationExecutionState.failed)
            {
                CreateDefaultEngineConfig(configPath);
            }

            DataStorage = new DataStorageInFiles(EngineConfig.Path,EngineConfig.BlockSize);
        }

        private void CreateDefaultEngineConfig(string path)
        {
            EngineConfig = new EngineConfig
            {
                Path = "/MainDataBase",
                BlockSize = 4096
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
            throw new NotImplementedException();
        }

        public OperationResult<Table> CreateTable(string name)
        {
            throw new NotImplementedException();
        }

        public OperationResult<Table> DeleteColumnFromTable(string tableName, string ColumnName)
        {
            throw new NotImplementedException();
        }

        public OperationResult<Table> DeleteTable(string tableName)
        {
            throw new NotImplementedException();
        }


        public OperationResult<string> Commit()
        {
            throw new NotImplementedException();
        }

        public OperationResult<Table> GetTable(string name)
        {
            throw new NotImplementedException();
        }

        public OperationResult<Table> Insert(string tableName, List<string> columnNames, List<List<string>> rows)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Select statment
        /// </summary>
        /// <param name="tableNames">tableNames</param>
        /// <param name="columnNames">Item1 - tableName, Item2 - columnName. For *, Item2 == "*" then adding all colums from table Item1</param>
        /// <returns>New table</returns>
        public OperationResult<Table> Select(List<string> tableNames, List<Tuple<string, string>> columnNames)
        {

            throw new NotImplementedException();
        }

        public OperationResult<Table> Update(string tableName, Dictionary<string, string> row)
        {
            throw new NotImplementedException();
        }
        public OperationResult<Table> Delete(string tableName)
        {
            throw new NotImplementedException();
        }
    }

}
