using System.Collections.Generic;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
#pragma warning disable IDE0044 // Add readonly modifier
        private string[] file;
#pragma warning restore IDE0044 // Add readonly modifier
        public static bool executionHandled = false;

        public Dictionary<string, Value> variables { private set; get; }
        public Stack<string> callStack { private set; get; }

        private int line;
        public static int Line;
        public static int Column;

        public int[] errorArea { private set; get; }

        private readonly string[] validTypes = {"INTEGER", "CHAR", "BOOLEAN", "REAL", "STRING"};
        private readonly Dictionary<string, dynamic> DefaultVal = new Dictionary<string, dynamic>{
            {"INTEGER", 0 },
            {"CHAR", ' ' },
            {"BOOLEAN", false },
            {"REAL", 0.0 },
            {"STRING", string.Empty }
        };

        #region INTERPRETER_FLAGS
        public struct FLAGS
        {
            /// <summary>
            /// INTERPRETER FLAG: Flag which prints the line in the script when it calls a print function (SEND)
            /// </summary>
            public bool DebugSendRequests { get; set; }
            /// <summary>
            ///  INTERPRETER FLAG: Value of flags to allow auto input if request (RECIEVE)
            /// </summary>
            public Dictionary<string, string> Inputs { get; set; }
        }

        public FLAGS _flags { get; private set; }
        #endregion

        public Interpreter(string[] Contents, FLAGS flags)
        {
            this.file = Contents;
            this.variables = new Dictionary<string, Value>(1);
            this.line = 0;
            Line = 0;
            //this.col = 0;
            this._flags = flags;
            this.callStack = new Stack<string>(1);
        }
   
    }
}
