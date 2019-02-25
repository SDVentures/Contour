@echo off
SET PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%
@echo on
dotnet restore dotnet-fake.csproj
dotnet fake %*
