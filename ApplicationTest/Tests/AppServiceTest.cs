using System.Windows.Controls;
using ApplicationTest.Data;
using ImageUploader.Models.Services;
using ImageUploaderLibrary.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApplicationTest.Tests
{
    [TestClass]
    public class AppServiceTest
    {
        [TestMethod]
        public void GettingTagsWithTheirImage()
        {
            var result = Dummy.FileWithTags.GetFileTags();
            Assert.AreNotEqual(string.Empty, result);
        }

        [TestMethod]
        public void GettingTagsWithOurImage()
        {
            var result = Dummy.FileWithoutTags.GetFileTags();
            Assert.AreNotEqual(string.Empty, result);
        }

        [TestMethod]
        public void MoveImagesToUpload()
        {
            var result = new AppService().MoveToUpload(new[] {Dummy.FileWithTags});
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void UpdateImageStatusInGrid()
        {
            var result = new AppService().UpdateImageStatus(Dummy.FailedIcon, Dummy.GetGrid());
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void ImageUploadedSuccessfully()
        {
            var result = new AppService().UploadSuccess(Dummy.FileWithTags, Dummy.GetGrid());
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void ImageUploadedFailed()
        {
            var result = new AppService().UploadFailed(Dummy.FileWithTags, Dummy.GetGrid());
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void CreateImagesGrid()
        {
            var result =
                new AppService().CreateImagesGrid(new[] {Dummy.FileWithTags}, Dummy.GetGrid(), new ScrollViewer(),
                    false);
            Assert.AreEqual(true, result);
        }
    }
}