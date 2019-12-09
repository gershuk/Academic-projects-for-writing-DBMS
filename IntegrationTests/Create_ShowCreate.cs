#define FIXALLTESTSa
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace IntegrationTests
{
    [TestClass]
    public class Create_ShowCreate
    {
        private TestData GetTestData()
        {
            var st = new StackTrace();
            var currentMethodName = st.GetFrame(1).GetMethod().Name;
            return new TestData(this.GetType().FullName, currentMethodName);
        }

        private void SendSQLQuery(TestClient cl, string query, TestData expected)
        {
            var res = cl.SendQuery(query);

#if FIXALLTESTS
            if (expected.GetResult()!=res)
            {
                Trace.WriteLine($"Fix test: {query}\nExpected:\n{expected.GetResult()}\nGet:\n{res}\n00");
                expected.FixResult(res);
            }
#endif
            Assert.AreEqual(expected.GetResult(), res);
            expected.Next();
        }

        [TestMethod]
        public void TestTest()
        {
            var results = GetTestData();
            var test = results.GetResult();
            Assert.AreEqual("test1", test);
            results.FixResult("test1");
            results.Save();
            test = results.GetResult();
            Assert.AreEqual("test2", test);
            results.FixResult("test2");
            results.Save();            
        }

        [TestMethod]
        public void TestCreateCommandSynax ()
        {
            var expected = GetTestData();
            var server = new TestServer();
            var client1 = new TestClient();

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
