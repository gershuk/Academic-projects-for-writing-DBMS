using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DataBaseEngine;
using IntegrationTests.TestApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunflowerDB;
using TransactionManagement;


namespace IntegrationTests
{
    public class BaseSQLTest
    {
        protected readonly string _testPath;
        protected readonly bool _fixtests;
        protected void DelFiles ()
        {
            if (Directory.Exists(_testPath))
            {
                Directory.Delete(_testPath, true);
            }
        }
        public BaseSQLTest (bool fixtests)
        {
            _fixtests = fixtests;
            _testPath = this.GetType().FullName;

        }

        protected TestData GetTestData ()
        {
            var st = new StackTrace();
            var currentMethodName = st.GetFrame(1).GetMethod().Name;
            var currentClassName = this.GetType().FullName;
            return new TestData(currentClassName, currentMethodName);
        }

        protected void SendSQLQuery (TestClient cl, string query, TestData expected)
        {
            Trace.WriteLine(query);
            var res = cl.SendQuery(query);
            if (_fixtests)
            {
                if (expected.GetResult(cl) != res)
                {
                    Console.WriteLine($"Fix test: {query}\nExpected:\n{expected.GetResult(cl)}\nGet:\n{res}\n00");
                    Console.WriteLine("Fix?(Y/N)");
                    expected.FixResult(res, cl);
                    if (Console.ReadLine().Trim().ToLower() == "y")
                    {
                        expected.FixResult(res, cl);
                    }
                    expected.Save();
                }
            }
            Assert.AreEqual(expected.GetResult(cl), res);
            expected.Next(cl);
        }
        protected void SendSQLQuery (TestApClient cl, string query, TestData expected)
        {
            Trace.WriteLine(query);
            var res = cl.SendQuery(query);
            if (_fixtests)
            {
                if (expected.GetResult(cl) != res)
                {
                    Console.WriteLine($"Fix test: {query}\nExpected:\n{expected.GetResult(cl)}\nGet:\n{res}\n00");
                    Console.WriteLine("Fix?(Y/N)");
                    expected.FixResult(res, cl);
                    /* if (Console.ReadLine().Trim().ToLower() == "y")
                     {
                         expected.FixResult(res, cl);
                     }*/
                    //Console.ReadLine();
                    expected.Save();
                }
            }
            Assert.AreEqual(expected.GetResult(cl), res);
            expected.Next(cl);
        }


    }
}