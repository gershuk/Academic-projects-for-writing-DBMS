using System;
using System.Diagnostics;
using System.Linq;
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
    class MultyThreadTest : BaseSQLTest
    {
        public MultyThreadTest (bool fixtests) : base(fixtests)
        {
        }

        public MultyThreadTest () : base(false)
        {
        }

        [TestMethod]
        public void InsertTest ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var n = 2;
            var cl = Enumerable.Range(0, n).Select(i=>new TestClient($"cl{i}", _core)).ToArray();
            cl[1].SendQuery("create table test (i int,c char(40),d double)");

            var query = "";
            for (var i = 0; i < 10; i++)
            {
                query += $"insert into test values ({i},'{i}',{i * 1.0 + 0.1.ToString().Replace(',', '.')})\n";
            }
            Console.WriteLine("insert start");
            _core.ExecuteSqlSequence(query);
            Console.WriteLine("insert end");
           var tasks = Enumerable.Range(0, n).Select(
                i => Task.Run(() => SendSQLQuery(cl[i], $"BEGIN TRANSACTION cl2\n" +
                $"update test set i = -1 where i = {i};" +
                "select * from test;" +
                $"commit",
                expected))).ToArray();
            Task.WaitAll(tasks);


        
                }

    }
}
