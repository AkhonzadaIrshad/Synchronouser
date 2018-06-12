using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using ImageUploader.Models.Helpers;
using ImageUploader.ViewModels;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Services
{
    public class AppService : IAppService
    {
        public AppService()
        {
            DirectoryManager = new DirectoryManager();
            //DirectoryManager.ClearDirectory(DirectoryManager.TempDirectory);
            LoggingHelper = new LoggingHelper();
            LoggList      = new ObservableCollection<LoggViewModel>();
        }

        public DirectoryManager                    DirectoryManager { get; set; }
        public ObservableCollection<LoggViewModel> LoggList         { get; set; }
        public LoggingHelper                       LoggingHelper    { get; set; }
        public Bootstrapper                        Bootstrapper     { get; set; }


        public bool MoveToUpload(ICollection<string> files)
        {
            if (files == null || files.Count == 0) return false;
            foreach (var path in files) DirectoryManager.HelperJob(HelperAction.Copy, path);
            return true;
        }

        public bool UploadFailed(string filePath, Grid grid)
        {
            DirectoryManager.HelperJob(HelperAction.Move, filePath,
                DirectoryManager.FailedImagesPath);
            const string icon = @"/Assets/Images/failed.png";
            return UpdateImageStatus(icon, grid);
        }

        public bool UploadSuccess(string filePath, Grid grid)
        {
            var icon = @"/Assets/Images/done.png";
            return UpdateImageStatus(icon, grid);
        }

        public bool ClearMarked(Grid grid)
        {
            var (grids, _) = GetSelected(grid);
            foreach (var g in grids) grid.Children.Remove(g);
            return true;
        }


        public bool MarkAll(Grid grid)
        {
            return Action(grid, true);
        }

        public bool UnMarkAll(Grid grid)
        {
            return Action(grid, false);
        }

        public bool CreateImagesGrid(IEnumerable<string> files, Grid grid, ScrollViewer scrollViewer, bool checkBoxes)
        {
            var scc = scrollViewer.Tag
                .ToString()
                .Split(',')
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            var row    = scc[0];
            var column = scc[1];
            var count  = scc[2];
            try
            {
                foreach (var image in files)
                {
                    var button = new Button {Visibility = Visibility.Hidden};
                    if (checkBoxes)
                    {
                        button = new Button
                        {
                            Content    = "unchecked",
                            Height     = 15,
                            Width      = 15,
                            Margin     = new Thickness(60, -61, 0, 0),
                            Padding    = new Thickness(0,  -3,  0, 0),
                            Foreground = new SolidColorBrush(Colors.Transparent)
                        };
                        button.Click += (sender, args) =>
                        {
                            var btn = (Button) sender;
                            if (btn.Content.Equals("unchecked"))
                            {
                                var brush = new ImageBrush
                                {
                                    ImageSource =
                                        new BitmapImage(new Uri("Assets/Images/checkbox.png", UriKind.Relative))
                                };
                                btn.Background = brush;
                                btn.Content    = "checked";
                            }
                            else
                            {
                                btn.Background = null;
                                btn.Content    = "unchecked";
                            }
                        };
                    }

                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height    = GridLength.Auto,
                        MinHeight = 103
                    });
                    var childGrid = new Grid
                    {
                        Tag        = image.GetFileName(),
                        Height     = 80,
                        Width      = 80,
                        Background = Brushes.White,
                        Children =
                        {
                            new Image
                            {
                                Source = new BitmapImage(new Uri(image))
                            },
                            button
                        },
                        Effect = new DropShadowEffect
                        {
                            Color = new Color
                            {
                                ScA = 1,
                                ScB = 0,
                                ScG = 0,
                                ScR = 0
                            },
                            ShadowDepth = 1,
                            Opacity     = 0.6
                        }
                    };
                    Grid.SetRow(childGrid, row);
                    Grid.SetColumn(childGrid, column);
                    grid.Children.Add(childGrid);

                    if (count == 6)
                    {
                        row++;
                        column      =  -1;
                        count       =  0;
                        grid.Height += 103;
                    }

                    column++;
                    count++;
                }

                scrollViewer.Tag = $"{row},{column},{count}";
                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
                return false;
            }
        }

        public (List<Grid>, List<string>) GetSelected(Grid grid)
        {
            var list = grid.Children.OfType<Grid>().ToList();
            var temp = (from g in list
                    where g.Children
                        .OfType<Button>()
                        .First()
                        .Content
                        .Equals("checked")
                    select g)
                .ToList();
            return (temp, (from g in temp
                    select g.Children
                        .OfType<Image>()
                        .First()
                        .Source.ToString())
                .ToList());
        }

        public bool ClearAll(Grid grid)
        {
            grid.Children.Clear();
            return true;
        }

        public bool UpdateImageStatus(string statusIcon, Grid grid)
        {
            try
            {
                try
                {
                    var icon = grid.Children.OfType<GifHelper>().First();
                    icon.Dispose();
                    grid.Children.Remove(icon);
                }
                catch
                {
                    var icon = grid.Children.OfType<Image>().ToList()[1];
                    grid.Children.Remove(icon);
                }
                finally
                {
                    var simpleImage = new Image
                    {
                        Width  = 15,
                        Margin = new Thickness(-60, -61, 0, 0)
                    };
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(statusIcon, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    simpleImage.Source = bi;
                    grid.Children.Add(simpleImage);
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
                return false;
            }
        }

        private bool Action(Grid grid, bool mark)
        {
            try
            {
                var abc  = grid.Children.OfType<Grid>();
                var temp = abc.Select(x => x.Children.OfType<Button>().First()).ToList();
                foreach (var btn in temp)
                {
                    var brush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri("Assets/Images/checkbox.png", UriKind.Relative))
                    };
                    btn.Background = mark ? brush : null;
                    btn.Content    = mark ? "checked" : "unchecked";
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
                return false;
            }
        }
    }
}