using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace ImageUploader.Models.Helpers
{
    public class GifHelper : Image
    {
        private Bitmap _bitmap;

        private bool         _isInitialized;
        private BitmapSource _source;

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource GetSource()
        {
            if (_bitmap == null)
                _bitmap = new Bitmap(Application.GetResourceStream(
                                             new Uri(GifSource, UriKind.RelativeOrAbsolute))
                                         ?.Stream ?? throw new InvalidOperationException());

            IntPtr handle;
            handle = _bitmap.GetHbitmap();

            var bs = Imaging.CreateBitmapSourceFromHBitmap(
                handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(handle);
            return bs;
        }

        private void Initialize()
        {
            //        Console.WriteLine("Init: " + GifSource);
            if (GifSource != null)
                Source = GetSource();
            _isInitialized = true;
        }

        private void FrameUpdatedCallback()
        {
            ImageAnimator.UpdateFrames();

            if (_source != null) _source.Freeze();

            _source = GetSource();

            //  Console.WriteLine("Working: " + GifSource);

            Source = _source;
            InvalidateVisual();
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(FrameUpdatedCallback));
        }

        /// <summary>
        ///     Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                Initialize();


            //   Console.WriteLine("Start: " + GifSource);

            ImageAnimator.Animate(_bitmap, OnFrameChanged);
        }

        /// <summary>
        ///     Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            _isInitialized = false;
            if (_bitmap != null)
            {
                ImageAnimator.StopAnimate(_bitmap, OnFrameChanged);
                _bitmap.Dispose();
                _bitmap = null;
            }

            _source = null;
            Initialize();
            GC.Collect();
            GC.WaitForFullGCComplete();

            //   Console.WriteLine("Stop: " + GifSource);
        }

        public void Dispose()
        {
            _isInitialized = false;
            if (_bitmap != null)
            {
                ImageAnimator.StopAnimate(_bitmap, OnFrameChanged);
                _bitmap.Dispose();
                _bitmap = null;
            }

            _source = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            // Console.WriteLine("Dispose: " + GifSource);
        }

        #region gif Source, such as "/IEXM;component/Images/Expression/f020.gif"

        public string GifSource
        {
            get => (string) GetValue(GifSourceProperty);
            set => SetValue(GifSourceProperty, value);
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(string),
                typeof(GifHelper), new UIPropertyMetadata(null, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs                        e)
        {
            (sender as GifHelper)?.Initialize();
        }

        #endregion

        #region control the animate

        /// <summary>
        ///     Defines whether the animation starts on it's own
        /// </summary>
        public bool IsAutoStart
        {
            get => (bool) GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("IsAutoStart", typeof(bool),
                typeof(GifHelper), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs                        e)
        {
            if ((bool) e.NewValue)
                (sender as GifHelper)?.StartAnimation();
            else
                (sender as GifHelper)?.StopAnimation();
        }

        #endregion
    }
}