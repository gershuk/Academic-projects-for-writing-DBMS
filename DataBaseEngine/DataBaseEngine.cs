using System;
using System.Collections.Generic;
using System.Text;

using StorageEngine;
using DataBaseType;
namespace DataBaseEngine
{
    public class EngineConfig
    {
        public string Path { get; set; }
    }

    public interface IDataBaseEngine
    {
        OperationResult<Table> CreateTable(List<string> name);

        OperationResult<Table> DeleteColumnFromTable(List<string> tableName, string ColumnName);

        OperationResult<Table> AddColumnToTable(List<string> tableName, Column column);


        OperationResult<Table> GetTable(List<string> name);

        OperationResult<TableMetaInf> GetTableMetaInf(List<string> name);

        OperationResult<Table> DropTable(List<string> name);

        OperationResult<Table> Insert(List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams);

        OperationResult<Table> Select(List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression);

        OperationResult<Table> Update(List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction);

        OperationResult<Table> Delete(List<string> tableName, ExpressionFunction expression);

        OperationResult<Table> ShowTable(List<string> tableName);

        OperationResult<Table> Join(List<string> leftId,
                                                List<string> rightId,
                                                JoinKind joinKind,
                                                List<string> statmentLeftId,
                                                List<string> statmentRightId);

        OperationResult<Table> Union(List<string> leftId, List<string> rightId, UnionKind unionKind);

        OperationResult<Table> Intersect(List<string> leftId, List<string> rightId);

        OperationResult<Table> Except(List<string> leftId, List<string> rightId);

        void RollBackTransaction(Guid transactionGuid);

        void CommitTransaction(Guid transactionGuid);
    }

    public class DataBaseEngineMain : IDataBaseEngine
    {
        private const string _pathDefault = "DataBaseStorage";
        private const int _blockSizeDefault = 4096;
        private IDataStorage _dataStorage;
        public DataBaseEngineMain()
        {
            _dataStorage = new DataStorageInFiles(_pathDefault, _blockSizeDefault);
        }
        public DataBaseEngineMain(string pathDataBaseStorage, int blockSize = _blockSizeDefault)
        {
            _dataStorage = new DataStorageInFiles(pathDataBaseStorage, blockSize);
        }

        public OperationResult<Table> AddColumnToTable(List<string> tableName, Column column) => throw new NotImplementedException();
        public void CommitTransaction(Guid transactionGuid) => throw new NotImplementedException();
        public OperationResult<Table> CreateTable(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> Delete(List<string> tableName, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> DeleteColumnFromTable(List<string> tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<Table> DropTable(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> Except(List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> GetTable(List<string> name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInf(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> Insert(List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams) => throw new NotImplementedException();
        public OperationResult<Table> Intersect(List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> Join(List<string> leftId, List<string> rightId, JoinKind joinKind, List<string> statmentLeftId, List<string> statmentRightId) => throw new NotImplementedException();
        public void RollBackTransaction(Guid transactionGuid) => throw new NotImplementedException();
        public OperationResult<Table> Select(List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTable(List<string> tableName) => throw new NotImplementedException();
        public OperationResult<Table> Union(List<string> leftId, List<string> rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> Update(List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
    }
}
