using Daw.DB.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Daw.DB.Data
{
    public static class ApiFactory
    {
        private static IClientApi _clientApi;

        public static IClientApi GetClientApi()
        {
            if (_clientApi == null)
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices();
                _clientApi = serviceProvider.GetRequiredService<IClientApi>();
            }
            return _clientApi;
        }
    }
}
