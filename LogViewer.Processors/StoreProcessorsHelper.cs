using LogViewer.Abstractions;
using LogViewer.Processors;
using LogViewer.Resources;
using LogViewer.Types;
using System.Collections.Generic;

namespace LogViewer.Processors
{
    public static class StoreProcessorsHelper
    {
        private static IEnumerable<IStoreProcessor> _storeProcessors;
        public static IEnumerable<IStoreProcessor> GetStoreProcessorsList(IEnrichedCommand command)
        {
            if (_storeProcessors == null)
            {
                _storeProcessors = new HashSet<IStoreProcessor>
                {
                    StoreTypeProcessorFactory.CreateNewStoreProcessor(Constants.StoreTypes.RAM,
                        $"{Constants.Images.ImagePath}{Constants.Images.ImageRam}", command, StoreTypes.Local),
                    StoreTypeProcessorFactory.CreateNewStoreProcessor(Constants.StoreTypes.MongoDB,
                        $"{Constants.Images.ImagePath}{Constants.Images.ImageMongoDB}", command, StoreTypes.MongoDB),
                    StoreTypeProcessorFactory.CreateNewStoreProcessor(Constants.StoreTypes.Sqlite,
                        $"{Constants.Images.ImagePath}{Constants.Images.ImageSqlite}", command, StoreTypes.SqlLite),
                    StoreTypeProcessorFactory.CreateNewStoreProcessor(Constants.StoreTypes.SqlServer,
                        $"{Constants.Images.ImagePath}{Constants.Images.ImageSqlServer}", command, StoreTypes.SqlServer)
                };
            }
            return _storeProcessors;
        }
    }
}