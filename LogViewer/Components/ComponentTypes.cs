using System.ComponentModel;

namespace LogViewer.Components
{
    public enum ComponentTypes
    {
        [Description("File")]
        File,

        [Description("HTTP")]
        Http,

        [Description("TCP")]
        Tcp,

        [Description("UDP")]
        Udp
    }
}
