using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageUploaderLibrary.Managers;

namespace ImageUploader.Models.Helpers
{
    public class ImageEventArgs : EventArgs
    {
        public string ImagePath { get; set; }
        public string TargetPath { get; set; }
    }


    public class SyncHelper
    {
        //private  static SyncHelper Helper { get; set; }
        // public SyncHelper CreateInstance()
        // {
        //     if (Helper==null)
        //         return new SyncHelper();
        //     return Helper;
        // }
        protected SyncHelper()
        {
            Timer = new Timer();
            DirectoryHelper = new DirectoryManager();
            LoggingHelper = new LoggingHelper();
        }

        protected Timer Timer { get; }
        protected DirectoryManager DirectoryHelper { get; }
        private LoggingHelper LoggingHelper { get; }

        public event EventHandler OnFileUploaded;
        public event EventHandler OnUploadFailed;
        public event EventHandler OnAllFilesUploaded;

        private byte[] ImageToByteArray(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }

        private static string tempFileName = String.Empty;
        protected async Task<bool> Sync(string[] images, object sender)
        {
            try
            {
                foreach (var filePath in images)
                {
                    var isUploaded = false;
                    var tags = filePath.GetFileTags();
                    var fileNotExists = !DirectoryHelper.FileExists(Path.Combine(
                        DirectoryHelper.UploadedImagesPath,
                        filePath.GetFileName()));
                    var path = DirectoryHelper.FailedImagesPath;
                    if (fileNotExists)
                    {
                        path = DirectoryHelper.UploadedImagesPath;
                        try
                        {
                            var fileName = filePath.GetFileName();
                            if (tempFileName.Equals(fileName)) return false;
                            tempFileName = fileName;
                            using (var img = Image.FromFile(filePath))
                            {
                                isUploaded = await Upload(ImageToByteArray(img), fileName, tags);
                            }
                        }
                        catch (Exception e)
                        {
                            LoggingHelper.Save(e);
                            isUploaded = false;
                        }
                    }

                    var done = fileNotExists && isUploaded;
                    if (done)
                        OnFileUploaded?.Invoke(sender, new ImageEventArgs
                        {
                            ImagePath = filePath,
                            TargetPath = path
                        });
                    else
                        OnUploadFailed?.Invoke(sender, new ImageEventArgs
                        {
                            ImagePath = filePath,
                            TargetPath = path
                        });
                    OnAllFilesUploaded?.Invoke(sender, EventArgs.Empty);
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.Save(ex);
                return false;
            }
        }


        private async Task<bool> Upload(byte[] fileBytes, string fileName, string fileTags)
        {

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders
                      .Authorization =
                       new AuthenticationHeaderValue("Bearer", ConfigManager.GetValue(ConfigKeys.AuthToken));
                var apiUri = ConfigManager.GetValue(ConfigKeys.UploadImagesUrl);
                var imageBinaryContent = new ByteArrayContent(fileBytes);
                var fileNameContent = new StringContent(fileName);
                var fileTagsContent = new StringContent(fileTags);

                var multipartContent = new MultipartFormDataContent
                {
                    {
                        fileNameContent, "fileName"
                    },
                    {
                        fileTagsContent, "fileTags"
                    },
                    {
                        imageBinaryContent, "image"
                    }
                };
                var response = await client.PostAsync(apiUri, multipartContent);
                return response.IsSuccessStatusCode;
            }
        }

        protected void Stop()
        {
            Timer.Enabled = false;
            Timer.Dispose();
            Timer.Stop();
        }
    }
}