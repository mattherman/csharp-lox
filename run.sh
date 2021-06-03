#!/bin/bash

if ! [ -z "$1" ]
then
    dotnet run --project Lox -- $1
else
    dotnet run --project Lox
fi
