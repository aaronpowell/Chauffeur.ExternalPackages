module internal Downloader

open Chauffeur.ExternalPackages.UmbracoRepositoryWrapper
open System.IO
open System.IO.Abstractions

let downloadPackage (writer : TextWriter) packageId =
    async {
        use client = new UmbracoPackageFeed()

        do! writer.WriteLineAsync("Downloading the package from the Umbraco package repo") |> Async.AwaitTask

        return! (client.GetPackagesByVersionAsync packageId) |> Async.AwaitTask
    }

let savePackage (writer : TextWriter) chauffeurDirectory (path : PathBase) packageId byteArray =
    async {
        use fileStream = new FileStream (path.Combine (chauffeurDirectory, sprintf "%s.umb" packageId), FileMode.CreateNew)
        do! fileStream.WriteAsync(byteArray, 0, byteArray.Length) |> Async.AwaitTask
        do! writer.WriteLineAsync("Package saved to the Chauffeur folder and ready for install. Run the following command") |> Async.AwaitTask
        do! writer.WriteLineAsync(sprintf "external-package install %s" packageId) |> Async.AwaitTask
    }