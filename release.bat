@echo off
@rem https://ci.appveyor.com/project/0xd4d/de4dot
echo "Script started."
echo %time%

set RELEASEDIR=.\Release
rmdir /S /Q %RELEASEDIR%
mkdir %RELEASEDIR%

set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\

@rem https://www.nuget.org/downloads
@rem Put nuget command-line executable into Tools dir of this working dir first
Tools\nuget restore de4dot.netframework.sln

set PRODUCTNAME=de4dot
set NETFRAMEWORK=net35
set NETCOREVER=netcoreapp2.1

msbuild de4dot.netframework.sln /t:clean /m /p:Configuration=Release
echo "Building %NETFRAMEWORK% release..."
msbuild de4dot.netframework.sln /p:Configuration=Release /m /verbosity:normal
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)

del %RELEASEDIR%\%NETFRAMEWORK%\*.pdb %RELEASEDIR%\%NETFRAMEWORK%\*.xml %RELEASEDIR%\%NETFRAMEWORK%\Test.Rename.*

set PATH=%ProgramFiles%\dotnet
@rem Make sure .NET Core SDK installed
dotnet restore de4dot.netcore.sln
echo "Publishing %NETCOREVER% release..."
set OUTPUTPATH=publish-%NETCOREVER%
dotnet publish -c Release -f %NETCOREVER% -o %OUTPUTPATH% %PRODUCTNAME%
dotnet clean -c Release -f %NETCOREVER% -o %OUTPUTPATH% %PRODUCTNAME%

rmdir /S /Q %RELEASEDIR%\%NETCOREVER%

del %PRODUCTNAME%\%OUTPUTPATH%\*.pdb
del %PRODUCTNAME%\%OUTPUTPATH%\*.xml

echo "Output files..."
set PATH=%systemroot%\system32\
set OUTPUT1=%PRODUCTNAME%-%NETFRAMEWORK%.zip
set OUTPUT2=%PRODUCTNAME%-%NETCOREVER%.zip

cd %RELEASEDIR%\%NETFRAMEWORK%
..\..\Tools\zip -9 -r %OUTPUT1% .
copy /y /b %OUTPUT1% ..\

cd ..\..\%PRODUCTNAME%\%OUTPUTPATH%
..\..\Tools\zip -9 -r %OUTPUT2% .
copy /y /b %OUTPUT2% ..\..\%RELEASEDIR%\

cd ..\..\
rmdir /S /Q %RELEASEDIR%\%NETFRAMEWORK%
rmdir /S /Q %PRODUCTNAME%\%OUTPUTPATH%
echo %time%
