#define FIXALLTESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseEngine;
using IntegrationTests.TestApi.QueryGenerator;
using SunflowerDB;
using TransactionManagement;

namespace IntegrationTests
{
    class TestsDebuger
    {
        private static void Main (string[] args)
        {/*
            var test1 = new Create_ShowCreate(true);
            test1.TestTest();
            test1.TestCreateCommandSynax();
            Console.ReadKey();
            */
            var test = new MultyThreadTest(true);
            test.InsertTest();
            //test.MainTest();
            /*
            var test = new Test_Errors(true);
            //test.MainTest();
            
            var _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
            Console.WriteLine(_core.ExecuteSqlSequence("create table I6h5YVPAg (ViS1F char(100));"));
          Console.WriteLine(_core.ExecuteSqlSequence("insert into I6h5YVPAg (ViS1F) values (\"jy\"+\"sb\"+\"Qn\"), (\"dBqSU\"), (\"peC4Oi\"), (\"IAPn\")"));
            var gen = new QueryGenerator();
            for(var i = 0; i < 0; i++)
            {
                var res = gen.GenerateQuery();
                Console.WriteLine($"{res}\n\n\n");
                Console.WriteLine($"{_core.ExecuteSqlSequence(res)}\n\n\n");
                Console.ReadKey();
            }
            Console.ReadKey();
            */
        }
    }
}
