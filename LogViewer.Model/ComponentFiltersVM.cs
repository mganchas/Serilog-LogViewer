using LogViewer.Types;
using System.Collections.Generic;
using System.Linq;

namespace LogViewer.Model
{
    public class ComponentFiltersVM
    {
        public string FilterText { get; set; }
        public IEnumerable<LevelTypes> Levels { get; set; }

        public bool Equals(ComponentFiltersVM obj)
        {
            return FilterText == obj.FilterText && Levels.SequenceEqual(obj.Levels);
        }
    }
}