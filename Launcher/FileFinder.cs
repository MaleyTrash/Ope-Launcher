using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using FileHelpers;
using System.Linq;

namespace Launcher
{
    public partial class MainWindow
    {
        class FileFinder
        {
            private static string _dirDataFile = "data.txt";
            private static string _exeInfoFile = "info.txt";
            public static List<Exe> GetExes(Project project) 
            {
                List<Exe> ret = new List<Exe>();
                    
                DirectoryInfo info = new DirectoryInfo(project.directory);
                if (!info.Exists)
                {
                    MessageBox.Show(String.Format("Složka {0} nenalezena!", project.directory), "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return ret;
                }
                foreach (FileInfo file in info.GetFiles("*.sln", SearchOption.AllDirectories))
                {
                    DirectoryInfo binDir = new DirectoryInfo(file.Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(file.Name) + "\\bin");
                    if (!binDir.Exists) continue;
                    foreach (FileInfo exe in binDir.GetFiles("*.exe", SearchOption.AllDirectories))
                    {
                        ret.Add(new Exe(exe));
                    }
                }
                return ret;
            }
            public static List<Project> GetPaths()
            {
                FileHelperEngine<Project> engine = new FileHelperEngine<Project>();
                if (!File.Exists(_dirDataFile)) File.Create(_dirDataFile).Dispose();
                return engine.ReadFileAsList(_dirDataFile);
            }

            public static ExeInfo GetExeInfo(Exe target)
            {
                string file = target.ProjectDir.FullName + "\\" + _exeInfoFile;
                FileHelperEngine<ExeInfo> engine = new FileHelperEngine<ExeInfo>();
                if (!File.Exists(file)) File.Create(file).Dispose();
                List<ExeInfo> infos =  engine.ReadFileAsList(file);
                ExeInfo ret = infos.Where(i => i.path.Equals(target.SimplePath)).FirstOrDefault();
                if(ret == null)
                {
                    return new ExeInfo();
                }
                return ret;
            }

            public static void SetExeInfo(Exe target,  ExeInfo info)
            {
                string file = target.ProjectDir.FullName + "\\" + _exeInfoFile;
                FileHelperEngine<ExeInfo> engine = new FileHelperEngine<ExeInfo>();
                if (!File.Exists(file)) File.Create(_dirDataFile).Dispose();
                List<ExeInfo> infos = engine.ReadFileAsList(file);
                ExeInfo curr = infos.Where(i => i.path.Equals(target.SimplePath)).FirstOrDefault();
                if (curr == null)
                {
                    infos.Add(info);
                }
                else
                {
                    infos[infos.IndexOf(curr)] = info;
                }
                engine.WriteFile(file, infos);
            }
        }
    }
}
