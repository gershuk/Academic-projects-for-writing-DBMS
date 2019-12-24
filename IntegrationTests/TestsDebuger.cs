using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseEngine;
using DataBaseType;
using IntegrationTests.TestApi.QueryGenerator;
using ProtoBuf;
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
            
            var test = new DurabilityTest(true);
            test.DeleteDurability();
            
            var _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
            using (var binaryData = new MemoryStream())
            {
                Serializer.Serialize(binaryData, _core.ExecuteSqlSequence("create table test(i int)"));
                Console.WriteLine(Serializer.Deserialize <OperationResult<SqlSequenceResult>>(binaryData));
                binaryData.Flush();
                Serializer.Serialize(binaryData, _core.ExecuteSqlSequence("insert test values (1),(2)"));
                Console.WriteLine(Serializer.Deserialize<OperationResult<SqlSequenceResult>>(binaryData));
            }
        }
    }
}
