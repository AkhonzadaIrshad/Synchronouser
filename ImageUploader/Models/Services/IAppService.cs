using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ImageUploader.Models.Helpers;
using ImageUploader.ViewModels;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Services
{
    public interface IAppService
    {
        DirectoryManager                    DirectoryManager { get; set; }
        ObservableCollection<LoggViewModel> LoggList         { get; set; }
        LoggingHelper                       LoggingHelper    { get; set; }
        Bootstrapper                        Bootstrapper     { get; set; }
        bool                                MoveToUpload(ICollection<string> files);
        bool                                UploadFailed(string              filePath, Grid grid);
        bool                                UploadSuccess(string             filePath, Grid grid);
        (List<Grid>, List<string>)          GetSelected(Grid                 grid);
        bool                                ClearAll(Grid                    grid);
        bool                                ClearMarked(Grid                 grid);
        bool                                MarkAll(Grid                     grid);
        bool                                UnMarkAll(Grid                   grid);

        bool CreateImagesGrid(IEnumerable<string> files,        Grid grid,
            ScrollViewer                          scrollViewer, bool checkBoxes);
    }
}