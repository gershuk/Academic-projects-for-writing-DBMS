using System;
using System.Collections.Generic;
using DataBaseType;
using StorageEngine;
using ZeroFormatter;
using System.IO;
using System.Text;

namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
    }

    public interface IDataBaseEngine
    {
        OperationResult<Table> CreateTableCommand (Guid transactionGuid, List<string> tableName);

        OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, List<string> tableName, string ColumnName);

        OperationResult<Table> AddColumnCommand (Guid transactionGuid, List<string> tableName, Column column);

        OperationResult<Table> GetTableCommand (Guid transactionGuid, List<string> name);

        OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, List<string> name);

        OperationResult<Table> DropTableCommand (Guid transactionGuid, List<string> name);

        OperationResult<Table> InsertCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams);

        OperationResult<Table> SelectCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression);

        OperationResult<Table> UpdateCommand (Guid transactionGuid, List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction);

        OperationResult<Table> DeleteCommand (Guid transactionGuid, List<string> tableName, ExpressionFunction expression);

        OperationResult<Table> ShowTableCommand (Guid transactionGuid, List<string> tableName);

        OperationResult<Table> JoinCommand (Guid transactionGuid,
                                           List<string> leftId,
                                           List<string> rightId,
                                           JoinKind joinKind,
                                           List<string> statmentLeftId,
                                           List<string> statmentRightId);

        OperationResult<Table> UnionCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, UnionKind unionKind);

        OperationResult<Table> IntersectCommand (Guid transactionGuid, List<string> leftId, List<string> rightId);

        OperationResult<Table> ExceptCommand (Guid transactionGuid, List<string> leftId, List<string> rightId);

        void RollBackTransaction (Guid transactionGuid);

        void CommitTransaction (Guid transactionGuid);

        void StartTransaction (Guid transactionGuid);
    }

    public class DataBaseEngineMain : IDataBaseEngine
    {




        private const string _pathDefault = "DataBaseStorage";
        private const int _blockSizeDefault = 4096;
        private const string _fileNameDbMetaInf = "DbMetaInf.bin";
        private readonly IDataStorage _dataStorage;
        private readonly string _path;
        private readonly object _idLocker;
        private readonly DbEngineMetaInf _dbEngineMetaInf;

        public DataBaseEngineMain ()
        {
            _path = _pathDefault;
            _dataStorage = new DataStorageInFiles(_pathDefault, _blockSizeDefault);
            _idLocker = new object();
            _dbEngineMetaInf = CheckRestoreDb();
        }


        public DataBaseEngineMain (string pathDataBaseStorage, int blockSize = _blockSizeDefault)
        {
            _dataStorage = new DataStorageInFiles(pathDataBaseStorage, blockSize);
            _idLocker = new object();
            _dbEngineMetaInf = CheckRestoreDb();
        }

        private DbEngineMetaInf CheckRestoreDb ()
        {
            var metaInf = LoadDbMetaInf();
            if (metaInf == null)
            {
                metaInf = CreateDefaultDbMetaInf();
                return metaInf;
            }
            if (metaInf.Transactions.Count > 0)
            {
                foreach (var tran in metaInf.Transactions)
                {
                    //To do //RollBackTransaction(tran.Key);
                }
            }
            return new DbEngineMetaInf(metaInf);
        }

        static private DbEngineMetaInf CreateDefaultDbMetaInf ()
        {
            return new DbEngineMetaInf(0);
        }

        static private string GetFullName (List<string> tableName)
        {
            _ = tableName ?? throw new ArgumentNullException(nameof(tableName));
            var sb = new StringBuilder();
            foreach (var n in tableName)
            {
                sb.Append(n);
            }
            return sb.ToString();
        }

        private DbEngineMetaInf LoadDbMetaInf ()
        {
            if (!File.Exists(_path + "/" + _fileNameDbMetaInf))
            {
                return null;
            }
            using var fs = new FileStream(_path + "/" + _fileNameDbMetaInf, FileMode.Open);
            return ZeroFormatterSerializer.Deserialize<DbEngineMetaInf>(fs);

        }

        private void SaveDbMetaInf (DbEngineMetaInf dbMetaInf)
        {
            using var fs = new FileStream(_path + "/" + _fileNameDbMetaInf, FileMode.OpenOrCreate);
            ZeroFormatterSerializer.Serialize(fs, dbMetaInf);
        }

        private void AddChangedTable (Guid transactionGuid, List<string> tableName)
        {
            if (_dbEngineMetaInf.Transactions[transactionGuid].ChangedTables.ContainsKey(GetFullName(tableName)))
            {
                lock (_idLocker)
                {
                    _dbEngineMetaInf.Transactions[transactionGuid].ChangedTables.Add(GetFullName(tableName), 0);
                    SaveDbMetaInf(_dbEngineMetaInf);
                }
            }
        }

        public void StartTransaction (Guid transactionGuid)
        {
            lock (_idLocker)
            {
                _dbEngineMetaInf.Transactions.Add(transactionGuid, new TransactionTempInfo(++_dbEngineMetaInf.LastId, _dbEngineMetaInf.LastId));
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }

        public void RollBackTransaction (Guid transactionGuid) => throw new NotImplementedException();

        public void CommitTransaction (Guid transactionGuid)
        {

            lock (_idLocker)
            {
                var tr = _dbEngineMetaInf.Transactions[transactionGuid];
                foreach (var table in tr.TempTables)
                {
                    _dataStorage.AddTable(table.Value);
                }

                _dbEngineMetaInf.LastId = tr.Id;
                _dbEngineMetaInf.Transactions.Remove(transactionGuid);
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }
        public OperationResult<Table> CreateTableCommand (Guid transactionGuid, List<string> tableName)
        {
            var state = _dataStorage.ContainsTable(tableName);
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];

            if (state.Result == true || tr.TempTables.ContainsKey(GetFullName(tableName)))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableAlreadyExistError(tableName));
            }

            var table = new Table(tableName);
            table.TableMetaInf.CreatedTrId = _dbEngineMetaInf.Transactions[transactionGuid].Id;
            tr.TempTables.Add(GetFullName(tableName), table);
            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            return new OperationResult<Table>(ExecutionState.performed, table);
        }

        public OperationResult<Table> InsertCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams)
        {
            var res = _dataStorage.LoadTable(tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var table = res.Result;
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];

            if (columnNames == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(columnNames)));
            }
            if (objectParams == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(objectParams)));
            }

            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            var row = new Row(new Field[table.TableMetaInf.ColumnPool.Count]);
            for (var i = 0; i < columnNames.Count; ++i)
            {
                if (!table.TableMetaInf.ColumnPool.ContainsKey(GetFullName(columnNames[i])))
                {
                    return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistError(GetFullName(columnNames[i]), GetFullName(table.TableMetaInf.Name)));
                }
                row.Fields[i] = table.TableMetaInf.ColumnPool[GetFullName(columnNames[i])].CreateField(objectParams[i].CalcFunc());
            }
            row.TrStart = tr.Id;
            row.TrEnd = long.MaxValue;
            var resInsert = _dataStorage.InsertRow(table.TableMetaInf.Name, row);
            return new OperationResult<Table>(resInsert.State, table, resInsert.OperationError);
        }


        public OperationResult<Table> AddColumnCommand (Guid transactionGuid, List<string> tableName, Column column)
        {
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];
            if (!tr.TempTables.ContainsKey(GetFullName(tableName)))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(GetFullName(tableName)));
            }
            var table = tr.TempTables[GetFullName(tableName)];
            table.AddColumn(column);
            return new OperationResult<Table>(ExecutionState.performed, table);
        }
        public OperationResult<Table> GetTableCommand (Guid transactionGuid, List<string> name)
        {
            var state = _dataStorage.ContainsTable(name);
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];

            if (!state.Result)
            {
                if (tr.TempTables.ContainsKey(GetFullName(name)))
                {

                    return new OperationResult<Table>(ExecutionState.performed, tr.TempTables[GetFullName(name)]);
                }
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(GetFullName(name)));
            }
            return new OperationResult<Table>(ExecutionState.performed, _dataStorage.LoadTable(name).Result);
        }

        public OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, List<string> tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<Table> DeleteCommand (Guid transactionGuid, List<string> tableName, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> DropTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> ExceptCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
       
        public OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();

        public OperationResult<Table> IntersectCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> JoinCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, JoinKind joinKind, List<string> statmentLeftId, List<string> statmentRightId) => throw new NotImplementedException();
        public OperationResult<Table> SelectCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTableCommand (Guid transactionGuid, List<string> tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> UpdateCommand (Guid transactionGuid, List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
    }

    [ZeroFormattable]
    public class DbEngineMetaInf
    {
        [Index(0)] public virtual long LastId { get; set; }
        [Index(1)] public virtual Dictionary<Guid, TransactionTempInfo> Transactions { get; set; }

        public DbEngineMetaInf ()
        {

        }
        public DbEngineMetaInf (long lastId)
        {
            LastId = lastId;
            Transactions = new Dictionary<Guid, TransactionTempInfo>();
        }
        public DbEngineMetaInf (DbEngineMetaInf metaInf)
        {

            LastId = metaInf.LastId;
            Transactions = new Dictionary<Guid, TransactionTempInfo>();
        }
    }

    [ZeroFormattable]
    public class TransactionTempInfo
    {
        [Index(1)]
        public virtual long Id { get; protected set; }
        [Index(2)]
        public virtual long PrevVerId { get; protected set; }
        [Index(3)]
        public virtual Dictionary<string, int> ChangedTables { get; set; }
        [IgnoreFormat]
        public Dictionary<string, Table> TempTables { get; set; }

        public TransactionTempInfo ()
        {
        }

        public TransactionTempInfo (long id, long prevVerId)
        {
            Id = id;
            PrevVerId = prevVerId;
            TempTables = new Dictionary<string, Table>();
            ChangedTables = new Dictionary<string, int>();
        }
    }

}

//public enum TransactionState
//{
//    COMITED,
//    RUNNING,
//    FAILED
//}

