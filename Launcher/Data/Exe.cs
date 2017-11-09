using System;
using System.IO;

namespace Launcher
{
    public partial class MainWindow
    {
        class Exe
        {
            private static int c = 1;

            public string Display;
            public string Path;
            public string SimplePath;
            public DirectoryInfo Dir;
            public DirectoryInfo ProjectDir;
            public Exe(FileInfo exe)
            {
                Path = exe.FullName;
                SimplePath = exe.Directory.Name+"\\"+exe.Name;
                Dir = exe.Directory;
                ProjectDir = exe.Directory.Parent.Parent.Parent;
                Display = string.Format("{0}. {1}\\{2} - {3}", c,ProjectDir.Name, exe.Name, exe.Directory.Name);
                c++;
            }

            public static void ResetCounter()
            {
                c = 1;
            }
        }
    }
}
