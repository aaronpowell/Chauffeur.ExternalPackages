module IOUtils

open System.IO

let writeLineAsync (writer: TextWriter) (input: string) =
    writer.WriteLineAsync(input) |> Async.AwaitTask

let writeAsync (writer: TextWriter) (input: string) =
    writer.WriteAsync(input) |> Async.AwaitTask

let readLineAsync (reader: TextReader) =
    reader.ReadLineAsync() |> Async.AwaitTask