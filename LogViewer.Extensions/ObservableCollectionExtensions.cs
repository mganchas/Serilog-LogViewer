using System;
using System.Collections.ObjectModel;
using System.Linq;
using LogViewer.Abstractions;

namespace LogViewer.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static bool Contains(this ObservableCollection<ICustomComponent> components, ICustomComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return components.Any(comp => comp.Name == component.Name || comp.Path == component.Path);
        }
        
        public static bool Contains(this ObservableCollection<ICustomComponent> components, string name, string path)
        {
            return components.Any(comp => comp.Name == name || comp.Path == path);
        }
    }
}