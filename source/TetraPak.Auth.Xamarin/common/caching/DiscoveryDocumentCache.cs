using System.IO;
using TetraPak.Auth.Xamarin.oidc;
using Xamarin.Essentials;

namespace TetraPak.Auth.Xamarin.common
{
    public class DiscoveryDocumentCache : FileCache<DiscoveryDocument>
    {
        public DiscoveryDocumentCache() : base(new DirectoryInfo(FileSystem.CacheDirectory), ".taxDiscoDocument", false)
        {
        }
    }
}