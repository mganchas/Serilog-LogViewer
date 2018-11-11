using System;
using System.Collections.Generic;
using LogViewer.Configs;

namespace LogViewer.Components.Helpers
{
    public static class ComponentImageFactory
    {
        private static readonly Dictionary<ComponentTypes, string> _availableComponents;

        static ComponentImageFactory()
        {
            _availableComponents = new Dictionary<ComponentTypes, string>
            {
                {ComponentTypes.File, $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}"},
                {ComponentTypes.Http, $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}"},
                {ComponentTypes.Tcp, $"{Constants.Images.ImagePath}{Constants.Images.ImageTcp}"},
                {ComponentTypes.Udp, $"{Constants.Images.ImagePath}{Constants.Images.ImageUdp}"}
            };
        }
        
        public static string GetComponentImage(this ComponentTypes componentType)
        {
            if (!_availableComponents.ContainsKey(componentType)) {
                throw new ArgumentException("Invalid component");
            }
            return _availableComponents[componentType];
        }
    }
}