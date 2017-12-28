module internal Categories

open FSharp.Data
open System.IO

type PackageCategories = JsonProvider< "./package-categories.json" >

let apiUrl = "https://our.umbraco.org/webapi/packages/v1"

let getCategories = PackageCategories.AsyncLoad apiUrl

let displayCategoryResults (reader : TextReader) (writer : TextWriter) (categories : PackageCategories.Root []) = 
    async { 
        do! writer.WriteLineAsync("Here are the results") |> Async.AwaitTask
        let printer i (c : PackageCategories.Root) = 
            writer.WriteLine(sprintf "%d) %s" (i + 1) c.Name)
        categories |> Array.iteri printer
        do! writer.WriteAsync("Select a category to search> ") |> Async.AwaitTask
        let! selection = reader.ReadLineAsync() |> Async.AwaitTask
        return categories.[(int selection) - 1]
    }
