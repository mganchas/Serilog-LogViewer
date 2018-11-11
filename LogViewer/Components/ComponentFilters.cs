using System.Collections.Generic;
using System.Linq;
using LogViewer.Components.Levels;

namespace LogViewer.Components
{
    public class ComponentFilters
    {
        public string FilterText { get; set; }
        public IEnumerable<LevelTypes> Levels { get; set; }

        public bool Equals(ComponentFilters obj)
        {
            return FilterText == obj.FilterText && Levels.SequenceEqual(obj.Levels);
        }
    }
}