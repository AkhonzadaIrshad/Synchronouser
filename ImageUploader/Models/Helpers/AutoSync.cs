using System;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Helpers
{
    public class AutoSync : SyncHelper
    {
        public void Start(object sender)
        {
            Timer.Tick     += (x, y) => SynchFiles(sender);
            Timer.Interval =  Convert.ToInt32(ConfigManager.GetValue(ConfigKeys.SyncImagesInterval));
            Timer.Start();
        }

        private async void SynchFiles(object sender = null)
        {
            var files = DirectoryHelper.GetDirectoryFile(DirectoryHelper.SourcePath);
            await Sync(files, sender);
        }
    }
}