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
            var test = new Test_Errors(true);
            //test.MainTest();
            var _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());

            var gen = new QueryGenerator();
            for(var i = 0; i < 20; i++)
            {
                var res = gen.GenerateQuery();
                Console.WriteLine($"{res}\n\n\n{_core.ExecuteSqlSequence(res)}");
                //Console.ReadKey();
            }
            while(true)
            {
                var res = $"select * from t where {gen.Expression().Replace(",",".")};";
                Console.WriteLine($"{res}\n\n\n{_core.ExecuteSqlSequence(res)};");
                Console.ReadKey();

            }
            //select * from t where ~(((((49702-31590-219935))!=(28238))or((203345)<>((163457+48307))))and((("OJA0qquTcbgqn7ibWOIpiZNaR4uqjLJjUfl4arnL2r0VYPVQ886euC2y6kiw6DtRS6WHVnBol7x0i1CCjdslOozRnx80J0Oy8jDb2N4U7R2xOBPEi")!=(((247215)*(116078))))))
        }
    }
}
