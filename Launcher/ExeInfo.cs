using FileHelpers;

namespace Launcher
{
    public partial class MainWindow
    {
        [DelimitedRecord(",")]
        class ExeInfo
        {
            public string path = "";
            public string title = "";
            public string description = "";
            public string banner = "";

            public ExeInfo(string path,string title,string description,string banner)
            {
                this.path = path;
                this.title = title;
                this.description = description;
                this.banner = banner;
            }

            public ExeInfo() { }
        }
    }
}
