using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageUploader.Models.Helpers;
using ImageUploaderLibrary.Managers;

namespace ApplicationTest.Data
{
    public static class Dummy
    {
        public const string FileWithTags =
            @"C:\Users\Hassan\Documents\Visual Studio 2017\Projects\ImageUploader\ApplicationTest\Assets\image.jpg";

        public const string FileWithoutTags =
            @"C:\Users\Hassan\Documents\Visual Studio 2017\Projects\ImageUploader\ApplicationTest\Assets\image2.jpg";

        public const string FailedIcon =
            @"C:\Users\Hassan\documents\visual studio 2017\Projects\ImageUploader\ApplicationTest\Assets\failed.png";

        public static Grid GetGrid()
        {
            return new Grid
            {
                Tag        = FileWithTags.GetFileName(),
                Height     = 80,
                Width      = 80,
                Background = Brushes.LightGray,
                Children =
                {
                    new Image
                    {
                        Source = new BitmapImage(new Uri(FileWithTags))
                    },
                    new GifHelper()
                }
            };
        }
    }
}