@echo off
rem Builds the .NET runtime (bin\Release\net8.0\publish\wwwroot\_framework). Run again after changing any C# (Program.cs, Photos.cs, SimFormulas.cs, the core).
rem Changes to wwwroot (JS, CSS, HTML) do NOT need this: the tests serve those files directly.
cd /d "%~dp0.."
dotnet publish -c Release
