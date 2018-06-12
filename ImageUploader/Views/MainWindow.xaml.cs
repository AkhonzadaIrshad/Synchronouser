using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ImageUploader.Models.Helpers;
using ImageUploader.Models.Services;
using ImageUploader.ViewModels;
using ImageUploaderLibrary.Managers;
using Informanagement.Infrastructure.Helpers;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageUploader.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BusyIndicator = InstancePoolService.CreateInstance<BusyIndicator>();
            AppService = InstancePoolService.CreateInstance<AppService>();
            LoggingHelper = AppService.LoggingHelper;
            DirectoryHelper = AppService.DirectoryManager;
            Bootstrapper = new Bootstrapper(AppService);
            LoggList = AppService.LoggList;

            LoggingHelper.OnNewLog += LoggingHelper_OnNewLog;
            Bootstrapper.OnLoad += OnBootstrapp;
            Bootstrapper.Load();

            AutoSync autoSync = InstancePoolService.CreateInstance<AutoSync>();
            autoSync.OnFileUploaded += AutoSyncOnOnFileUploaded;
            autoSync.OnUploadFailed += AutoSyncOnOnUploadFailed;
            autoSync.Start(this);

        }



        private BusyIndicator BusyIndicator { get; }
        private IAppService AppService { get; }
        private LoggingHelper LoggingHelper { get; }
        private DirectoryManager DirectoryHelper { get; }
        private Bootstrapper Bootstrapper { get; }

        private static ObservableCollection<LoggViewModel> LoggList { get; set; }

        private void OnBootstrapp(object sender, EventArgs args)
        {
            BusyIndicator.Start(LoaderInfoGif, LoaderInfoText, "bootstraping...");
            var grids = new List<KeyValuePair<ElementKeys, Grid>>
            {
                new KeyValuePair<ElementKeys, Grid>(ElementKeys.Pending,   PendingImagesGrid),
                new KeyValuePair<ElementKeys, Grid>(ElementKeys.Completed, CompletedImagesGrid),
                new KeyValuePair<ElementKeys, Grid>(ElementKeys.Failed,    FailedImagesGrid)
            };
            var scrolls = new List<KeyValuePair<ElementKeys, ScrollViewer>>
            {
                new KeyValuePair<ElementKeys, ScrollViewer>(ElementKeys.Pending,   PendingScroller),
                new KeyValuePair<ElementKeys, ScrollViewer>(ElementKeys.Completed, CompletedScroller),
                new KeyValuePair<ElementKeys, ScrollViewer>(ElementKeys.Failed,    FailedScroller)
            };
            var tabs = new List<KeyValuePair<ElementKeys, TabItem>>
            {
                new KeyValuePair<ElementKeys, TabItem>(ElementKeys.Pending,   PendingTab),
                new KeyValuePair<ElementKeys, TabItem>(ElementKeys.Completed, CompletedTab),
                new KeyValuePair<ElementKeys, TabItem>(ElementKeys.Failed,    FailedTab)
            };
            Bootstrapper.Start(grids, scrolls, tabs);
            BusyIndicator.Stop(LoaderInfoGif, LoaderInfoText);
        }


        private void LoggingHelper_OnNewLog(object sender, EventArgs e)
        {
            var args = (CustomLoggArgs)e;
            LoggList.Add(new LoggViewModel
            {
                Id = LoggList.Count + 1,
                Date = DateTime.Now,
                Summary = args.Exception.Message,
                Description = args.Exception.ToFriendlyError()
            });
            LogsTab.Tag = LoggList.Count;
            LogsTab.Header = $"Logs ({LoggList.Count})";
            LoggsGrid.ItemsSource = LoggList;
        }



        private void Move(ImageEventArgs args)
        {
            var moved = DirectoryHelper.HelperJob(HelperAction.Move,
                args.ImagePath,
                args.TargetPath);
            if (!moved)
            {
                var nameWithStamp = $"{DateTime.Now:yyyyMMddHHmmssffff}|" +
                                    $"{args.ImagePath?.GetFileName()}";
                DirectoryHelper.HelperJob(HelperAction.Move,
                    args.ImagePath, DirectoryHelper.TempDirectory, nameWithStamp.Replace("|", ""));
            }
        }

        private void SyncHelper_OnUploadFailed(object sender, EventArgs e)
        {
            try
            {
                var list = ((MainWindow)sender).PendingImagesGrid.Children
                    .OfType<Grid>()
                    .ToList();
                if (list.Count <= 0) return;
                var args = (ImageEventArgs)e;
                Move(args);
                var tag = args.ImagePath.GetFileName();
                var g = list.First(x => x.Tag.Equals(tag));
                PendingImagesGrid.Children.Remove(g);
                AppService.CreateImagesGrid(new List<string>
                {
                    Path.Combine(DirectoryHelper.FailedImagesPath, args.ImagePath.GetFileName())
                }, FailedImagesGrid, FailedScroller, false);

                var up = Convert.ToInt32(FailedTab.Tag) + 1;
                FailedTab.Tag = up;
                FailedTab.Header = $"Failed ({up})";
                var down = Convert.ToInt32(PendingTab.Tag) - 1;
                PendingTab.Tag = down;
                PendingTab.Header = $"Pending ({down})";
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
            }
        }

        private void SyncHelper_onFileUploaded(object sender, EventArgs e)
        {
            try
            {
                var list = ((MainWindow)sender).PendingImagesGrid.Children
                    .OfType<Grid>()
                    .ToList();

                var args = (ImageEventArgs)e;
                //DirectoryHelper.HelperJob(HelperAction.Move, args.ImagePath, args.TargetPath);
                Move(args);

                var tag = args.ImagePath.GetFileName();

                AppService.CreateImagesGrid(new List<string>
                {
                    Path.Combine(DirectoryHelper.UploadedImagesPath, args.ImagePath.GetFileName())
                }, CompletedImagesGrid, CompletedScroller, false);

                var up = Convert.ToInt32(CompletedTab.Tag) + 1;
                CompletedTab.Tag = up;
                CompletedTab.Header = $"Completed ({up})";
                var down = Convert.ToInt32(PendingTab.Tag) - 1;
                PendingTab.Tag = down;
                PendingTab.Header = $"Pending ({down})";

                var done = Convert.ToInt32(LoaderInfoArea.Tag) + 1;
                LoaderInfoArea.Tag = done;



                if (list.Count <= 0) return;


                var g = list.First(x => x.Tag.Equals(tag));
                PendingImagesGrid.Children.Remove(g);
                BusyIndicator.Start(LoaderInfoGif, LoaderInfoText, $"uploaded ({done})");


            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
            }
        }



        private void AutoSyncOnOnUploadFailed(object sender, EventArgs e)
        {
            var args = (ImageEventArgs)e;
            Move(args);
            AppService.CreateImagesGrid(new List<string>
            {
                Path.Combine(DirectoryHelper.FailedImagesPath, args.ImagePath.GetFileName())
            }, FailedImagesGrid, FailedScroller, false);
            var up = Convert.ToInt32(FailedTab.Tag) + 1;
            FailedTab.Tag = up;
            FailedTab.Header = $"Failed ({up})";

        }

        private void AutoSyncOnOnFileUploaded(object sender, EventArgs e)
        {
            var args = (ImageEventArgs)e;
            Move(args);
            AppService.CreateImagesGrid(new List<string>
            {
                Path.Combine(DirectoryHelper.UploadedImagesPath, args.ImagePath.GetFileName())
            }, CompletedImagesGrid, CompletedScroller, false);
            var up = Convert.ToInt32(CompletedTab.Tag) + 1;
            CompletedTab.Tag = up;
            CompletedTab.Header = $"Completed ({up})";

        }






        private void BrowseBtnClick(object sender, RoutedEventArgs e)
        {
            PendingTab.Tag = 0;
            PendingImagesGrid.Children.Clear();

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = ConfigManager.GetValue(ConfigKeys.FileDialogFilter),
                Multiselect = true
            };
            var result = dialog.ShowDialog();
            if (result != true) return;
            DragDropArea.Visibility = Visibility.Hidden;
            PendingScroller.Tag = "0,0,1";
            var count = Convert.ToInt32(PendingTab.Tag) + dialog.FileNames.Length;
            PendingTab.Tag = count;
            PendingTab.Header = $"pending ({count})";
            var gridMade = AppService.CreateImagesGrid(dialog.FileNames, PendingImagesGrid, PendingScroller, true);
            if (!gridMade) MessageBox.Show("Grid Generation Failed");
        }


        private void UploadBtnClick(object sender, RoutedEventArgs e)
        {
            var images = AppService.GetSelected(PendingImagesGrid).Item2;
            if (images.Count > 0)
            {
                AppService.MoveToUpload(images);
                var manualSync = InstancePoolService.CreateInstance<ManualSync>();
                manualSync.OnFileUploaded += SyncHelper_onFileUploaded;
                manualSync.OnUploadFailed += SyncHelper_OnUploadFailed;
                manualSync.OnAllFilesUploaded += SyncHelper_OnAllFilesUploaded;
                manualSync.Start(this);
                BusyIndicator.Start(LoaderInfoGif, LoaderInfoText);
            }
            else
            {
                MessageBox.Show("Add Images To Upload");
            }
        }

        private void SyncHelper_OnAllFilesUploaded(object sender, EventArgs e)
        {
            BusyIndicator.Stop(LoaderInfoGif, LoaderInfoText);
            PendingTab.Tag = "0";
            PendingTab.Header = "Pending";
            PendingScroller.Tag = "0,0,1";
            PendingImagesGrid.Height = 103;
            DragDropArea.Visibility = Visibility.Visible;
        }

        private void MarkAll_Click(object sender, RoutedEventArgs e)
        {
            AppService.MarkAll(PendingImagesGrid);
        }

        private void UnmarkAll_Click(object sender, RoutedEventArgs e)
        {
            AppService.UnMarkAll(PendingImagesGrid);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            DragDropArea.Visibility = Visibility.Visible;
            PendingScroller.Tag = "0,0,1";
            AppService.ClearAll(PendingImagesGrid);
        }

        private void ClearMarked_Click(object sender, RoutedEventArgs e)
        {
            AppService.ClearMarked(PendingImagesGrid);
        }

        private void PendingImagesGrid_OnDragLeave(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            DragDropArea.Visibility = Visibility.Hidden;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var count = Convert.ToInt32(PendingTab.Tag) + files?.Length;
            PendingTab.Tag = count == null ? 0 : Convert.ToInt32(count);
            PendingTab.Header = $"pending ({count?.ToString()})";
            AppService.CreateImagesGrid(files, PendingImagesGrid, PendingScroller, true);
        }

        private void SettingsDiaglogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true
                };

                dialog.ShowDialog();
                CustomPath.Text = dialog.FileName;
                new SettingHelper().CustomPath = CustomPath.Text;
                DirectoryHelper.CreateDirectories();
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
                CustomPath.Text = new SettingHelper().CustomPath;
            }
        }


        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            CustomPath.Text = new SettingHelper().CustomPath;
            actionMenu.Visibility = Visibility.Hidden;
        }

        private void CompletedTab_OnGotFocus(object sender, RoutedEventArgs e)
        {
            actionMenu.Visibility = Visibility.Hidden;
        }

        private void FailedTab_OnGotFocus(object sender, RoutedEventArgs e)
        {
            actionMenu.Visibility = Visibility.Hidden;
        }

        private void LogsTab_OnGotFocus(object sender, RoutedEventArgs e)
        {
            actionMenu.Visibility = Visibility.Hidden;
        }

        private void PendingTab_OnGotFocus(object sender, RoutedEventArgs e)
        {
            actionMenu.Visibility = Visibility.Visible;
        }
    }
}