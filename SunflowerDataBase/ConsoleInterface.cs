using System;

using DataBaseEngine;

namespace SunflowerDB
{
    public class ConsoleInterface
    {

        public static void Main()
        {
            var core = new DataBase(20, new DataBaseEngineMain());
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
                    var ans = core.SendSqlSequence(input);
                    ans.AnswerNotify.WaitOne();
                    Console.WriteLine(ans);
                    Console.WriteLine("--------------------------------------------------------------------------------");
                }
                //"CREATE TABLE Customers (Id INT NOT NULL,Age FLOAT, Name VARCHAR(20));"
                //"SHOW TABLE Customers;"
            }
        }
    }
}