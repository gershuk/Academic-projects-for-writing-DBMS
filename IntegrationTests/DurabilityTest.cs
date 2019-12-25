using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DataBaseEngine;
using IntegrationTests.TestApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunflowerDB;
using TransactionManagement;

namespace IntegrationTests
{
    [TestClass]
    class DurabilityTest : BaseSQLTest
    {
        public DurabilityTest (bool fixtests) : base(fixtests)
        {
        }

        public DurabilityTest () : base(false)
        {
        }

        [TestMethod]
        public void DeleteDurability ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var s1 = new TestApServer("s1", _core);
            var cl1 = new TestClient("cl1", _core);
            cl1.SendQuery("create table test (i int)");
            for(var i = 0; i<100; i++)
            {
                var res = cl1.SendQuery($"insert into test values ({i})");
            }
            var comand = "";
            for (var i = 0; i < 100; i++)
            {
                comand+=($"delete from test where i = ({i});\n");
            }
            var transaction = "BEGIN TRANSACTION cl1\n" +
                comand +
                $"commit";

            var t = Task.Run(() => { s1.SendQuery(transaction); });

            System.Threading.Thread.Sleep(100);
            s1.Kill();
            System.Threading.Thread.Sleep(100);

            var s2 = new TestApServer("cl2", _core);
            var cl2 = new TestClient("cl1", _core);

            SendSQLQuery(cl2, "select * from test", expected);
            s2.Kill();
        }

        [TestMethod]
        public void InsertDurability ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var s1 = new TestApServer("s1", _core);
            var cl1 = new TestClient("cl1", _core);
            cl1.SendQuery("create table test (i int)");
            for (var i = 0; i < 100; i++)
            {
                var res = cl1.SendQuery($"insert into test values ({i})");
            }
            var comand = "";
            for (var i = 0; i < 100; i++)
            {
                comand += ($"insert into test values ({i});\n");
            }
            var transaction = "BEGIN TRANSACTION cl1\n" +
                comand +
                $"commit";

            var t = Task.Run(() => { s1.SendQuery(transaction); });

            System.Threading.Thread.Sleep(100);
            s1.Kill();
            System.Threading.Thread.Sleep(100);

            var s2 = new TestApServer("cl2", _core);
            var cl2 = new TestClient("cl1", _core);

            SendSQLQuery(cl2, "select * from test", expected);
            s2.Kill();
        }

        [TestMethod]
        public void UpdateDurability ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var s1 = new TestApServer("s1", _core);
            var cl1 = new TestClient("cl1", _core);
            cl1.SendQuery("create table test (i int)");
            for (var i = 0; i < 100; i++)
            {
                var res = cl1.SendQuery($"insert into test values ({i})");
            }
            var comand = "";
            for (var i = 0; i < 100; i++)
            {
                comand += ($"update test set i = 0 where i = ({i});\n");
            }
            var transaction = "BEGIN TRANSACTION cl1\n" +
                comand +
                $"commit";

            var t = Task.Run(() => { s1.SendQuery(transaction); });

            System.Threading.Thread.Sleep(100);
            s1.Kill();
            System.Threading.Thread.Sleep(100);

            var s2 = new TestApServer("cl2", _core);
            var cl2 = new TestClient("cl1", _core);

            SendSQLQuery(cl2, "select * from test", expected);
            s2.Kill();
        }

    }
}
