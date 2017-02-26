namespace Chauffeur.ExternalPackages

open Chauffeur
open Chauffeur.Host
open FSharp.Data
open Searcher
open Downloader
open Installer
open System.IO.Abstractions

[<DeliverableNameAttribute("external-package")>]
type ExternalPackagesDeliverable(reader, writer, settings : IChauffeurSettings, fileSystem : IFileSystem) = 
    inherit Deliverable(reader, writer)
    let searchForPackage' = searchForPackage settings.UmbracoVersion
    let displayResults' = displayResults reader writer
    let downloadPackage' = downloadPackage writer
    let savePackage' = savePackage writer
    let _, chauffeurFolder = settings.TryGetChauffeurDirectory()

    let search list = 
        async { 
            let! response = match list with
                            | q :: "category" :: c :: rest -> searchForPackage' 0 c q
                            | q :: rest -> searchForPackage' 0 "" q
                            | _ -> searchForPackage' 0 "" ""
            let! selectedPackage = displayResults' response.Packages
            let packageId = selectedPackage.Id.ToString()
            let! byteArray = downloadPackage' packageId
            do! savePackage' chauffeurFolder fileSystem.Path packageId byteArray
            return DeliverableResponse.Continue
        }
    
    override x.Run(command, args) = 
        let list = args |> Array.toList
        async { 
            match list with
            | "search" :: rest -> return! search rest
            | "download" :: id :: _ ->
                let! byteArray = downloadPackage' id
                do! savePackage' chauffeurFolder fileSystem.Path id byteArray
                return DeliverableResponse.Continue
            | "install" :: id :: _ ->
                do! install writer fileSystem chauffeurFolder id
                return DeliverableResponse.Continue
            | _ -> 
                do! writer.WriteLineAsync("Command is known, ignoring") |> Async.AwaitTask
                return DeliverableResponse.Continue
        }
        |> Async.StartAsTask
