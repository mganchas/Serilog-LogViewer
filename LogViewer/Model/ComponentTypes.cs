using System.ComponentModel;

namespace LogViewer.Model
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
