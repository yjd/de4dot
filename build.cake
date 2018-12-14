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
        settings.SetConfiguration(configuration));

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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NetCoreClean");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
