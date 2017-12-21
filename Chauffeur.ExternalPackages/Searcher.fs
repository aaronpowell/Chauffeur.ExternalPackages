module internal Searcher

open FSharp.Data
open System.IO

type UmbracoPackages = JsonProvider< "./umbraco-packages.json" >

let apiUrl = 
    sprintf 
        "https://our.umbraco.org/webapi/packages/v1?pageIndex=%d&pageSize=24&category=%s&query=%s&order=Default&version=%s"

let searchForPackage version page category query = UmbracoPackages.AsyncLoad(apiUrl page category query version)

let displayResults (reader : TextReader) (writer : TextWriter) (packages : UmbracoPackages.Package []) = 
    async { 
        do! writer.WriteLineAsync("Here are the results") |> Async.AwaitTask
        let printer i (p : UmbracoPackages.Package) = 
            writer.WriteLine(sprintf "%d) %s" (i + 1) p.Name)
        packages |> Array.iteri printer
        do! writer.WriteAsync("Select a package to install> ") |> Async.AwaitTask
        let! selection = reader.ReadLineAsync() |> Async.AwaitTask
        return packages.[int selection]
    }
