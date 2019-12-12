using System;
using System.Diagnostics;
using System.Text;
using DataBaseEngine;
using IntegrationTests.TestApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunflowerDB;
using TransactionManagement;

namespace IntegrationTests
{
    [TestClass]
    public class Create_ShowCreate : BaseSQLTest
    {
        
        public Create_ShowCreate (bool fixtests) : base(fixtests)
        {
        }

        public Create_ShowCreate () : base(false)
        {
        }

        [TestMethod]
        public void TestTest ()
        {
            var results = GetTestData();
            var client1 = new TestClient("test");
            var test = results.GetResult(client1);
            Assert.AreEqual("test1", test);
            results.FixResult("test1", client1);
            results.Next(client1);
            test = results.GetResult(client1);
            Assert.AreEqual("test2", test);
            results.FixResult("test2", client1);
            results.Save();
        }

        [TestMethod]
        public void TestCreateCommandSynax ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var cl1 = new TestClient("cl1", _core);

            {
                var _tail = "";
                for (int i = 0; i < 1000; i++)
                {
                    SendSQLQuery(cl1, $"CREATE TABLE UnitMeasure{i}(Name CHAR({i}), UnitMeasureCode CHAR({i}), ModifiedDate INT{_tail});", expected);
                    _tail += $", Name{i} CHAR({i}), UnitMeasureCode{i} CHAR({i})";
                }
            }
        }
    }
}
