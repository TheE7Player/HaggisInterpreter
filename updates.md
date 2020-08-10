# Updates

> This page shows what the interpreter can do in each given stage of development
>
> *Dates are based on the date the repo commit took place*

------

## Build 0.1 (2nd July 2020)

### "Hello world!"

- Ability to declare global variables with `DECLEAR` (`Literal`)
  - Ability to assign it inline or leave blank with a default value
- Ability to understand the difference between a `Literal` and an `Expression`
- Uses a `Stack` data structure to perform `Expressions`
  - Can do operations such as adding, concatenation with strings etc
- Able to assign a global variable with an `Expression`
- Only able to do simple operations with `Expressions` at this current time



Current test scripts that work with this build:

- Normal Test 1.haggis

------

## Build 0.2 (3rd July 2020)

### "Getting stronger"

- Re-added `Interpreter Flags` back into the project (From my first attempt)
  - Added Flag Support for `DEBUG`: This allows to automate inputs if given the right name
    - `#<DEBUG: [name]-James>` : This will automatically put "James" in a `RECIEVE` command if the correct name is given (`RECEIVE name FROM (STRING) KEYBOARD`)
  - Added support for `RECEIVE` function
  - `SET` now has effect to create variables, but is discouraged due to its declaration nature (Good for simplicity)



Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis

------

## Build 0.3 (8th July 2020)

### "A more intelligent mind"

- Interpreter is now managed into multiple files
  - `Interpreter` : Holds variables and constructor
  - `Interpreter_Methods`: Holds the methods that run the Interpreter
- Introduced a `Call Stack` into the Interpreter
  - For now, it will only stack `script run`, as `procedures` and `functions` aren't implemented yet
- Improved Error System
  - Points arrows on where the exception happens
    - Handled Exception: The error happens in the script itself (Safe)
    - Unhandled Exception: Unexcepted error happened from attempting to evaluate an expression (Non-Safe)

Here is an example of one (Cleaned up):

```
== RUNNING: Manual Set ==
Line 7 : Col 10 - ASSIGNMENT FAULT: NEEDED "TO" TO ASSIGN A VARIABLE, GOT ATO INSTEAD!

SET myVar ATO 50
          ^^^
=========================

HEAP ON EXECUTION:
[myVar] 0

CALLSTACK ON EXECUTION:
[0] script run

== Finished: Manual Set ==

OK

RESULTS:
Manual Set.haggis : FAIL
```

This example shows where we went wrong with our script, we did `ATO` instead of typing `TO`

- Ability to run now from an executable (Require parameters!)
  - You can reference multiple scripts to run in sequence
  - You can reference a folder to run from (One param only)

> Program will not allow you to use a mixture of files and folders.
>
> If wanting to run files in a folder, you only reference the folder location. Only 1 parameter is needed



This update focuses only to improve the logic, no further features or options were added in this build...



Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis

## Build 0.4 (9th July 2020)

### "Brackets Galore!"

- Added Title to the Console and show message if running without any parameters
- Added ability to apply Brackets, based on `Highest Order`
  - This means it can perform `BOMDAS` / `PEMDAS` calculations

Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis
- CtoFCalcuator.haggis

## Build 0.5 (10th July 2020)

### "To be or not to be?" - That doesn't answer my question...

- Fixed `REAL` conversion error that occurs
- Fixed spelling error when the comparing unbalanced data types
  - Comparing `INTERGER` as `REAL`
  - Comparing `REAL` as `INTERGER`
- Added support for IF statements
  - Added `Vertical If Statement`
    - You can only assign one statement on `True` and `False` clause, error if you assign more functionality to a vertical statement
  - Added `Horizontal If Statement`
    - Here you can call as many functions or commands as you like, you cannot do this with the `Vertical If Statement`
  - Can support `NOT` function wrapper or `!=`
  - Can support an extra `IF` statement from an `ELSE` clause
- Fixed wrong operator used for comparing options in `IF` statement ( `==` to `=` )
- Fixed comparisons return type if evaluating instead of modifying (`INTERGER` to `BOOLEAN`)
  - This made it hard to evaluate `NOT` expressions
  - This would also impact values of type `INTERGER`

- Fixed conversion issue of data types
- Cached local string to prevent unnecessary CPU cycle wastes ( Script Name )
- Added timer on how long the script executed for (Minutes, Seconds and Milliseconds)

Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis
- CtoFCalcuator.haggis
- If statement.haggis

## Build 0.6 (19th July 2020)

### "Goodbye Stacks & Queues"

- Changed the data structure from `Stack` and `Queue` to `Blocks`
  - This allows more flexibility to expand the logic without affecting the whole project
  - Code did expand by `75%` by using this structure (`+59 Lines`)
- Fixed a few grammar issues with error messages
- More improvements to cache and performance
- Ability to use `Pseudo` `String` Functions
  - `Lower`, `Upper`, `Trim`, `Title`
  - Look at `StrFunctions.haggis` to see these pseudo-functions in action!



Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis
- CtoFCalcuator.haggis
- If statement.haggis
- StrFunctions.haggis



## Build 0.7 (24th July 2020)

### "Stop punching yourself, Stop punching yourself, Stop punch..."

- Added support for `REPEAT` & `WHILE` iterations

- Fixed function wrapper for `NOT` function (`NOT(number = 1)`)

- Added function evaluation support

- Improved Abstractness of Block Logic

- Fixed behaviours that ignore a single block as a valid return value

- Fixed confusing logic with operands (Which made it complex in logic, but oh well.)

