using LiveViewer.Services;
using LiveViewer.Types;
using LiveViewer.ViewModel;
using System.Web.Http;

namespace LiveViewer.Controllers
{
    public class LogEventsController : ApiController
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
