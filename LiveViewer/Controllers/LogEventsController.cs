using System.Web.Http;
using LiveViewer.Utils;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.Controllers
{
    public class LogEventsController : ApiController
    {
        [Route("{route?}")]
        public void Post([FromBody] LogEvents body)
        {
            foreach (var logEvent in body.Events)
            {
                string path = this.Url.Request.RequestUri.AbsoluteUri;
                MessageContainer.HttpMessages[path].Add(logEvent);
            }
        }
    }
}
