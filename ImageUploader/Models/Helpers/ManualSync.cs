using System;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Helpers
{
    public  class ManualSync : SyncHelper
    {
        public void Start(object sender)
        {
            Timer.Tick     += (x, y) => SynchFiles(sender);
            Timer.Interval =  Convert.ToInt32(ConfigManager.GetValue(ConfigKeys.SyncImagesInterval));
            Timer.Start();
        }

        private async void SynchFiles(object sender = null)
        {
            Timer.Interval = 10000;
            var files = DirectoryHelper.GetDirectoryFile(DirectoryHelper.PendingImagesPath);
            await Sync(files, sender);
        }
    }
}