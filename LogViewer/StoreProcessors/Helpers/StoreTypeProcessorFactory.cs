using System;
using System.Collections.Generic;
using LogViewer.StoreProcessors.Abstractions;

namespace LogViewer.StoreProcessors.Helpers
{
    public static class StoreTypeProcessorFactory
    {
        private static Dictionary<StoreTypes, IDbProcessor> _availableProcessors { get; }

        static StoreTypeProcessorFactory()
        {
            _availableProcessors = new Dictionary<StoreTypes, IDbProcessor>
            {
                {StoreTypes.Local, LocalProcessor.Instance},
                {StoreTypes.MongoDB, MongoDBProcessor.Instance},
                {StoreTypes.ElasticSearch, ElasticSearchProcessor.Instance},
                {StoreTypes.SqlLite, SqlLiteProcessor.Instance},
                {StoreTypes.SqlServer, SqlServerProcessor.Instance}
            };
        }

        public static IDbProcessor GetStoreProcessor(StoreTypes storeType)
        {
            if (!_availableProcessors.ContainsKey(storeType))
            {
                throw new ArgumentException("Invalid store type");
            }
            return _availableProcessors[storeType];
        }
    }
}