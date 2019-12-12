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
    class Test_Errors : BaseSQLTest
    {
        public Test_Errors (bool fixtests) : base(fixtests)
        {
        }

        public Test_Errors () : base(false)
        {
        }

        [TestMethod]
        public void MainTest ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var cl1 = new TestClient("cl1", _core);

            SendSQLQuery(cl1, $"CREATE TABLE i(id INT);", expected);
            SendSQLQuery(cl1, $"CREATE TABLE i(id INT);", expected);

            SendSQLQuery(cl1, $"CREATE TABLE asa(id INT UNIQUE UNIQUE, id1 DOUBLE NOT NULL);", expected);

            SendSQLQuery(cl1, $"CREATE TABLE ac(id INT UNIQUE, id DOUBLE NOT NULL); ", expected);

            SendSQLQuery(cl1, $"insert into qqq values(1, 2);", expected);

            SendSQLQuery(cl1, $"CREATE TABLE ac(id INT UNIQUE, di DOUBLE NOT NULL);", expected);
            SendSQLQuery(cl1, $"insert into ac(id) values(1, 2);", expected);


            SendSQLQuery(cl1, $"CREATE TABLE qw(id INT UNIQUE, id1 DOUBLE);", expected);
            SendSQLQuery(cl1, $"insert into qw(id, id) values (1, 3);", expected);

            SendSQLQuery(cl1, $"CREATE TABLE j(id INT);", expected);
            SendSQLQuery(cl1, $"insert into j values(159753159753);", expected);


            SendSQLQuery(cl1, $"CREATE TABLE qh(id CHAR(1));", expected);
            SendSQLQuery(cl1, $"insert into qh values ('');", expected);
            SendSQLQuery(cl1, $"insert into qh values ('a');", expected);
            SendSQLQuery(cl1, $"insert into qh values ('aa');", expected);


            SendSQLQuery(cl1, $"CREATE TABLE fn(id INT NOT NULL , age DOUBLE, name char(150));", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values(1, 2.9);", expected);



            SendSQLQuery(cl1, $"CREATE TABLE fn(age DOUBLE);", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values('dfgdg');", expected);

            SendSQLQuery(cl1, $"CREATE TABLE qg(id int unique);", expected);
            SendSQLQuery(cl1, $"insert into qg values (1);", expected);
            SendSQLQuery(cl1, $"insert into qg values (2);", expected);
            SendSQLQuery(cl1, $"update qg set id = 2 where id = 1;", expected);


            SendSQLQuery(cl1, $"CREATE TABLE qg(id int unique);", expected);
            SendSQLQuery(cl1, $"insert into qg values (1);", expected);
            SendSQLQuery(cl1, $"insert into qg values (2);", expected);
            SendSQLQuery(cl1, $"update qg set id = 3;", expected);

            SendSQLQuery(cl1, $"select * from( t join tt);", expected);
            SendSQLQuery(cl1, $"select * from t jon tt on t.id = tt.id;", expected);
            SendSQLQuery(cl1, $"select * from t as join tt on t.id = tt.id;", expected);
            SendSQLQuery(cl1, $"select * from t a t1 join tt on t.id = tt.id;", expected);
            SendSQLQuery(cl1, $"select * from t as t1 join tt t.id = tt.id;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);






            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);



        }
    }
}
