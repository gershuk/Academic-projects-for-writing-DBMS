using DataBaseEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestingFramework
{
    public class Test
    {
        public List<string> Input { get; }
        public List<string> Output { get; }
        public List<string> Status { get; }
        public bool ExpectOutput { get; set; }

        public Test()
        {
            Input = new List<string>();
            Output = new List<string>();
            Status = new List<string>();
        }
    }


    class TestResult
    {
        public Test UsedTest { get; set; }
        public List<string> ReturnedOutput { get; }
        public List<string> ReturnedStatus { get; }
        public bool TestPassed { get; set; }

        public TestResult() 
        {
            ReturnedOutput = new List<string>();
            ReturnedStatus = new List<string>();
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
