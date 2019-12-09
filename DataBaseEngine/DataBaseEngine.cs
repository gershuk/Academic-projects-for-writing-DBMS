using System;
using System.Collections.Generic;
using DataBaseType;
using StorageEngine;
using ZeroFormatter;
using System.IO;

namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
    }

    public interface IDataBaseEngine
    {
        OperationResult<Table> CreateTableCommand (Guid transactionGuid, Table table);

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


        [ZeroFormattable]
        private class DbEngineMetaInf
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
        private class TransactionTempInfo
        {
            [Index(1)] public virtual long Id { get; protected set; }
            [Index(2)] public virtual long PrevVerId { get; protected set; }
            [Index(3)] public virtual HashSet<string> ChangedTables { get; set; }
            private Dictionary<string, Table> TempTables { get; set; }

            public TransactionTempInfo ()
            {
            }

            public TransactionTempInfo (long id, long prevVerId)
            {
                Id = id;
                PrevVerId = prevVerId;
                TempTables = new Dictionary<string, Table>();
                ChangedTables = new HashSet<string>();
            }
        }

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
            if(metaInf.Transactions.Count > 0)
            {
                foreach (var tran in metaInf.Transactions)
                {
                    RollBackTransaction(tran.Key);
                }
            }
            return new DbEngineMetaInf(metaInf);
        }

        private DbEngineMetaInf CreateDefaultDbMetaInf ()
        {
            return new DbEngineMetaInf(0);
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

        public void StartTransaction (Guid transactionGuid)
        {
            lock (_idLocker)
            {
                _dbEngineMetaInf.Transactions.Add(transactionGuid, new TransactionTempInfo(++_dbEngineMetaInf.LastId, _dbEngineMetaInf.LastId));
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }

        public void RollBackTransaction (Guid transactionGuid) => throw new NotImplementedException();

        public void CommitTransaction (Guid transactionGuid) {
            lock (_idLocker)
            {
                _dbEngineMetaInf.LastId = _dbEngineMetaInf.Transactions[transactionGuid].Id;
                _dbEngineMetaInf.Transactions.Remove(transactionGuid);
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }
        public OperationResult<Table> CreateTableCommand (Guid transactionGuid, Table table)
        {
           var state  = _dataStorage.AddTable(table);
           return new  OperationResult<Table>(state.State, table, state.OperationError);
        }

        public OperationResult<Table> InsertCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams) {
            throw new NotImplementedException();
        }


        public OperationResult<Table> AddColumnCommand (Guid transactionGuid, List<string> tableName, Column column) => throw new NotImplementedException();

        public OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, List<string> tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<Table> DeleteCommand (Guid transactionGuid, List<string> tableName, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> DropTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> ExceptCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> GetTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
       
        public OperationResult<Table> IntersectCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> JoinCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, JoinKind joinKind, List<string> statmentLeftId, List<string> statmentRightId) => throw new NotImplementedException();
        public OperationResult<Table> SelectCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTableCommand (Guid transactionGuid, List<string> tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> UpdateCommand (Guid transactionGuid, List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
    }
}

//public enum TransactionState
//{
//    COMITED,
//    RUNNING,
//    FAILED
//}

