using System;
using System.Collections.Generic;
using LogViewer.Components.Processors;
using LogViewer.Components.Processors.Abstractions;

namespace LogViewer.Components.Helpers
{
    public static class ComponentProcessorFactory
    {
        private static readonly Dictionary<ComponentTypes, IComponentProcessor> _availableComponents;

        static ComponentProcessorFactory()
        {
            _availableComponents = new Dictionary<ComponentTypes, IComponentProcessor>
            {
                {ComponentTypes.File, FileProcessor.Instance},
                {ComponentTypes.Http, HttpProcessor.Instance},
                {ComponentTypes.Tcp, TcpProcessor.Instance},
                {ComponentTypes.Udp, UdpProcessor.Instance}
            };
        }
        
        public static IComponentProcessor GetComponentProcessor(this ComponentTypes componentType)
        {
            if (!_availableComponents.ContainsKey(componentType)) {
                throw new ArgumentException("Invalid component");
            }
            return _availableComponents[componentType];
        }
    }
}