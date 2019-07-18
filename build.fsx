#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake
open Fake.AssemblyInfoFile

open System
open System.IO

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let project = "Contour"
let authors = ["SDVentures Team"]
let summary = "A library contains implementation of several EIP patterns to build the service bus."
let description = """
  The package contains abstract interfaces of service bus and specific transport implementation for AMQP/RabbitMQ."""
let license = "MIT License"
let tags = "rabbitmq client servicebus"

let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "RELEASE_NOTES.md")

let buildDir = @"build\"
let nugetDir = @"nuget\"

let projects =
    !! "Sources/**/*.csproj"

let tests =
    !! "Tests/**/*.csproj"

let isAppVeyorBuild = environVar "APPVEYOR" <> null
let appVeyorBuildNumber = environVar "APPVEYOR_BUILD_NUMBER"
let appVeyorRepoCommit = environVar "APPVEYOR_REPO_COMMIT"
let nugetPublishUrl = "https://api.nuget.org/v3/index.json"


Target "CleanUp" (fun _ ->
    CleanDirs [ buildDir ]
)

Target "BuildVersion" (fun _ ->
    let buildVersion = sprintf "%s-build%s" release.NugetVersion appVeyorBuildNumber
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" buildVersion) |> ignore
)

Target "AssemblyInfo" (fun _ ->
    printfn "%A" release
    let info =
        [ Attribute.Title project
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version release.AssemblyVersion
          Attribute.FileVersion release.AssemblyVersion
          Attribute.InformationalVersion release.NugetVersion
          Attribute.Copyright license ]
    CreateCSharpAssemblyInfo <| "./Sources/" @@ project @@ "/Properties/AssemblyInfo.cs" <| info
)

Target "Build" (fun () ->
    MSBuildRelease buildDir "Build" projects |> Log "Build Target Output: "
)

Target "RunUnitTests" (fun () ->
    tests |> MSBuildDebug "" "Rebuild" |> ignore
    !! "Tests/**/bin/Debug/*Common.Tests.dll"
    |> NUnit (fun p ->
           { p with
                DisableShadowCopy = false
                ToolPath = "./packages/NUnit.Runners/tools/"
                Framework = "4.0"
                OutputFile = "TestResults.xml"
                TimeOut = TimeSpan.FromMinutes 20. })
)

Target "Deploy" (fun () ->
    NuGet (fun p ->
        { p with
            Authors = authors
            Project = project
            Summary = summary
            Description = description
            Version = release.NugetVersion
            ReleaseNotes = toLines release.Notes
            Tags = tags
            PublishUrl = nugetPublishUrl
            OutputPath = buildDir
            ToolPath = "./packages/NuGet.CommandLine/tools/Nuget.exe"
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies =
                [ "RabbitMQ.Client", GetPackageVersion "packages" "RabbitMQ.Client";
                  "Common.Logging", GetPackageVersion "packages" "Common.Logging";
                  "FluentValidation", GetPackageVersion "packages" "FluentValidation";
                  "Newtonsoft.Json", GetPackageVersion "packages" "Newtonsoft.Json" ]
            Files =
                [ (@"..\" + buildDir + "Contour.dll", Some "lib/net40", None);
                  (@"..\" + buildDir + "Contour.pdb", Some "lib/net40", None) ]})
        <| (nugetDir + project + ".nuspec")
)

"CleanUp"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "Deploy"

RunTargetOrDefault "Deploy"
