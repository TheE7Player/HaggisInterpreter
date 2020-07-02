using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HaggisInterpreter2;

namespace HaggisInterpreter2Run
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"C:\Users\Owner\Desktop\Haggis";

            Dictionary<string, bool> filePasses = new Dictionary<string, bool>(1);

            foreach (var file in Directory.GetFiles(folder))
            {

                if (!Path.GetExtension(file).Equals(".haggis"))
                    return;

                //filePasses.Add(Path.GetFileName(file), false);

                string[] fileFull = File.ReadAllLines(file);

                //Interpreter.FLAGS my_flags = new Interpreter.FLAGS();

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
                                //my_flags.DebugSendRequests = !my_flags.DebugSendRequests;
                                //var result = (my_flags.DebugSendRequests) ? "ON" : "OFF";
                                //Console.WriteLine($"PRINT_LINE_NUMBER IS {result}");
                                ignore_count++;
                                continue;
                            }

                            if (val.Contains('[') && val.Contains(']'))
                            {
                                var key = val.Substring(1, val.IndexOf(']') - 1);
                                var input = val.Substring(val.IndexOf('-') + 1);

                                //if (Object.ReferenceEquals(my_flags.Inputs, null))
                                //    my_flags.Inputs = new Dictionary<string, string>(1);

                                //my_flags.Inputs.Add(key, input);

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

                HaggisInterpreter2.Interpreter basic = new HaggisInterpreter2.Interpreter(File.ReadAllLines(file));
                try
                {
                    basic.Execute();
                    //filePasses[Path.GetFileName(file)] = true;
                }
                catch (Exception e)
                {
                    //filePasses[Path.GetFileName(file)] = false;
                    Console.WriteLine("ERROR:");

                    //int line = basic.lineMarker.Line;
                    //int col = basic.lineMarker.Column;

                    //var sb = new StringBuilder(fileFull[line - 1].Length);
                    //Console.WriteLine($"Line {line} : Col {col} - {e.Message}\n");

                    //for (int i = 0; i < col; i++)
                    //{
                    //    sb.Append(" ");
                    //}
                    //sb.Append("^");

                    //Console.WriteLine(fileFull[line - 1]);
                    //Console.WriteLine(sb.ToString());
                    //sb = null;

                    var lineNumber = new System.Diagnostics.StackTrace(e, true).GetFrame(1).GetFileLineNumber();
                    var fileFault = new System.Diagnostics.StackTrace(e, true).GetFrame(1).GetFileName();
                    Console.WriteLine($"{Path.GetFileNameWithoutExtension(fileFault)} @ {lineNumber}\n({e.Message})");

                    fileFault = null;
                }

                Console.WriteLine($"\n== Finished: {Path.GetFileNameWithoutExtension(file)} ==\n");
            }

            Console.WriteLine("OK");
            Console.WriteLine();

            Console.WriteLine("RESULTS:");
            foreach (var f in filePasses)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"{f.Key}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" : ");
                Console.ForegroundColor = (f.Value) ? ConsoleColor.Green : ConsoleColor.Red;
                var _out = (f.Value) ? "PASS" : "FAIL";
                Console.Write($"{_out}\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Read();
        }
    }
}
