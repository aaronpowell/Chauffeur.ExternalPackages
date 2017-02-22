namespace Chauffeur.ExternalPackages

open Chauffeur
open Chauffeur.Host
open FSharp.Data

type UmbracoPackages = JsonProvider< "./umbraco-packages.json" >

[<DeliverableNameAttribute("external-package")>]
type ExternalPackagesDeliverable(reader, writer, settings : IChauffeurSettings) = 
    inherit Deliverable(reader, writer)
    let apiUrl = 
        sprintf 
            "https://our.umbraco.org/webapi/packages/v1?pageIndex=%d&pageSize=24&category=%s&query=%s&order=Default&version=%s"
    let search page category query = UmbracoPackages.AsyncLoad(apiUrl page category query settings.UmbracoVersion)
    
    let displayResults (packages : UmbracoPackages.Package []) = 
        async { 
            let printer i (p : UmbracoPackages.Package) = 
                writer.WriteLine(sprintf "%d) %s (%s)" i p.Name (p.Id.ToString()))
            packages |> Array.iteri printer
            do! writer.WriteAsync("Select a package to install> ") |> Async.AwaitTask
            let! selection = reader.ReadLineAsync() |> Async.AwaitTask
            do! writer.WriteLineAsync(sprintf "You selected %s" selection) |> Async.AwaitTask

            let selectedPackage = packages.[int selection]

            do! writer.WriteLineAsync(selectedPackage.Id.ToString()) |> Async.AwaitTask
        }
    
    override x.Run(command, args) = 
        let list = args |> Array.toList
        async { 
            do! writer.WriteLineAsync("Here are the results") |> Async.AwaitTask
            match list with
            | "search" :: q :: "category" :: c :: rest -> let! response = search 0 c q
                                                          do! displayResults response.Packages
            | "search" :: q :: rest -> let! response = search 0 "" q
                                       do! displayResults response.Packages
            | _ -> let! response = search 0 "" ""
                   do! displayResults response.Packages
            return DeliverableResponse.Continue
        }
        |> Async.StartAsTask
