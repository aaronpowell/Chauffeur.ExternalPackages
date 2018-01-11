module internal PackageAction

open System.Xml.Linq
open System.Xml
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

let runPackageActions writeLineAsync (packagePath: string) =
    let package = Package.Load packagePath
    let packageActions = TypeFinder.FindClassesOfType<IPackageAction>()
                            |> Seq.map (fun t -> (Activator.CreateInstance t) |> castAsPackageAction)
                            |> Seq.filter (fun p -> p <> null)

    package.Actions
    |> Array.filter (fun action -> action.Runat = "install")
    |> Array.map (fun action -> async {
        do! writeLineAsync (sprintf "Running action %s" action.Alias)
        let type' = packageActions |> Seq.find (fun t -> t.Alias() = action.Alias)
        let success = runPackageAction package.Info.Package.Name action type'
        return match success with
                | true -> None
                | false -> Some(action.Alias) })