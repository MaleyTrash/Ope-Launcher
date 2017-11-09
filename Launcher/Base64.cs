using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace Launcher
{
    public partial class MainWindow
    {
        public static class Base64
        {
            public static BitmapSource StringToBitmap(string b64string)
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
}
