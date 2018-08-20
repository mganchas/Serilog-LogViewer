namespace LiveViewer.Configs
{
    public static class Constants
    {
        public class Component
        {
            public const string DefaultHttpName = "DummyComponent";
            public const string DefaultFileName = "TM main log";
            public const string DefaultHttpPath = "http://localhost:8080/";
            public const string DefaultHttpRoute = "workflow-log-events";
            public const string DefaultFilePath = @"C:\Temp\Logs\tm\log-TransactionManagerApp-XXXX.txt";
            public const int DefaultTimer = 5000;
        }

        public class Images
        {
            public const string ImagePath = "/LiveViewer;component/Images/";
            public const string ImageSearch = "search.png";
            public const string ImageCancel = "cancel.png";
            public const string ImageEntry = "entry.png";
            public const string ImageExit = "exit.png";
            public const string ImageClear = "clear.png";
            public const string ImageDelete = "delete.png";
            public const string ImageEdit = "edit.png";
            public const string ImageSave = "save.png";
            public const string ImageAdd = "add.png";
            public const string ImageHttp = "component.png";
            public const string ImageFile = "file.png";
            public const string ImageTerminal = "terminal.png";
            public const string ImageMonitor = "monitor.png";
            public const string ImageFilter = "filter.png";
            public const string ImagePlay = "play.png";
        }

        public class Labels
        {
            public const string Messages = nameof(Messages);
            public const string Filters = nameof(Filters);
            public const string Levels = nameof(Levels);
            public const string Edit = nameof(Edit);
            public const string Remove = nameof(Remove);
            public const string StartStop = "Start/Stop execution";
            public const string SaveChanges = "Save changes";
            public const string Timestamp = nameof(Timestamp);
            public const string Level = nameof(Level);
            public const string ComponentName = "Component Name";
            public const string HttpPath = "Http path";
            public const string HttpRoute = "Http Route";
            public const string FileName = "File name";
            public const string FilePath = "File path";
        }
    }
}
