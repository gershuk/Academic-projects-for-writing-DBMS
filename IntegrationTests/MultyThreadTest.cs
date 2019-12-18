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
            var cl1 = new TestClient("cl1", _core);
            var cl2 = new TestClient("cl2", _core);

            cl1.SendQuery("create table test (i int,c char(40),d double)");

            var query = "";
            for(var i = 0; i<1000; i++)
            {
                query += $"insert into test values ({i},'{i}',{i * 1.0 + 0.1.ToString().Replace(',', '.')})\n";
            }
            Console.WriteLine(query+"\n");
            Console.WriteLine(cl1.SendQuery(query));

            var t1 = Task.Run(()=>SendSQLQuery(cl1, $"BEGIN TRANSACTION cl1\n" +
                $"update test set i = 0 where c=\"1\"" +
                $"commit",
                expected));
            var t2 = Task.Run(()=>SendSQLQuery(cl2, $"BEGIN TRANSACTION cl2\n" +
                $"update test set i = 1 where c=\"2\"" +
                $"commit",
                expected));
            t1.Wait();
            t2.Wait();

        }

    }
}
