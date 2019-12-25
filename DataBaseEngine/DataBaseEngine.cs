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

        OperationResult<Table> SelectCommand (Guid transactionGuid, Id tableName, TimeSelectorDelegate timeSelector, List<Id> columnNames, ExpressionFunction expression);

        OperationResult<Table> UpdateCommand (Guid transactionGuid, Id tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction);

        OperationResult<Table> DeleteCommand (Guid transactionGuid, Id tableName, ExpressionFunction expression);

        OperationResult<Table> ShowTableCommand (Guid transactionGuid, Id tableName);

        OperationResult<Table> JoinCommand (Guid transactionGuid,
                                           Id leftId,
                                           Id rightId,
                                           JoinKind joinKind,
                                           ExpressionFunction expressionFunction);

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
        private DbEngineMetaInf _dbEngineMetaInf;
        private readonly Dictionary<string, Dictionary<string, SortedDictionary<dynamic, Tuple<long, long>>>> _indexes;

        public DataBaseEngineMain ()
        {
            _path = _pathDefault;
            _dataStorage = new DataStorageInFiles(_pathDefault, _blockSizeDefault);
            _idLocker = new object();
            _dbEngineMetaInf = CheckRestoreDb();
            _indexes = new Dictionary<string, Dictionary<string, SortedDictionary<dynamic, Tuple<long, long>>>>();
        }


        public DataBaseEngineMain (string pathDataBaseStorage, int blockSize = _blockSizeDefault)
        {
            _path = pathDataBaseStorage;
            _dataStorage = new DataStorageInFiles(pathDataBaseStorage, blockSize);
            _idLocker = new object();
            _dbEngineMetaInf = CheckRestoreDb();
            _indexes = new Dictionary<string, Dictionary<string, SortedDictionary<dynamic, Tuple<long,long>>>>();
        }

        private DbEngineMetaInf CheckRestoreDb ()
        {
            var metaInf = LoadDbMetaInf();
            if (metaInf == null)
            {
                metaInf = CreateDefaultDbMetaInf();
                return metaInf;
            }
            if (metaInf.TransactionsInRun.Count > 0)
            {
                _dbEngineMetaInf = metaInf;
                var trs = new Dictionary<Guid, TransactionTempInfo>(metaInf.TransactionsInRun);
                foreach (var tran in trs)
                {
                    RollBackTransaction(tran.Key);
                }
            }
            var newMetaInf = new DbEngineMetaInf(metaInf);
            SaveDbMetaInf(newMetaInf);
            return newMetaInf;
        }
        private SortedDictionary<dynamic, Tuple<long, long>> GetIndex (Id tableName,Id column)
        {
            var tableNameStr = tableName.ToString();
            var columnStr = column.ToString();
            if (_indexes.ContainsKey(tableNameStr))
            {
                if (_indexes[tableNameStr].ContainsKey(columnStr))
                {
                    return _indexes[tableNameStr][columnStr];
                }
                else
                {
                    _indexes[tableNameStr].Add(columnStr, new SortedDictionary<dynamic, Tuple<long,long>>());
                    var table = _dataStorage.LoadTable(tableName).Result;
                    foreach (var row in table.TableData)
                    {
                        var index = table.TableMetaInf.ColumnPool.FindIndex((Column n) => columnStr == n.Name.ToString());
                        if (index >= 0)
                        {
                            switch (row.Fields[index].Type)
                            {
                                case DataType.INT:
                                    _indexes[tableNameStr][columnStr].Add(((FieldInt)(row.Fields[index])).Value,new Tuple<long, long>(row.FilePtrBlock,row.InBlockPos));
                                    break;
                                case DataType.DOUBLE:
                                    _indexes[tableNameStr][columnStr].Add(((FieldDouble)(row.Fields[index])).Value, new Tuple<long, long>(row.FilePtrBlock, row.InBlockPos));
                                    break;
                                case DataType.CHAR:
                                    _indexes[tableNameStr][columnStr].Add(((FieldChar)(row.Fields[index])).Value, new Tuple<long, long>(row.FilePtrBlock, row.InBlockPos));
                                    break;
                            }
                        }
                        else
                        {
                            throw new Exception($"Unknown variable {columnStr}");
                        }
                    }
                }
            }
            else
            {
                _indexes.Add(tableNameStr, new Dictionary<string, SortedDictionary<dynamic, Tuple<long, long>>>());
                return GetIndex(tableName, column);
            }
            return null;
        }
        private static DbEngineMetaInf CreateDefaultDbMetaInf () => new DbEngineMetaInf(0, 0);

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
            if (!_dbEngineMetaInf.TransactionsInRun[transactionGuid].ChangedTables.ContainsKey(tableName.ToString()))
            {
                lock (_idLocker)
                {
                    _dbEngineMetaInf.TransactionsInRun[transactionGuid].ChangedTables.Add(tableName.ToString(), tableName);
                    SaveDbMetaInf(_dbEngineMetaInf);
                }
            }
        }

        public void StartTransaction (Guid transactionGuid)
        {
            lock (_idLocker)
            {
                _dbEngineMetaInf.TransactionsInRun.Add(transactionGuid, new TransactionTempInfo(++_dbEngineMetaInf.LastId, _dbEngineMetaInf.LastCommitedId));
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }

        public void RollBackTransaction (Guid transactionGuid)
        {
            lock (_idLocker)
            {
                var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
                foreach (var tName in tr.ChangedTables)
                {
                    _dataStorage.RemoveAllRow(tName.Value, (Row r) => r.TrStart == tr.Id);
                    _dataStorage.UpdateAllRow(tName.Value, (Row r) => r.TrEnd == tr.Id ? r.SetTrEnd(long.MaxValue) : null);
                }
                _dbEngineMetaInf.TransactionsInRun.Remove(transactionGuid);
                SaveDbMetaInf(_dbEngineMetaInf);
            }
        }

        public void CommitTransaction (Guid transactionGuid)
        {

            lock (_idLocker)
            {
                var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
                foreach (var table in tr.NewTables)
                {
                    _dataStorage.AddTable(table.Value);
                }

                _dbEngineMetaInf.LastCommitedId = tr.Id > _dbEngineMetaInf.LastCommitedId ? tr.Id : _dbEngineMetaInf.LastCommitedId;
                _dbEngineMetaInf.TransactionsInRun[transactionGuid].timeCommit = DateTime.Now.ToUniversalTime();
                _dbEngineMetaInf.TransactionsHistory.Add(_dbEngineMetaInf.TransactionsInRun[transactionGuid].Id, _dbEngineMetaInf.TransactionsInRun[transactionGuid]);
                _dbEngineMetaInf.TransactionsInRun.Remove(transactionGuid);
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
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];

            if (state.Result == true || tr.NewTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableAlreadyExistError(name.ToString()));
            }

            var table = new Table(name);
            table.TableMetaInf.CreatedTrId = _dbEngineMetaInf.TransactionsInRun[transactionGuid].Id;
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
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            var colPool = table.TableMetaInf.ColumnPool;
            if (columnNames == null)
            {
                columnNames = new List<Id>();
                foreach (var col in table.TableMetaInf.ColumnPool)
                {
                    columnNames.Add(col.Name);
                }
            }
            if (objectParams == null)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new NullError(nameof(objectParams)));
            }

            if (colPool.Count < columnNames.Count)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ColumnTooMachError(table.TableMetaInf.Name.ToString()));
            }
            if (objectParams.Count > table.TableMetaInf.ColumnPool.Count)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ColumnTooMachError(table.TableMetaInf.Name.ToString()));
            }

            foreach (var col in columnNames)
            {
                if (colPool.FindIndex((Column n) => col.ToString() == n.Name.ToString()) < 0)
                {
                    return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistError(col.ToString(), table.TableMetaInf.Name.ToString()));
                }
            }

            if (columnNames.Count != objectParams.Count)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new DataCountNotEqualWithColumnCountInInsert(""));
            }

            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            var row = new Row(new Field[table.TableMetaInf.ColumnPool.Count]);
            for (var i = 0; i < colPool.Count; ++i)
            {
                var index = columnNames.FindIndex((Id n) => colPool[i].Name.ToString() == n.ToString());
                if (index < 0)
                {
                    return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistInInsert(colPool[i].Name.ToString()));
                }
                var exprDict = new Dictionary<string, dynamic>();
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
            _dbEngineMetaInf.ReadCountStat += resInsert.Result.Item1;
            _dbEngineMetaInf.WriteCountStat += resInsert.Result.Item2;
            return new OperationResult<Table>(resInsert.State, table, resInsert.OperationError);
        }

        public OperationResult<Table> AddColumnCommand (Guid transactionGuid, Id tableName, Column column)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            if (!tr.NewTables.ContainsKey(tableName.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(tableName.ToString()));
            }
            var table = tr.NewTables[tableName.ToString()];
            var res = table.AddColumn(column);
            return new OperationResult<Table>(res.State, table, res.OperationError);
        }

        public OperationResult<Table> GetTableCommand (Guid transactionGuid, Id name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            if (tr.TempTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.performed, tr.TempTables[name.ToString()]);
            }
            if (tr.NewTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.performed, tr.NewTables[name.ToString()]);
            }
            if (tr.DroppedTables.ContainsKey(name.ToString()))
            {
                var table = tr.DroppedTables[name.ToString()];
                tr.DroppedTables.Remove(name.ToString());
                return new OperationResult<Table>(ExecutionState.performed, table);
            }
            var state = _dataStorage.LoadTable(name);
            return state.State == ExecutionState.performed && (state.Result.TableMetaInf.CreatedTrId <= tr.PrevVerId || state.Result.TableMetaInf.CreatedTrId <= tr.Id)
                ? new OperationResult<Table>(ExecutionState.performed, state.Result)
                : new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(name.ToString()));
        }
        public OperationResult<Table> DeleteCommand (Guid transactionGuid, Id tableName, ExpressionFunction expression)
        {
            var res = _dataStorage.LoadTable(tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            var table = res.Result;
            OperationResult<Tuple<long, long>> resUpdate = null;
            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            try
            {
                resUpdate = _dataStorage.UpdateAllRow(table.TableMetaInf.Name,
                       (Row r) => expression.CalcFunc(CompileExpressionData(expression.VariablesNames, r, table.TableMetaInf.ColumnPool)) && ChekRowVersion(transactionGuid, r) ? r.SetTrEnd(tr.Id) : null);
            }
            catch (Exception ex)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
            }
            _dbEngineMetaInf.ReadCountStat += resUpdate.Result.Item1;
            _dbEngineMetaInf.WriteCountStat += resUpdate.Result.Item2;
            return new OperationResult<Table>(resUpdate.State, table, resUpdate.OperationError);
        }

        public OperationResult<Table> DropTableCommand (Guid transactionGuid, Id name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var state = _dataStorage.ContainsTable(name);
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];

            if (state.Result == false && !tr.NewTables.ContainsKey(name.ToString()))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(name.ToString()));
            }
            if (state.Result)
            {
                var table = _dataStorage.LoadTable(name);
                table.Result.TableData = null;
                _dataStorage.RemoveTable(name);
                tr.DroppedTables.Add(table.Result.TableMetaInf.Name.ToString(), table.Result);
                return new OperationResult<Table>(ExecutionState.performed, table.Result);
            }
            if (tr.NewTables.ContainsKey(name.ToString()))
            {
                var table = tr.NewTables[name.ToString()];
                tr.NewTables.Remove(name.ToString());
                tr.DroppedTables.Add(table.TableMetaInf.Name.ToString(), table);
                return new OperationResult<Table>(ExecutionState.performed, table);
            }
            return new OperationResult<Table>(ExecutionState.performed, null);
        }
        public OperationResult<Table> UpdateCommand (Guid transactionGuid, Id tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction)
        {
            var res = _dataStorage.LoadTable(tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            var table = res.Result;
            OperationResult<Tuple<long, long>> resUpdate = null;
            res = _dataStorage.LoadTable(tableName);
            table = res.Result;
            var updatedRows = new List<Row>();
            foreach (var row in table.TableData)
            {
                var exprRes = true;
                if (expressionFunction != null)
                {
                    try
                    {
                        exprRes = expressionFunction.CalcFunc(CompileExpressionData(expressionFunction.VariablesNames, row, table.TableMetaInf.ColumnPool)) && ChekRowVersion(transactionGuid, row);
                    }
                    catch (Exception ex)
                    {
                        return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                    }
                }
                else
                {
                    exprRes = ChekRowVersion(transactionGuid, row);
                }
                if (exprRes)
                {
                    foreach (var ass in assigmentList)
                    {
                        var index = table.TableMetaInf.ColumnPool.FindIndex((Column col) => col.Name.ToString() == ass.Id.ToString());
                        if (index >= 0)
                        {
                            try
                            {
                                var val = ass.EpressionFunction.CalcFunc(CompileExpressionData(ass.EpressionFunction.VariablesNames, row, table.TableMetaInf.ColumnPool));
                                switch (row.Fields[index].Type)
                                {
                                    case DataType.INT:
                                        row.Fields[index] = new FieldInt(val);
                                        break;
                                    case DataType.DOUBLE:
                                        row.Fields[index] = new FieldDouble(val);
                                        break;
                                    case DataType.CHAR:
                                        row.Fields[index] = new FieldChar(val, ((FieldChar)(row.Fields[index])).ValueBytes.Length);
                                        break;
                                }

                            }
                            catch (Exception ex)
                            {
                                return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                            }
                        }
                        else
                        {
                            return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistError(ass.Id.ToString(), table.TableMetaInf.Name.ToString()));
                        }
                    }
                    row.TrStart = tr.Id;
                    row.TrEnd = long.MaxValue;
                    updatedRows.Add(row);
                }

            }
            AddChangedTable(transactionGuid, table.TableMetaInf.Name);
            try
            {
                resUpdate = _dataStorage.UpdateAllRow(table.TableMetaInf.Name,
                     (Row r) => (expressionFunction == null ? true : expressionFunction.CalcFunc(CompileExpressionData(expressionFunction.VariablesNames, r, table.TableMetaInf.ColumnPool))) && ChekRowVersion(transactionGuid, r) ? r.SetTrEnd(tr.Id) : null);
            }
            catch (Exception ex)
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
            }
            foreach (var row in updatedRows)
            {
                _dataStorage.InsertRow(tableName, row);
            }
            _dbEngineMetaInf.ReadCountStat += resUpdate.Result.Item1;
            _dbEngineMetaInf.WriteCountStat += resUpdate.Result.Item2;

            return new OperationResult<Table>(resUpdate.State, table, resUpdate.OperationError);

        }


        private static Dictionary<string, dynamic> CompileExpressionData (List<string> variablesNames, Row row, List<Column> colPool)
        {
            var exprDict = new Dictionary<string, dynamic>();

            foreach (var v in variablesNames)
            {
                var index = colPool.FindIndex((Column n) => v == n.Name.ToString());
                if (index >= 0)
                {
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
                else
                {
                    throw new Exception($"Unknown variable {v}");
                }

            }
            return exprDict;
        }

        private bool ChekRowVersion (Guid transactionGuid, Row r)
        {
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            return (r.TrStart <= tr.PrevVerId && r.TrEnd > tr.PrevVerId && r.TrEnd != tr.Id) || r.TrStart == tr.Id;
        }

        public OperationResult<Table> JoinCommand (Guid transactionGuid,
                                           Id leftId,
                                           Id rightId,
                                           JoinKind joinKind,
                                           ExpressionFunction expressionFunction)
        {
            var leftRes = GetTableCommand(transactionGuid, leftId);
            if (leftRes.State == ExecutionState.failed)
            {
                return leftRes;
            }

            var rightRes = GetTableCommand(transactionGuid, rightId);
            if (rightRes.State == ExecutionState.failed)
            {
                return rightRes;
            }
            var leftTable = leftRes.Result;
            var rightTable = rightRes.Result;
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            var resulTableMetaInf = new TableMetaInf
            {
                ColumnPool = new List<Column>(),
                Name = new Id(new List<string>(leftId.SimpleIds))
            };
            resulTableMetaInf.Name.SimpleIds.AddRange(rightId.SimpleIds);
            foreach (var col in leftTable.TableMetaInf.ColumnPool)
            {
                var newColName = new List<string>(leftTable.TableMetaInf.Name.SimpleIds);
                newColName.AddRange(col.Name.SimpleIds);
                var newCol = new Column(new Id(newColName), col.DataType, col.DataParam, new List<string>(col.Constrains), col.TypeState);
                resulTableMetaInf.ColumnPool.Add(newCol);
            }
            foreach (var col in rightTable.TableMetaInf.ColumnPool)
            {
                var newColName = new List<string>(rightTable.TableMetaInf.Name.SimpleIds);
                newColName.AddRange(col.Name.SimpleIds);
                var newCol = new Column(new Id(newColName), col.DataType, col.DataParam, new List<string>(col.Constrains), col.TypeState);
                resulTableMetaInf.ColumnPool.Add(newCol);
            }
            var resultTable = new Table(resulTableMetaInf);
            var resultTableData = new List<Row>();
            {
                using var enumerator1 = leftTable.TableData.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    var leftRow = enumerator1.Current;
                    if (ChekRowVersion(transactionGuid, leftRow))
                    {
                        using var enumerator2 = rightTable.TableData.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            var rightRow = enumerator2.Current;
                            if (ChekRowVersion(transactionGuid, rightRow))
                            {
                                var newRowFields = new Field[leftTable.TableMetaInf.ColumnPool.Count + rightTable.TableMetaInf.ColumnPool.Count];
                                for (var i = 0; i < leftRow.Fields.Length; ++i)
                                {
                                    newRowFields[i] = leftRow.Fields[i];
                                }
                                for (var i = 0; i < rightRow.Fields.Length; ++i)
                                {
                                    newRowFields[i + leftTable.TableMetaInf.ColumnPool.Count] = rightRow.Fields[i];
                                }
                                var exprRes = true;
                                if (expressionFunction != null)
                                {
                                    try
                                    {
                                        exprRes = expressionFunction.CalcFunc(CompileExpressionData(expressionFunction.VariablesNames, new Row(newRowFields), resulTableMetaInf.ColumnPool));
                                    }
                                    catch (Exception ex)
                                    {
                                        return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                                    }
                                }
                                else
                                {
                                    exprRes = true;
                                }
                                if (exprRes)
                                {

                                    resultTableData.Add(new Row(newRowFields));
                                }
                            }
                        }
                        if (enumerator2 is DataStorageRowsInFilesEnumerator)
                        {
                            _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.readCount;
                            _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.writeCount;
                        }

                    }
                }
                if (enumerator1 is DataStorageRowsInFilesEnumerator)
                {
                    _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.readCount;
                    _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.writeCount;
                }
            }
            if (joinKind == JoinKind.Left)
            {
                using var enumerator1 = leftTable.TableData.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    var leftRow = enumerator1.Current;
                    var IsFind = false;
                    if (ChekRowVersion(transactionGuid, leftRow))
                    {
                        using var enumerator2 = rightTable.TableData.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            var rightRow = enumerator2.Current;
                            if (ChekRowVersion(transactionGuid, rightRow))
                            {
                                var newRowFields = new Field[leftTable.TableMetaInf.ColumnPool.Count + rightTable.TableMetaInf.ColumnPool.Count];
                                for (var i = 0; i < leftRow.Fields.Length; ++i)
                                {
                                    newRowFields[i] = leftRow.Fields[i];
                                }
                                for (var i = 0; i < rightRow.Fields.Length; ++i)
                                {
                                    newRowFields[i + leftTable.TableMetaInf.ColumnPool.Count] = rightRow.Fields[i];
                                }
                                var exprRes = true;
                                if (expressionFunction != null)
                                {
                                    try
                                    {
                                        exprRes = expressionFunction.CalcFunc(CompileExpressionData(expressionFunction.VariablesNames, new Row(newRowFields), resulTableMetaInf.ColumnPool));
                                    }
                                    catch (Exception ex)
                                    {
                                        return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                                    }
                                }
                                else
                                {
                                    exprRes = true;
                                }
                                if (exprRes)
                                {
                                    IsFind = true;
                                    break;
                                }
                            }
                        }
                        if (enumerator2 is DataStorageRowsInFilesEnumerator)
                        {
                            _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.readCount;
                            _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.writeCount;
                        }
                    }
                    if (!IsFind)
                    {
                        var newRowFields = new Field[leftTable.TableMetaInf.ColumnPool.Count + rightTable.TableMetaInf.ColumnPool.Count];
                        for (var i = 0; i < leftRow.Fields.Length; ++i)
                        {
                            newRowFields[i] = leftRow.Fields[i];
                        }
                        for (var i = 0; i < rightTable.TableMetaInf.ColumnPool.Count; ++i)
                        {
                            newRowFields[i + leftTable.TableMetaInf.ColumnPool.Count] = null;
                        }
                        resultTableData.Add(new Row(newRowFields));
                    }
                }
                if (enumerator1 is DataStorageRowsInFilesEnumerator)
                {
                    _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.readCount;
                    _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.writeCount;
                }
            }
            if (joinKind == JoinKind.Right)
            {
                using var enumerator1 = rightTable.TableData.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    var rightRow = enumerator1.Current;
                    var IsFind = false;
                    if (ChekRowVersion(transactionGuid, rightRow))
                    {
                        using var enumerator2 = leftTable.TableData.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            var leftRow = enumerator2.Current;
                            if (ChekRowVersion(transactionGuid, rightRow))
                            {
                                var newRowFields = new Field[leftTable.TableMetaInf.ColumnPool.Count + rightTable.TableMetaInf.ColumnPool.Count];
                                for (var i = 0; i < leftRow.Fields.Length; ++i)
                                {
                                    newRowFields[i] = leftRow.Fields[i];
                                }
                                for (var i = 0; i < rightRow.Fields.Length; ++i)
                                {
                                    newRowFields[i + leftTable.TableMetaInf.ColumnPool.Count] = rightRow.Fields[i];
                                }
                                var exprRes = true;
                                if (expressionFunction != null)
                                {
                                    try
                                    {
                                        exprRes = expressionFunction.CalcFunc(CompileExpressionData(expressionFunction.VariablesNames, new Row(newRowFields), resulTableMetaInf.ColumnPool));
                                    }
                                    catch (Exception ex)
                                    {
                                        return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                                    }
                                }
                                else
                                {
                                    exprRes = true;
                                }
                                if (exprRes)
                                {
                                    IsFind = true;
                                    break;
                                }
                            }
                        }
                        if (enumerator2 is DataStorageRowsInFilesEnumerator)
                        {
                            _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.readCount;
                            _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator2))._tManager.writeCount;
                        }
                    }
                    if (!IsFind)
                    {
                        var newRowFields = new Field[leftTable.TableMetaInf.ColumnPool.Count + rightTable.TableMetaInf.ColumnPool.Count];
                        for (var i = 0; i < leftTable.TableMetaInf.ColumnPool.Count; ++i)
                        {
                            newRowFields[i] = null;
                        }
                        for (var i = 0; i < rightTable.TableMetaInf.ColumnPool.Count; ++i)
                        {
                            newRowFields[i + leftTable.TableMetaInf.ColumnPool.Count] = rightRow.Fields[i];
                        }
                        resultTableData.Add(new Row(newRowFields));
                    }
                }
                if (enumerator1 is DataStorageRowsInFilesEnumerator)
                {
                    _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.readCount;
                    _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator1))._tManager.writeCount;
                }
            }

            resultTable.TableData = resultTableData;
            tr.TempTables.Add(resultTable.TableMetaInf.Name.ToString(), resultTable);
            return new OperationResult<Table>(ExecutionState.performed, resultTable);
        }
        public OperationResult<Table> SelectCommand (Guid transactionGuid, Id tableName, TimeSelectorDelegate timeSelector, List<Id> columnNames, ExpressionFunction expression)
        {

            var res = GetTableCommand(transactionGuid, tableName);
            if (res.State == ExecutionState.failed)
            {
                return res;
            }
            var table = res.Result;
            var tr = _dbEngineMetaInf.TransactionsInRun[transactionGuid];
            var resulTableMetaInf = new TableMetaInf
            {
                ColumnPool = new List<Column>(),
                Name = new Id(new List<string>(table.TableMetaInf.Name.SimpleIds))
            };
            resulTableMetaInf.Name.SimpleIds.Add(transactionGuid.ToString());
            if (columnNames == null || columnNames[0].ToString() == "*")
            {
                foreach (var col in table.TableMetaInf.ColumnPool)
                {
                    resulTableMetaInf.ColumnPool.Add(new Column(col));
                }
                resulTableMetaInf.ColumnPool = new List<Column>(table.TableMetaInf.ColumnPool);
            }
            else
            {
                foreach (var colName in columnNames)
                {
                    var index = table.TableMetaInf.ColumnPool.FindIndex((Column c) => c.Name.ToString() == colName.ToString());
                    if (index < 0)
                    {
                        return new OperationResult<Table>(ExecutionState.failed, null, new ColumnNotExistError(colName.ToString(), table.TableMetaInf.Name.ToString()));
                    }
                    resulTableMetaInf.ColumnPool.Add(new Column(table.TableMetaInf.ColumnPool[index]));
                }
            }
            var resultTable = new Table(resulTableMetaInf);
            var resultTableData = new List<Row>();
            using var enumerator = table.TableData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var row = enumerator.Current;

                var exprRes = true;
                if (expression != null)
                {
                    try
                    {
                        exprRes = expression.CalcFunc(CompileExpressionData(expression.VariablesNames, row, table.TableMetaInf.ColumnPool)) && ChekRowVersion(transactionGuid, row);
                    }
                    catch (Exception ex)
                    {
                        return new OperationResult<Table>(ExecutionState.failed, null, new ExpressionCalculateError(ex.Message));
                    }
                }
                else
                {
                    exprRes = ChekRowVersion(transactionGuid, row);
                }
                if (timeSelector != null)
                {
                    exprRes = exprRes && row.TrStart != tr.Id && timeSelector(_dbEngineMetaInf.TransactionsHistory[row.TrStart].timeCommit, row.TrEnd == long.MaxValue ? DateTime.MaxValue : _dbEngineMetaInf.TransactionsHistory[row.TrEnd].timeCommit);
                }
                if (exprRes)
                {
                    var len = resultTable.TableMetaInf.ColumnPool.Count;
                    var newFields = new Field[len];
                    for (var i = 0; i < len; ++i)
                    {
                        var coll = resultTable.TableMetaInf.ColumnPool[i];
                        var index = table.TableMetaInf.ColumnPool.FindIndex((Column c) => c.Name.ToString() == coll.Name.ToString());
                        newFields[i] = row.Fields[index];
                    }
                    resultTableData.Add(new Row(newFields));
                }
            }
            if (enumerator is DataStorageRowsInFilesEnumerator)
            {
                _dbEngineMetaInf.ReadCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator))._tManager.readCount;
                _dbEngineMetaInf.WriteCountStat += ((DataStorageRowsInFilesEnumerator)(enumerator))._tManager.writeCount;
            }
            resultTable.TableData = resultTableData;
            tr.TempTables.Add(resultTable.TableMetaInf.Name.ToString(), resultTable);
            return new OperationResult<Table>(ExecutionState.performed, resultTable);
        }
        public OperationResult<Table> ExceptCommand (Guid transactionGuid, Id leftId, Id rightId) => throw new NotImplementedException();

        public OperationResult<TableMetaInf> GetTableMetaInfCommand (Guid transactionGuid, Id name) => throw new NotImplementedException();

        public OperationResult<Table> IntersectCommand (Guid transactionGuid, Id leftId, Id rightId) => throw new NotImplementedException();


        public OperationResult<Table> ShowTableCommand (Guid transactionGuid, Id tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand (Guid transactionGuid, Id leftId, Id rightId, UnionKind unionKind) => throw new NotImplementedException();

        public OperationResult<Table> DeleteColumnCommand (Guid transactionGuid, Id tableName, Id ColumnName) => throw new NotImplementedException();
    }

    [ZeroFormattable]
    public class DbEngineMetaInf
    {
        [Index(0)] public virtual long LastId { get; set; }
        [Index(1)] public virtual long LastCommitedId { get; set; }
        [Index(2)] public virtual Dictionary<Guid, TransactionTempInfo> TransactionsInRun { get; set; }
        [Index(3)] public virtual Dictionary<long, TransactionTempInfo> TransactionsHistory { get; set; }
        [Index(4)] public virtual long ReadCountStat { get; set; }
        [Index(5)] public virtual long WriteCountStat { get; set; }
        public DbEngineMetaInf ()
        {

        }
        public DbEngineMetaInf (long lastId, long lastCommitedId)
        {
            LastId = lastId;
            LastCommitedId = lastCommitedId;
            TransactionsInRun = new Dictionary<Guid, TransactionTempInfo>();
            TransactionsHistory = new Dictionary<long, TransactionTempInfo>();
            ReadCountStat = 0;
            WriteCountStat = 0;
        }
        public DbEngineMetaInf (DbEngineMetaInf metaInf)
        {
            if (metaInf == null)
            {
                throw new ArgumentNullException(nameof(metaInf));
            }
            if (metaInf.TransactionsHistory != null)
            {
                TransactionsHistory = new Dictionary<long, TransactionTempInfo>(metaInf.TransactionsHistory);
            }
            else
            {
                TransactionsHistory = new Dictionary<long, TransactionTempInfo>();
            }
            LastId = metaInf.LastId;
            LastCommitedId = metaInf.LastCommitedId;
            TransactionsInRun = new Dictionary<Guid, TransactionTempInfo>();
            ReadCountStat = metaInf.ReadCountStat;
            WriteCountStat = metaInf.WriteCountStat;
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
        public virtual Dictionary<string, Id> ChangedTables { get; set; }
        [Index(4)]
        public virtual DateTime timeCommit { get; set; }

        [IgnoreFormat]
        public Dictionary<string, Table> TempTables { get; }
        [IgnoreFormat]
        public Dictionary<string, Table> DroppedTables { get; }
        [IgnoreFormat]
        public Dictionary<string, Table> NewTables { get; }

        public TransactionTempInfo ()
        {
            TempTables = new Dictionary<string, Table>();
            NewTables = new Dictionary<string, Table>();
            DroppedTables = new Dictionary<string, Table>();
        }

        public TransactionTempInfo (long id, long prevVerId)
        {
            Id = id;
            PrevVerId = prevVerId;
            TempTables = new Dictionary<string, Table>();
            NewTables = new Dictionary<string, Table>();
            ChangedTables = new Dictionary<string, Id>();
            DroppedTables = new Dictionary<string, Table>();
        }
    }

}

//public enum TransactionState
//{
//    COMITED,
//    RUNNING,
//    FAILED
//}

