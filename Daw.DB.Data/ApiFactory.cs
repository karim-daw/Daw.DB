using Daw.DB.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Daw.DB.Data
{
    public static class ApiFactory
    {
        private static IClientApi _clientApi;
        private static IGhClientApi _ghClientApi;

        public static IClientApi GetClientApi()
        {
            if (_clientApi == null)
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices();
                _clientApi = serviceProvider.GetRequiredService<IClientApi>();
            }
            return _clientApi;
        }

        /// <summary>
        /// Get Grasshopper client api
        /// Basic access ot functionality for CRUD operations
        /// </summary>
        /// <returns></returns>
        public static IGhClientApi GetGhClientApi()
        {
            if (_ghClientApi == null)
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices();
                _ghClientApi = serviceProvider.GetRequiredService<IGhClientApi>();
            }
            return _ghClientApi;
        }
    }
}
