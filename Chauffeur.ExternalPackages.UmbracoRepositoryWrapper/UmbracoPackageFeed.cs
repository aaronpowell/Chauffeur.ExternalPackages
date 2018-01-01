using Chauffeur.ExternalPackages.UmbracoRepositoryWrapper.UmbracoFeed;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Chauffeur.ExternalPackages.UmbracoRepositoryWrapper
{
    public class UmbracoPackageFeed
    {
        public async Task<byte[]> GetPackagesByVersionAsync(string packageGuid)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxBufferPoolSize = long.MaxValue
            };
            var soapClient = new RepositorySoapClient(
                binding,
                new EndpointAddress("http://packages.umbraco.org/umbraco/webservices/api/repository.asmx")
            );
            var response = await soapClient.fetchPackageByVersionAsync(packageGuid, UmbracoFeed.Version.Version41);
            return response.Body.fetchPackageByVersionResult;
        }
    }
}
