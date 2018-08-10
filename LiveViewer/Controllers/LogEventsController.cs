using System;
using System.Linq;
using System.Web.Http;
using System.Windows;
using System.Web.Http;
using LiveViewer.Utils;
using LiveViewer.ViewModel;
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
                //MessageContainer.Messages.Add(logEvent);
            }
        }
    }
}
