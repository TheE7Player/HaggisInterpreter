using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
        public void Execute()
        {
            var Exit = false;

            if (callStack.Count == 0)
                callStack.Push("script run");

            while (line < file.Length)
            {
                if (file[line].Trim().StartsWith("#") || string.IsNullOrEmpty(file[line]))
                {
                    line++;
                    Line++;
                    continue;
                }

                var commands = file[line].Trim().Split();

                switch (commands[0])
                {

                    case "DECLEAR":
                        Declear(commands, true);
                        break;

                    case "SET":
                        Declear(commands, false);
                        break;

                    case "SEND":
                        Send(String.Join(" ", commands));
                        break;

                    case "RECEIVE":
                        Receive(String.Join(" ", commands));
                        break;

                    default:
                        Error($"Expected a keyword, but got: {commands[0]} instead!", commands[0]);
                        Exit = true;
                        break;
                }

                if (Exit)
                    break;

                line++;
                Line++;
            }
            callStack.Pop();
        }

        private int GetColumnFault(string toFind)
        {
            try
            {
                return file[line].Trim().IndexOf(toFind);
            }
            catch (Exception)
            {
                return -1;
            }         
        }

        internal void Error(string message, string fault)
        {
            /* Order:         
                [0] = Line fault
                [1] = Column fault 
                [2] = Length of fault
            */
            int columnFault = GetColumnFault(fault);
            this.errorArea = new int[] { line, columnFault, fault.Length };
            executionHandled = true;
            throw new Exception(message);
        }

        /// <summary>
        /// Method to assign a variable, either from SET or DECLEAR declaration
        /// </summary>
        /// <param name="information">The information passed in</param>
        /// <param name="isGlobal">True if 'DECLEAR', False if 'SET'</param>
        private void Declear(string[] information, bool isGlobal)
        {
            if (isGlobal)
            {
                // Method is "DELCEAR"
                // Pattern: [DECLEAR] <VAR NAME> [AS] <DATA TYPE> ([INITIALLY AS] <Val>)?

                // we can now assume that [0] is correct, as this method wouldn't be called if it wasn't
                var var_name = information[1];

                // Check if 'AS' is used afterwards
                if (!information[2].Equals("AS"))
                {
                    Column = GetColumnFault(information[2]);
                    Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [AS] but got: {information[2]} instead!", information[2]);
                    return;
                }

                var var_type = information[3];
                if (!validTypes.Contains(var_type))
                {
                    Column = GetColumnFault(information[3]);
                    Error($"DECLEARATION FAULT: Declear requires a realiable data type: {information[3]} instead a valid type!", information[3]);
                    return;
                }

                Value value = new Value();

                if (information.Length == 4)
                {
                    value = new Value(DefaultVal[var_type]);
                }
                else
                {
                    // "INITALLY AS" has been called (Set a value)
                    if (!information[4].Equals("INITIALLY"))
                    {
                        Column = GetColumnFault(information[4]);
                        Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [INITALLY] but got: {information[4]} instead!", information[4]);
                        return;
                    }

                    var express = Expression.GetExpression(information, 4);
                    var exp_type = Expression.GetExpressionType(express);

                    if (exp_type.Equals(Expression.ExpressionType.EXPRESSION))
                    {
                        Column = GetColumnFault(express);
                        var result = Expression.PerformExpression(this.variables, express);
                        variables.Add(var_name, result);
                        return;
                    }


                    if (var_type == "STRING" || var_type == "CHAR")
                    {
                        value = (var_type == "STRING") ? new Value(information[5]) : new Value((information[5].ToCharArray()[0]));
                    }
                    else if (var_type == "INTEGER")
                    {
                        int i;
                        if (Int32.TryParse(information[5], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                        {
                            value = new Value(i);
                        }
                        else
                        {
                            Column = GetColumnFault(information[5]);
                            Error($"ASSIGN TYPE FAULT: Failed to assign an INTEGER operation with: {information[5]}!", information[5]);
                            return;
                        }
                    }
                    else if (var_type == "REAL")
                    {
                        double i;
                        if (Double.TryParse(information[5], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                        {
                            value = new Value(i);
                        }
                        else
                        {
                            Column = GetColumnFault(information[5]);
                            Error($"ASSIGN TYPE FAULT: Failed to assign an REAL operation with: {information[5]}!", information[5]);
                            return;
                        }
                    }
                    else if (var_type == "BOOLEAN")
                    {
                        bool r;
                        if (Boolean.TryParse(information[5], out r))
                        {
                            value = new Value(r);
                        }
                        else
                        {
                            Column = GetColumnFault(information[5]);
                            Error($"ASSIGN TYPE FAULT: Failed to assign an BOOLEAN operation with: {information[5]}!", information[5]);
                            return;
                        }
                    }
                }

                // Now we assign it
                variables.Add(var_name, value);
            }
            else
            {
                // Method is "SET"
                // Pattern: [SET] <VAR NAME> [TO] <Val>
                var name = information[1];
                var express = information.Skip(3).ToArray();

                if(information[2] != "TO")
                {
                    Column = GetColumnFault(information[2]);
                    Error($"ASSIGNMENT FAULT: NEEDED \"TO\" TO ASSIGN A VARIABLE, GOT {information[2]} INSTEAD!", information[2]);
                }

                var result = Expression.PerformExpression(this.variables, String.Join(" ", express));

                if (!variables.ContainsKey(information[1]))
                {
                    variables.Add(name, new Value(result));
                    name = null;
                    express = null;
                }
                else
                {
                    variables[name] = new Value(result);
                    name = null;
                    express = null;
                }

            }
        }

        private void Send(string express)
        {
            string[] ex = Expression.Evaluate(express);

            bool endsCorrectly = (ex[ex.Length - 2] == "TO" && ex[ex.Length - 1] == "DISPLAY") ? true : false;

            if (!endsCorrectly) 
            {
                Column = GetColumnFault($"{ex[ex.Length - 2]} {ex[ex.Length - 1]}");
                Error($"Excepted \"TO DISPLAY\" ending, got {ex[ex.Length - 2]} {ex[ex.Length - 1]} instead", $"{ex[ex.Length - 2]} {ex[ex.Length - 1]}"); 
            }

            express = express.Replace("SEND", "");
            express = express.Replace("TO DISPLAY", "").Trim();

            Column = GetColumnFault(express);
            var exp = Expression.PerformExpression(this.variables, express);

            if (_flags.DebugSendRequests)
                Console.WriteLine($"LINE {this.line}: {exp.ToString()}");
            else
                Console.WriteLine(exp.ToString());
        }

        private void Receive(string express)
        {
            //[2] FROM
            //[lastIndex] KEYBOARD
            string[] ex = Expression.Evaluate(express);
            string varName = ex[1];
            string varType = ex[4];

            if (!ex[2].Equals("FROM"))
            {
                Column = GetColumnFault(ex[2]);
                Error($"Excepted \"FROM\", got {ex[2]} instead", ex[2]);
            }

            if (!ex[ex.Length - 1].Equals("KEYBOARD")) 
            {
                Column = GetColumnFault(ex[ex.Length - 1]);
                Error($"\"KEYBOARD\" is the only supported device at this time, got {ex[ex.Length - 1]} instead", ex[ex.Length - 1]); 
            }

            // Good, it meant that the syntax pattern was correct!
            string input = string.Empty;

            if (!DefaultVal.Keys.Any(y => y.Equals(varType))) {
                Column = GetColumnFault(varType);
                Error($"\"{varType}\" isn't a recognisable data type for {varName}!", varType);
            }

            // Add the variable in if it hasn't already
            if (!variables.ContainsKey(varName)) variables.Add(varName, new Value(DefaultVal[varType]));

            if (!Object.ReferenceEquals(_flags.Inputs, null))
            {
                if (_flags.Inputs.ContainsKey(varName))
                    input = _flags.Inputs[varName];
                else {
                    Column = GetColumnFault(varName);
                    Error($"{varName} isn't decleared or exists at time of execution", varName);
                }
            }
            else
                input = Console.ReadLine();

            if (varType == "STRING")
            {
                variables[varName] = new Value(input);
            }

            if (varType == "CHARACTER")
            {
                variables[varName] = new Value(input[0]);
            }

            if (varType == "REAL")
            {
                double d;
                // try to parse as double, if failed read value as string
                if (double.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                    variables[varName] = new Value(d);
                else 
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED REAL, GOT {input} INSTEAD", input);
                }
            }

            if (varType == "INTEGER")
            {
                int i;
                if (int.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                    variables[varName] = new Value(i);
                else
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED REAL, GOT {input} INSTEAD", input);
                }
            }

        }

    }
}
