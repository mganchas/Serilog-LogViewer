using LogViewer.ViewModel;
using LogViewer.ViewModel.Abstractions;
using System;
using System.Collections.ObjectModel;

namespace LogViewer.Services
{
    public static class ComponentFactory<T>
    {
        private const string MethodName = "IsValidComponent";
        public static T GetComponent<T>(object context, string name, string path, in ObservableCollection<ComponentVM> components, Func<string, string, T> creator) where T : ICustomComponent
        {
            T retObj = default;
            bool isValid = Convert.ToBoolean(typeof(T).GetMethod(MethodName).Invoke(context, new object[] { name, path, components }));
            if (isValid)
            {
                retObj = creator.Invoke(name, path);
            }
            return retObj;
        }
    }
}
