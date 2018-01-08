module internal Searcher

open FSharp.Data
open System.IO
open IOUtils

type UmbracoPackages = JsonProvider< "../Chauffeur.ExternalPackages/umbraco-packages.json" >

let apiUrl = 
    sprintf 
        "https://our.umbraco.org/webapi/packages/v1?pageIndex=%d&pageSize=24&category=%s&query=%s&order=Default&version=%s"

let searchForPackage version page category query = UmbracoPackages.AsyncLoad(apiUrl page category query version)

let displaySearchResults readLineAsync writeLineAsync writeAsync (packages : UmbracoPackages.Package []) =
    async { 
        do! writeLineAsync("Here are the results")
        let printer i (p : UmbracoPackages.Package) = 
            writeLineAsync(sprintf "%d) %s (id: %s)" (i + 1) p.Name (p.Id.ToString()))

        packages
        |> Array.mapi printer
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        do! writeLineAsync("q) Cancel")
        do! writeAsync("Select a package to download> ")
        let! selection = readLineAsync

        return match selection with
                | "q" -> None
                | _ -> Some(packages.[(int selection) - 1].Id)
    }
