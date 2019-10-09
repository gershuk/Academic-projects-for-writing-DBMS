using Newtonsoft.Json;

namespace TestingFramework
{
    public class Test
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Status { get; set; }
        public bool ExpectOutput { get; set; }
    }


    class TestResult
    {
        public Test UsedTest { get; set; }
        public string ReturnedOutput { get; set; }
        public string ReturnedStatus { get; set; }
        public bool TestPassed { get; set; }

        public TestResult() { }

        public TestResult(Test t, string output, string status, bool passed)
        {
            UsedTest = t;
            ReturnedOutput = output;
            ReturnedStatus = status;
            TestPassed = passed;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
