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

        private readonly PictureBox picture;
        private readonly ToolStripStatusLabel? toolStripStatusLabel = null;

        private VideoCapture? capture;
        private Bitmap? image1;
        private Bitmap? image2;
        private Mat? frame;

        private bool toggle = false;

        #endregion

        #region Properties

        public int DisplayIndex { get; }

        public bool Enabled { get; set; } = true;

        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region Constructors

        public ImageDisplay(PictureBox picture, System.Windows.Forms.Timer timer, int displayIndex)
        {
            this.picture = picture;

            timer.Tick += Timer_Tick;

            DisplayIndex = displayIndex;
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

        public void Start(int index)
        {
            DisposeCameraResources();

            this.capture = new VideoCapture(index);
            this.capture.Open(index);
        }

        public void Stop()
        {
            DisposeCaptureResources();
        }

        #endregion

        #region Support routines

        private void Capture()
        {
            if (!this.Enabled) return;

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

                if (this.toolStripStatusLabel is not null)
                    this.toolStripStatusLabel.Text = this.toggle ? "-" : "|";
            }
        }

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

        #region Event handler routines

        private void Timer_Tick(object? sender, EventArgs e) => Capture();

        #endregion
    }
}
