module internal Downloader

open Chauffeur.ExternalPackages.UmbracoRepositoryWrapper
open System.IO
open System.IO.Abstractions

let downloadPackage (writer : TextWriter) packageId =
    async {
        let client = new UmbracoPackageFeed()

        do! writer.WriteLineAsync("Downloading the package from the Umbraco package repo") |> Async.AwaitTask

        return! (client.GetPackagesByVersionAsync packageId) |> Async.AwaitTask
    }

let getFilePath chauffeurDirectory (path : PathBase) (file: FileBase) packageId =
    let filePath = path.Combine (chauffeurDirectory, sprintf "%s.umb" packageId)

    if (file.Exists filePath) then file.Delete filePath

    filePath

let savePackage (writer : TextWriter) filePath byteArray =
    async {
        use fileStream = new FileStream (filePath, FileMode.CreateNew)
        do! fileStream.WriteAsync(byteArray, 0, byteArray.Length) |> Async.AwaitTask
        do! writer.WriteLineAsync("Package saved to the Chauffeur folder and ready for unpacking. Run the following command") |> Async.AwaitTask
        // do! writer.WriteLineAsync(sprintf "external-package unpack %s" packageId) |> Async.AwaitTask
    }