using LogViewer.Model;
using LogViewer.ViewModel;
using System.Web.Http;

namespace LogViewer.Services
{
    public class HttpEventLogger : ApiController
    {
        [Route("{route?}")]
        public void Post([FromBody] LogEvents body)
        {
            string path = this.Url.Request.RequestUri.AbsoluteUri;

            foreach (var item in body.Events)
            {
                item.LevelType = Levels.GetLevelTypeFromString(item.Level);
                MessageContainer.HttpMessages[path].Add(item);
            }
        }
    }
}
