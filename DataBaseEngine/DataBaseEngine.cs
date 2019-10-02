using System;
using System.Collections.Generic;
using DataBaseTable;

namespace DataBaseEngine
{
    public enum DataBaseTypes
    {
        BIT,
        DATE,
        TIME,
        TIMESTAMP,
        DECIMAL,
        REAL,
        FLOAT,
        SMALLINT,
        INTEGER,
        INTERVAL,
        CHARACTER,
        DATETIME,
        INT,
        DOUBLE,
        CHAR,
        NCHAR,
        VARCHAR,
        NVARCHAR,
        IMAGE,
        TEXT,
        NTEXT
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
        public T Result { get; set; }

        public OperationResult(OperationExecutionState state, T result)
        {
            State = state;
            Result = result;
        }
    }

    public interface IDataBaseEngineFunction
    {
        OperationResult<string> CreateTable(string name);
        OperationResult<string> CreateTable(string name, TableMetaInf metaInf);

        OperationResult<string> DeleteColumnFromTable(string id);

        OperationResult<string> AddColumnToTable(string tableName, string columnName, DataBaseTypes type, string typeParams,params string[] constraint);

        OperationResult<string> GetTableData(string name);
        OperationResult<string> GetTableMetaInf(string name);

        OperationResult<string> DeleteTable(string name);
    }

    public interface IDataBaseEngineDuty
    {
        OperationExecutionState LoadTablePool(string path);
        OperationExecutionState SaveTablePool(string path);
    }

    public class SimpleDataBaseEngine : IDataBaseEngineDuty, IDataBaseEngineFunction
    {
        public OperationResult<string> AddColumnToTable(string tableName, string columnName, DataBaseTypes type, string typeParams, params string[] constraint) { return new OperationResult<String>(OperationExecutionState.performed, ""); }
        public OperationResult<string> CreateTable(string name) { return new OperationResult<String>(OperationExecutionState.performed, ""); }
        public OperationResult<string> CreateTable(string name, TableMetaInf metaInf) => throw new NotImplementedException();
        public OperationResult<string> DeleteColumnFromTable(string id) => throw new NotImplementedException();
        public OperationResult<string> DeleteTable(string name) => throw new NotImplementedException();
        public OperationResult<string> GetTableData(string name) => throw new NotImplementedException();
        public OperationResult<string> GetTableMetaInf(string name) => throw new NotImplementedException();
        public OperationExecutionState LoadTablePool(string path) => throw new NotImplementedException();
        public OperationExecutionState SaveTablePool(string path) => throw new NotImplementedException();
    }
}
