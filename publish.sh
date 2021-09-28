#!/bin/bash

LINUX=linux-x64
LINUX_SELFCONTAINED=$LINUX-selfcontained
WINDOWS=win-x64
WINDOWS_SELFCONTAINED=$WINDOWS-selfcontained

# Linux, self-contained
echo "Publishing $LINUX_SELFCONTAINED..."
dotnet publish -c Release -r linux-x64 -o publish/$LINUX_SELFCONTAINED -p:PublishSingleFile=true
zip -j $LINUX_SELFCONTAINED.zip publish/$LINUX_SELFCONTAINED/Lox
echo "Publish complete!"

# Windows, self-contained
echo "Publishing $WINDOWS_SELFCONTAINED..."
dotnet publish -c Release -r win-x64 -o publish/$WINDOWS_SELFCONTAINED -p:PublishSingleFile=true
zip -j $WINDOWS_SELFCONTAINED.zip publish/$WINDOWS_SELFCONTAINED/Lox.exe
echo "Publish complete!"

# Linux, framework-dependent
echo "Publishing $LINUX..."
dotnet publish -c Release -r linux-x64 -o publish/$LINUX -p:PublishSingleFile=true --self-contained false
zip -j $LINUX.zip publish/$LINUX/Lox
echo "Publish complete!"

# Windows, framework-dependent
echo "Publishing $WINDOWS..."
dotnet publish -c Release -r win-x64 -o publish/$WINDOWS -p:PublishSingleFile=true --self-contained false
zip -j $WINDOWS.zip publish/$WINDOWS/Lox.exe
echo "Publish complete!"