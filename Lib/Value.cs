using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    
        public enum ValueType
        {
            REAL,
            STRING,
            INTEGER,
            BOOLEAN,
            CHARACTER
        }

        public struct Value
        {
            #region Meta Data
            public static readonly Value Zero = new Value(0);
            public ValueType Type { get; set; }
            #endregion

            #region Data types
            public double REAL { get; set; }
            public int INT { get; set; }
            public bool BOOLEAN { get; set; }
            public string STRING { get; set; }
            public char CHARACTER { get; set; }

            #endregion

            #region Constructors

            public Value(Value val) : this() { this = val; }

            public Value(double val) : this()
            {
                this.Type = ValueType.REAL;
                this.REAL = val;
            }

            public Value(string val, bool evalType = false) : this()
            {
                if (!evalType) 
                { 
                    this.Type = ValueType.STRING;
                    this.STRING = val;
                }
                else
                {
                    Int32 intVal;
                    double doubleVal;

                    if(val.ToLower() == "false" || val.ToLower() == "true")
                    {
                        this.Type = ValueType.BOOLEAN;
                        this.BOOLEAN = (val.ToLower() == "false") ? false:true;
                    }
                    else if (Int32.TryParse(val, out intVal))
                    {
                        this.Type = ValueType.INTEGER;
                        this.INT = intVal;
                    }
                    else if (double.TryParse(val, out doubleVal))
                    {
                        this.Type = ValueType.REAL;
                        this.REAL = doubleVal;
                    }
                    else
                    {
                        if(val.Length == 1)
                        {
                            this.Type = ValueType.CHARACTER;
                            this.CHARACTER = val[0];
                        }
                        else
                        {
                            this.Type = ValueType.STRING;
                            this.STRING = val;
                        }
                    }
                    

                }

            }

            public Value(int val) : this()
            {
                this.Type = ValueType.INTEGER;
                this.INT = val;
            }

            public Value(bool val) : this()
            {
                this.Type = ValueType.BOOLEAN;
                this.BOOLEAN = val;
            }

            public Value(char val) : this()
            {
                this.Type = ValueType.CHARACTER;
                this.CHARACTER = val;
            }

            #endregion

            public Value Convert(ValueType type)
            {
                //TODO: Work on this functionality
                if (this.Type != type)
                {
                    string target;
                    switch (type)
                    {
                        case ValueType.REAL:
                            double d;
                            target = (!Object.ReferenceEquals(this.STRING, null) ? this.STRING : this.INT.ToString());
                            // try to parse as double, if failed read value as string
                            if (double.TryParse(target, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                            {
                                this.REAL = new Value(d).REAL;
                                this.Type = ValueType.REAL;
                            }
                            else
                            {
                                throw new Exception($"ERROR: Failed to attempt to covert {this.ToString()} as REAL");
                            }
                            break;

                        case ValueType.INTEGER:
                            // Change INT to REAL
                            int i;
                            target = (!Object.ReferenceEquals(this.STRING, null) ? this.STRING : this.REAL.ToString());

                            if (Int32.TryParse(target, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                            {
                                this.REAL = new Value(i).INT;
                                this.Type = ValueType.INTEGER;
                            }
                            else
                            {
                                throw new Exception($"ERROR: Failed to attempt to covert {this.ToString()} as INT");
                            }
                            break;

                        case ValueType.STRING:
                            this.STRING = this.REAL.ToString();
                            this.Type = ValueType.STRING;
                            break;
                    }
                }
                return this;
            }

            public override string ToString()
            {
                if (this.Type == ValueType.REAL)
                    return this.REAL.ToString();

                if (this.Type == ValueType.INTEGER)
                    return this.INT.ToString();

                if (this.Type == ValueType.BOOLEAN)
                    return this.BOOLEAN == true ? "TRUE" : "FALSE";

                if (this.Type == ValueType.CHARACTER)
                    return this.CHARACTER.ToString();

                return this.STRING;
            }
        }
}
