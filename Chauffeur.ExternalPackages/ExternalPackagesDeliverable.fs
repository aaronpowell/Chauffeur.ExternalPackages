namespace Chauffeur.ExternalPackages

open Chauffeur

[<DeliverableNameAttribute("external-package")>]
type ExternalPackagesDeliverable(reader, writer) = 
    inherit Deliverable(reader, writer)
    
    override x.Run (command, args) =
        let out = x.Out
        async {
            do! out.WriteLineAsync("I'm an F# package") |> Async.AwaitTask

            return DeliverableResponse.Continue
        } |> Async.StartAsTask
