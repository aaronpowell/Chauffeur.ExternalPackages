#r @".tools/FAKE.Core/tools/FakeLib.dll"
#r @".tools/FSharpLint.Fake/tools/FSharpLint.Core.dll"
#r @".tools/FSharpLint.Fake/tools/FSharpLint.Fake.dll"

open Fake
open Fake.Core
open Fake.Core.Environment
open Fake.Core.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.DotNet.AssemblyInfoFile
open Fake.DotNet.NuGet.Restore
open Fake.DotNet.NuGet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.DotNet.Testing.XUnit2
open Fake.Tools
open FSharpLint.Fake

Environment.setEnvironVar "VisualStudioVersion" "15.0"

let authors = ["Aaron Powell"]
let buildDir = "./Chauffeur.ExternalPackages/bin/"
let packagingRoot = "./.packaging/"
let packagingDir = packagingRoot @@ "chauffeur.externalpackages"
let testDir = "./.testresults"
let buildMode = environVarOrDefault "buildMode" "Release"
let isAppVeyorBuild = not (isNull (environVar "APPVEYOR"))
let projectName = "Chauffeur.ExternalPackages"
let nugetSummary = "A tool for managing packages from the Umbraco package feed using Chauffeur"
let nugetDescription = nugetSummary

let releaseNotes =
    File.read "ReleaseNotes.md"
        |> Fake.ReleaseNotesHelper.parseReleaseNotes

let trimBranchName (branch: string) =
    let trimmed = match branch.Length > 10 with
                    | true -> branch.Substring(0, 10)
                    | _ -> branch

    trimmed.Replace(".", "")

let prv = match environVar "APPVEYOR_REPO_BRANCH" with
            | null -> ""
            | "master" -> ""
            | branch -> sprintf "-%s%s" (trimBranchName branch) (
                            match environVar "APPVEYOR_BUILD_NUMBER" with
                            | null -> ""
                            | _ -> sprintf "-%s" (environVar "APPVEYOR_BUILD_NUMBER")
                            )
let nugetVersion = sprintf "%d.%d.%d%s" releaseNotes.SemVer.Major releaseNotes.SemVer.Minor releaseNotes.SemVer.Patch prv

Target.Create "Default" Target.DoNothing

Target.Create "AssemblyInfo" (fun _ ->
    let commitHash = Git.Information.getCurrentHash()

    let attributes =
        [ Fake.DotNet.AssemblyInfo.Product projectName
          Fake.DotNet.AssemblyInfo.Title "Chauffeur External Package tools"
          Fake.DotNet.AssemblyInfo.Version releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.InformationalVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.ComVisible false
          Fake.DotNet.AssemblyInfo.Metadata("githash", commitHash) ]

    CreateFSharp "AssemblyInfo.fs" attributes
      
)

Target.Create "Clean" (fun _ ->
    Shell.CleanDirs [buildDir; testDir]
)

Target.Create "RestorePackages" (fun _ ->
    RestorePackage id "./Chauffeur.ExternalPackages/packages.config"
)

Target.Create "RestoreDemoPackages" (fun _ ->
    RestorePackage id "./Chauffeur.ExternalPackages.Demo/packages.config"
)

Target.Create "RestoreTestsPackages" (fun _ ->
    RestorePackage id "./Chauffeur.ExternalPackages.Tests/packages.config"
)

Target.Create "Build" (fun _ ->
    let setParams (defaults: MSBuildParams) =
        let p = { defaults with
                    Verbosity = Some(Quiet)
                    Targets = ["Build"]
                    Properties =
                    [
                        "Configuration", buildMode
                        "Optimize", "True"
                        "DebugSymbols", "True"
                    ] }
        if isAppVeyorBuild then p
        else { p with ToolPath = "C:\Program Files (x86)\Microsoft Visual Studio\Preview\Enterprise\MSBuild\15.0\Bin\msbuild.exe" }

    build setParams "./Chauffeur.ExternalPackages.sln"
)

Target.Create "UnitTests" (fun _ ->
    !! (sprintf "./Chauffeur.ExternalPackages.Tests/bin/%s/**/Chauffeur.ExternalPackages.Tests.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

Target.Create "CreateNuGetPackage" (fun _ ->
    let libDir = packagingDir @@ "lib/net45/"
    Shell.CleanDirs [libDir]

    Shell.CopyFile libDir (buildDir @@ "Release/Chauffeur.ExternalPackages.dll")
    Shell.CopyFile libDir (buildDir @@ "Release/Chauffeur.ExternalPackages.UmbracoRepositoryWrapper.dll")
    Shell.CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = nugetDescription
            OutputPath = packagingRoot
            Summary = nugetSummary
            WorkingDir = packagingDir
            Version = nugetVersion
            ReleaseNotes = String.toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.None
            AccessKey = environVarOrDefault "nugetkey" ""
            Publish = hasEnvironVar "nugetkey" }) "Chauffeur.ExternalPackages/Chauffeur.ExternalPackages.nuspec"
)

Target.Create "BuildVersion" (fun _ ->
    Process.Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

Target.Create "Package" Target.DoNothing

Target.Create "Lint" (fun _ ->
    !! "**/*.fsproj"
        |> Seq.iter (FSharpLint id))

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    // ==> "Lint"
    ==> "Build"

"RestorePackages"
    ==> "RestoreDemoPackages"
    ==> "RestoreTestsPackages"
    ==> "Build"

// "Build"
//    ==> "SourceLink"

"UnitTests"
    ==> "Default"

// "SourceLink"
//    ==>
"CreateNuGetPackage"
    ==> "Package"

Target.RunOrDefault "Default"