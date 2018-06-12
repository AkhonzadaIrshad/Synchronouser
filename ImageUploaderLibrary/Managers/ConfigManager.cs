using System.Configuration;

namespace ImageUploaderLibrary.Managers
{
    public enum ConfigKeys
    {
        UploadImagesPath,
        UploadedImagesPath,
        FailedImagesPath,
        PendingImagesPath,
        SyncImagesInterval,
        UploadImagesUrl,
        TempDirectory,
        FileDialogFilter,
        AuthToken
    }

    public static class ConfigManager
    {
        public static string GetValue(ConfigKeys key)
        {
            var path = ConfigurationManager.AppSettings[key.ToString()];
            return path;
        }
    }
}