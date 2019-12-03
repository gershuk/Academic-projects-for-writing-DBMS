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
        OperationResult<Table> CreateTableCommand(List<string> name);

        OperationResult<Table> DeleteColumnCommand(List<string> tableName, string ColumnName);

        OperationResult<Table> AddColumnCommand(List<string> tableName, Column column);

        OperationResult<Table> GetTableCommand(List<string> name);

        OperationResult<TableMetaInf> GetTableMetaInfCommand(List<string> name);

        OperationResult<Table> DropTableCommand(List<string> name);

        OperationResult<Table> InsertCommand(List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams);

        OperationResult<Table> SelectCommand(List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression);

        OperationResult<Table> UpdateCommand(List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction);

        OperationResult<Table> DeleteCommand(List<string> tableName, ExpressionFunction expression);

        OperationResult<Table> ShowTableCommand(List<string> tableName);

        OperationResult<Table> JoinCommand(List<string> leftId,
                                                List<string> rightId,
                                                JoinKind joinKind,
                                                List<string> statmentLeftId,
                                                List<string> statmentRightId);

        OperationResult<Table> UnionCommand(List<string> leftId, List<string> rightId, UnionKind unionKind);

        OperationResult<Table> IntersectCommand(List<string> leftId, List<string> rightId);

        OperationResult<Table> ExceptCommand(List<string> leftId, List<string> rightId);

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
        public OperationResult<Table> CreateTableCommand(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> DeleteCommand(List<string> tableName, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> DeleteColumnCommand(List<string> tableName, string ColumnName) => throw new NotImplementedException();
        public OperationResult<Table> DropTableCommand(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> ExceptCommand(List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> GetTableCommand(List<string> name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInfCommand(List<string> name) => throw new NotImplementedException();
        public OperationResult<Table> InsertCommand(List<string> tableName, List<List<string>> columnNames, List<ExpressionFunction> objectParams) => throw new NotImplementedException();
        public OperationResult<Table> IntersectCommand(List<string> leftId, List<string> rightId) => throw new NotImplementedException();
        public OperationResult<Table> JoinCommand(List<string> leftId, List<string> rightId, JoinKind joinKind, List<string> statmentLeftId, List<string> statmentRightId) => throw new NotImplementedException();
        public void RollBackTransaction(Guid transactionGuid) => throw new NotImplementedException();
        public OperationResult<Table> SelectCommand(List<string> tableName, List<List<string>> columnNames, ExpressionFunction expression) => throw new NotImplementedException();
        public OperationResult<Table> ShowTableCommand(List<string> tableName) => throw new NotImplementedException();
        public OperationResult<Table> UnionCommand(List<string> leftId, List<string> rightId, UnionKind unionKind) => throw new NotImplementedException();
        public OperationResult<Table> UpdateCommand(List<string> tableName, List<Assigment> assigmentList, ExpressionFunction expressionFunction) => throw new NotImplementedException();
    }
}
