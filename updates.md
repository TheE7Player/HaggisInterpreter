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