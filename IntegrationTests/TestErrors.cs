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


            SendSQLQuery(cl1, $"CREATE TABLE fn1(id INT NOT NULL , age DOUBLE, name char(150));", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn1 values(1, 2.9);", expected);



            SendSQLQuery(cl1, $"CREATE TABLE fn2(age DOUBLE);", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn2 values('dfgdg');", expected);

            SendSQLQuery(cl1, $"CREATE TABLE qg1(id int unique);", expected);
            SendSQLQuery(cl1, $"insert into qg1 values (1);", expected);
            SendSQLQuery(cl1, $"insert into qg1 values (2);", expected);
            SendSQLQuery(cl1, $"update qg1 set id = 2 where id = 1;", expected);


            SendSQLQuery(cl1, $"CREATE TABLE qg2(id int unique);", expected);
            SendSQLQuery(cl1, $"insert into qg2 values (1);", expected);
            SendSQLQuery(cl1, $"insert into qg2 values (2);", expected);
            SendSQLQuery(cl1, $"update qg2 set id = 3;", expected);

            SendSQLQuery(cl1, $"CREATE TABLE qg(id int unique, name char(50));", expected);
            SendSQLQuery(cl1, $"insert into qg values (1, 'qwe');", expected);
            SendSQLQuery(cl1, $"insert into qg values (2, 'ewq');", expected);
            SendSQLQuery(cl1, $"select * from qg where id = name + 1;", expected);

            SendSQLQuery(cl1, $"CREATE TABLE fn(id INT NOT NULL , age double, name char(150));", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values(1, 2.9, 'sfsf');", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values(3, 3.789, 'qwerty');", expected);
            SendSQLQuery(cl1, $"SELECT * from (fn join nf on fn.id = nf.id);", expected);
            SendSQLQuery(cl1, $"SELECT * from fn ;", expected);
            SendSQLQuery(cl1, $"SELECT * from qg2;", expected);
            SendSQLQuery(cl1, $"SELECT * from (fn join qg2 on fn.id = qg2.id);", expected);


            SendSQLQuery(cl1, $"select * from( t join tt);", expected);
            SendSQLQuery(cl1, $"select * from (t jon tt on t.id = tt.id);", expected);
            SendSQLQuery(cl1, $"select * from (t as join tt on t.id = tt.id);", expected);
            SendSQLQuery(cl1, $"select * from (t a t1 join tt on t.id = tt.id);", expected);
            SendSQLQuery(cl1, $"select * from (t as t1 join tt t.id = tt.id);", expected);
            SendSQLQuery(cl1, $"select * from (t unon select * from t1);", expected);

            SendSQLQuery(cl1, $"INSERT INTO fn values(1, 2.9, 'sfsf');", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values(2, 3.789, 'qwerty');", expected);
            SendSQLQuery(cl1, $"SELECT * from (fn join qg on fn.id = fn.id11);", expected);
            SendSQLQuery(cl1, $"SELECT * from (SELECT * from qg);", expected);
            SendSQLQuery(cl1, $"drop table fn", expected);

            SendSQLQuery(cl1, $"CREATE TABLE fn(id INT NOT NULL , age double, name char(150));", expected);
            SendSQLQuery(cl1, $"INSERT INTO fn values(1, 2.9, 'sfsf');", expected);
            SendSQLQuery(cl1, $"SELECT dgdfg, id from fn ;", expected);

            SendSQLQuery(cl1, $"CREATE TABLE table1(id INT NOT NULL, age DOUBLE, name char(150));", expected);
            SendSQLQuery (cl1, $"CREATE TABLE table2(id INT NOT NULL, age DOUBLE);", expected);
            SendSQLQuery (cl1, $"INSERT INTO table1 values(1, 2.9, 'sfsf');", expected);
            SendSQLQuery (cl1, $"INSERT INTO table2 values(1, 3.5);", expected);
            //SendSQLQuery (cl1, $"SELECT age from table1 UNION SELECT id from table2;", expected);


            SendSQLQuery(cl1, $"create table t ();", expected);
            SendSQLQuery (cl1, $"creat table t (id int);", expected);
            SendSQLQuery (cl1, $"create tale t (id int);", expected);
            SendSQLQuery (cl1, $"create table 555 (id int);", expected);
            //SendSQLQuery (cl1, $"SELECT age from table1 INTERSECT SELECT id from table2;", expected);

            /*
            SendSQLQuery (cl1, $"SELECT * from (fn join fn on fn.id = fn.id);", expected);
            SendSQLQuery (cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            SendSQLQuery(cl1, $"select * from t unon select * from t1;", expected);
            */


        }
    }
}
