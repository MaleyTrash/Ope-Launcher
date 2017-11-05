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

namespace Launcher
{
    public partial class MainWindow : Window
    {
        List<string> display;
        List<Exe> exes;
        public MainWindow()
        {
            InitializeComponent();

            List<Exe> data = new List<Exe>();
            foreach (Project item in FileFinder.GetPaths())
            {
                data.AddRange(FileFinder.GetExes(item));
            }
            display = data.Select(i => i.Display).ToList();
            exes = data;

            List.SelectionChanged += SelectChanged;
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
