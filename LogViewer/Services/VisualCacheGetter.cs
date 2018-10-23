namespace LogViewer.Services
{
    public static class VisualCacheGetter
    {
        public static T GetCachedValue<T>(ref T cacheObj, T defaultValue) where T : class
        {
            if (cacheObj == null) {
                cacheObj = defaultValue;
            }
            return cacheObj;
        }
    }
}
