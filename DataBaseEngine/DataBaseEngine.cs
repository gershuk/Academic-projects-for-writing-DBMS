using System;
using System.Collections.Generic;
using DataBaseTable;

namespace DataBaseEngine
{
    public enum OperationExecutionState
    {
        failed,
        performed
    }

    public struct OperationResult<T>
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
        OperationExecutionState CreateTable(string name);
        OperationExecutionState CreateTable(string name,TableMetaInf metaInf);

        OperationExecutionState DeleteColumnFromTable(string id);

        OperationExecutionState AddColumnToTable<T>(T columnName);

        OperationResult<TableData> GetTableData(string name);
        OperationResult<TableMetaInf> GetTableMetaInf(string name);
    }

    public interface IDataBaseEngineDuty
    {
        OperationExecutionState LoadTablePool(string path);
        OperationExecutionState SaveTablePool(string path);
    }

    public class SimpleDataBaseEngine : IDataBaseEngineDuty, IDataBaseEngineFunction
    {
        public Dictionary<string, Table> TablePool { get; set; }

        public OperationExecutionState AddColumnToTable<T>(T columnName) => throw new NotImplementedException();
        public OperationExecutionState CreateTable(string name) => throw new NotImplementedException();
        public OperationExecutionState CreateTable(string name, TableMetaInf metaInf) => throw new NotImplementedException();
        public OperationExecutionState DeleteColumnFromTable(string id) => throw new NotImplementedException();
        public OperationResult<TableData> GetTableData(string name) => throw new NotImplementedException();
        public OperationResult<TableMetaInf> GetTableMetaInf(string name) => throw new NotImplementedException();
        public OperationExecutionState LoadTablePool(string path) => throw new NotImplementedException();
        public OperationExecutionState SaveTablePool(string path) => throw new NotImplementedException();
    }
}
