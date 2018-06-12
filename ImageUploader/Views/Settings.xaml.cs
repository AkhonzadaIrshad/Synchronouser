using System.Windows;
using ImageUploader.Models.Helpers;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageUploader.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            customPath.Text = new SettingHelper().CustomPath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            dialog.ShowDialog();
            customPath.Text = dialog.FileName;
        }

        private void saveCustomPath_Click(object sender, RoutedEventArgs e)
        {
            new SettingHelper().CustomPath = customPath.Text;
            new DirectoryHelper().CreateDirectories();
            Close();
        }
    }
}
