using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class TestClient
    {
        private Process _client;
        private StreamWriter _input;
        private StreamReader _output;
        private StreamReader _error;
        public TestClient()
        {
            var dirr = Directory.GetCurrentDirectory();
            dirr=dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), 1);
            dirr = dirr.Remove(dirr.LastIndexOf("\\"), dirr.Length-dirr.LastIndexOf("\\"));
            var ap_dirr = dirr+"\\Aplications";
            _client = new Process();
            _client.StartInfo.UseShellExecute = false;
            _client.StartInfo.FileName = ap_dirr+ "\\SunflowerDBClient.exe";
            _client.StartInfo.CreateNoWindow = true;
            _client.StartInfo.RedirectStandardInput = true;
            _client.StartInfo.RedirectStandardOutput = true;
            _client.StartInfo.RedirectStandardError = true;
            _client.Start();
            _input = _client.StandardInput;
            _output = _client.StandardOutput;
            _error = _client.StandardError;

            {
                var tries = 2;
                var conection_res = "";
                while (true)
                {
                    do
                    {
                        conection_res = _output.ReadLine();
                    } while (conection_res == $"Conection to 127.0.0.1:8888 failed");
                    if (conection_res == $"Conection to 127.0.0.1:8888 established")
                    {
                        break;
                    }
                    else
                    {
                        tries--;
                        if (tries < 0)
                        {
                            throw new ConnectIssue("Client cant find server");
                        }
                        else
                        {
                            _input.WriteLine("y");
                        }
                    }
                }
            }
        }

        public string SendQuery(string sqlquery)
        {
            _input.WriteLine(sqlquery);
            var res = "";
            var line = _output.ReadLine();
            while(line != "*")
            {
                res += line;
                line = _output.ReadLine();
            }
            return res;
        }

        ~TestClient ()
        {
            _client.Kill();
            _client.Close();
        }

    }
}
