#r @".tools/FAKE.Core/tools/FakeLib.dll"
#r @".tools/FSharpLint.Fake/tools/FSharpLint.Fake.dll"

open Fake
open Fake.Testing.XUnit2
open Fake.AssemblyInfoFile
open FSharpLint.Fake

EnvironmentHelper.setBuildParam "VisualStudioVersion" "15.0"

let authors = ["Aaron Powell"]
let buildDir = "./Chauffeur.ExternalPackages/bin/"
let packagingRoot = "./.packaging/"
let packagingDir = packagingRoot @@ "chauffeur.externalpackages"
let testDir = "./.testresults"
let buildMode = getBuildParamOrDefault "buildMode" "Release"
let isAppVeyorBuild = not (isNull (environVar "APPVEYOR"))
let projectName = "Chauffeur.ExternalPackages"
let nugetSummary = "A tool for managing packages from the Umbraco package feed using Chauffeur"
let nugetDescription = nugetSummary

let releaseNotes =
    ReadFile "ReleaseNotes.md"
        |> ReleaseNotesHelper.parseReleaseNotes

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

Target "Default" DoNothing

Target "AssemblyInfo" (fun _ ->
    CreateFSharpAssemblyInfo "AssemblyInfo.fs"
      [ Attribute.Product projectName
        Attribute.Version releaseNotes.AssemblyVersion
        Attribute.FileVersion releaseNotes.AssemblyVersion
        Attribute.ComVisible false ]
)

Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "RestorePackages" (fun _ ->
    RestorePackage id "./Chauffeur.ExternalPackages/packages.config"
)

Target "RestoreDemoPackages" (fun _ ->
    RestorePackage id "./Chauffeur.ExternalPackages.Demo/packages.config"
)

Target "RestoreTestsPackages" (fun _ ->
    RestorePackage id "./Chauffeur.Tests/packages.config"
    RestorePackage id "./Chauffeur.Tests.Integration/packages.config"
)

Target "Build" (fun _ ->
    let setParams defaults =
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
    |> DoNothing
)

Target "UnitTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests/bin/%s/**/Chauffeur.Tests.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

Target "CreateNuGetPackage" (fun _ ->
    let libDir = packagingDir @@ "lib/net45/"
    CleanDirs [libDir]

    CopyFile libDir (buildDir @@ "Release/Chauffeur.ExternalPackages.dll")
    CopyFile libDir (buildDir @@ "Release/Chauffeur.ExternalPackages.UmbracoRepositoryWrapper.dll")
    CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = nugetDescription
            OutputPath = packagingRoot
            Summary = nugetSummary
            WorkingDir = packagingDir
            Version = nugetVersion
            ReleaseNotes = toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.None
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Dependencies =
                ["Chauffeur", "0.13.0-package-wi-121"]
            Publish = hasBuildParam "nugetkey" }) "Chauffeur.ExternalPackages/Chauffeur.ExternalPackages.nuspec"
)

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

Target "Package" DoNothing

Target "Lint" (fun _ ->
    !! "src/**/*.fsproj"
        |> Seq.iter (FSharpLint id))

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "Lint"
    ==> "Build"

"RestorePackages"
    ==> "RestoreDemoPackages"
    // ==> "RestoreChauffeurTestsPackages"
    ==> "Build"

"UnitTests"
    ==> "Default"

"CreateNuGetPackage"
    ==> "Package"

RunTargetOrDefault "Default"