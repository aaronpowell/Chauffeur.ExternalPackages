namespace Chauffeur.ExternalPackages

open Chauffeur
open Chauffeur.Host
open Searcher
open Downloader
open PackageExpander
open Categories
open StarterKit
open PackageAction
open System.IO.Abstractions
open IOUtils

module Async =
    open System.Threading.Tasks

    let inline awaitPlainTask (task: Task) = 
        // rethrow exception from preceding task if it fauled
        let continuation (t : Task) : unit =
            match t.IsFaulted with
            | true -> raise t.Exception
            | _ -> ()
        task.ContinueWith continuation |> Async.AwaitTask

    let inline StartAsPlainTask (work : Async<unit>) =
        Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

[<DeliverableNameAttribute("external-package")>]
type ExternalPackagesDeliverable(reader, writer, settings : IChauffeurSettings, fileSystem : IFileSystem) = 
    inherit Deliverable(reader, writer)
    let writeLineAsync' = writeLineAsync writer
    let writeAsync' = writeAsync writer
    let readLineAsync' = readLineAsync reader

    let searchForPackage' = searchForPackage settings.UmbracoVersion
    let displaySearchResults' = displaySearchResults readLineAsync' writeLineAsync' writeAsync'
    let downloadPackage' = downloadPackage writeLineAsync'
    let _, chauffeurFolder = settings.TryGetChauffeurDirectory()
    let savePackage' id byteArray = 
        getFilePath chauffeurFolder fileSystem.Path fileSystem.File id
        |> (fun filePath -> savePackage writeLineAsync' filePath byteArray id)

    let search list = 
        let handleFoundPackage packageId =
            async {
                do! writer.WriteLineAsync(sprintf "external-package download %s" packageId) |> Async.AwaitTask
                let! byteArray = downloadPackage' packageId
                do! savePackage' packageId byteArray
                return DeliverableResponse.Continue
            }

        async { 
            let! response = match list with
                            | q :: "category" :: c :: _ -> searchForPackage' 0 c q
                            | q :: _ -> searchForPackage' 0 "" q
                            | _ -> searchForPackage' 0 "" ""
            let! selectedPackage = displaySearchResults' response.Packages

            return!
                match selectedPackage with
                | Some id -> handleFoundPackage (id.ToString())
                | None -> async { return DeliverableResponse.Continue }
        }

    override __.Run(_, args) = 
        let list = args |> Array.toList
        async { 
            match list with
            | "search" :: rest -> return! search rest
            | "download" :: id :: _ ->
                let! byteArray = downloadPackage' id
                do! savePackage' id byteArray
                return DeliverableResponse.Continue
            | "unpack" :: id :: _ ->
                do! packageExpander writer fileSystem chauffeurFolder id
                return DeliverableResponse.Continue
            | "categories" :: _ ->
                let! categories = getCategories
                let! selectedCategory = displayCategoryResults reader writer categories
                do! writer.WriteLineAsync(sprintf "You selected %s" selectedCategory.Name) |> Async.AwaitTask
                return! search [selectedCategory.Name]
            | "starter-kit" :: id :: _ ->
                return! selectStarterKit settings.UmbracoVersion savePackage' id
            | "starter-kit" :: _ ->
                return! getStarterKits reader writer settings.UmbracoVersion savePackage'
            | "actions" :: id :: _ ->
                fileSystem.Path.Combine(chauffeurFolder, id)
                |> runPackageActions
                |> ignore
                return DeliverableResponse.Continue
            | _ ->
                do! writer.WriteLineAsync("Command is known, ignoring") |> Async.AwaitTask
                return DeliverableResponse.Continue
        }
        |> Async.StartAsTask

    interface IProvideDirections with
        member __.Directions() =
            let output = __.Out
            async {
                do! output.WriteLineAsync("external-package") |> Async.AwaitTask
                do! output.WriteLineAsync("\tDiscover, download and install packages from the Umbraco package feed") |> Async.AwaitTask
                do! output.WriteLineAsync("Available Operations:") |> Async.AwaitTask
                do! output.WriteLineAsync("\tsearch <pattern>") |> Async.AwaitTask
                do! output.WriteLineAsync("\t\tShow the available packages") |> Async.AwaitTask
                do! output.WriteLineAsync("\tdownload <id>") |> Async.AwaitTask
                do! output.WriteLineAsync("\t\tDownloads a package with a specific ID from the Umbraco package feed") |> Async.AwaitTask
                do! output.WriteLineAsync("\tunpack <id>") |> Async.AwaitTask
                do! output.WriteLineAsync("\t\tUnpacks an Umbraco package so it can be installed") |> Async.AwaitTask
                do! output.WriteLineAsync("\tcategories") |> Async.AwaitTask
                do! output.WriteLineAsync("\t\tList the Umbraco package categories") |> Async.AwaitTask
                do! output.WriteLineAsync("\tstarter-kit <?id>") |> Async.AwaitTask
                do! output.WriteLineAsync("\t\tIf the ID is provided it'll download a specific starter kit, otherwise it'll list the available starter kits") |> Async.AwaitTask
            }
            |> Async.StartAsPlainTask
