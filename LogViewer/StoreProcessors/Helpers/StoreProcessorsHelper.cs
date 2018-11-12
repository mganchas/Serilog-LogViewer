using System.Collections.Generic;
using LogViewer.Configs;
using LogViewer.ViewModelHelpers;

namespace LogViewer.StoreProcessors.Helpers
{
    public static class StoreProcessorsHelper
    {
        public static IEnumerable<StoreProcessorsVM> GetStoreProcessorsVMList(IEnrichedCommand command)
        {
            return new HashSet<StoreProcessorsVM>
            {
                new StoreProcessorsVM
                {
                    Name = Constants.StoreTypes.RAM,
                    Image = $"{Constants.Images.ImagePath}{Constants.Images.ImageRam}",
                    ExecutionParameter = StoreTypes.Local,
                    ExecutionCommand = command
                },
                new StoreProcessorsVM
                {
                    Name = Constants.StoreTypes.MongoDB,
                    Image = $"{Constants.Images.ImagePath}{Constants.Images.ImageMongoDB}",
                    ExecutionParameter = StoreTypes.MongoDB,
                    ExecutionCommand = command
                },
                new StoreProcessorsVM
                {
                    Name = Constants.StoreTypes.Sqlite,
                    Image = $"{Constants.Images.ImagePath}{Constants.Images.ImageSqlite}",
                    ExecutionParameter = StoreTypes.SqlLite,
                    ExecutionCommand = command
                },
                new StoreProcessorsVM
                {
                    Name = Constants.StoreTypes.SqlServer,
                    Image = $"{Constants.Images.ImagePath}{Constants.Images.ImageSqlServer}",
                    ExecutionParameter = StoreTypes.SqlServer,
                    ExecutionCommand = command
                },
                new StoreProcessorsVM
                {
                    Name = Constants.StoreTypes.ElasticSearch,
                    Image = $"{Constants.Images.ImagePath}{Constants.Images.ImageElasticSearch}",
                    ExecutionParameter = StoreTypes.ElasticSearch,
                    ExecutionCommand = command
                }
            };
        }
    }
}