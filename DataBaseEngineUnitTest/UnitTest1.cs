using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataBaseEngine;
using DataBaseTable;
using System.Collections.Generic;
using System.IO;
namespace DataBaseEngineUnitTest
{
    [TestClass]
    public class DataBaseUnitTests
    {
        DataBaseEngineMain dataBase;
        const string testConfigPath = "TestConfig.json";
        [TestInitialize]
        public void TestInitialize()
        {
            
            dataBase = new DataBaseEngineMain(testConfigPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(dataBase.EngineConfig.Path)) { 
                File.Delete(dataBase.EngineConfig.Path);
            }
        }

        [TestMethod]
        public void AddTableOnlyNameTest()
        {
            var tableName = "Table1";
            var result  = dataBase.CreateTable(tableName);
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.CreateTable(tableName);
            Assert.AreEqual(result.State, OperationExecutionState.failed);
        }

        [TestMethod]
        public void AddTableWithMetaInfOnlyNameTest()
        {
            var tableName = "Table1";
            var tableMetaInf = new TableMetaInf(tableName);
            var result = dataBase.CreateTable(tableMetaInf);
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.CreateTable(tableName);
            Assert.AreEqual(result.State, OperationExecutionState.failed);
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
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result = dataBase.CreateTable(new TableMetaInf(tableName2));
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result =  dataBase.Commit();
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result = dataBase.LoadTablePool();
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName2), true);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(column.Name), true);
        }

        [TestMethod]
        public void CommitTest()
        {
            var tableName = "Table1";
            var tableName2 = "Table2";
            var column = new Column("age", ColumnDataType.CHAR, 0, new List<string>());
            var path = "SaveLoadTablePoolTest.db";
            var tableMetaInf = new TableMetaInf(tableName);
            tableMetaInf.AddColumn(column);
            var result = dataBase.CreateTable(tableMetaInf);
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result = dataBase.CreateTable(new TableMetaInf(tableName2));
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result = dataBase.Commit();
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            result = dataBase.LoadTablePool();
            Assert.AreEqual(result.State, OperationExecutionState.performed);
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
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool.ContainsKey(tableName), true);
            result = dataBase.AddColumnToTable(tableName, column);
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(columnName),true);
            result = dataBase.DeleteColumnFromTable(tableName, column.Name);
            Assert.AreEqual(result.State, OperationExecutionState.performed);
            Assert.AreEqual(dataBase.TablePool[tableName].TableMetaInf.ColumnPool.ContainsKey(columnName), false);
        }


    }
}
