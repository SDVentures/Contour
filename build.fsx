#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Testing.NUnit
nuget Fake.DotNet.Paket
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.DotNet
open Fake.DotNet.Testing
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators


System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let project = "Contour"
let authors = [ "Mikhail Zabolotko" ]
let summary = "A library contains implementation of several EIP patterns to build the service bus."
let description = """
The package contains abstract interfaces of service bus and specific transport implementation for AMQP/RabbitMQ."""
let license = "MIT License"
let tags = "rabbitmq client servicebus"

let release = ReleaseNotes.parse (System.IO.File.ReadLines "RELEASE_NOTES.md")

let tempDir = "temp"

let solution = "Contour.sln"

let tests =
    !! "Tests/**/*.csproj"

Target.create "CleanUp" (fun _ ->
    Shell.cleanDirs [ tempDir ]
)

Target.create "AssemblyInfo" (fun _ ->
    if not BuildServer.isLocalBuild then
        let info =
            [ AssemblyInfo.Title project
              AssemblyInfo.Company (authors |> String.concat ",")
              AssemblyInfo.Product project
              AssemblyInfo.Description summary
              AssemblyInfo.Version release.AssemblyVersion
              AssemblyInfo.FileVersion release.AssemblyVersion
              AssemblyInfo.InformationalVersion release.NugetVersion
              AssemblyInfo.Copyright license ]
        AssemblyInfoFile.createCSharp <| "./Sources/" @@ project @@ "/Properties/AssemblyInfo.cs" <| info
)

Target.create "Build" (fun _ ->
    solution |> DotNet.build (fun p -> { p with Configuration = DotNet.BuildConfiguration.Release })
)

Target.create "RunUnitTests" (fun _ ->

    !! "Tests/**/bin/Release/*Common.Tests.dll"
    |> NUnit.Sequential.run (fun p ->
           { p with
                DisableShadowCopy = false
                OutputFile = "TestResults.xml"
                TimeOut = System.TimeSpan.FromMinutes 20. })
)

Target.create "RunAllTests" (fun _ ->

    !! "Tests/**/bin/Release/*.Tests.dll"
    |> NUnit.Sequential.run (fun p ->
           { p with
                DisableShadowCopy = false
                OutputFile = "TestResults.xml"
                TimeOut = System.TimeSpan.FromMinutes 20. })
)

Target.create "BuildPacket" (fun _ ->
    Paket.pack (fun p ->
                   { p with
                       Version = release.NugetVersion })
)

Target.create "Default" ignore

"CleanUp"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "RunUnitTests"
    ==> "RunAllTests"
    ==> "BuildPacket"
    ==> "Default"

Target.runOrDefault "Default"
