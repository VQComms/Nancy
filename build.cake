#addin "Newtonsoft.Json"

// Usings
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

// Arguments
var target = Argument<string>("target", "Default");
var source = Argument<string>("source", null);
var apiKey = Argument<string>("apikey", null);
var version = Argument<string>("targetversion", null);
var skipClean = Argument<bool>("skipclean", false);
var skipTests = Argument<bool>("skiptests", false);

// Variables
var configuration = IsRunningOnWindows() ? "Release" : "MonoRelease";
var projectJsonFiles = GetFiles("./src/**/project.json");

// Directories
var output = Directory("build");
var outputBinaries = output + Directory("binaries");
var outputBinariesNet452 = outputBinaries + Directory("net452");
var outputBinariesNetstandard = outputBinaries + Directory("netstandard1.5");
var outputPackages = output + Directory("packages");
var outputNuGet = output + Directory("nuget");

///////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  // Clean artifact directories.
  CleanDirectories(new DirectoryPath[] {
    output, outputBinaries, outputPackages, outputNuGet,
    outputBinariesNet452, outputBinariesNetstandard
  });

  if(!skipClean) {
    // Clean output directories.
    CleanDirectories("./src/**/" + configuration);
    CleanDirectories("./test/**/" + configuration);
    CleanDirectories("./samples/**/" + configuration);
  }
});

Task("Restore-NuGet-Packages")
  .Description("Restores NuGet packages")
  .Does(() =>
{
  var settings = new DotNetCoreRestoreSettings
  {
    Verbose = false,
    Verbosity = DotNetCoreRestoreVerbosity.Warning,
    Sources = new [] {
        "https://www.myget.org/F/xunit/api/v3/index.json",
        "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json",
        "https://dotnet.myget.org/F/cli-deps/api/v3/index.json",
        "https://api.nuget.org/v3/index.json",
    }
  };
  
  //Restore at root until preview1-002702 bug fixed
  DotNetCoreRestore("./", settings);
  //DotNetCoreRestore("./src", settings);
  //DotNetCoreRestore("./samples", settings);
  //DotNetCoreRestore("./test", settings);
});

Task("Compile")
  .Description("Builds the solution")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
{
  var projects = GetFiles("./**/*.xproj");
  foreach(var project in projects)
  {
    DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings {
      Configuration = configuration,
      Verbose = false
    });
  }
});

Task("Test")
  .Description("Executes xUnit tests")
  .WithCriteria(!skipTests)
  .IsDependentOn("Compile")
  .Does(() =>
{
  var projects = GetFiles("./test/**/*.xproj")
    - GetFiles("./test/**/Nancy.ViewEngines.Razor.Tests.Models.xproj");

  foreach(var project in projects)
  {
    DotNetCoreTest(project.GetDirectory().FullPath, new DotNetCoreTestSettings {
      Configuration = configuration
    });
  }
});

Task("Publish")
  .Description("Gathers output files and copies them to the output folder")
  .IsDependentOn("Compile")
  .Does(() =>
{
  // Copy net452 binaries.
  CopyFiles(GetFiles("src/**/bin/" + configuration + "/net452/*.dll")
    + GetFiles("src/**/bin/" + configuration + "/net452/*.xml")
    + GetFiles("src/**/bin/" + configuration + "/net452/*.pdb")
    + GetFiles("src/**/*.ps1"), outputBinariesNet452);

  // Copy netstandard1.5 binaries.
  CopyFiles(GetFiles("src/**/bin/" + configuration + "/netstandard1.5/*.dll")
    + GetFiles("src/**/bin/" + configuration + "/netstandard1.5/*.xml")
    + GetFiles("src/**/bin/" + configuration + "/netstandard1.5/*.pdb")
    + GetFiles("src/**/*.ps1"), outputBinariesNetstandard);

});

Task("Package")
  .Description("Zips up the built binaries for easy distribution")
  .IsDependentOn("Publish")
  .Does(() =>
{
  var package = outputPackages + File("Nancy-Latest.zip");
  var files = GetFiles(outputBinaries.Path.FullPath + "/**/*");

  Zip(outputBinaries, package, files);
});

Task("Nuke-Symbol-Packages")
  .Description("Deletes symbol packages")
  .Does(() =>
{
  DeleteFiles(GetFiles("./**/*.Symbols.nupkg"));
});

Task("Package-NuGet")
  .Description("Generates NuGet packages for each project that contains a nuspec")
  .IsDependentOn("Publish")
  .Does(() =>
{
  var projects = GetFiles("./**/*.xproj");
  foreach(var project in projects)
  {
    var settings = new DotNetCorePackSettings {
      Configuration = "Release",
      OutputDirectory = outputNuGet
    };

    DotNetCorePack(project.GetDirectory().FullPath, settings);
  }
});

Task("Publish-NuGet")
  .Description("Pushes the nuget packages in the nuget folder to a NuGet source. Also publishes the packages into the feeds.")
  .Does(() =>
{
  // Make sure we have an API key.
  if(string.IsNullOrWhiteSpace(apiKey)){
    throw new CakeException("No NuGet API key provided.");
  }

  // Upload every package to the provided NuGet source (defaults to nuget.org).
  var packages = GetFiles(outputNuGet.Path.FullPath + "/*" + version + ".nupkg");
  foreach(var package in packages)
  {
    NuGetPush(package, new NuGetPushSettings {
      Source = source,
      ApiKey = apiKey
    });
  }
});

///////////////////////////////////////////////////////////////

Task("Tag")
  .Description("Tags the current release.")
  .Does(() =>
{
  StartProcess("git", new ProcessSettings {
    Arguments = string.Format("tag \"v{0}\"", version)
  });
});

Task("Prepare-Release")
  .Does(() =>
{
  // Update version.
  UpdateProjectJsonVersion(version, projectJsonFiles);

  // Add
  foreach (var file in projectJsonFiles) 
  {
    StartProcess("git", new ProcessSettings {
      Arguments = string.Format("add {0}", file.FullPath)
    });
  }

  // Commit
  StartProcess("git", new ProcessSettings {
    Arguments = string.Format("commit -m \"Updated version to {0}\"", version)
  });
  // Tag
  StartProcess("git", new ProcessSettings {
    Arguments = string.Format("tag \"v{0}\"", version)
  });
});

Task("Update-Version")
  .Does(() =>
{
  if(string.IsNullOrWhiteSpace(version)) {
    throw new CakeException("No version specified!");
  }
  
  UpdateProjectJsonVersion(version, projectJsonFiles);
});

///////////////////////////////////////////////////////////////

public void UpdateProjectJsonVersion(string version, FilePathCollection filePaths)
{
  Verbose(logAction => logAction("Setting version to {0}", version));
  foreach (var file in filePaths) 
  {
    var project = Newtonsoft.Json.Linq.JObject.Parse(
      System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8));

    project["version"].Replace(version);

    System.IO.File.WriteAllText(file.FullPath, project.ToString(), Encoding.UTF8);
  }
}


Task("Default")
  .IsDependentOn("Test")
  .IsDependentOn("Package");

Task("Mono")
  .IsDependentOn("Test");

///////////////////////////////////////////////////////////////

RunTarget(target);
