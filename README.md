# csharp-lox

[Lox](http://www.craftinginterpreters.com) interpreter written in C#.

## Releases

You can download the executable from the [Releases](https://github.com/mattherman/csharp-lox/releases) page. There are artifacts for both Linux and Windows. If you do not have .NET 5 installed you will need to use the `-self-contained` binaries.

## TODO

The interpreter is finished, but there are a number of additions I would like to make:
* Language Features
  * Arrays
  * Lambdas
  * Escape sequences in strings
  * Optional function arguments
* Standard Library
  * File IO
* Tools
  * Create a VS Code extension to add language support ([In progress](http://github.com/mattherman/lox-vscode))
  * Ability to run Lox scripts, ex. source files with `#!/bin/lox`
  * REPL commands (e.g. `#help`, `#load <file>`)
