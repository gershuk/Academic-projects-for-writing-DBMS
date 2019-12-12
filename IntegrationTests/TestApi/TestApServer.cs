using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    class TestApServer
    {
        private Process _client;
        private StreamWriter _input;
        private StreamReader _output;
        private StreamReader _error;
        public TestApServer ()
        {
            var dirr = Directory.GetCurrentDirectory();
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), dirr.Length - dirr.LastIndexOf("\\"));
            var ap_dirr = dirr + "\\Aplications";
            _client = new Process();
            _client.StartInfo.UseShellExecute = false;
            _client.StartInfo.FileName = ap_dirr + "\\SunflowerDBServer.exe";
            _client.StartInfo.CreateNoWindow = true;
            _client.StartInfo.RedirectStandardInput = true;
            _client.StartInfo.RedirectStandardOutput = true;
            _client.StartInfo.RedirectStandardError = true;
            _client.Start();
            _input = _client.StandardInput;
            _output = _client.StandardOutput;
            _error = _client.StandardError;
            Trace.WriteLine(_output.ReadLine());
        }

        public string SendQuery (string sqlquery)
        {
            _input.WriteLine(sqlquery);
            var res = "";
            var line = _output.ReadLine();
            while (line != "*")
            {
                res += line;
                line = _output.ReadLine();
            }
            return res;
        }

        ~TestApServer ()
        {
            _client.Kill();
            _client.Close();
        }
    }
}
