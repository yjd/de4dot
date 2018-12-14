// #tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./") + Directory(configuration);
var netCoreVer = "netcoreapp2.2";
var netFrameworkVer = "net472";

var dotnetsettings = new DotNetCorePublishSettings
    {
        Framework = netCoreVer,
        Configuration = "Release",
        OutputDirectory = buildDir + Directory("publish-" + netCoreVer)
    };

var cleansettings = new DotNetCoreCleanSettings
     {
         Framework = netCoreVer,
         Configuration = "Release",
        OutputDirectory = buildDir + Directory("publish-" + netCoreVer)
     };

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./de4dot.netframework.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
      // Use MSBuild
      MSBuild("./de4dot.netframework.sln", settings =>
        settings.SetConfiguration(configuration)
        .SetMaxCpuCount(System.Environment.ProcessorCount)
        );

});

Task("NetCoreBuild")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCorePublish("de4dot", dotnetsettings);

});

Task("NetCoreClean")
    .IsDependentOn("NetCoreBuild")
    .Does(() =>
{
    DotNetCoreClean("de4dot", cleansettings);

});

//Task("Run-Unit-Tests")
    //.IsDependentOn("Build")
    //.Does(() =>
//{
    //NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        //NoResults = true
        //});
//});

Task("Zip-Files")
    .IsDependentOn("NetCoreClean")
    .Does(() =>
{
    // .NET Framework
    DeleteFiles($"{buildDir}/{netFrameworkVer}/*.pdb");
    DeleteFiles($"{buildDir}/{netFrameworkVer}/*.xml");
    DeleteFiles($"{buildDir}/{netFrameworkVer}/Test.Rename.*");
    Zip(buildDir + Directory(netFrameworkVer), $"{buildDir}/de4dot-{netFrameworkVer}.zip");

    // .NET Core
    DeleteFiles($"{buildDir}/publish-{netCoreVer}/*.pdb");
    DeleteFiles($"{buildDir}/publish-{netCoreVer}/*.xml");
    Zip(buildDir + Directory("publish-" + netCoreVer), $"{buildDir}/de4dot-{netCoreVer}.zip");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Zip-Files");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
