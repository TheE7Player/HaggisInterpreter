using HaggisInterpreter2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HaggisInterpreter2Run
{
    internal class Program
    {
        private static string build = "0.6";

        private static void Title(string file = "")
        {
            Console.Title = (string.IsNullOrEmpty(file)) ? $"HAGGIS INTERPRETER {build}" : $"HAGGIS INTERPRETER {build}: {file} ";
        }

        private static void Main(string[] args)
        {
            Title();

            bool toExit = false;
            Dictionary<string, bool> filePasses = new Dictionary<string, bool>(1);
            Dictionary<string, System.Diagnostics.Stopwatch> filePassesTimes = new Dictionary<string, System.Diagnostics.Stopwatch>(1);
            List<string> files = new List<string>(1);

            // Invalid rule: First param has to be folder, if folder futher on, error!
            FileAttributes attr;
            for (int i = 0; i < args.Length; i++)
            {
                if (!Directory.Exists(args[i]) && !args[i].Contains(".haggis"))
                {
                    Console.WriteLine($"ERROR: Unknown Directory has been entered... {args[i]}");
                }

                try
                {
                    attr = File.GetAttributes(args[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR: {e.Message}");
                    toExit = true;
                    break;
                }

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (args.Length == 1)
                    {
                        files = Directory.GetFiles(args[i]).Where(x => x.ToLower().EndsWith(".haggis")).ToList<string>();
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: You cannot pass in a folder with other files!\nPassing a folder should only be 1 param!");
                        toExit = true;
                        break;
                    }
                }
                else
                {
                    files.Add(args[i]);
                }
            }

            if (toExit)
            {
                Console.WriteLine("PRESSING ANY KEY WILL EXIT THE APPLICATION");
                Console.ReadLine();
                return;
            }

            foreach (var file in files)
            {
                if (!Path.GetExtension(file).Equals(".haggis"))
                {
                    Console.WriteLine("Sorry, only .haggis files are able to run in this interpreter");
                    return;
                }

                filePasses.Add(Path.GetFileName(file), false);

                string[] fileFull = File.ReadAllLines(file);

                Interpreter.FLAGS my_flags = new Interpreter.FLAGS();

                var flag_target = "#<DEBUG:";

                int ignore_count = 5;

                // Check for any flags set in the files
                for (int i = 0; i < fileFull.Length; i++)
                {
                    if (ignore_count == 0)
                        break;

                    if (string.IsNullOrEmpty(fileFull[i]))
                        continue;

                    if (fileFull[i].StartsWith(flag_target))
                    {
                        // Attempt the strip the debug flag
                        var expression = new Regex(@"#<DEBUG: (.+)>");

                        // ... See if we matched.
                        Match match = expression.Match(fileFull[i]);

                        if (match.Success)
                        {
                            var val = match.Groups[1].Value;

                            if (val.Equals("PRINT_LINE_NUMBER"))
                            {
                                my_flags.DebugSendRequests = !my_flags.DebugSendRequests;
                                //var result = (my_flags.DebugSendRequests) ? "ON" : "OFF";
                                //Console.WriteLine($"PRINT_LINE_NUMBER IS {result}");
                                ignore_count++;
                                continue;
                            }

                            if (val.Contains('[') && val.Contains(']'))
                            {
                                var key = val.Substring(1, val.IndexOf(']') - 1);
                                var input = val.Substring(val.IndexOf('-') + 1);

                                if (Object.ReferenceEquals(my_flags.Inputs, null))
                                    my_flags.Inputs = new Dictionary<string, string>(1);

                                my_flags.Inputs.Add(key, input);

                                ignore_count++;
                                continue;
                            }

                            Console.WriteLine($"IGNORING UNKOWN FLAG: {match.Groups[1].Value}");
                            return;
                        }
                    }
                    ignore_count = (ignore_count > 0) ? ignore_count -= 1 : 0;
                }

                Console.WriteLine($"\n== RUNNING: {Path.GetFileNameWithoutExtension(file)} ==\n");

                var filePath = Path.GetFileName(file);

                HaggisInterpreter2.Interpreter basic = new HaggisInterpreter2.Interpreter(File.ReadAllLines(file), my_flags);
                try
                {
                    Title(filePath);
                    filePassesTimes.Add(filePath, new System.Diagnostics.Stopwatch());
                    filePassesTimes[filePath].Start();
                    basic.Execute();
                    filePassesTimes[filePath].Stop();
                    filePasses[filePath] = true;
                }
                catch (Exception e)
                {
                    filePasses[filePath] = false;
                    filePassesTimes[filePath].Stop();
                    Console.WriteLine("ERROR:");

                    int line = (Interpreter.executionHandled) ? basic.errorArea[0] : Interpreter.Line;
                    int col = (Interpreter.executionHandled) ? basic.errorArea[1] : Interpreter.Column;

                    var sb = new StringBuilder(fileFull[line - 1].Length);
                    if (Interpreter.executionHandled)
                        Console.WriteLine($"Line {line} : Col {col} - {e.Message}\n");
                    else
                        Console.WriteLine($"UNHANDLED EXCEPTION AT LINE {line}: {e.Message}");

                    for (int i = 0; i < col; i++)
                    {
                        sb.Append(" ");
                    }
                    sb.Append('^', (!Interpreter.executionHandled) ? 1 : basic.errorArea[2]);

                    Console.WriteLine(fileFull[line - 1]);
                    Console.WriteLine(sb.ToString());
                    sb = null;

                    Console.WriteLine("\n ============================ \n");
                    var lineNumber = new System.Diagnostics.StackTrace(e, true).GetFrame(1).GetFileLineNumber();
                    var fileFault = new System.Diagnostics.StackTrace(e, true).GetFrame(1).GetFileName();
                    Console.WriteLine($"INTERAL INFORMATION:\n{Path.GetFileNameWithoutExtension(fileFault)} @ {lineNumber}\n({e.Message})");

                    if (basic.variables.Count == 0)
                        Console.WriteLine("\nHEAP ON EXECUTION: EMPTY");
                    else
                    {
                        Console.WriteLine("\nHEAP ON EXECUTION:");
                        foreach (var item in basic.variables)
                        {
                            Console.WriteLine($"[{item.Key}] {item.Value}");
                        }
                    }

                    var _stack = basic.callStack.ToArray();
                    Console.WriteLine("\nCALLSTACK ON EXECUTION:");
                    for (int i = _stack.Length; i > 0; i--)
                    {
                        Console.WriteLine($"[{i - 1}] {_stack[i - 1]}");
                    }

                    fileFault = null;
                }

                Console.WriteLine($"\n== Finished: {Path.GetFileNameWithoutExtension(file)} ==\n");
            }

            if (files.Count > 0)
            {
                Console.WriteLine("OK");
                Console.WriteLine();

                Console.WriteLine("RESULTS:");
                Title("PASS");

                string time;
                TimeSpan _t;

                foreach (var f in filePasses)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"{f.Key}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" : ");
                    Console.ForegroundColor = (f.Value) ? ConsoleColor.Green : ConsoleColor.Red;
                    var _out = (f.Value) ? "PASS" : "FAIL";

                    if (_out == "FAIL")
                    {
                        Title("FAIL");
                    }
                    _t = filePassesTimes[f.Key].Elapsed;

                    if(_t.TotalMinutes >= 1.0)
                        time = $"{_t.TotalMinutes} minutes, {_t.Seconds} seconds and {_t.Milliseconds} millisconds";
                    else if(_t.TotalSeconds > 1.0)
                        time = $"{_t.Seconds} seconds and {_t.Milliseconds} millisconds";
                    else
                        time = $"{_t.Milliseconds} millisconds";

                    Console.Write($"{_out} ({time})\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                filePasses = null;
                files = null;
                filePassesTimes = null;
            }
            else
            {
                Console.WriteLine("Haggis Interpreter written in C# by TheE7Player");
                Console.WriteLine(@"https://www.github.com/TheE7Player/HaggisInterpreter");

                Console.WriteLine();
                Console.WriteLine("You started this application without any parameters. Please pass in a folder or files to run this evaluator\n~ Thank you.");
            }
            Console.Read();
        }
    }
}