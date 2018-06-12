using System;
using System.IO;
using System.Linq;

namespace ImageUploaderLibrary.Managers
{
    public enum HelperAction
    {
        Copy,
        Move
    }

    public class DirectoryManager
    {
        public DirectoryManager()
        {
            LoggingHelper = new LoggingHelper();
        }

        private LoggingHelper LoggingHelper { get; }

        private string BinDirectory
        {
            get
            {
                var binPath = AppDomain.CurrentDomain.BaseDirectory;
                return binPath;
            }
        }

        public string TempDirectory
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(),
                    ConfigManager.GetValue(ConfigKeys.TempDirectory));
                return path;
            }
        }

        public string SourcePath
        {
            get
            {
                var customPath = new SettingHelper().CustomPath;
                var path = string.IsNullOrEmpty(customPath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    : customPath;
                var uploadImageDir = ConfigManager.GetValue(ConfigKeys.UploadImagesPath);
                path = Path.Combine(path,
                    string.IsNullOrEmpty(uploadImageDir) ? "InformaticsUploader" : uploadImageDir);
                return path;
            }
        }
        public string PendingImagesPath
        {
            get
            {
                var pendingImageDir = ConfigManager.GetValue(ConfigKeys.PendingImagesPath);
                var path = Path.Combine(SourcePath,
                    string.IsNullOrEmpty(pendingImageDir) ? "Pending" : pendingImageDir);
                return path;
            }
        }

        public string FailedImagesPath
        {
            get
            {
                var failedImageDir = ConfigManager.GetValue(ConfigKeys.FailedImagesPath);
                var path = Path.Combine(SourcePath,
                    string.IsNullOrEmpty(failedImageDir) ? "Failed" : failedImageDir);
                return path;
            }
        }

        public string UploadedImagesPath
        {
            get
            {
                var uploadedImageDir = ConfigManager.GetValue(ConfigKeys.UploadedImagesPath);
                var path = Path.Combine(SourcePath,
                    string.IsNullOrEmpty(uploadedImageDir) ? "Uploaded" : uploadedImageDir);
                return path;
            }
        }

        public void CreateDirectories(string path = null)
        {
            try
            {
                if (path == null)
                    path = SourcePath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                Directory.CreateDirectory(TempDirectory);
                Directory.CreateDirectory(UploadedImagesPath);
                Directory.CreateDirectory(FailedImagesPath);
                Directory.CreateDirectory(PendingImagesPath);
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
            }
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool ClearDirectory(string dirPath)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(dirPath).ToArray()) File.Delete(file);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string[] GetDirectoryFile(string path)
        {
            var files = Directory.EnumerateFiles(path).ToArray();
            return files;
        }

        public bool HelperJob(HelperAction action, string filePath, string destinationPath = null,
            string                         newFileName = "")
        {
            try
            {
                if (destinationPath == null) destinationPath = PendingImagesPath;
                if (action == HelperAction.Copy)
                    File.Copy(filePath.ToUri().LocalPath, Path.Combine(destinationPath,
                        filePath.GetFileName()));
                else
                    File.Move(filePath, Path.Combine(destinationPath,
                        newFileName == "" ? filePath.GetFileName() : newFileName));

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