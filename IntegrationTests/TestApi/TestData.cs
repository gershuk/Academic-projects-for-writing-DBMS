using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    class TestData
    {
        private readonly List<string> _expected = new List<string>();
        private string _dirr;
        private int _last_res = 0;

        public TestData (string group, string test)
        {
            var dirr = Directory.GetCurrentDirectory();
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), dirr.Length - dirr.LastIndexOf("\\"));
            _dirr = $"{dirr}\\TestsData\\{group}\\{test}";
            Directory.CreateDirectory(_dirr);
            _dirr += "\\data.txt";
            if (!File.Exists(_dirr))
            {
                var f = File.Create(_dirr);
                f.Close();
            }
            using (var sr = new StreamReader(_dirr))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadToEnd();
                if (line.Length > 0)
                {
                    _expected.AddRange(line.Split(new string[] { "\"\n\"" }, StringSplitOptions.None));
                }
            }
        }

        public string GetResult ()
        {
            
            if (_expected.Count > _last_res)
            {
                return _expected[_last_res].Replace("\\\"", "\"");
            }
            else
            {
                while (_expected.Count <= _last_res)
                {
                    _expected.Add("");
                }
                return _expected[_last_res];
            }
        }

        public void Next()
        {
            _last_res++;
        }

        public void FixResult(string newres)
        {
            _expected[_last_res] = newres.Replace("\"", "\\\"");
        }

        public void Save ()
        {
            var f = File.Create(_dirr);
            f.Close();
            using (var sw = new StreamWriter(_dirr))
            {
                sw.Write($"{String.Join("\"\n\"", _expected)}");
            }
        }

    }
}
