module Installer

open System.IO.Abstractions
open System.IO
open ICSharpCode.SharpZipLib.Zip

let private notNullOrEmpty = not << System.String.IsNullOrEmpty

let private unzip (unpackPath : string) packageStream (fs : IFileSystem) =
    use stream = new ZipInputStream(packageStream)
    let rec processEntry (entry : ZipEntry) =
        match entry with
        | null -> ()
        | _ ->
            let fileName = entry.Name |> fs.Path.GetFileName
            if (notNullOrEmpty fileName) then
                use streamWriter = fs.File.Create(fs.Path.Combine(unpackPath, fileName))
                let data = Array.init<byte> 2048 (fun x -> byte(0))
                let rec read size =
                    match size with
                    | 0 -> ()
                    | _ ->
                        streamWriter.Write(data, 0, size)
                        read (stream.Read(data, 0, data.Length))
                read (stream.Read(data, 0, data.Length))
            stream.GetNextEntry() |> processEntry
    stream.GetNextEntry() |> processEntry
    unpackPath

let install (writer : TextWriter) (fs : IFileSystem) chauffeurDirectory packageId =
    let packagePath = fs.Path.Combine (chauffeurDirectory, sprintf "%s.umb" packageId)
    async {
        match fs.File.Exists (packagePath) with
        | false ->
            do! writer.WriteLineAsync("To package isn't downloaded yet, try running the following command:") |> Async.AwaitTask
            do! writer.WriteLineAsync(sprintf "external-package install %s" packageId) |> Async.AwaitTask
        | true ->
            let installer = new umbraco.cms.businesslogic.packager.Installer(0);

            let unpackPath = fs.Path.Combine(chauffeurDirectory, sprintf "%s-unpack" packageId)

            match fs.Directory.Exists unpackPath with
            | true -> fs.Directory.Delete(unpackPath, true)
            | _ -> ()

            fs.Directory.CreateDirectory(unpackPath) |> ignore

            unzip unpackPath (fs.File.OpenRead(packagePath)) fs
            |> installer.LoadConfig

            let id = installer.CreateManifest(unpackPath, packageId, "65194810-1f85-11dd-bd0b-0800200c9a66")

            do! writer.WriteLineAsync(sprintf "%s %d" unpackPath id) |> Async.AwaitTask
    }
