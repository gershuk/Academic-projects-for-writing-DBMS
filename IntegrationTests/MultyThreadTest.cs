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
            for(var i = 0; i<100; i++)
            {
                query += $"insert into test values ({i},'{i}',{i * 1.0 + 0.1.ToString().Replace(',', '.')})\n";
            }
            Console.WriteLine("isert start");
            _core.ExecuteSqlSequence(query);
            Console.WriteLine("isert end");

            SendSQLQuery(cl1,"select * from test",expected);
            var t1 = Task.Run(()=>SendSQLQuery(cl1, $"BEGIN TRANSACTION cl1\n" +
                $"update test set i = 0 where c='1';" +
                "select * from test;"+
                $"commit",
                expected));
            t1.Wait();

            {
                var t2 = Task.Run(() => SendSQLQuery(cl2, $"BEGIN TRANSACTION cl2\n" +
                    $"update test set i = 1 where c='2';" +
                    "select * from test;" +
                    $"commit",
                    expected));
                t2.Wait();

            }
            
        }

    }
}
