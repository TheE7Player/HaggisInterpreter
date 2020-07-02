using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    public class Interpreter
    {
        private string[] file;
        private Dictionary<string, Value> variables;

        private int line;
        private int col;

        private string[] validTypes = {"INTEGER", "CHAR", "BOOLEAN", "REAL", "STRING"};
        private Dictionary<string, dynamic> DefaultVal = new Dictionary<string, dynamic>{
            {"INTEGER", 0 },
            {"CHAR", ' ' },
            {"BOOLEAN", false },
            {"REAL", 0.0 },
            {"STRING", string.Empty }
        };

        public Interpreter(string[] Contents)
        {
            this.file = Contents;
            this.variables = new Dictionary<string, Value>(1);
            this.line = 0;
            this.col = 0;
        }

        public void Execute()
        {
            var Exit = false;
            while (line < file.Length)
            {
                if (file[line].Trim().StartsWith("#") || string.IsNullOrEmpty(file[line]))
                {
                    line++;
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

                    default:
                        Error($"Expected a keyword, but got: {commands[0]} instead!");
                        Exit = true;
                        break;
                }

                if (Exit)
                    break;

                line++;
            }
        }

        private void Error(string message)
        {
            throw new Exception(message);
        }

        #region Methods

       
        /// <summary>
        /// Method to assign a variable, either from SET or DECLEAR declaration
        /// </summary>
        /// <param name="information">The information passed in</param>
        /// <param name="isGlobal">True if 'DECLEAR', False if 'SET'</param>
        private void Declear(string[] information, bool isGlobal)
        {
            if(isGlobal)
            {
                // Method is "DELCEAR"
                // Pattern: [DECLEAR] <VAR NAME> [AS] <DATA TYPE> ([INITIALLY AS] <Val>)?

                // we can now assume that [0] is correct, as this method wouldn't be called if it wasn't
                var var_name = information[1];

                // Check if 'AS' is used afterwards
                if(!information[2].Equals("AS"))
                {
                    Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [AS] but got: {information[2]} instead!");
                    return;
                }

                var var_type = information[3];
                if(!validTypes.Contains(var_type))
                {
                    Error($"DECLEARATION FAULT: Declear requires a realiable data type: {information[3]} instead a valid type!");
                    return;
                }

                Value value = new Value();

                if(information.Length == 4)
                {
                    value = new Value(DefaultVal[var_type]);
                }
                else
                {
                    // "INITALLY AS" has been called (Set a value)
                    if (!information[4].Equals("INITIALLY"))
                    {
                        Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [INITALLY] but got: {information[4]} instead!");
                        return;
                    }

                    var express = Expression.GetExpression(information, 4);
                    var exp_type = Expression.GetExpressionType(express);

                    if(exp_type.Equals(Expression.ExpressionType.EXPRESSION))
                    {
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
                            Error($"ASSIGN TYPE FAULT: Failed to assign an INTEGER operation with: {information[5]}!");
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
                            Error($"ASSIGN TYPE FAULT: Failed to assign an REAL operation with: {information[5]}!");
                            return;
                        }
                    }
                    else if(var_type == "BOOLEAN")
                    {
                        bool r;
                        if(Boolean.TryParse(information[5], out r))
                        {
                            value = new Value(r);
                        }
                        else
                        {
                            Error($"ASSIGN TYPE FAULT: Failed to assign an BOOLEAN operation with: {information[5]}!");
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
            }
        }

        private void Send(string express)
        {
            string[] ex = Expression.Evaluate(express);

            bool endsCorrectly = (ex[ex.Length - 2] == "TO" && ex[ex.Length - 1] == "DISPLAY") ? true : false;

            if (!endsCorrectly)
                throw new Exception($"Excepted \"TO DISPLAY\" ending, got {ex[ex.Length - 2]} {ex[ex.Length - 1]} instead");

            express = express.Replace("SEND", "");
            express = express.Replace("TO DISPLAY", "").Trim();

            var exp = Expression.PerformExpression(this.variables, express);
            Console.WriteLine(exp.ToString());
        }
        #endregion
    }
}
