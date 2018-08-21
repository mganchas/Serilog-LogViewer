
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static LiveViewer.Types.Levels;

namespace LiveViewer.Model
{
    public class Entry
    {
        //public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        //public string LevelRaw { get; set; }
        public LevelTypes LevelType { get; set; }
        public string RenderedMessage { get; set; }
        public string Component { get; set; }
    }
}
