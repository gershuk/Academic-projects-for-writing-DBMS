using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationTests.TestApi;

namespace IntegrationTests
{
    class TestData
    {
        private readonly Dictionary<String,List<string>> _expected = new Dictionary<String, List<string>>();
        private string _dirr;
        private Dictionary<String, int> _last_res = new Dictionary<String, int>();

        public TestData (string group, string test)
        {
            var dirr = Directory.GetCurrentDirectory();
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), dirr.Length - dirr.LastIndexOf("\\"));
            _dirr = $"{dirr}\\TestsData\\{group}\\{test}";
            Directory.CreateDirectory(_dirr);
        }
        private void Load(string file)
        {
            var dirr = $"{_dirr}\\{file}.txt";
            if (!File.Exists(dirr))
            {
                var f = File.Create(dirr);
                f.Close();
            }
            using (var sr = new StreamReader(dirr))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadToEnd();
                _expected[file] = new List<string>();
                if (line.Length > 0)
                {
                    _expected[file].AddRange(line.Split(new string[] { "\"\n\"" }, StringSplitOptions.None));
                }
            }
            _last_res[file] = 0;
        }
        public string GetResult (TestClient cl)
        {
            if(!_expected.ContainsKey(cl.Name))
            {
                Load(cl.Name);
            }
            if (_expected[cl.Name].Count > _last_res[cl.Name])
            {
                return _expected[cl.Name][_last_res[cl.Name]].Replace("\\\"", "\"");
            }
            else
            {
                while (_expected[cl.Name].Count <= _last_res[cl.Name])
                {
                    _expected[cl.Name].Add("");
                }
                return _expected[cl.Name][_last_res[cl.Name]];
            }
        }
        public string GetResult (TestApClient cl)
        {
            if (!_expected.ContainsKey(cl.Name))
            {
                Load(cl.Name);
            }
            if (_expected[cl.Name].Count > _last_res[cl.Name])
            {
                return _expected[cl.Name][_last_res[cl.Name]].Replace("\\\"", "\"");
            }
            else
            {
                while (_expected[cl.Name].Count <= _last_res[cl.Name])
                {
                    _expected[cl.Name].Add("");
                }
                return _expected[cl.Name][_last_res[cl.Name]];
            }
        }
        public void Next(TestClient cl)
        {
            _last_res[cl.Name]++;
        }

        public void Next (TestApClient cl)
        {
            _last_res[cl.Name]++;
        }

        public void FixResult(string newres, TestClient cl)
        {
            _expected[cl.Name][_last_res[cl.Name]] = newres.Replace("\"", "\\\"");
        }
        public void FixResult (string newres, TestApClient cl)
        {
            _expected[cl.Name][_last_res[cl.Name]] = newres.Replace("\"", "\\\"");
        }
        public void Save ()
        {
            foreach (var i in _expected)
            {
                var dirr = $"{_dirr}\\{i.Key}.txt";
                var f = File.Create(dirr);
                f.Close();
                using (var sw = new StreamWriter(dirr))
                {
                    sw.Write($"{String.Join("\"\n\"", i.Value)}");
                }
            }
            
        }

    }
}
