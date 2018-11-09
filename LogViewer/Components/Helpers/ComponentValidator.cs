using System;
using LogViewer.Components;

namespace LogViewer.ViewModel.Helpers
{
    public static class ComponentValidator
    {
        public static bool Contains(this Span<ComponentVM> components, ComponentVM component)
        {
            foreach (var comp in components)
            {
                if (comp.Name == component.Name || comp.Path == component.Path) {
                    return true;
                }
            }
            return false;
        }
        
        public static bool Contains(this Span<ComponentVM> components, string name, string path)
        {
            foreach (var comp in components)
            {
                if (comp.Name == name || comp.Path == path) {
                    return true;
                }
            }
            return false;
        }
    }
}