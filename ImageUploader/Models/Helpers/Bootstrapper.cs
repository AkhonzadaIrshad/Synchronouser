using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ImageUploader.Models.Services;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Helpers
{
    public enum ElementKeys
    {
        Pending,
        Completed,
        Failed
    }

    public class Bootstrapper
    {
        public Bootstrapper(IAppService appService)
        {
            AppService       = appService;
            DirectoryManager = AppService.DirectoryManager;
            DirectoryManager.CreateDirectories();
        }

        private IAppService                                   AppService       { get; }
        private DirectoryManager                              DirectoryManager { get; }
        private List<KeyValuePair<ElementKeys, Grid>>         Grids            { get; set; }
        private List<KeyValuePair<ElementKeys, ScrollViewer>> ScrollViewers    { get; set; }
        private List<KeyValuePair<ElementKeys, TabItem>>      Tabs             { get; set; }
        public event EventHandler                             OnLoad;

        public void Start(List<KeyValuePair<ElementKeys, Grid>> grids,
            List<KeyValuePair<ElementKeys, ScrollViewer>>       scrollViewers,
            List<KeyValuePair<ElementKeys, TabItem>>            tabs)
        {
            Grids         = grids;
            ScrollViewers = scrollViewers;
            Tabs          = tabs;
            foreach (var key in Enum.GetValues(typeof(ElementKeys)))
            {
                var  grid   = Grids.FirstOrDefault(x => x.Key.Equals(key)).Value;
                var  scroll = ScrollViewers.FirstOrDefault(x => x.Key.Equals(key)).Value;
                var  tab    = Tabs.FirstOrDefault(x => x.Key.Equals(key)).Value;
                int? count  = null;
                switch (key)
                {
                    case ElementKeys.Pending:
                        var files = DirectoryManager
                            .GetDirectoryFile(DirectoryManager.PendingImagesPath);
                        AppService
                            .CreateImagesGrid(
                                files,
                                grid,
                                scroll, true);
                        count = files.Length;
                        break;
                    case ElementKeys.Completed:
                        files = DirectoryManager
                            .GetDirectoryFile(DirectoryManager.UploadedImagesPath);
                        AppService
                            .CreateImagesGrid(
                                files,
                                grid,
                                scroll, false);
                        count = files.Length;
                        break;
                    case ElementKeys.Failed:
                        files = DirectoryManager
                            .GetDirectoryFile(DirectoryManager.FailedImagesPath);
                        AppService
                            .CreateImagesGrid(
                                files,
                                grid,
                                scroll, false);
                        count = files.Length;
                        break;
                }

                tab.Tag    = count == null ? 0 : Convert.ToInt32(count);
                tab.Header = $"{key} ({count?.ToString()})";
            }
        }

        public void Load()
        {
            OnLoad?.Invoke(this, EventArgs.Empty);
        }
    }
}