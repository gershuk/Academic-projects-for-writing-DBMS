using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseEngine;
using DataBaseType;
using SunflowerDB;
using TransactionManagement;

namespace IntegrationTests.TestApi
{
    class TestClient
    {
        private readonly DataBase _core;
        public readonly string Name;

        public TestClient(string name, ref DataBase core)
        {
            _core = core;
            Name = name.Trim();
        }
        public TestClient (string name)
        {
            Name = name.Trim();
        }
        public string SendQuery (string sqlquery)
        {
            var value = _core.ExecuteSqlSequence(sqlquery);
            var result = "";
            switch (value.State)
            {
                case ExecutionState.notProcessed:
                    break;
                case ExecutionState.parserError:
                case ExecutionState.failed:
                    result += "Error" + "\n";
                    result += value.OperationError + "\n";
                    break;
                case ExecutionState.performed:
                    foreach (var info in value.Result.Answer)
                    {
                        result += info.ToString() + "\n";
                        result += "\n";
                    }
                    break;
            }
            return result.ToString();
        }
    }
}
