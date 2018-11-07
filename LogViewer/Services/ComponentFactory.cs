using System;
using System.Collections.Generic;
using LogViewer.Model;
using LogViewer.ViewModel;

namespace LogViewer.Services
{
    public class ComponentFactory
    {
        private static readonly Dictionary<ComponentTypes, Type> _availableComponents = new Dictionary<ComponentTypes, Type>();

        static ComponentFactory()
        {
            _availableComponents.Add(ComponentTypes.File, typeof(FileComponentVM));
            _availableComponents.Add(ComponentTypes.Http, typeof(HttpComponentVM));
            _availableComponents.Add(ComponentTypes.Tcp, typeof(TcpComponentVM));
            _availableComponents.Add(ComponentTypes.Udp, typeof(UdpComponentVM));
        }
        
        public static ComponentVM GetNewComponent(ComponentTypes componentType, string name, string path)
        {
            if (!_availableComponents.ContainsKey(componentType))
            {
                throw new ArgumentException("Invalid component");
            }

            return (ComponentVM)Activator.CreateInstance(_availableComponents[componentType], name, path);
        }
    }
}