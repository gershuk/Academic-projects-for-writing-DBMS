using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    class TestClient
    {
        private Process client;

        TestClient()
        {
            client = new Process();
           client.StartInfo.UseShellExecute = false;
            client.StartInfo.FileName = "C:\\HelloWorld.exe";
            client.StartInfo.CreateNoWindow = true;
            client.Start();
        }

        public string SendQuery(string sqlquery)
        {
            throw new NotImplementedException();
        }

    }
}
