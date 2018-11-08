﻿namespace LogViewer.Configs
{
    public sealed class Constants
    {
        public class Component
        {
            public const int DefaultRows = 10;
        }

        public class Formats
        {
            public const string TimeFormat = "dd MMM yyyy HH:mm:ss";
        }

        public class Images
        {
            public const string ImagePath = "/LogViewer;component/Configs/Images/";
            public const string ImageSearch = "search.png";
            public const string ImageCancel = "cancel.png";
            public const string ImageClear = "clear.png";
            public const string ImageDelete = "delete.png";
            public const string ImageReset = "garbage.png";
            public const string ImageEdit = "edit.png";
            public const string ImageSave = "save.png";
            public const string ImageAdd = "add.png";
            public const string ImageError = "error.png";

            public const string ImageTerminal = "terminal.png";
            public const string ImageMonitor = "monitor.png";
            public const string ImageFilter = "filter.png";
            public const string ImagePlay = "play.png";
            public const string ImagePlayBlue = "play_blue.png";
            public const string ImageExpand = "expand.png";

            public const string ImageFile = "file.png";
            public const string ImageHttp = "http.png";
            public const string ImageTcp = "tcp.png";
            public const string ImageUdp = "udp.png";
        }

        public class Labels
        {
            public const string Messages = nameof(Messages);
            public const string Filters = nameof(Filters);
            public const string Levels = nameof(Levels);
            public const string Edit = nameof(Edit);
            public const string Remove = nameof(Remove);
            public const string StartRAM = "Start (RAM) execution";
            public const string StartDisk = "Start (Disk) execution";
            public const string Stop = "Stop execution";            
            public const string SaveChanges = "Save changes";
            public const string Timestamp = nameof(Timestamp);
            public const string Level = nameof(Level);
            public const string ComponentName = "Component Name";
            public const string ComponentType = "Component Type";
            public const string Path = "Path";
        }

        public class Tooltips
        {
            public const string StartAllRAM = "Start all";
            public const string StartAllDisk = "Start all (Disk)";
            public const string CancelAll = "Cancel all (if running)";
            public const string ClearAll = "Clear all data";
            public const string ResetAll = "Reset components";
            public const string ShowContext = "Show context";
            public const string ExportVisibleMessages = "Export visible messages";
        }

        public class Messages
        {
            public const string AlertTitle = "Alert";
            public const string ErrorTitle = "Error";
            public const string DuplicateComponent = "Component already on the Components list";
            public const string MandatoryFieldsMissingComponent = "Component Name Path are mandatory";
            public const string InvalidUrlComponent = "Invalid url: Remove protocol prefix (e.g. 127.0.0.1:12345)";
            public const string FileNotFoundComponent = "File not found";
            public const string ComponentTypeMissing = "You must select a component type";
        }
    }
}
