using System;
using System.Collections.Generic;
using LogViewer.Abstractions;
using LogViewer.Types;

namespace LogViewer.Listeners
{
    public static class ComponentProcessorFactory
    {
        private static readonly Dictionary<ComponentTypes, Type> _availableComponents;

        static ComponentProcessorFactory()
        {
            _availableComponents = new Dictionary<ComponentTypes, Type>
            {
                {ComponentTypes.File, typeof(FileListener)},
                {ComponentTypes.Http, typeof(HttpListener)},
                {ComponentTypes.Tcp, typeof(TcpListener)},
                {ComponentTypes.Udp, typeof(UdpListener)}
            };
        }
        
        public static IComponentProcessor GetComponentProcessor(this ComponentTypes componentType)
        {
            if (!_availableComponents.ContainsKey(componentType)) {
                throw new ArgumentException("Invalid component");
            }

            return (IComponentProcessor)Activator.CreateInstance(_availableComponents[componentType]);
        }
    }
}