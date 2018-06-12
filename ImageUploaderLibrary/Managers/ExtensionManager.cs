using System;
using XperiCode.JpegMetadata;

namespace ImageUploaderLibrary.Managers
{
    public static class ExtensionHelper
    {
        public static string GetFileName(this string path)
        {
            var imageName         = path.ToUri().LocalPath.Split("\\".ToCharArray());
            var nameWithExtension = imageName[imageName.Length - 1].Split('.');
            return $"{nameWithExtension[0]}.{nameWithExtension[1]}";
        }

        public static string GetFileTags(this string path)
        {
            try
            {
                var metadataKeywords = new JpegMetadataAdapter(path).Metadata?.Keywords;
                if (metadataKeywords != null && metadataKeywords.Count > 0)
                    return string.Join(",", metadataKeywords);
                return " ";
            }
            catch
            {
                return " ";
            }
        }

        public static string GetFilePath(this string file)
        {
            var imageName = file.ToUri().LocalPath.Split("\\".ToCharArray());
            var lastIndex = imageName.Length - 1;
            var path      = string.Empty;
            for (var i = 0; i < imageName.Length; i++)
                if (i != lastIndex)
                    path += imageName[i] + "\\";

            return path;
        }

        public static Uri ToUri(this string path)
        {
            return new Uri(path);
        }
    }
}