using FileHelpers;

namespace Launcher
{
    public partial class MainWindow
    {
        [DelimitedRecord(",")]
        class ExeInfo
        {
            public string path;
            public string title;
            public string description;
            public string banner;
        }
    }
}
