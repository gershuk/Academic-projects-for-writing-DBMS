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
            var expected = GetTestData();
            var s1 = new TestApServer();
            s1.SendQuery("create table test (i int)");
            for(var i = 0; i<100; i++)
            {
                var res = s1.SendQuery($"insert into test values ({i})");
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
            var s2 = new TestApServer();
            var cl2 = new TestApClient("cl2");
            SendSQLQuery(cl2, "select * from test", expected);
        }

        [TestMethod]
        public void InsertDurability ()
        {

        }

        [TestMethod]
        public void UpdateDurability ()
        {

        }

    }
}
