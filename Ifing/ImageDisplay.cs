using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Ifing
{
    /// <summary>
    /// Class to manage the display of a video stream via an <see cref="Image"/> on a <see cref="Form"/>.
    /// </summary>
    internal class ImageDisplay :
            IDisposable
    {
        #region Fields

        private readonly IPresenter? presenter;
        private readonly PictureBox picture;
        private readonly ToolStripMenuItem toolStripMenuItem;
        private VideoCapture? capture;
        private Bitmap? image1;
        private Bitmap? image2;
        private Mat? frame;

        private bool toggle = false;
        private bool enabled = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index into the video devices.
        /// </summary>
        public int DisplayIndex { get; }

        /// <summary>
        /// Gets and sets the enabled state.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                CheckMenuItem();
            }
        }

        /// <summary>
        /// Gets whether the current <see cref="ImageDisplay"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region Constructors

        public ImageDisplay(IPresenter? presenter,
                            PictureBox picture,
                            int displayIndex,
                            ToolStripMenuItem toolStripMenuItem)
        {
            this.presenter = presenter;
            this.picture = picture;
            this.DisplayIndex = displayIndex;
            this.toolStripMenuItem = toolStripMenuItem;
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    DisposeCaptureResources();
                    DisposeCameraResources();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.capture = null;
                this.frame = null;
                this.image1 = null;
                this.image2 = null;

                IsDisposed = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ImageDisplay()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Captures video.
        /// </summary>
        public void CaptureVideo()
        {
            if (!this.enabled) return;

            if (!(this.capture?.IsOpened() ?? false)) return;

            try
            {
                this.frame = new Mat();
                var captured = this.capture.Read(this.frame);
                if (this.frame is null || !captured || this.frame.Empty()) return;

                if (this.image2 is null)
                {
                    this.toggle = true;
                    this.image2 = BitmapConverter.ToBitmap(this.frame);
                }
                else if (this.image1 is null)
                {
                    this.toggle = false;
                    this.image1 = BitmapConverter.ToBitmap(this.frame);
                }

                this.picture.Image = this.toggle ? this.image2 : this.image1;
            }
            catch (Exception)
            {
                this.picture.Image = null;
            }
            finally
            {
                this.frame?.Dispose();
                if (this.toggle)
                {
                    this.image1?.Dispose();
                    this.image1 = null;
                }
                else
                {
                    this.image2?.Dispose();
                    this.image2 = null;
                }
            }
        }

        /// <summary>
        /// Asynchronously starts the current <see cref="ImageDisplay"/> instance.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation and contains a <see cref="bool"/> that is true on success; false otherwise.</returns>
        public async Task<bool> StartAsync()
        {
            DisposeCameraResources();

            await Task.Run(() =>
            {
                this.capture = new VideoCapture(this.DisplayIndex);
                this.enabled = this.capture.Open(this.DisplayIndex);

                this.presenter?.ResizePictureBox(this.picture, this.capture.FrameWidth, this.capture.FrameHeight);
            }).ContinueWith(t => CheckMenuItem());

            return this.enabled;
        }

        /// <summary>
        /// Stops the current <see cref="ImageDisplay"/>
        /// </summary>
        public void Stop()
        {
            DisposeCaptureResources();
        }

        #endregion

        #region Support routines

        private void CheckMenuItem() => this.presenter?.CheckMenuItem(this.toolStripMenuItem, this.enabled);

        private void DisposeCameraResources()
        {
            this.frame?.Dispose();
            this.image1?.Dispose();
            this.image2?.Dispose();
        }

        private void DisposeCaptureResources()
        {
            if (this.capture != null)
            {
                if (!this.capture.IsDisposed)
                    this.capture.Release();
                capture.Dispose();
            }
        }

        #endregion
    }
}
