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
            */
            var test = new DurabilityTest(true);
            test.DeleteDurability();
            
            
            
        }
    }
}
