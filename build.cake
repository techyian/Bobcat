#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.Prompt&version=1.0.15
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");
var clientRuntime = Argument("runtime", "linux-arm");
var framework = Argument("framework", "netcoreapp3.1");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var commonDir = Directory("./src/Bobcat.Common/bin") + Directory(configuration);
var webDir = Directory("./src/Bobcat.Web/bin") + Directory(configuration);
var clientDir = Directory("./src/Bobcat.Client/bin") + Directory(configuration);
var clientProject = Directory("./src/Bobcat.Client/Bobcat.Client.csproj");

DirectoryPath solutionDir = MakeAbsolute(Directory("./"));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(commonDir);	
	CleanDirectory(webDir);
	CleanDirectory(clientDir);	
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./Bobcat.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./Bobcat.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./Bobcat.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

//Task("Run-Unit-Tests")
//    .IsDependentOn("Build")
//    .Does(() =>
//{
//    NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
 //       NoResults = true
 //       });
//});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

//Task("Default")
  //  .IsDependentOn("Run-Unit-Tests");


Task("Assets")
	.Does(() =>
	{	
		// Start the npm restore process.
		StartProcess("cmd", new ProcessSettings {
			Arguments = "/c \" npm install -g grunt-cli \"",
			WorkingDirectory = solutionDir
		});
		 
		StartProcess("cmd", new ProcessSettings {
			Arguments = "/c \" npm install \"",
			WorkingDirectory = solutionDir
		});
		
		StartProcess("cmd", new ProcessSettings {
			Arguments = "/c \" grunt --gruntfile Gruntfile-Prod.js \"",
			WorkingDirectory = solutionDir
		});
		
	  
	}).IsDependentOn("Build");
	

Task("Publish")
	.Does(() =>
	{	
		var clientSettings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Configuration = configuration,            
            Runtime = clientRuntime
        };
		
        DotNetCorePublish(clientProject, clientSettings);				
	  
	}).IsDependentOn("Assets");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
