using LogViewer.Model;
using System.Web.Http;

namespace LogViewer.Services
{
    public class HttpProcessor : ApiController
    {
        [Route("{route?}")]
        public void Post([FromBody] LogEntries body)
        {
            string path = this.Url.Request.RequestUri.AbsoluteUri;

            foreach (var item in body.Entries)
            {
                item.LevelType = Levels.GetLevelTypeFromString(item.Level);
                item.RenderedMessage = $"{item.RenderedMessage} {item.Exception}";

                MessageContainer.HttpMessages[path].Add(item);
            }
        }
    }
}
