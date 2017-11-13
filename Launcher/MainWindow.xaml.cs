using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        ObservableCollection<string> paths = new ObservableCollection<string>();
        List<string> display;
        List<Exe> exes;
        public MainWindow()
        {
            InitializeComponent();

            foreach (Project item in FileFinder.GetPaths()) {
                paths.Add(item.directory);
            }

            PathList.ItemsSource = paths;
            PathList.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                RenderFiles();
            };
            List.SelectionChanged += SelectChanged;

            RenderFiles();
        }

        private void RenderFiles()
        {
            Project proj;
            try
            {
                proj = new Project() { directory = (string)PathList.SelectedItems[0] };
            }
            catch
            {
                return;
            }
            List<Exe> data = FileFinder.GetExes(proj);

            display = data.Select(i => i.Display).ToList();
            exes = data;

            List.ItemsSource = display;
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;
            try
            {
                Process.Start(target.Path);
            }
            catch
            {
                MessageBox.Show("Chyba při spouštění souboru!", "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                exes.RemoveAll(item => item.Equals(target));
                display.RemoveAll(item => item.Equals(target.Display));
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Vyberte obrázek";
            dlg.Filter = "Obrázky (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            dlg.ShowDialog();
            if (!dlg.FileName.Equals(String.Empty))
            {
                Debug.WriteLine(dlg.FileName);
                Banner.Source = new BitmapImage(new Uri(dlg.FileName));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;

            string img = "";
            string title = "";
            string description = "";

            if (Banner.Source != null)
            {
                img = Base64.ImageToBase64((BitmapSource)Banner.Source);
            }
            if (Title.Text.Length > 0)
            {
                title = Title.Text;
            }
            if (Description.Text.Length > 0)
            {
                description = Description.Text;
            }
            FileFinder.SetExeInfo(target, new ExeInfo(target.SimplePath, title, description, img));
        }

        public void NewLocation_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Vyberte složku s projekty";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    if (!dialog.SelectedPath.Equals(string.Empty))
                    {
                        FileFinder.NewPath(new Project() { directory = dialog.SelectedPath });
                        paths.Add(dialog.SelectedPath);
                    }
                }
            }
        }

        public void RemoveLocation_Click(object sender, RoutedEventArgs e)
        {
            Project proj;
            try
            {
                proj = new Project() { directory = (string)PathList.SelectedItems[0] };
            }
            catch
            {
                return;
            }
            paths.Remove(proj.directory);
            FileFinder.RemovePath(proj);
        }

        public void CopyToLocation_Click(object seSnder, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Vyberte cílovou složku";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    if (!dialog.SelectedPath.Equals(string.Empty))
                    {
                        try
                        {
                            string SourceDir = target.ProjectDir.FullName;
                            string TargetDir = dialog.SelectedPath + "\\" + target.ProjectDir.Name;
                            if (Directory.Exists(TargetDir))
                            {
                                MessageBoxResult res = MessageBox.Show(string.Format("Složka {0} v cíli již existuje. Chcete jí přepsat?", target.ProjectDir.Name), "Upozornění", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                                if (res == MessageBoxResult.No) return;
                            }
                            //Now Create all of the directories
                            foreach (string dirPath in Directory.GetDirectories(SourceDir, "*", SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(dirPath.Replace(SourceDir, TargetDir));
                            }
                            //Copy all the files & Replaces any files with the same name
                            foreach (string newPath in Directory.GetFiles(SourceDir, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(newPath, newPath.Replace(SourceDir, TargetDir), true);
                            }
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("Chyba při přesování projektu!", "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        public void Delete_Click(object seSnder, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;
            MessageBoxResult res = MessageBox.Show(string.Format("Opravdu chcete smazat projekt v {0}?", target.ProjectDir.FullName), "Upozornění", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (res == MessageBoxResult.No) return;
            //Delet
            target.ProjectDir.Delete(true);
            return;
        }

        private void SelectChanged(object sender, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;
            ExeInfo info = FileFinder.GetExeInfo(target);
            if (info.banner.Length > 0)
            {
                Banner.Source = Base64.StringToBitmap(info.banner);
            }
            else
            {
                Banner.Source = null;
            }
            Title.Text = info.title;
            Description.Text = info.description;
        }

        private Exe GetSelected()
        {
            try
            {
                string x = (string)List.SelectedItems[0];
                Exe file = exes.Where(i => i.Display.Equals(x)).FirstOrDefault();
                if (file == null) return null;
                return file;
            }
            catch
            {
                return null;
            }
        }
    }
}
