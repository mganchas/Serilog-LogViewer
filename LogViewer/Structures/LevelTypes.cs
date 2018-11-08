using System.ComponentModel;

namespace LogViewer.Model
{
    public enum LevelTypes
    {
        All,

        [Description("VRB")]
        Verbose,

        [Description("DBG")]
        Debug,

        [Description("INF")]
        Information,

        [Description("WRN")]
        Warning,

        [Description("ERR")]
        Error,

        [Description("FTL")]
        Fatal
    }
}