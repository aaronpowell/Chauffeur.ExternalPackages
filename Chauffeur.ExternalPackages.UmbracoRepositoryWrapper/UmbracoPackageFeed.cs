using Chauffeur.ExternalPackages.UmbracoRepositoryWrapper.UmbracoFeed;
using System;
using System.Threading.Tasks;

namespace Chauffeur.ExternalPackages.UmbracoRepositoryWrapper
{
    public class UmbracoPackageFeed : IDisposable
    {
        private readonly RepositorySoapClient soapClient;

        public UmbracoPackageFeed()
        {
            soapClient = new RepositorySoapClient();
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
