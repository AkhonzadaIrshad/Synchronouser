using System;
using System.Windows.Controls;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Helpers
{
    public class UploadingGif
    {
        public UploadingGif(string source, float height, float width, bool autoStart)
        {
            Width = width;
            Height = height;
            Source = source;
            AutoStart = autoStart;
        }

        public float Height { get; set; }
        public float Width { get; set; }
        public string Source { get; set; }
        public bool AutoStart { get; set; }
    }

    public class BusyIndicator
    {
        public BusyIndicator(string source = "/Assets/Images/uploading.gif",
            float height = 22,
            float width = 22,
            bool autoStart = true)
        {
            Gif = new UploadingGif(source, height, width, autoStart);
        }
        public BusyIndicator()
        {
             Gif = new UploadingGif("/Assets/Images/uploading.gif", 22, 22, true);
        }

        private UploadingGif Gif { get; }

        public bool Start(Grid grid, Label label, string message = "uploading..")
        {
            try
            {
                grid.Children.Add(new GifHelper
                {
                    Width = Gif.Width,
                    Height = Gif.Height,
                    GifSource = Gif.Source,
                    IsAutoStart = Gif.AutoStart
                });
                label.Content = message;
                return true;
            }
            catch (Exception ex)
            {
                new LoggingHelper().Save(ex);
                return false;
            }
        }

        public bool Stop(Grid grid, Label label, string message = "")
        {
            try
            {
                grid.Children.Clear();
                label.Content = message;
                return true;
            }
            catch (Exception ex)
            {
                new LoggingHelper().Save(ex);
                return false;
            }
        }
    }
}