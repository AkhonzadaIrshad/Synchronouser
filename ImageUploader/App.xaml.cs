using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using ImageUploader.Models.Helpers;
using ImageUploader.Views;
using Application = System.Windows.Application;

namespace ImageUploader
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool       _isExit;
        private NotifyIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow              =  new MainWindow();
            MainWindow.Closing      += MainWindowClosing;
            _notifyIcon             =  new NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon        =  ImageUploader.Properties.Resources.MyIcon;
            _notifyIcon.Visible     =  true;
            CreateContextMenu();

        

        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
                new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("MainWindow").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click          += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            MainWindow?.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow != null && MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized) MainWindow.WindowState = WindowState.Normal;

                MainWindow.Activate();
            }
            else
            {
                MainWindow?.Show();
            }
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow?.Hide();
            }
        }
    }
}