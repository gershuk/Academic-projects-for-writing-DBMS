using System;
using System.Collections.Generic;
using System.IO;

using DataBaseEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SunflowerDB;

namespace TestingFramework
{
    public class Test
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Status { get; set; }
        public bool ExpectOutput { get; set; } 
    }


    class TestResult
    {
        public Test UsedTest { get; set; }
        public string ReturnedOutput { get; set; }
        public string ReturnedStatus { get; set; }
        public bool TestPassed { get; set; }

        public TestResult() { }

        public TestResult(Test t, string output, string status, bool passed)
        {
            UsedTest = t;
            ReturnedOutput = output;
            ReturnedStatus = status;
            TestPassed = passed;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
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
        private string groupName;
        private string groupDescription;
        private readonly List<Test> testsList;
        private readonly List<TestResult> resultsList;
        private DataBase core;


        public Engine()
        {
            selectedLevel = PrintLevel.normal;
            currentLevel = PrintLevel.normal;
            testsList = new List<Test>();
            resultsList = new List<TestResult>();
            core = GetDataBase();
        }


        private static DataBase GetDataBase() => new DataBase(1, new DataBaseEngineMain(DataBaseConfigPath));


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


        private void PrintHeader()
        {
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
            ColoredOutput("Running test set: " + groupName, forColor: ConsoleColor.Yellow);
            ColoredOutput("Description: " + groupDescription, forColor: ConsoleColor.Yellow);
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


        private void CleanAfterTest()
        {
            File.WriteAllText(DataBaseFilePath, "DATA_BASE_TABLE_METAINF_FILE");
            core.Dispose();
            core = GetDataBase();
        }

        private void CleanData()
        {
            testsList.Clear();
            resultsList.Clear();
        }

        private SqlCommandResult CommandRunner(string query)
        {
            var ans = core.SendSqlSequence(query);
            ans.AnswerNotify.WaitOne();
            return ans;
        }


        private void WriteTestResults()
        {
            var tests = new JArray();
            foreach (var res in resultsList)
            {
                tests.Add(res.ToJson());
            }
            var obj = new JObject
            {
                ["groupName"] = groupName,
                ["groupDescription"] = groupDescription
            };
            obj.Add("tests", tests);
            File.WriteAllText(ResultsPath + "/" + groupName + ".json", obj.ToString());
        }


        bool LoadTests()
        {
            testsList.Clear();

            var path = TestsPath + "/" + groupName + ".json";

            if (!File.Exists(path))
            {
                Console.WriteLine("Error: tests set with name " + groupName + ".json does not exists");
                return false;
            }

            var obj = JObject.Parse(File.ReadAllText(path));
            groupDescription = (string)obj["description"];
            var array = (JArray)obj[groupName];
            foreach (var test in array)
            {
                testsList.Add(JsonConvert.DeserializeObject<Test>(test.ToString()));
            }

            return true;
        }


        private void RunTests()
        {
            char[] trimSyms = {';', ' ', '\n', '\r'};
            int count = 0, countFailed = 0, countSuccess = 0;

            if (!LoadTests())
            {
                return;
            }

            currentLevel = PrintLevel.normal;
            PrintHeader();
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var test in testsList)
            {
                currentLevel = PrintLevel.all;
                ColoredOutput("Running test #" + ++count, forColor: ConsoleColor.Cyan);

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
                    
                    ColoredOutput("Command: " + queryList[i] + " ", newLine: false, forColor: ConsoleColor.DarkBlue);
                    
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

                resultsList.Add(new TestResult(test, outputResult, statusResult, testPassed));

                CleanAfterTest();             
            }
            watch.Stop();

            WriteTestResults();

            currentLevel = PrintLevel.silent;
            PrintResult(count, countSuccess, countFailed, watch.ElapsedMilliseconds);
            CleanData();
        } 


        private void Shell()
        {
            while (true)
            {
                ColoredOutput("$$ ", forColor: ConsoleColor.Red, newLine: false);
                var command = Console.ReadLine();
                
                if (command == "exit")
                {
                    break;
                }
                
                var ans = CommandRunner(command);
                Console.WriteLine(ans.Answer.Result);
                Console.WriteLine("State: " + ans.Answer.State.ToString());
            }

            CleanAfterTest();
            CleanData();
        }


        public void Run()
        {
            var exitState = false;
            while (!exitState)
            {
                ColoredOutput(">> ", forColor: ConsoleColor.Red, newLine: false);
                var command = Console.ReadLine().Split();
                
                if (string.IsNullOrEmpty(command[0]))
                {
                    continue;
                }
                
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

                        groupName = command[1];
                        RunTests();
                        break;
                    case "shell":
                        Shell();
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "exit":
                        exitState = true;
                        break;
                    default:
                        Console.WriteLine("No such command: " + command[0]);
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
