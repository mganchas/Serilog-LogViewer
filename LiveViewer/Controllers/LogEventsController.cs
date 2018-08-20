using LiveViewer.Services;
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
            string dbColl = HttpComponentVM.HttpListeners[path];

            foreach (var item in body.Events)
            {
                item.Component = dbColl;
            }
            
            // insert all entries into db
            new DbProcessor().InsertMany(body.Events);
        }
    }
}
