using System;

namespace ImageUploader.ViewModels
{
    public class LoggViewModel
    {
        public int      Id          { get; set; }
        public string   Summary     { get; set; }
        public DateTime Date        { get; set; }
        public string   Description { get; set; }
    }
}