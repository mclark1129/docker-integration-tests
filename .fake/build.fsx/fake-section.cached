#load ".fake/build.fsx/intellisense.fsx"
open Fake.DotNet
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "src/**/publish"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    ++ "tests/**/publish"
    |> Shell.cleanDirs 
)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter (DotNet.build id)
)

let publishParameters (p:DotNet.PublishOptions) = { 
    p with 
        OutputPath = Some "publish" 
        Configuration = DotNet.Release
    }

Target.create "IntegrationTest" <| fun _ ->    
    DotNet.publish publishParameters "src/NumberTrackingService.Api"
    DotNet.publish publishParameters "src/NumberTrackingService.Processor"
    DotNet.publish publishParameters "tests/NumberTrackingService.IntegrationTests"

"Clean" ==> "IntegrationTest"  

Target.runOrDefault "IntegrationTest"
