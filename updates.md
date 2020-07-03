# Updates

> This page shows what the interpreter can do in each given stage of development
>
> *Dates are based on the date the repo commit took place*

------

## Build 0.1 (2nd July 2020)

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

