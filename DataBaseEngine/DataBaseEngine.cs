using System;
using System.Collections.Generic;
using System.IO;

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
        OperationResult<Table> CreateTableCommand (Guid transactionGuid, Id name);

        OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, Id tableName, Id ColumnName);

        OperationResult<Table> AddColumnCommand (Guid transactionGuid, Id tableName, Column column);

        OperationResult<Table> GetTableCommand (Guid transactionGuid, Id name);

        OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, Id name);

        OperationResult<Table> DropTableCommand (Guid transactionGuid, Id name);

        OperationResult<Table> InsertCommand (Guid transactionGuid, Id tableName, List<Id> columnNames, List<ExpressionFunction> objectParams);

        OperationResult<Table> SelectCommand (Guid transactionGuid, Id tableName, List<Id> columnNames, ExpressionFunction expression);

        OperationResult<Table> UpdateCommand (Guid transactionGuid, Id tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction);

        OperationResult<Table> DeleteCommand (Guid transactionGuid, Id tableName, ExpressionFunction expression);

        OperationResult<Table> ShowTableCommand (Guid transactionGuid, Id tableName);

        OperationResult<Table> JoinCommand (Guid transactionGuid,
                                           Id leftId,
                                           Id rightId,
                                           JoinKind joinKind,
                                           Id statmentLeftId,
                                           Id statmentRightId);

        OperationResult<Table> UnionCommand (Guid transactionGuid, Id leftId, Id rightId, UnionKind unionKind);

        OperationResult<Table> IntersectCommand (Guid transactionGuid, Id leftId, Id rightId);

        OperationResult<Table> ExceptCommand (Guid transactionGuid, Id leftId, Id rightId);

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

        private static DbEngineMetaInf CreateDefaultDbMetaInf () => new DbEngineMetaInf(0);

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

        private void AddChangedTable (Guid transactionGuid, Id tableName)
        {
            if (!_dbEngineMetaInf.Transactions[transactionGuid].ChangedTables.ContainsKey(tableName.ToString()))
            {
                lock (_idLocker)
                {
                    _dbEngineMetaInf.Transactions[transactionGuid].ChangedTables.Add(tableName.ToString(), 0);
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
                foreach (var table in tr.NewTables)
                {
                    _dataStorage.AddTable(table.Value);
                }

                _dbEngineMetaInf.LastId = tr.Id;
                _dbEngineMetaInf.Transactions.Remove(transactionGuid);
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }
        public OperationResult<Table> CreateTableCommand (Guid transactionGuid, Id name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var state = _dataStorage.ContainsTable(name);
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];

            if (state.Result == true || tr.TempTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableAlreadyExistError(name.ToString()));
            }

            var table = new Table(name);
            table.TableMetaInf.CreatedTrId = _dbEngineMetaInf.Transactions[transactionGuid].Id;
            tr.NewTables.Add(name.ToString(), table);
            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            return new OperationResult<Table>(ExecutionState.performed, table);
        }

        public OperationResult<Table> InsertCommand (Guid transactionGuid, Id tableName, List<Id> columnNames, List<ExpressionFunction> objectParams)
        {
            var res = _dataStorage.LoadTable(tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var table = res.Result;
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];
            var colPool = table.TableMetaInf.ColumnPool;
            if (columnNames == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(columnNames)));
            }
            if (objectParams == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(objectParams)));
            }

            if (colPool.Count < columnNames.Count)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ColumnTooMachError(table.TableMetaInf.Name.ToString()));
            }

            foreach (var col in columnNames)
            {
                if (colPool.FindIndex((Column n) => col.ToString() == n.Name) < 0)
                {
                    return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistError(col.ToString(), table.TableMetaInf.Name.ToString()));
                }
            }

            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            var row = new Row(new Field[table.TableMetaInf.ColumnPool.Count]);
            for (var i = 0; i < colPool.Count; ++i)
            {
                var index = columnNames.FindIndex((Id n) => colPool[i].Name == n.ToString());
                var exprDict = new Dictionary<Id, dynamic>();
                foreach (var v in objectParams[index].VariablesNames)
                {
                    exprDict.Add(v, null);
                }
                dynamic data = null;
                try
                {
                    data = objectParams[index].CalcFunc(exprDict);
                }
                catch (Exception ex)
                {
                    return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                }
                row.Fields[i] = index >= 0
                    ? (Field)table.TableMetaInf.ColumnPool[i].CreateField(data).Result
                    : table.TableMetaInf.ColumnPool[i].CreateField("0").Result;
            }
            row.TrStart = tr.Id;
            row.TrEnd = long.MaxValue;
            var resInsert = _dataStorage.InsertRow(table.TableMetaInf.Name, row);
            return new OperationResult<Table>(resInsert.State, table, resInsert.OperationError);
        }

        public OperationResult<Table> AddColumnCommand (Guid transactionGuid, Id tableName, Column column)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];
            if (!tr.NewTables.ContainsKey(tableName.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(tableName.ToString()));
            }
            var table = tr.NewTables[tableName.ToString()];
            table.AddColumn(column);
            return new OperationResult<Table>(ExecutionState.performed, table);
        }

        public OperationResult<Table> GetTableCommand (Guid transactionGuid, Id name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];
            if (tr.TempTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.performed, tr.TempTables[name.ToString()]);
            }
            if (tr.NewTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.performed, tr.NewTables[name.ToString()]);
            }
            var state = _dataStorage.ContainsTable(name);
            return state.Result
                ? new OperationResult<Table>(ExecutionState.performed, _dataStorage.LoadTable(name).Result)
                : new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(name.ToString()));
        }
        public OperationResult<Table> DeleteCommand (Guid transactionGuid, Id tableName, ExpressionFunction expression)
        {
            var res = _dataStorage.LoadTable(tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var tr = _dbEngineMetaInf.Transactions[transactionGuid];
            var table = res.Result;
            OperationResult<string> resUpdate = null;
            try
            {
                resUpdate = _dataStorage.UpdateAllRow(table.TableMetaInf.Name,
                       (Row r) => expression.CalcFunc(CompileExpressionData(expression.VariablesNames, r, table.TableMetaInf.ColumnPool)) ? r.SetTrEnd(tr.Id) : null);
            }
            catch (Exception ex)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
            }

            return new OperationResult<Table>(resUpdate.State, table, resUpdate.OperationError);
        }
        private static Dictionary<Id, dynamic> CompileExpressionData (List<Id> variablesNames, Row row, List<Column> colPool)
        {
            var exprDict = new Dictionary<Id, dynamic>();

            foreach (var v in variablesNames)
            {
                var index = colPool.FindIndex((Column n) => v.ToString() == n.Name);
                switch (row.Fields[index].Type)
                {
                    case DataType.INT:
                        exprDict.Add(v, ((FieldInt)(row.Fields[index])).Value);
                        break;
                    case DataType.DOUBLE:
                        exprDict.Add(v, ((FieldDouble)(row.Fields[index])).Value);
                        break;
                    case DataType.CHAR:
                        exprDict.Add(v, ((FieldChar)(row.Fields[index])).Value);
                        break;
                }

            }
            return exprDict;
        }

        public OperationResult<Table> DropTableCommand (Guid transactionGuid, Id name) => throw new NotImplementedException();
        public OperationResult<Table> ExceptCommand (Guid transactionGuid, Id leftId, Id rightId) => throw new NotImplementedException();

        public OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, Id name) => throw new NotImplementedException();

        public OperationResult<Table> IntersectCommand (Guid transactionGuid, Id leftId, Id rightId) => throw new NotImplementedException();
        public OperationResult<Table> JoinCommand (Guid transactionGuid, Id leftId, Id rightId, JoinKind joinKind, Id statmentLeftId, Id statmentRightId) => throw new NotImplementedException();
        public OperationResult<Table> SelectCommand (Guid transactionGuid, Id tableName, List<Id> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTableCommand (Guid transactionGuid, Id tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand (Guid transactionGuid, Id leftId, Id rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> UpdateCommand (Guid transactionGuid, Id tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
        public OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, Id tableName, Id ColumnName) => throw new NotImplementedException();
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
            if (metaInf == null)
            {
                throw new ArgumentNullException(nameof(metaInf));
            }
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
        public Dictionary<string, Table> TempTables { get; }
        [IgnoreFormat]
        public Dictionary<string, Table> NewTables { get; }

        public TransactionTempInfo ()
        {
            TempTables = new Dictionary<string, Table>();
            NewTables = new Dictionary<string, Table>();
        }

        public TransactionTempInfo (long id, long prevVerId)
        {
            Id = id;
            PrevVerId = prevVerId;
            TempTables = new Dictionary<string, Table>();
            NewTables = new Dictionary<string, Table>();
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

