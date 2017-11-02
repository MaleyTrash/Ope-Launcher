using System.IO;

namespace Launcher
{
    public partial class MainWindow
    {
        class Exe
        {
            public string Display;
            public string Path;
            public string SimplePath;
            public DirectoryInfo Dir;
            public DirectoryInfo ProjectDir;
            public Exe(FileInfo exe)
            {
                Display = string.Format("{0} - {1}", exe.Name, exe.Directory.Name);
                Path = exe.FullName;
                SimplePath = exe.Directory.Name+"\\"+exe.Name;
                Dir = exe.Directory;
                ProjectDir = exe.Directory.Parent.Parent.Parent;
            }
        }
    }
}
