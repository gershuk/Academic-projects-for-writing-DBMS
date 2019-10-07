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
        public string input;
        public string output;
        public string status;
        public bool expect_output;

        public Test(JObject json)
        {
            input = ((string)json["input"]).Trim(';');
            output = ((string)json["output"]).Trim(';');
            status = ((string)json["status"]).Trim(';');
            expect_output = (bool)json["expect_output"];
        }

        public override string ToString() => input;
    }


    class Engine : IDisposable
    {
        private const string CONFIG_PATH = "testing.cfg";
        private const string DB_CONFIG_PATH = "../../../tests/DataEngineConfig.json";
        private const string DBFILE_PATH = "../../../tests/TestDataBase.db";

        private enum PrintLevel { silent, normal, all }

        private string tests_dir;
        private string results_dir;
        private PrintLevel print_level;
        private string set_description;
        private ArrayList tests;
        private DataBase core;


        public Engine()
        {
            print_level = PrintLevel.normal;
            tests = new ArrayList();
            core = GetDB();
        }


        private DataBase GetDB() => new DataBase(1, new DataBaseEngineMain(DB_CONFIG_PATH));


        public void Dispose() => core.Dispose();


        private void ColoredOutput(PrintLevel level, string msg = "", ConsoleColor backColor = ConsoleColor.Black, ConsoleColor forColor = ConsoleColor.White, bool newLine = true)
        {
            if (level > print_level)
            {
                return;
            }

            Console.BackgroundColor = backColor;
            Console.ForegroundColor = forColor;
            if (newLine)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Console.Write(msg);
            }
            
            Console.ResetColor();
        }


        private SqlCommandResult CommandRunner(string query)
        {
            var ans = core.SendSqlSequence(query);
            ans.AnswerNotify.WaitOne();
            return ans;
        }


        void CreateConfig()
        {
            var file = File.CreateText(CONFIG_PATH);
            Console.Write("Input tests dir: ");
            var temp = Console.ReadLine();
            Directory.CreateDirectory(temp);
            tests_dir = temp;
            file.WriteLine(temp);
            Console.Write("Input tests results dir: ");
            temp = Console.ReadLine();
            Directory.CreateDirectory(temp);
            results_dir = temp;
            file.WriteLine(temp);
            file.Close();
        }


        bool LoadConfig()
        {
            if (File.Exists(CONFIG_PATH))
            {
                var lines = File.ReadAllLines(CONFIG_PATH);
                if (lines.Length > 0)
                {
                    tests_dir = lines[0];
                    results_dir = lines[1];
                    return true;
                }
            }
            return false;
        }


        bool LoadTests(string set_name)
        {
            tests.Clear();

            var path = tests_dir + "/" + set_name + ".in";

            if (!File.Exists(path))
            {
                Console.WriteLine("Error: tests set with name " + set_name + ".in does not exists");
                return false;
            }

            var obj = JObject.Parse(File.ReadAllText(path));
            set_description = (string)obj["description"];
            var array = (JArray)obj[set_name];
            foreach (var test in array)
            {
                tests.Add(new Test((JObject)test));
            }
            return true;
        }


        private void RunTests(string set_name)
        {
            const string DELIMETER = "------------------------------------------------------------------------------";
            char[] symbols = {';', ' ', '\n', '\r'};
            int count = 0, count_failed = 0, count_success = 0;

            if (!LoadTests(set_name))
            {
                return;
            }

            ColoredOutput(PrintLevel.normal, DELIMETER, forColor: ConsoleColor.Cyan);
            ColoredOutput(PrintLevel.normal, "Running test set: " + set_name, forColor: ConsoleColor.Yellow);
            ColoredOutput(PrintLevel.normal, "Description: " + set_description, forColor: ConsoleColor.Yellow);
            ColoredOutput(PrintLevel.normal, DELIMETER, forColor: ConsoleColor.Cyan);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (Test test in tests)
            {
                ColoredOutput(PrintLevel.all, "Running test #" + ++count, forColor: ConsoleColor.Blue);

                var queries = test.input.Trim(';').Split(";");
                var outputs = test.output.Trim(';').Split(";");
                var status_list = test.status.Trim(';').Split(";");
                
                var output_result = "";
                var status_result = "";
                var test_passed = true;
               
                for (var i = 0; i < queries.Length; i++)
                {
                    var command_passed = true;
                    
                    var ans = CommandRunner(queries[i]);
                    if (ans.Answer.State.ToString() != status_list[i].Trim(symbols) || (test.expect_output && outputs[i].Trim(symbols) != ans.Answer.Result.Trim(symbols)))
                    {
                        test_passed = false;
                        command_passed = false;
                    }

                    output_result += ans.Answer.Result + ";";
                    status_result += ans.Answer.State + ";";

                    var level = command_passed ? PrintLevel.all : PrintLevel.normal;
                    if (!test_passed)
                    {
                        ColoredOutput(level, "\tFAIL IN TEST #" + count, forColor: ConsoleColor.Red);
                    }
                    ColoredOutput(level, "Command: " + queries[i] + " ", newLine: false);
                    if (command_passed)
                    {
                        ColoredOutput(level, "PASSED", forColor: ConsoleColor.Green);
                    }
                    else
                    {
                        ColoredOutput(level, "NOT PASSED", forColor: ConsoleColor.Red);
                    }
                    ColoredOutput(level, "Output: " + ans.Answer.Result.Trim(symbols) + "\nSTATUS: " + ans.Answer.State.ToString());
                    
                    if (!command_passed)
                    {
                        ColoredOutput(level, "EXPECTED STATUS: " + status_list[i].Trim(), forColor: ConsoleColor.Red);
                        if (test.expect_output)
                        {
                            ColoredOutput(level, "EXPECTED OUTPUT: " + outputs[i].Trim(), forColor: ConsoleColor.Red);
                        }
                    }
                }

                count_success += test_passed ? 1 : 0;
                count_failed += !test_passed ? 1 : 0;

                File.WriteAllText(DBFILE_PATH, "DATA_BASE_TABLE_METAINF_FILE");
                core.Dispose();
                core = GetDB();
            }
            watch.Stop();

            ColoredOutput(PrintLevel.silent, DELIMETER, forColor: ConsoleColor.Cyan);
            ColoredOutput(PrintLevel.silent, "Results | Passed ", newLine: false);
            ColoredOutput(PrintLevel.silent, count_success + "/" + count, ConsoleColor.Green, newLine: false);
            ColoredOutput(PrintLevel.silent, " Failed ", newLine: false);
            ColoredOutput(PrintLevel.silent, count_failed + "/" + count, ConsoleColor.Red, newLine: false);
            ColoredOutput(PrintLevel.silent, " | " + Math.Round((double) 100 * count_success / count) + "% | Time " + watch.ElapsedMilliseconds + " ms");
            ColoredOutput(PrintLevel.silent, DELIMETER, forColor: ConsoleColor.Cyan);
        } 


        public void Run()
        {
            if (!LoadConfig())
            {
                CreateConfig();
            }

            var exitState = false;
            while (!exitState)
            {
                Console.Write(">> ");
                var command = Console.ReadLine().Split();
                switch (command[0])
                {
                    case "run":
                        foreach (var s in new ArraySegment<string>(command, 2, command.Length - 2))
                        {
                            switch (s.Trim())
                            {
                                case "--silent":
                                    print_level = PrintLevel.silent;
                                    break;
                                case "--all":
                                    print_level = PrintLevel.all;
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
