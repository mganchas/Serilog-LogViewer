using System;
using System.Collections.Generic;
using LogViewer.Components.Processors.Abstractions;
using LogViewer.Services;
using LogViewer.Structures;
using LogViewer.ViewModel;

namespace LogViewer.Components.Processors
{
    public class ComponentFactory
    {
        private static readonly Dictionary<ComponentTypes, Type> _availableComponents = new Dictionary<ComponentTypes, Type>();

        static ComponentFactory()
        {
            _availableComponents.Add(ComponentTypes.File, typeof(FileProcessor));
            _availableComponents.Add(ComponentTypes.Http, typeof(HttpProcessor));
            _availableComponents.Add(ComponentTypes.Tcp, typeof(TcpProcessor));
            _availableComponents.Add(ComponentTypes.Udp, typeof(UdpProcessor));
        }
        
        public static IComponentProcessor GetNewComponent(ComponentTypes componentType)
        {
            if (!_availableComponents.ContainsKey(componentType)) {
                throw new ArgumentException("Invalid component");
            }

            return (IComponentProcessor)Activator.CreateInstance(_availableComponents[componentType]);
        }
    }
}