using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataBaseEngine;
using DataBaseTable;
using System.Collections.Generic;

namespace DataBaseEngineUnitTest
{
    [TestClass]
    public class DataBaseUnitTests
    {
        DataBaseEngineMain dataBase;
        [TestInitialize]
        public void TestInitialize()
        {
            dataBase = new DataBaseEngineMain();
        }
        [TestMethod]
        public void AddTableOnlyNameTest()
        {
            var tableName = "Table1";
            var result  = dataBase.CreateTable(tableName);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.CreateTable(tableName);
            Assert.AreEqual(result, OperationExecutionState.failed);
        }
        [TestMethod]
        public void AddTableWithMetaInfOnlyNameTest()
        {
            var tableName = "Table1";
            var tableMetaInf = new TableMetaInf(tableName);
            var result = dataBase.CreateTable(tableMetaInf);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.CreateTable(tableName);
            Assert.AreEqual(result, OperationExecutionState.failed);
        }
        [TestMethod]
        public void SaveLoadTablePoolTest()
        {
            var tableName = "Table1";
            var tableName2 = "Table2";
            var column = new Column("age", ColumnDataType.CHAR,0,new List<string>());
            var path = "SaveLoadTablePoolTest.db";
            var tableMetaInf = new TableMetaInf(tableName);
            tableMetaInf.AddColumn(column);
            var result = dataBase.CreateTable(tableMetaInf);
            Assert.AreEqual(result, OperationExecutionState.performed);
            result = dataBase.CreateTable(new TableMetaInf(tableName2));
            Assert.AreEqual(result, OperationExecutionState.performed);
            result =  dataBase.SaveTablePool(path);
            Assert.AreEqual(result, OperationExecutionState.performed);
            result = dataBase.LoadTablePool(path);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName2), true);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(column.Name), true);
        }
        [TestMethod]
        public void AddDeleteColumnTest()
        {
            var tableName = "Table1";
            var columnName = "age";
            var columnType = ColumnDataType.FLOAT;
            var column = new Column(columnName, columnType,0, new List<string>());
            var result = dataBase.CreateTable(tableName);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.AddColumnToTable(tableName, column);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(columnName),true);
            result = dataBase.DeleteColumnFromTable(tableName, column.Name);
            Assert.AreEqual(result, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(columnName), false);
        }
    }
}
