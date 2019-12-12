using System;
using System.Diagnostics;
using System.Text;
using IntegrationTests.TestApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace IntegrationTests
{
    [TestClass]
    public class Create_ShowCreate
    {
        private readonly bool _fixtests;
        public Create_ShowCreate()
        {
            _fixtests = false;
        }
        public Create_ShowCreate (bool fixtests) => _fixtests = fixtests;

        private TestData GetTestData ()
        {
            var st = new StackTrace();
            var currentMethodName = st.GetFrame(1).GetMethod().Name;
            return new TestData(this.GetType().FullName, currentMethodName);
        }

        private void SendSQLQuery (TestClient cl, string query, TestData expected)
        {
            var res = cl.SendQuery(query);

            if (_fixtests)
            {
                if (expected.GetResult(cl) != res)
                {
                    Console.WriteLine($"Fix test: {query}\nExpected:\n{expected.GetResult(cl)}\nGet:\n{res}\n00");
                    Console.WriteLine("Fix?(Y/N)");
                    if (Console.ReadLine().Trim().ToLower() == "y")
                    {
                        expected.FixResult(res,cl);
                    }
                }
            }
            Assert.AreEqual(expected.GetResult(cl), res);
            expected.Next(cl);
        }
        private void SendSQLQuery (TestApClient cl, string query, TestData expected)
        {
            var res = cl.SendQuery(query);

            if (_fixtests)
            {
                if (expected.GetResult(cl) != res)
                {
                    Console.WriteLine($"Fix test: {query}\nExpected:\n{expected.GetResult(cl)}\nGet:\n{res}\n00");
                    Console.WriteLine("Fix?(Y/N)");
                    if (Console.ReadLine().Trim().ToLower() == "y")
                    {
                        expected.FixResult(res, cl);
                    }
                }
            }
            Assert.AreEqual(expected.GetResult(cl), res);
            expected.Next(cl);
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
            var expected = GetTestData();
            var server = new TestApServer();
            var client1 = new TestApClient("cl1");

            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            SendSQLQuery(client1, "create table t(i int primary key)", expected);
            expected.Save();
        }
    }
}
