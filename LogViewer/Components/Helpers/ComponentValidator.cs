using System;
using System.Collections.Generic;
using System.Linq;

namespace LogViewer.Components.Helpers
{
    public static class ComponentValidator
    {
        public static bool Contains(this IEnumerable<ComponentVM> components, ComponentVM component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return components.Any(comp => comp.Name == component.Name || comp.Path == component.Path);
        }
        
        public static bool Contains(this IEnumerable<ComponentVM> components, string name, string path)
        {
            return components.Any(comp => comp.Name == name || comp.Path == path);
        }
    }
}