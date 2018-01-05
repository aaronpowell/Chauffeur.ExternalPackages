module internal PackageAction

open System.Xml.Linq
open System.Xml
open System.IO
open System.Reflection
open FSharp.Data
open Umbraco.Core
open umbraco.interfaces
open System

type Package = XmlProvider<"./package-sample.xml">

let castAsPackageAction (o:obj) = 
    match o with
    | :? IPackageAction as res -> res
    | _ -> null

let xelementToXmlNode (xml: XElement) =
    use reader = xml.CreateReader()
    let xmlDoc = new XmlDocument()
    xmlDoc.Load reader
    xmlDoc

let runPackageAction name (actionXml: Package.Action) (action: IPackageAction) =
    xelementToXmlNode actionXml.XElement
        |> (fun x -> action.Execute(name, x.DocumentElement))

let runPackageActions (packagePath: string) =
    let package = Package.Load packagePath
    let packageActions = TypeFinder.FindClassesOfType<IPackageAction>()
                            |> Seq.map (fun t -> (Activator.CreateInstance t) |> castAsPackageAction)
                            |> Seq.filter (fun p -> p <> null)

    package.Actions
    |> Array.filter (fun pkg -> pkg.Runat = "install")
    |> Array.iter (fun pkg ->
        let type' = packageActions |> Seq.find (fun t -> t.Alias() = pkg.Alias)
        runPackageAction package.Info.Package.Name pkg type' |> ignore)