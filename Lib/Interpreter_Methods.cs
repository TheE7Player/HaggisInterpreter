using System;
using System.Linq;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
        #region Logic
        private bool MoreThanOneFunction(string input)
        {
            var validFunctions = new string[] { "DECLEAR", "SET", "SEND", "RECEIVE" };
            var inArr = input.Trim().Split();
            var count = inArr.Count(x => validFunctions.Contains(x));
            return (count > 1);
        }

        private bool _execute(string[] executionLine)
        {
            string joinedExpression = String.Join(" ", executionLine);
            switch (executionLine[0])
            {
                case "DECLEAR":
                    Declear(executionLine, true);
                    return false;

                case "SET":
                    Declear(executionLine, false);
                    return false;

                case "SEND":
                    Send(joinedExpression);
                    return false;

                case "RECEIVE":
                    Receive(joinedExpression);
                    return false;

                case "IF":
                    If(joinedExpression);
                    return false;

                case "REPEAT":
                    Loop(true);
                    return false;

                case "WHILE":
                    Loop(false);
                    return false;

                default:
                    Error($"Expected a keyword, but got: {executionLine[0]} instead!", executionLine[0]);
                    return true;
            }
        }

        private string GetNextLine(int ForceIndex = -1, bool Trim = true)
        {
            if (ForceIndex > -1)
            {
                if (ForceIndex > file.Length)
                    throw new Exception($"Supplied ForceIndex has succeeded past the max value (Interpreter called for {ForceIndex}, but max is: {file.Length})");

                line = ForceIndex;
                Line = ForceIndex;
            }

            while (line < file.Length)
            {
                if (file[line].Trim().StartsWith("#") || string.IsNullOrEmpty(file[line]))
                {
                    line++; Line++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (line == file.Length)
                return null;

            line++; Line++;
            return (Trim) ? file[line - 1].Trim() : file[line - 1];
        }

        public void Execute()
        {
            bool Exit = false;

            if (callStack.Count == 0)
                callStack.Push("script run");

            string line;
            while ((line = GetNextLine()) != null)
            {
                Exit = _execute(line.Split());

                if (Exit)
                    break;
            }

            callStack.Pop();
        }

        private int GetColumnFault(string toFind)
        {
            try
            {
                return file[line - 1].Trim().IndexOf(toFind);
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

        #endregion

        #region Keywords Functionality

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
                        if (Int32.TryParse(information[5], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int i))
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
                        if (Double.TryParse(information[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double i))
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
                        if (Boolean.TryParse(information[5], out bool r))
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

                if (information[2] != "TO")
                {
                    Column = GetColumnFault(information[2]);
                    Error($"ASSIGNMENT FAULT: NEEDED \"TO\" TO ASSIGN A VARIABLE, GOT {information[2]} INSTEAD!", information[2]);
                }

                var result = Expression.PerformExpression(this.variables, String.Join(" ", express));

                if (!variables.ContainsKey(information[1]))
                {
                    variables.Add(name, new Value(result));
                    name = null; express = null;
                }
                else
                {
                    variables[name] = new Value(result);
                    name = null; express = null;
                }
            }
        }

        private void Send(string express)
        {
            string[] ex = Expression.Evaluate(express);

            bool endsCorrectly = (ex[ex.Length - 2] == "TO" && ex[ex.Length - 1] == "DISPLAY");

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
                Console.WriteLine($"LINE {this.line}: {exp}");
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

            if (!DefaultVal.Keys.Any(y => y.Equals(varType)))
            {
                Column = GetColumnFault(varType);
                Error($"\"{varType}\" isn't a recognisable data type for {varName}!", varType);
            }

            // Add the variable in if it hasn't already
            if (!variables.ContainsKey(varName)) variables.Add(varName, new Value(DefaultVal[varType]));

            if (!(_flags.Inputs is null))
            {
                if (_flags.Inputs.ContainsKey(varName))
                    input = _flags.Inputs[varName];
                else
                {
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
                // try to parse as double, if failed read value as string
                if (double.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    variables[varName] = new Value(d);
                else
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED REAL, GOT {input} INSTEAD", input);
                }
            }

            if (varType == "INTEGER")
            {
                if (int.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int i))
                    variables[varName] = new Value(i);
                else
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED REAL, GOT {input} INSTEAD", input);
                }
            }
        }

        private void If(string expression)
        {       
            #region Verticle If Statement

            if (expression.StartsWith("IF") && expression.EndsWith("END IF") && expression.Contains("THEN"))
            {
                Column = GetColumnFault("IF");
                string condition_expression = expression.Substring(3, expression.IndexOf("THEN") - 3).Trim();
                Column = GetColumnFault(condition_expression);
                var result = Expression.PerformExpression(this.variables, condition_expression);
                string trueExpression;
                string falseExpression;

                //Avoid evaluation on horiz. IF statment if it evaluates to false (No need to waste cpu cycles)
                if (!expression.Contains("ELSE") && result.BOOLEAN == false)
                    return;

                int tIndex = expression.IndexOf("THEN") + 4;
                int tIndexEnd = expression.LastIndexOf("ELSE");
                int fIndexEnd = expression.LastIndexOf("END");

                if (expression.Contains("ELSE"))
                {
                    trueExpression = expression.Substring(tIndex, tIndexEnd - tIndex).Trim();

                    if (MoreThanOneFunction(trueExpression))
                    {
                        Column = GetColumnFault(trueExpression);
                        Error("EXPRESSION FAULT: HAD MORE THAN 1 STATEMENT IN A SINGLE LINE, USE VERTICAL IF STATEMENT INSTEAD", trueExpression);
                        return;
                    }

                    falseExpression = expression.Substring(tIndexEnd + 4, fIndexEnd - (tIndexEnd + 4)).Trim();

                    if (MoreThanOneFunction(falseExpression))
                    {
                        Column = GetColumnFault(falseExpression);
                        Error("EXPRESSION FAULT: HAD MORE THAN 1 STATEMENT IN A SINGLE LINE, USE VERTICAL IF STATEMENT INSTEAD", trueExpression);
                        return;
                    }

                    if (result.BOOLEAN == true)
                    { _execute(trueExpression.Split()); return; }
                    else
                    { _execute(falseExpression.Split()); return; }
                }
                else
                    trueExpression = expression.Substring(tIndex, fIndexEnd - tIndex).Trim();

                _execute(trueExpression.Split());
            }

            #endregion Verticle If Statement

            #region Horizontal If Statement

            if (expression.StartsWith("IF") && expression.EndsWith("THEN"))
            {
                //TODO: Handle if 'ELSE' or 'END IF' isn't included?

                Column = GetColumnFault("IF");
                string condition_expression = expression.Substring(3, expression.IndexOf("THEN") - 3).Trim();
                Column = GetColumnFault(condition_expression);
                var result = Expression.PerformExpression(this.variables, condition_expression);

                // If false, skip to when we reach `ELSE` case
                if (result.BOOLEAN == false)
                {
                    // Skip the lines till we hit 'ELSE'
                    string _l;
                    while (!(_l = GetNextLine()).Contains("ELSE")) { }

                    if (_l.StartsWith("ELSE IF"))
                    {
                        _execute(_l.Trim().Substring(5).Split());
                    }
                    else
                    {
                        while ((_l = GetNextLine()) != "END IF")
                            _execute(_l.Trim().Split());

                        GetNextLine();
                    }
                }
                else
                {
                    // Result is true
                    string _l;
                    while (!(_l = GetNextLine()).Contains("ELSE"))
                    {
                        if (_l == "END IF")
                            return;

                        _execute(_l.Trim().Split());
                    }

                    while ((_ = GetNextLine()) != "END IF") { }
                }
            }

            #endregion Horizontal If Statement
        }

        private void Loop(bool isRepeatLoop = true)
        {

            int iterStart = line;
            string condition = string.Empty;

            bool Exit = false;

            if(isRepeatLoop)
            {
                // REPEAT loop
                string _line;
                while ((_line = GetNextLine()) != null)
                {
                    if (_line.StartsWith("UNTIL"))
                    {
                        if(object.ReferenceEquals(condition, string.Empty))
                            condition = _line.Substring(6);

                        Value result = Expression.PerformExpression(variables, condition);

                        if (!result.BOOLEAN)
                        {
                            break;
                        }
                        else
                        {
                            line = iterStart;
                            Line = line;
                        }
                    }
                    else 
                    {  
                        Exit = _execute(_line.Split());

                        if (Exit)
                            break;
                    }
                }              
            }
            else
            {
                // WHILE loop
                string _line;

                if (!(file[line - 1].EndsWith("DO")))
                {
                    if(!(file[line - 1].StartsWith("DO")))
                    {
                        var words = file[line - 1].Split();
                        Column = GetColumnFault(words[words.Length - 1]);
                        Error("Missing expression ender for WHILE loop - Please a 'DO' after expression", words[words.Length - 1]);
                    }
                }

                condition = file[line-1].Substring(6);
                condition = condition.Substring(0, condition.Length - 2).Trim();

                Value result = Expression.PerformExpression(variables, condition);

                if (!result.BOOLEAN)
                    return;

                while ((_line = GetNextLine()) != null)
                {
                    if (_line.StartsWith("END WHILE"))
                    {
                        result = Expression.PerformExpression(variables, condition);

                        if (!result.BOOLEAN)
                        {
                            break;
                        }
                        else
                        {
                            line = iterStart;
                            Line = line;
                        }
                    }
                    else
                    {
                        Exit = _execute(_line.Split());

                        if (Exit)
                            break;
                    }
                }
            }

        }

        #endregion
    }
}