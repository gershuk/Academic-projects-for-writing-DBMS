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
        public Exception OpException { get; set; }
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result, Exception opException = null)
        {
            State = state;
            Result = result;
            OpException = opException;
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

        public OperationResult<string> AddColumnToTable(string tableName, Column column)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<string>(OperationExecutionState.failed, "", new TableNotExistExeption(tableName));
            }
            return TablePool[tableName].AddColumn(column);
        }

        public OperationResult<string> CreateTable(string name)
        {
            if (TablePool.ContainsKey(name))
            {
                return new OperationResult<string>(OperationExecutionState.failed, "", new TableAlreadyExistExeption(name));
            }
            TablePool.Add(name, new Table(name));
            return new OperationResult<string>(OperationExecutionState.performed, "");
        }

        public OperationResult<string> CreateTable(TableMetaInf metaInf)
        {
            if (TablePool.ContainsKey(metaInf.Name))
            {
                return new OperationResult<string>(OperationExecutionState.failed, "", new TableAlreadyExistExeption(metaInf.Name));
            }
            TablePool.Add(metaInf.Name, new Table(metaInf));
            return new OperationResult<string>(OperationExecutionState.performed, "");
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
                return new OperationResult<string>(OperationExecutionState.failed, "", new TableNotExistExeption(tableName));
            }
            TablePool.Remove(tableName);
            return new OperationResult<string>(OperationExecutionState.performed, "ok");
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


        public OperationResult<string> ShowCreateTable(string tableName)
        {
            if (!TablePool.ContainsKey(tableName))
            {
                return new OperationResult<string>(OperationExecutionState.failed, "", new TableNotExistExeption(tableName));
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

        public OperationResult<string> Commit()
        {
            dataStorage.SaveTablePoolMetaInf(TablePool);
            return new OperationResult<string>(OperationExecutionState.performed, "Commited");
        }

    }

}
