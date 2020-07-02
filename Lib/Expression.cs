using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    static class Expression
    {
        private static char[] validOperations = new char[] { '+', '/', '*', '-', '(', ')', '&', '!', '=' };
        public enum ExpressionType
        {
            /// <summary>
            /// LITERAL - Constant number, expressed at compile time: 0, 200, "Hey"
            /// </summary>
            LITERAL,

            /// <summary>
            /// EXPRESSION - Modification of one or more values at run time (Comparsion, Concat, Addition etc)
            /// </summary>
            EXPRESSION
        }

        #region Expression Logic
        public static string GetExpression(string[] values, int exprStartIndex)
        {
            exprStartIndex++;
            var sb = new StringBuilder();

            for (int i = exprStartIndex; i < values.Length; i++)
                if (i < (values.Length - 1))
                    sb.Append($"{values[i]} ");
                else
                    sb.Append(values[i]);

            return sb.ToString();
        }

        public static ExpressionType GetExpressionType(object val)
        {
            if (val.ToString().ToCharArray().Any(x => validOperations.Contains(x)))
                return ExpressionType.EXPRESSION;
            
            return ExpressionType.LITERAL;        
        }

        public static string[] Evaluate(string Expression)
        {
            // Clean up the string
            var iter = Expression.Trim().ToCharArray();
            var sb = new StringBuilder();
            List<string> output = new List<string>(2);

            // 34 -> "
            // 32 -> (whitespace)
            for (int i = 0; i < iter.Length; i++)
            {
                // Is current char number '32'
                if ((int)iter[i] == 32)
                {
                    if (sb.Length > 0)
                    {
                        output.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }


                if ((int)iter[i] == 34)
                {
                    i++;
                    while ((int)iter[i] != 34)
                    {
                        sb.Append(iter[i]);
                        i++;
                    }
                    output.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                else
                {
                    sb.Append(iter[i]);
                }
            }

            if (sb.Length > 0)
            {
                output.Add(sb.ToString());
                sb.Clear();
            }

            iter = null;
            sb = null;

            return output.ToArray();
        }

        private static Value DoExpr(Value l, string op, Value r)
        {
           // Value l = ((Value)(Object.ReferenceEquals(forceCompare, null) ? this : forceCompare));

            if (l.Type != r.Type)
            {
                // promote one value to higher type

                // Ignore if the left hand is a string
                bool lOptIgnore = (l.Type == ValueType.STRING || l.Type == ValueType.CHARACTER) ? true : false;
                if (lOptIgnore == false)
                {
                    if (r.Type != ValueType.STRING)
                        if (l.Type > r.Type)
                            r = r.Convert(l.Type);
                        else
                            l = l.Convert(r.Type);
                }
            }

            if (op.Equals("+"))
            {
                if (l.Type == ValueType.REAL)
                    return new Value(l.REAL + r.REAL);

                if (l.Type == ValueType.INTEGER)
                    return new Value(l.INT + r.INT);

                if (l.Type == ValueType.CHARACTER || l.Type == ValueType.STRING)
                    throw new Exception("Left opp cannot use + to join text together! Use '&' instead");

                if (r.Type == ValueType.CHARACTER || r.Type == ValueType.STRING)
                    throw new Exception("Right opp cannot use + to join text together! Use '&' instead");
            }
            else if (op.Equals("&"))
            {

                if (l.Type == ValueType.STRING || r.Type == ValueType.STRING)
                    return new Value(l.ToString() + r.ToString());

                // You cannot append l char, need to convert to string (C# Does that automatically for us)
                if (l.Type == ValueType.CHARACTER || r.Type == ValueType.CHARACTER)
                    return new Value(l.CHARACTER + r.STRING);
            }
            else if (op.Equals("=="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL ? 1 : 0);
                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING ? 1 : 0);
                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN ? 1 : 0);
                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT ? 1 : 0);
                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER ? 1 : 0);
                }
            }
            else if (op.Equals("!="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL ? 0 : 1);
                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING ? 0 : 1);
                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN ? 0 : 1);
                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT ? 0 : 1);
                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER ? 0 : 1);
                }
            }
            else
            {
                if (l.Type == ValueType.STRING)
                    throw new Exception("Cannot perform requested \"BinOp\" on l \"STRING\"");

                if (l.Type == ValueType.CHARACTER)
                    throw new Exception("Cannot perform requested \"BinOp\" on l \"CHARACTER\"");

                if (l.Type == ValueType.REAL)
                    switch (op)
                    {
                        case "-": return new Value(l.REAL - r.REAL);
                        case "*": return new Value(l.REAL * r.REAL);
                        case "/": return new Value(l.REAL / r.REAL);
                        //case Token.Caret: return new Value(Math.Pow(l.REAL, r.REAL));
                        case "<": return new Value(l.REAL < r.REAL ? 1 : 0);
                        case ">": return new Value(l.REAL > r.REAL ? 1 : 0);
                        case "<=": return new Value((l.REAL <= r.REAL) ? 1 : 0);
                        case ">=": return new Value(l.REAL >= r.REAL ? 1 : 0);
                        case "AND": return new Value((l.REAL != 0) && (r.REAL != 0) ? 1 : 0);
                        case "OR": return new Value((l.REAL != 0) || (r.REAL != 0) ? 1 : 0);
                        case "!=": return new Value((l.REAL != 0) != (r.REAL != 0) ? 1 : 0);
                    }

                if (l.Type == ValueType.INTEGER)
                    switch (op)
                    {
                        case "-": return new Value(l.INT - r.INT);
                        case "*": return new Value(l.INT * r.INT);
                        case "/": return new Value(l.INT / r.INT);
                        //case Token.Caret: return new Value(Math.Pow(l.INT, r.INT));
                        case "<": return new Value(l.INT < r.INT ? 1 : 0);
                        case ">": return new Value(l.INT > r.INT ? 1 : 0);
                        case "<=": return new Value(l.INT <= r.INT ? 1 : 0);
                        case ">=": return new Value(l.INT >= r.INT ? 1 : 0);
                        case "AND": return new Value((l.INT != 0) && (r.INT != 0) ? 1 : 0);
                        case "OR": return new Value((l.INT != 0) || (r.INT != 0) ? 1 : 0);
                        case "!=": return new Value((l.INT != 0) != (r.INT != 0) ? 1 : 0);
                    }

                if (l.Type == ValueType.BOOLEAN)
                    switch (op)
                    {
                        case "AND": return new Value((l.BOOLEAN != false) && (r.BOOLEAN != false) ? 1 : 0);
                        case "OR": return new Value((l.BOOLEAN != false) || (r.BOOLEAN != false) ? 1 : 0);
                        case "NOT": return new Value((l.BOOLEAN != false) != (r.BOOLEAN != false) ? 1 : 0);
                    }
            }

            throw new Exception("Unknown binary operator.");
        }

        #endregion

        #region Expression Operations
        public static Value PerformExpression(Dictionary<string, Value> vals, string expression)
        {      
            var exp = Evaluate(expression.ToString());
            //Type finalType = exp[0].GetType();

            var stack = new Stack(exp.Length + 4);

            // Populate the stack
            foreach (var item in exp)
               stack.Push(item);

            bool runCycle = false;

            while(stack.Count > 0)
            {
                if (stack.Count == 1)
                {
                    if (runCycle)
                        break;
                    else
                    {
                        var singleObject = stack.Pop();
                        if (vals.ContainsKey(singleObject.ToString()))
                            singleObject = vals[singleObject.ToString()];
                        else
                            singleObject = new Value(singleObject as string, true);

                        stack.Push(singleObject);
                        runCycle = true;
                        continue;
                    }
                }
                
                var rightOp = stack.Pop();
                if (vals.ContainsKey(rightOp.ToString()))
                    rightOp = vals[rightOp.ToString()];
                else if( rightOp.GetType().ToString() != "HaggisInterpreter2.Value")
                    rightOp = new Value(rightOp as string, true);

                var opOper = (string)stack.Pop();
                var leftOp = stack.Pop();
                if (vals.ContainsKey(leftOp.ToString()))
                    leftOp = vals[leftOp.ToString()];
                else if (leftOp.GetType().ToString() != "HaggisInterpreter2.Value")
                    leftOp = new Value(leftOp as string, true);

                var eval = DoExpr((Value)leftOp, opOper, (Value)rightOp);

                stack.Push(eval);
                runCycle = true;
            }

            return (Value)stack.Pop();
        }

        #endregion
    }
}
