module InternalsVisibileTo

open System.Runtime.CompilerServices

[<assembly:InternalsVisibleTo("Chauffeur.ExternalPackages.Tests")>]

do
    ()