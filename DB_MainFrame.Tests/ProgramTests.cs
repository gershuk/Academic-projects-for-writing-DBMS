using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace DB_MainFrame.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        private StringWriter sw;
        private string PREFIX = "../../../tests/";
        
        [TestInitialize]
        public void TestInit()
        {
            sw = new StringWriter();
            Console.SetOut(sw);
            Console.SetError(sw);
        }


        [DataTestMethod]
        [DataRow("tests_set1.txt", "results/set1.txt")]
        [DataRow("tests_set2.txt", "results/set2.txt")]
        [DataRow("tests_set3.txt", "results/set3.txt")]
        [DataRow("tests_set4.txt", "results/set4.txt")]
        [DataRow("tests_set5.txt", "results/set5.txt")]
        public void MainTest(string input, string result)
        {
            Console.SetIn(new StreamReader(PREFIX + input));
            Program.Main();
            var answer = sw.ToString().Split("\n");
            var i = 0;

            using (var sr = File.OpenText(PREFIX + result))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    Assert.AreEqual(s.Trim(), answer[i++].Trim());
                }
            }
        }
    }
}