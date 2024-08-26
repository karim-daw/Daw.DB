using Daw.DB.Data.APIs;
using Microsoft.Extensions.DependencyInjection;

namespace Daw.DB.Data
{
    public static class ApiFactory
    {
        private static IGenericClientApi _clientApi;
        private static IGhClientApi _ghClientApi;
        private static IEventfulGhClientApi _eventDrivenGhClientApi;

        public static IEventfulGhClientApi GetEventDrivenGhClientApi()
        {
            if (_eventDrivenGhClientApi == null)
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices();
                _eventDrivenGhClientApi = serviceProvider.GetRequiredService<IEventfulGhClientApi>();
            }
            return _eventDrivenGhClientApi;
        }

        public static IGenericClientApi GetGenericClientApi()
        {
            if (_clientApi == null)
            {
                var serviceProvider = ServiceConfiguration.ConfigureServices();
                _clientApi = serviceProvider.GetRequiredService<IGenericClientApi>();
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
