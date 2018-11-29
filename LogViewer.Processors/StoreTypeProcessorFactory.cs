using LogViewer.Abstractions;
using System;
using System.Collections.Generic;
using LogViewer.Types;

namespace LogViewer.Processors
{
    public static class StoreTypeProcessorFactory
    {
        private static Dictionary<StoreTypes, Type> _availableProcessors { get; }

        static StoreTypeProcessorFactory()
        {
            _availableProcessors = new Dictionary<StoreTypes, Type>
            {
                {StoreTypes.Local, typeof(LocalProcessor)},
                {StoreTypes.MongoDB, typeof(MongoDBProcessor)},
                {StoreTypes.SqlLite, typeof(SqlLiteProcessor)},
                {StoreTypes.SqlServer, typeof(SqlServerProcessor)}
            };
        }

        public static IDbProcessor GetStoreProcessor(StoreTypes storeType)
        {
            if (!_availableProcessors.ContainsKey(storeType)) {
                throw new ArgumentException("Invalid store type");
            }
            return (IDbProcessor)Activator.CreateInstance(_availableProcessors[storeType]);
        }

        
        public static IStoreProcessor CreateNewStoreProcessor(string name, string image, IEnrichedCommand command, StoreTypes parameter)
        {
            var retObj = Activator.CreateInstance<IStoreProcessor>();
            retObj.Name = name;
            retObj.Image = image;
            retObj.ExecutionCommand = command;
            retObj.ExecutionParameter = parameter;
            return retObj;
        }
    }
}