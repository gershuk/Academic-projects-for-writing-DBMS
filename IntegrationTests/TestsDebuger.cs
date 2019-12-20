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
            *
            var test = new MultyThreadTest(true);
            test.InsertTest();
            //test.MainTest();
            /*
            var test = new Test_Errors(true);
            //test.MainTest();
            */
            var _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
            var gen = new QueryGenerator();
            for(var i = 0; i < 1000; i++)
            {
                var res = gen.GenerateQuery();
                _core.ExecuteSqlSequence(res);
              //  Console.WriteLine($"{res}\n\n\n");
              //  Console.WriteLine($"{}\n\n\n");
              //   Console.ReadKey();
            }
            Console.ReadKey();
            
        }
    }
}
