using Chauffeur.ExternalPackages.UmbracoRepositoryWrapper.UmbracoFeed;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Chauffeur.ExternalPackages.UmbracoRepositoryWrapper
{
    public class UmbracoPackageFeed : IDisposable
    {
        private readonly RepositorySoapClient soapClient;

        public UmbracoPackageFeed()
        {
            soapClient = new RepositorySoapClient(
                new BasicHttpBinding(BasicHttpSecurityMode.None),
                new EndpointAddress("http://packages.umbraco.org/umbraco/webservices/api/repository.asmx")
            );
        }

        public async Task<byte[]> GetPackagesByVersionAsync(string packageGuid)
        {
            var response = await soapClient.fetchPackageByVersionAsync(packageGuid, UmbracoFeed.Version.Version41);
            return response.Body.fetchPackageByVersionResult;
        }

        public void Dispose()
        {
            if (soapClient != null)
                soapClient.Close();
        }
    }
}
