namespace Chauffeur.ExternalPackages

open Chauffeur
open Chauffeur.Host
open Searcher
open Downloader
open Installer
open Categories
open System.IO.Abstractions

[<DeliverableNameAttribute("external-package")>]
type ExternalPackagesDeliverable(reader, writer, settings : IChauffeurSettings, fileSystem : IFileSystem) = 
    inherit Deliverable(reader, writer)
    let searchForPackage' = searchForPackage settings.UmbracoVersion
    let displaySearchResults' = displaySearchResults reader writer
    let downloadPackage' = downloadPackage writer
    let savePackage' = savePackage writer
    let _, chauffeurFolder = settings.TryGetChauffeurDirectory()

    let search list = 
        let handleFoundPackage packageId =
            async {
                let! byteArray = downloadPackage' packageId
                do! savePackage' chauffeurFolder fileSystem.Path packageId byteArray
                return DeliverableResponse.Continue
            }

        async { 
            let! response = match list with
                            | q :: "category" :: c :: _ -> searchForPackage' 0 c q
                            | q :: _ -> searchForPackage' 0 "" q
                            | _ -> searchForPackage' 0 "" ""
            let! selectedPackage = displaySearchResults' response.Packages

            return!
                if selectedPackage = System.Guid.Empty then async { return DeliverableResponse.Continue }
                else handleFoundPackage (selectedPackage.ToString())
            
        }

    override __.Run(_, args) = 
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
            | "categories" :: _ ->
                let! categories = getCategories
                let! selectedCategory = displayCategoryResults reader writer categories
                do! writer.WriteLineAsync(sprintf "You selected %s" selectedCategory.Name) |> Async.AwaitTask
                return! search [selectedCategory.Name]
            | _ ->
                do! writer.WriteLineAsync("Command is known, ignoring") |> Async.AwaitTask
                return DeliverableResponse.Continue
        }
        |> Async.StartAsTask
