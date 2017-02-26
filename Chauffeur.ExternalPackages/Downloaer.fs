module internal Downloader

open FSharp.Data.TypeProviders
open System.IO
open System.IO.Abstractions

type UmbracoPackageRepo = WsdlService< "http://packages.umbraco.org/umbraco/webservices/api/repository.asmx?WSDL" >

let downloadPackage (writer : TextWriter) packageId =
    async {
        use client = UmbracoPackageRepo.GetRepositorySoap()

        let ctx = client.DataContext
        let binding = ctx.Endpoint.Binding :?> System.ServiceModel.BasicHttpBinding
        binding.MaxReceivedMessageSize <- int64 (System.Int32.MaxValue)
        binding.MaxBufferPoolSize <- System.Int64.MaxValue
        binding.MaxBufferSize <- System.Int32.MaxValue

        do! writer.WriteLineAsync("Downloading the package from the Umbraco package repo") |> Async.AwaitTask

        let! response = (packageId, 
                             UmbracoPackageRepo.ServiceTypes.packages.umbraco.org.webservices.Version.Version41)
                            |> client.fetchPackageByVersionAsync
                            |> Async.AwaitTask
        return response.Body.fetchPackageByVersionResult
    }

let savePackage (writer : TextWriter) chauffeurDirectory (path : PathBase) packageId byteArray =
    async {
        use fileStream = new FileStream (path.Combine (chauffeurDirectory, sprintf "%s.umb" packageId), FileMode.CreateNew)
        do! fileStream.WriteAsync(byteArray, 0, byteArray.Length) |> Async.AwaitTask
        do! writer.WriteLineAsync("Package saved to the Chauffeur folder and ready for install. Run the following command") |> Async.AwaitTask
        do! writer.WriteLineAsync(sprintf "external-package install %s" packageId) |> Async.AwaitTask
    }