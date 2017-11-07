using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                proj = new Project() { directory=(string)PathList.SelectedItems[0] };
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

            if(Banner.Source != null)
            {
                img = ImageToBase64((BitmapSource)Banner.Source);
            }
            if(Title.Text.Length > 0)
            {
                title = Title.Text;
            }
            if(Description.Text.Length > 0)
            {
                description = Description.Text;
            }
            FileFinder.SetExeInfo(target,new ExeInfo(target.SimplePath,title,description,img));
        }

        public void NewLocation_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    if(!dialog.SelectedPath.Equals(string.Empty))
                    {
                        FileFinder.NewPath(new Project() {directory=dialog.SelectedPath});
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
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(target.ProjectDir.FullName, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(target.ProjectDir.FullName, DestinationPath));
            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(target.ProjectDir.FullName, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(target.ProjectDir.FullName, DestinationPath), true);
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

        private void SelectChanged(object sender, RoutedEventArgs e)
        {
            Exe target = GetSelected();
            if (target == null) return;
            ExeInfo info = FileFinder.GetExeInfo(target);
            if (info.banner.Length > 0)
            {
                Banner.Source = Base64StringToBitmap(info.banner);
            }
            else
            {
                Banner.Source = null;
            }
            Title.Text = info.title;
            Description.Text = info.description;
        }

        public static BitmapSource Base64StringToBitmap(string b64string)
        {
            var bytes = Convert.FromBase64String(b64string);

            using (var stream = new MemoryStream(bytes))
            {
                return BitmapFrame.Create(stream,
                    BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        public static string ImageToBase64(BitmapSource bitmap)
        {
            var encoder = new PngBitmapEncoder();
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}
