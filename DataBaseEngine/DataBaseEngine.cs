using System;
using System.Collections.Generic;
using DataBaseType;
using StorageEngine;
using ZeroFormatter;

namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
    }

    public interface IDataBaseEngine
    {
        OperationResult<Table> CreateTableCommand (Guid transactionGuid, List<string> name);

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
        private readonly IDataStorage _dataStorage;
        private long _lastId;
        private readonly object _idLocker;
        private readonly Dictionary<Guid, TransactionTempInfo> _transactions;

        private class TransactionTempInfo
        {
            public long Id { get; set; }
            private Dictionary<string, Table> TempTables { get; set; }

            public TransactionTempInfo ()
            {
            }

            public TransactionTempInfo (long id)
            {
                Id = id;
                TempTables = new Dictionary<string, Table>();
            }
        }

        public DataBaseEngineMain ()
        {
            _dataStorage = new DataStorageInFiles(_pathDefault, _blockSizeDefault);
            _lastId = 0;
            _idLocker = new object();
            _transactions = new Dictionary<Guid, TransactionTempInfo>();
        }

        public DataBaseEngineMain (IDataStorage dataStorage, long lastId)
        {
            _dataStorage = dataStorage ?? throw new ArgumentNullException(nameof(dataStorage));
            _lastId = lastId;
            _idLocker = new object();
            _transactions = new Dictionary<Guid, TransactionTempInfo>();
        }



        public void StartTransaction (Guid transactionGuid)
        {
            lock (_idLocker)
            {
                _transactions.Add(transactionGuid, new TransactionTempInfo(++_lastId));
            }
        }

        private void CloseTransaction (Guid transactionGuid) => _transactions.Remove(transactionGuid);

        public void RollBackTransaction (Guid transactionGuid) => CloseTransaction(transactionGuid);

        public void CommitTransaction (Guid transactionGuid) => CloseTransaction(transactionGuid);

        public DataBaseEngineMain (string pathDataBaseStorage, int blockSize = _blockSizeDefault) => _dataStorage = new DataStorageInFiles(pathDataBaseStorage, blockSize);

        public OperationResult<Table> AddColumnCommand (Guid transactionGuid, List<string> tableName, Column column) => throw new NotImplementedException();
        public OperationResult<Table> CreateTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, List<string> tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<Table> DeleteCommand (Guid transactionGuid, List<string> tableName, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> DropTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> ExceptCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> GetTableCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> InsertCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams) => throw new NotImplementedException();
        public OperationResult<Table> IntersectCommand (Guid transactionGuid, List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> JoinCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, JoinKind joinKind, List<string> statmentLeftId, List<string> statmentRightId) => throw new NotImplementedException();
        public OperationResult<Table> SelectCommand (Guid transactionGuid, List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTableCommand (Guid transactionGuid, List<string> tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand (Guid transactionGuid, List<string> leftId, List<string> rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> UpdateCommand (Guid transactionGuid, List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
    }

    public enum TransactionState
    {
        COMITED,
        RUNNING,
        FAILED
    }
    [ZeroFormattable]
    public class Transaction
    {
        [Index(0)] public virtual Guid GuidTr { get; protected set; }
        [Index(1)] public virtual long Id { get; protected set; }
        [Index(2)] public virtual long PrevVerId { get; protected set; }
        [Index(3)] public virtual TransactionState State { get; set; }
        public Transaction ()
        {

        }
        public Transaction (Guid guidTr, long id, long prevVerId)
        {
            GuidTr = guidTr;
            Id = id;
            PrevVerId = prevVerId;
        }
    }
}
