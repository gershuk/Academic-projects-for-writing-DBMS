using System;

using DataBaseEngine;

using TransactionManagement;

namespace SunflowerDB
{
    public static class ConsoleInterface
    {

        public static void Main ()
        {
            using var core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
            var exitState = true;

            Console.WriteLine("Hello!");
            Console.WriteLine("Please enter your sql request.");
            Console.WriteLine("If you want to quit write 'exit'.");

            while (exitState)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    exitState = false;
                    core.Dispose();
                }
                else
                {
                    var ans = core.ExecuteSqlSequence(input);
                    Console.WriteLine(ans);
                    Console.WriteLine("--------------------------------------------------------------------------------");
                }
            }
        }
    }
}