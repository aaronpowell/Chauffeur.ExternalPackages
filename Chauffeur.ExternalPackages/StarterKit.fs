module internal StarterKit

open FSharp.Data
open System.IO
open System.Net.Http
open Chauffeur

type StarterKit = JsonProvider< "./starter-kit.json" >

let internal apiUrl version = sprintf "http://our.umbraco.org/webapi/StarterKit/Get/?umbracoVersion=%s" version
let internal downloadUrl starterKitIt version =
    sprintf "https://our.umbraco.org/webapi/packages/v1/%s?version=%s&includeHidden=true&asFile=true" starterKitIt version

let internal getAvailableStarerKits version =
    StarterKit.AsyncLoad (apiUrl version)

let internal displayStarterKits (reader : TextReader) (writer : TextWriter) (kits: StarterKit.Root[]) =
    async { 
        do! writer.WriteLineAsync("Here are the results") |> Async.AwaitTask
        let printer i (c : StarterKit.Root) = 
            writer.WriteLine(sprintf "%d) %s (id: %s)" (i + 1) c.Name (c.Id.ToString()))
        kits |> Array.iteri printer
        do! writer.WriteLineAsync("q) Cancel") |> Async.AwaitTask
        do! writer.WriteAsync("Select a kit to install> ") |> Async.AwaitTask
        let! selection = reader.ReadLineAsync() |> Async.AwaitTask

        return match selection with
                | "q" -> None
                | _ ->
                    let kit = kits.[(int selection) - 1]
                    writer.WriteLine(sprintf "Downloading %s" kit.Name)
                    Some(kit.Id)
    }

let internal downloadStarterKit version starterKitId =
    async {
        use httpClient = new HttpClient()
        let! response  = downloadUrl starterKitId version
                            |> httpClient.GetAsync
                            |> Async.AwaitTask

        return! response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
    }

let selectStarterKit version savePackage idAsString =
    async {
        let! bytes = downloadStarterKit version idAsString
        do! savePackage idAsString bytes
        return DeliverableResponse.Continue
    }

let getStarterKits (reader : TextReader) (writer : TextWriter) version savePackage =
    async {
        let! starterKits = getAvailableStarerKits version

        let! selection = displayStarterKits reader writer starterKits

        return! match selection with
                | None -> async { return DeliverableResponse.Continue }
                | Some id -> id.ToString() |> selectStarterKit version savePackage
    }