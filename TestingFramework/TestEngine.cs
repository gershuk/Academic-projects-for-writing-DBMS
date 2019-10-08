using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections;
using SunflowerDB;
using DataBaseEngine;

namespace TestingFramework
{
    class Test
    {
        public string Input { get; }
        public string Output { get; }
        public string Status { get; }
        public bool ExpectOutput { get; } 

        public Test(JObject json)
        {
            Input = ((string)json["input"]).Trim(';');
            Output = ((string)json["output"]).Trim(';');
            Status = ((string)json["status"]).Trim(';');
            ExpectOutput = (bool)json["expect_output"];
        }

        public override string ToString() => Input;
    }


    class Engine : IDisposable
    {
        private const string Delimeter = "------------------------------------------------------------------------------";
        private const string DataBaseConfigPath = "../../../tests/DataEngineConfig.json";
        private const string DataBaseFilePath = "../../../tests/TestDataBase.db";
        private const string TestsPath = "../../../tests/tests/";
        private const string ResultsPath = "../../../tests/results";

        private enum PrintLevel { silent, normal, all }

        private PrintLevel selectedLevel;
        private PrintLevel currentLevel;
        private string groupDescription;
        private ArrayList testsList;
        private DataBase core;


        public Engine()
        {
            selectedLevel = PrintLevel.normal;
            currentLevel = PrintLevel.normal;
            testsList = new ArrayList();
            core = GetDataBaseCore();
        }


        private DataBase GetDataBaseCore() => new DataBase(1, new DataBaseEngineMain(DataBaseConfigPath));


        public void Dispose() => core.Dispose();


        private void ColoredOutput(string msg = "", ConsoleColor backColor = ConsoleColor.Black, ConsoleColor forColor = ConsoleColor.White, bool newLine = true)
        {
            if (currentLevel <= selectedLevel)
            {
                Console.BackgroundColor = backColor;
                Console.ForegroundColor = forColor;
                Console.Write(msg + (newLine ? "\n" : ""));
                Console.ResetColor();
            }
        }


        private void PrintError(string msg, bool line = true) => ColoredOutput(msg, forColor: ConsoleColor.Red, newLine: line);


        private void PrintHeader(string groupName, string groupDescription)
        {
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
            ColoredOutput("Running test set: " + groupName, forColor: ConsoleColor.Yellow);
            ColoredOutput("Description: " + this.groupDescription, forColor: ConsoleColor.Yellow);
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
        }


        private void PrintResult(int count, int countSuccess, int countFailed, long ms)
        {
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
            ColoredOutput("Results | Passed ", newLine: false);
            ColoredOutput(countSuccess + "/" + count, ConsoleColor.Green, newLine: false);
            ColoredOutput(" Failed ", newLine: false);
            ColoredOutput(countFailed + "/" + count, ConsoleColor.Red, newLine: false);
            ColoredOutput(" | " + Math.Round((double)100 * countSuccess / count) + "% | Time " + ms + " ms");
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
        }

        private SqlCommandResult CommandRunner(string query)
        {
            var ans = core.SendSqlSequence(query);
            ans.AnswerNotify.WaitOne();
            return ans;
        }


        bool LoadTests(string set_name)
        {
            testsList.Clear();

            var path = TestsPath + "/" + set_name + ".in";

            if (!File.Exists(path))
            {
                Console.WriteLine("Error: tests set with name " + set_name + ".in does not exists");
                return false;
            }

            var obj = JObject.Parse(File.ReadAllText(path));
            groupDescription = (string)obj["description"];
            var array = (JArray)obj[set_name];
            foreach (var test in array)
            {
                testsList.Add(new Test((JObject)test));
            }
            return true;
        }


        private void RunTests(string groupName)
        {
            char[] trimSyms = {';', ' ', '\n', '\r'};
            int count = 0, countFailed = 0, countSuccess = 0;

            if (!LoadTests(groupName))
            {
                return;
            }

            currentLevel = PrintLevel.normal;
            PrintHeader(groupName, groupDescription);
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (Test test in testsList)
            {
                currentLevel = PrintLevel.all;
                ColoredOutput("Running test #" + ++count, forColor: ConsoleColor.Blue);

                var queryList = test.Input.Trim(';').Split(";");
                var outputList = test.Output.Trim(';').Split(";");
                var statusList = test.Status.Trim(';').Split(";");
                
                var outputResult = "";
                var statusResult = "";
                var testPassed = true;
               
                for (var i = 0; i < queryList.Length; i++)
                {
                    var commandPassed = true;
                    
                    var ans = CommandRunner(queryList[i]);
                    if (ans.Answer.State.ToString() != statusList[i].Trim(trimSyms) || (test.ExpectOutput && outputList[i].Trim(trimSyms) != ans.Answer.Result.Trim(trimSyms)))
                    {
                        testPassed = false;
                        commandPassed = false;
                    }

                    outputResult += ans.Answer.Result + ";";
                    statusResult += ans.Answer.State + ";";

                    currentLevel = commandPassed ? PrintLevel.all : PrintLevel.normal;
                    
                    if (!commandPassed && selectedLevel < PrintLevel.all)
                    {
                        PrintError("\tFAIL IN TEST #" + count);
                    }
                    
                    ColoredOutput("Command: " + queryList[i] + " ", newLine: false);
                    
                    if (commandPassed)
                    {
                        ColoredOutput("PASSED", forColor: ConsoleColor.Green);
                    }
                    else
                    {
                        PrintError("NOT PASSED");
                    }
                    
                    ColoredOutput("Output: " + ans.Answer.Result.Trim(trimSyms) + "\nSTATUS: " + ans.Answer.State.ToString());
                    
                    if (!commandPassed)
                    {
                        ColoredOutput("EXPECTED STATUS: " + statusList[i].Trim(), forColor: ConsoleColor.Red);
                        if (test.ExpectOutput)
                        {
                            ColoredOutput("EXPECTED OUTPUT: " + outputList[i].Trim(), forColor: ConsoleColor.Red);
                        }
                    }
                }

                countSuccess += testPassed ? 1 : 0;
                countFailed += !testPassed ? 1 : 0;

                File.WriteAllText(DataBaseFilePath, "DATA_BASE_TABLE_METAINF_FILE");
                core.Dispose();
                core = GetDataBaseCore();
            }
            watch.Stop();

            currentLevel = PrintLevel.silent;
            PrintResult(count, countSuccess, countFailed, watch.ElapsedMilliseconds);
        } 


        public void Run()
        {
            var exitState = false;
            while (!exitState)
            {
                Console.Write(">> ");
                var command = Console.ReadLine().Split();
                switch (command[0])
                {
                    case "run":
                        selectedLevel = PrintLevel.normal;
                        foreach (var s in new ArraySegment<string>(command, 2, command.Length - 2))
                        {
                            switch (s.Trim())
                            {
                                case "--silent":
                                    selectedLevel = PrintLevel.silent;
                                    break;
                                case "--all":
                                    selectedLevel = PrintLevel.all;
                                    break;
                                default:
                                    Console.WriteLine("Error: unknown keyword " + s.Trim());
                                    break;
                            }
                        }

                        RunTests(command[1]);
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "exit":
                        exitState = true;
                        break;
                    default:
                        Console.WriteLine("No such command");
                        break;
                }
            }
        }
    }


    class TestProgram
    {
        static void Main()
        {
            var engine = new Engine();
            engine.Run();
            engine.Dispose();
        }
    }
}
