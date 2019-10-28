using System.Collections.Generic;
using System.IO;

using DataBaseEngine;

using DataBaseTable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StorageEngine;

using ZeroFormatter;

namespace DataBaseEngineUnitTest
{
    [TestClass]
    public class StorageEngineUnitTests
    {
        DataStorageInFiles dataStorage;
        const int blockSize = 4096;
        [TestInitialize]
        public void TestInitialize()
        {


        }

        [TestCleanup]
        public void TestCleanup()
        {

        }
        [TestMethod]
        public void AddTableTest()
        {
            const string testPath = "StorageTestAddTableTest";
            if (Directory.Exists(testPath))
            {
                Directory.Delete(testPath, true);
            }
            dataStorage = new DataStorageInFiles(testPath, blockSize);


            var tableName = "Table1";
            var columns = new Dictionary<string, Column> {
                { "Name",new Column("Name", ColumnDataType.CHAR, 25, new List<string>(), NullSpecOpt.Null) },
                { "Soname",new Column("Soname", ColumnDataType.CHAR, 30, new List<string>(), NullSpecOpt.Null) },
                { "Age",new Column("Age", ColumnDataType.INT, 0, new List<string>(), NullSpecOpt.Null) },
                { "Rating",new Column("Rating", ColumnDataType.DOUBLE, 0, new List<string>(), NullSpecOpt.Null) },
                };
            var table = new Table(new TableMetaInf(tableName) { ColumnPool = columns });
            var result = dataStorage.AddTable(table);
            Assert.AreEqual(result.State, OperationExecutionState.performed);

            var resultCont = dataStorage.ContainsTable(table.TableMetaInf.Name);
            Assert.AreEqual(resultCont.State, OperationExecutionState.performed);
            Assert.AreEqual(resultCont.Result, true);

            resultCont = dataStorage.ContainsTable("radomTable");
            Assert.AreEqual(resultCont.State, OperationExecutionState.failed);
            Assert.AreEqual(resultCont.Result, false);

            var resultTable = dataStorage.LoadTable(table.TableMetaInf.Name);
            Assert.AreEqual(resultTable.State, OperationExecutionState.performed);
            Assert.AreEqual(resultTable.Result.TableMetaInf.Name, table.TableMetaInf.Name);
            foreach (var col in resultTable.Result.TableMetaInf.ColumnPool)
            {
                Assert.AreEqual(columns[col.Key].Name, col.Value.Name);
                Assert.AreEqual(columns[col.Key].DataType, col.Value.DataType);
                Assert.AreEqual(columns[col.Key].DataParam, col.Value.DataParam);
            }
        }
        [TestMethod]
        public void InsertRowsTest()
        {
            const string testPath = "StorageTestInsertRowsTest";
            if (Directory.Exists(testPath))
            {
                Directory.Delete(testPath, true);
            }
            dataStorage = new DataStorageInFiles(testPath, blockSize);

            var tableName = "Table1";
            var columns = new Dictionary<string, Column> {
                { "Name",new Column("Name", ColumnDataType.CHAR, 25, new List<string>(), NullSpecOpt.Null) },
                { "Soname",new Column("Soname", ColumnDataType.CHAR, 15, new List<string>(), NullSpecOpt.Null) },
                { "Age",new Column("Age", ColumnDataType.INT, 0, new List<string>(), NullSpecOpt.Null) },
                { "Rating",new Column("Rating", ColumnDataType.DOUBLE, 0, new List<string>(), NullSpecOpt.Null) },
                };
            var table = new Table(new TableMetaInf(tableName) { ColumnPool = columns });
            var result = dataStorage.AddTable(table);
            Assert.AreEqual(result.State, OperationExecutionState.performed);

            var row1 = table.CreateDefaultRow();
            Assert.AreEqual(row1.State, OperationExecutionState.performed);
            dataStorage.InsertRow(tableName, row1.Result);

            var row2 = table.CreateRowFormStr(new string[] { "Ivan", "IvanovIvanovIvanov", "23", "44.345" });
            Assert.AreEqual(row2.State, OperationExecutionState.performed);
            dataStorage.InsertRow(tableName, row2.Result);


            var resultTable = dataStorage.LoadTable(table.TableMetaInf.Name);
            Assert.AreEqual(resultTable.State, OperationExecutionState.performed);

            var rowEnumerator = resultTable.Result.TableData.GetEnumerator();


            CheckRow(rowEnumerator.Current, row1.Result);
            rowEnumerator.MoveNext();
            CheckRow(rowEnumerator.Current, row2.Result);
        }

        void CheckRow(Field[] a,Field[] b)
        {
            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < b.Length; i++)
            {
                switch (b[i].Type)
                {
                    case ColumnDataType.CHAR: Assert.AreEqual(((FieldChar)(a[i])).Value, ((FieldChar)(b[i])).Value); break;
                    case ColumnDataType.INT: Assert.AreEqual(((FieldInt)(a[i])).Value, ((FieldInt)(b[i])).Value); break;
                    case ColumnDataType.DOUBLE: Assert.AreEqual(((FieldDouble)(a[i])).Value, ((FieldDouble)(b[i])).Value); break;
                }

            }
        }

    }
}

