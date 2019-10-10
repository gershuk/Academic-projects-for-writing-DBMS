using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using DataBaseEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SunflowerDB;

namespace TestingFramework
{
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
            ColoredOutput(" | " + Math.Round((double)100 * countSuccess / count) + "% | " + (ms == -1 ? "" : "Time " + ms + " ms"));
            ColoredOutput(Delimeter, forColor: ConsoleColor.Cyan);
        }



        private SqlCommandResult CommandRunner(string query)
        {
            var ans = core.SendSqlSequence(query);
            ans.AnswerNotify.WaitOne();
            return ans;
        }

        private void CleanAfterTest()
        {
            File.Delete(DataBaseFilePath);
            core.Dispose();
            core = GetDataBase();
        }

        private void CleanData()
        {
            testsList.Clear();
            resultsList.Clear();
        }


        private void ShowTests(bool onlyErrors)
        {
            var path = ResultsPath + "/" + groupName + ".json";
            
            if (!File.Exists(path))
            {
                Console.WriteLine("Error: tests result with name " + groupName + ".json does not exists");
                return;
            }

            var obj = JObject.Parse(File.ReadAllText(path));
            groupDescription = (string)obj["description"];
            var array = (JArray)obj["tests"];
            foreach (var testResult in array)
            {
                resultsList.Add(JsonConvert.DeserializeObject<TestResult>(testResult.ToString()));
            }

            int count = 0, countSuccess = 0, countFailed = 0;

            foreach (var res in resultsList)
            {
                count++;
                if (res.TestPassed && onlyErrors)
                {
                    countSuccess++;
                    continue;
                }

                selectedLevel = PrintLevel.all;
                currentLevel = PrintLevel.all;

                ColoredOutput("Test info", forColor: ConsoleColor.White, backColor: ConsoleColor.DarkBlue, newLine: false);
                ColoredOutput(res.TestPassed ? "PASSED" : "NOT PASSED", res.TestPassed ? ConsoleColor.Green : ConsoleColor.Red);
                Console.WriteLine("\tInput: " + res.UsedTest.Input);
                Console.WriteLine("\tStatus: " + res.UsedTest.Status);
                Console.WriteLine("\tExpect output: " + res.UsedTest.ExpectOutput);
                if (res.UsedTest.ExpectOutput)
                {
                    Console.WriteLine("\tExpected output: " + res.UsedTest.Output);
                }
                
                if (res.UsedTest.ExpectOutput)
                {
                    Console.WriteLine("\tReturned output: " + res.ReturnedOutput);
                }

                Console.WriteLine("\tReturned status: " + res.ReturnedStatus);
                Console.WriteLine();

                countSuccess += res.TestPassed ? 1 : 0;
                countFailed += !res.TestPassed ? 1 : 0;
            }
            PrintResult(count, countSuccess, countFailed, -1);
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
                Console.WriteLine("Error: tests group with name " + groupName + ".json does not exists");
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
            char[] trimSyms = { ';', ' ', '\n', '\r' };
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

                    string testOutput;
                    if (ans.Answer.State == OperationExecutionState.performed)
                    {
                        testOutput = ans.Answer.Result != null ? ans.Answer.Result.ToString() : "";
                    }
                    else
                    {
                        testOutput = ans.Answer.OperationException.ToString();
                    }
                    testOutput = testOutput.Trim();

                    if (ans.Answer.State.ToString() != statusList[i].Trim(trimSyms) || (test.ExpectOutput && outputList[i].Trim(trimSyms) != testOutput))
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

                    ColoredOutput("Output: " + testOutput + "\nSTATUS: " + ans.Answer.State.ToString());

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

                if (ans.Answer != null)
                {
                    switch (ans.Answer.State)
                    {
                        case OperationExecutionState.performed:
                            Console.WriteLine(ans.Answer.Result.ToString());
                            break;
                        case OperationExecutionState.parserError:
                        case OperationExecutionState.failed:
                            Console.WriteLine(ans.Answer.OperationException);
                            break;
                    }
                    Console.WriteLine("State: " + ans.Answer.State.ToString());
                }
            }

            CleanAfterTest();
            CleanData();
        }

        public void Run()
        {
            var exitState = false;
            while (!exitState)
            {
                CleanData();

                ColoredOutput(">> ", forColor: ConsoleColor.Red, newLine: false);
                var command = Console.ReadLine().Split();

                if (string.IsNullOrEmpty(command[0]))
                {
                    continue;
                }

                if (command.Length > 1)
                {
                    switch (command[0])
                    {
                        case "run":
                            selectedLevel = PrintLevel.normal;
                            groupName = command[1];

                            if (command.Length > 2)
                            {
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
                            }

                            RunTests();
                            break;
                        case "show":
                            var onlyErrors = false;
                            groupName = command[1];
                            if (command.Length > 2)
                            {
                                onlyErrors = true;
                            }
                            ShowTests(onlyErrors);
                            break;
                    }


                }
                else
                {
                    switch (command[0])
                    {
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
    }
}
