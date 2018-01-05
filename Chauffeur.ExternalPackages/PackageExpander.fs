module PackageExpander

open System.IO.Abstractions
open System.IO
open ICSharpCode.SharpZipLib.Zip

let private notNullOrEmpty = not << System.String.IsNullOrEmpty

let private unzip packageStream getFileName (fileCreate: (string -> Stream)) pathCombine (unpackPath : DirectoryInfoBase) =
    use stream = new ZipInputStream(packageStream)
    let rec processEntry (entry : ZipEntry) =
        match entry with
        | null -> ()
        | _ ->
            let fileName = entry.Name |> getFileName
            if (notNullOrEmpty fileName) then
                use streamWriter = fileCreate(pathCombine (unpackPath.FullName, fileName))
                let data = Array.init<byte> 2048 (fun _ -> byte(0))
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

let packageExpander (writer : TextWriter) (fs : IFileSystem) chauffeurDirectory packageId =
    let packagePath = fs.Path.Combine (chauffeurDirectory, sprintf "%s.umb" packageId)
    async {
        match fs.File.Exists (packagePath) with
        | false ->
            do! writer.WriteLineAsync("To package isn't downloaded yet, try running the following command:") |> Async.AwaitTask
            do! writer.WriteLineAsync(sprintf "external-package download %s" packageId) |> Async.AwaitTask
        | true ->
            let unpackPath = fs.Path.Combine(chauffeurDirectory, sprintf "%s-unpack" packageId)

            match fs.Directory.Exists unpackPath with
            | true -> fs.Directory.Delete(unpackPath, true)
            | _ -> ()

            do! fs.Directory.CreateDirectory(unpackPath)
                |> unzip (fs.File.OpenRead(packagePath)) fs.Path.GetFileName fs.File.Create fs.Path.Combine
                |> (fun p -> async {
                        do! writer.WriteLineAsync("Package has been expanded, next you need to run the 'package' deliverable to install it:") |> Async.AwaitTask
                        do! writer.WriteLineAsync(sprintf "pkg package -f:%s" p.FullName) |> Async.AwaitTask
                    })
    }