- Fixed sorting behaviour to respond to its original pattern

  

  **OPTIMISATIONS**

- Removed unnecessary `NuGet` package that was never used

- Cleaned up any unused `C# Library` that was appended automatically by `IntelliSense`

- Program version variable is now set to `read-only` to prevent any issues

- Removed `ternary operator` over a more simplified expression (`Block.cs`)

- Removed unused functions `GetOperands` & `FindFunc` from `Expressions.cs`

- Commented out `args` variable until `multiple function arguments` are implemented

- `#pragma` to tell `IntelliSense` to ignore the `file` array as it gets initialised in run-time

- Improved `Value.cs` 

  - by simplifying `Interpolation`
    - `this.ToString()` to `this`
  - Improving null checks
  - Using `Inline Variable Declaration` for `out` parameters
  - Simplifying conditional for Boolean conversion 

- Using discard string `_` for `END IF` check



> All these optimisations help the program to evaluate the commands and keywords much safer and faster.

Current test scripts that work with this build:

- Normal Test 1.haggis
- Input.haggis
- Manual Set.haggis
- CtoFCalcuator.haggis
- If statement.haggis
- StrFunctions.haggis
- RepeatLoop.haggis
- WhileLoop.haggis

## Build 0.8 (10th August 2020)

### "Its GUI time"

- Turned flag algorithm into a usable function for portability

- Fixed spelling for `INTEGER` `RECEIVE` errors (Was saying it was a `REAL`)

- Fixed input fault which made any input blank

- Added new argument `-input_output_only` to ignore any additional information

- Changed waiting behaviour with newly added argument

- Fixed `token parsing` problems when a `binary operator` is in play
  
  - If an binary operand (+, -, & etc) is due to be added, it will add the empty space one place ahead of itself, this caused issues with concatenation
  
- Fixed `ComparisonBlock` logic with errors preventing the `Binary Operations` to be assigned

- Fixed issue with `Comparsion` logic with a not operator `!=` is in play
  - It only allowed the `NOT` pseudo-function wrapper
  - Added `<>` as a valid `NOT` operator
  
- Improved arguments behaviour with dash parameters ( ```-<param>``` )

- Added `Socket` Server to communicate between the GUI and the program

  - Added the following arguments:

    - `-socket`
      - Tells the Interpreter that a socket server will be in use, ports it manually to `127.0.0.1` at port `595` as `TCP` ( ` 'Haggis'` in ascii sum is: 595 )
    - `-socket-ip`
      - Tells the Interpreter to ignore default `IP Address` and use the one inputted
    - `-socket-port`
      - Tells the Interpreter to ignore default `End Point Port` and use the one inputted

  - The server communicates with the following events:

    - > Event structure: [<event fired>] < Data separated with pipeline delimiter |  >

    - `time`  (Parse data into `double[3] ` from `string[3]` )

      - Returns `<time in minutes>|<time in seconds>|<time in miliseconds>`

      - > You'll need to covert it to a double from string and use Math Floor to round down the minutes as it can go to weird exponents like +E13 etc
        >
        > Gets called on script finish - Either on complete or error

    - `variable_decl`

      - Returns `<variable name>|<variable value>`

      - > Gets called when ever a variable is created from SET or DECLEAR call

    - `variable_inpt`

      - Returns `<variable name>|<variable input value>`

      - > Gets called when RECEIVE is called, returns it's declared name and its input value
    
    - `i_server`
    
      - Returns `<ip>|<port>|<protocol>`
    
      - > Gets Called before `time` to let the `REPL` know the server's information before connection closes

- Added support for parameter less function/procedure calls

- Improved logic for pseudocode functions

- Introduced date pseudocode functions

  - `DAY`, `MONTH`, `YEAR`, `HOURS`, `MINUTES`, `SECONDS`, `"MILISECONDS"`

  - > All functions here are parameterless: ```DAY()```

- Fixed spelling issue with default variable key `CHARACTER` which was mistakenly declared being `CHAR`

- Added support for `PROCEDURE` & `FUNCTION`

  - You can either `explicity` or `anonymously` call a function

  - > Explicit: SET < variable > TO < function >
    >
    > Anonymously: SEND < function > TO DISPLAY (Function with no given value assignment)
    >
    > Anonymously could be just calling the function as it is, without printing to result back

- Added feature when declaring a `FUNCTION` without a `RETURN` to suggest to change it into a `PROCEDURE` instead

- Fixed `Tokenization` issue with negative numbers

- Fixed issue with arguments (Due to `,`'s )

- Fixed issue with arguments which contains inner brackets

- Added conversion pseudocode functions

  - `INT`,  `REAL`
  
- Fixed interpreter from shouting on reassignment (Logic check called too soon)

- Fixed issue on key value call when it's empty (Searching in a empty `Dictionary`)

- Fixed multiple out of index exceptions with `Tokenization`

- Fixed issue with `IF` statement on `FALSE` ignoring all the lines if there is no `ELSE IF` clause (if any)

- Added `#DEBUG` flag to support `BOOLEAN`

- Moved `Tokenization` of `quotes logic` to a separate function due to `condition complexities with logic`

- Made `OrderLevel` changing more safer to lower levels

- Fixed issue with `ConditionBlock` for not assigning the correct right operand 

- Fixed issue with `FUNCTION` & `PROCEDURE` calling error ( No return was in place if it was executed well )

- Fixed Argument Parsing Issues



> All examples in the "example" folder has been tested and passed! :)