using DirectShowLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Ifing
{
    public partial class MainForm : Form
    {
        #region Fields

        private const int FIRST_CAMERA = 0;

        private readonly List<DsDevice> devices = new();
        private readonly Dictionary<int, ImageDisplay> displays = new();

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Support routines

        private void Initialise()
        {
            this.devices.AddRange(
                DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));

            this.toolStripStatusLabel1.Text = $"Cameras detected: {this.devices.Count}";
            this.tsmiVideoStart.Enabled = this.tsmiVideoStop.Enabled = this.devices.Count > 0;

            for (var index = FIRST_CAMERA; index < this.devices.Count; index++)
            {
                var tssl = new ToolStripStatusLabel
                {
                    AutoSize = false,
                    Name = $"tsslDevice{index}",
                    Size = new System.Drawing.Size(20,20),
                    Spring = false,
                };
                this.statusStrip1.Items.Add(tssl);
                Initialise(index, this.devices[index], tssl);
            }
        }

        private void Initialise(int index, DsDevice device, ToolStripStatusLabel? tssl)
        {
            var picture = new PictureBox
            {
                BackColor = SystemColors.ActiveCaptionText,
                Name = $"picture{index}",
                Size = new System.Drawing.Size(814, 435),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Tag = index,
            };
            picture.DoubleClick += Picture_DoubleClick;

            var display = new ImageDisplay(picture, tssl);
            this.displays[index] = display;

            this.flowLayoutPanel.Controls.Add(picture);

            var tsmi = new ToolStripMenuItem
            {
                Name = $"tsmiVideoDevicesDevice{index}",
                Text = device.Name,
                Tag = index,
            };
            tsmi.Click += TsmiVideoDevice_Click;
            this.tsmiVideoDevices.DropDownItems.Add(tsmi);
        }

        private void Start()
        {
            this.tsmiVideoStart.Visible = false;
            this.tsmiVideoStop.Visible = true;

            //this.timer.Enabled = true;
            //this.timer.Start();
            Task.Run(() =>
            {
                foreach (var item in displays)
                    item.Value.Start(item.Key);
            });
        }

        private void Stop()
        {
            this.tsmiVideoStart.Visible = true;
            this.tsmiVideoStop.Visible = false;

            this.timer.Stop();
            foreach (var kvp in this.displays)
                kvp.Value.Stop();
            //this.timer.Enabled = false;
        }

        private void ToggleDevice(ToolStripItem? tsi)
        {
            if (tsi is null) return;
            ToggleDevice(tsi.Tag as int?);
        }

        private void ToggleDevice(Control? control)
        {
            if (control is null) return;
            ToggleDevice(control.Tag as int?);
        }

        private void ToggleDevice(int? index)
        {
            if (index is null) return;
            ToggleDevice(this.displays[index.Value]);
        }

        private static void ToggleDevice(ImageDisplay display) => display.Enabled = !display.Enabled;

        #endregion

        #region Control event handler routines

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) =>
            this.displays?.Values
                .ToList()
                .ForEach(n => n?.Dispose());

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => Stop();

        private void MainForm_Load(object sender, EventArgs e) => Initialise();

        private void Picture_DoubleClick(object? sender, EventArgs e) => ToggleDevice(sender as PictureBox);

        private void Timer_Tick(object sender, EventArgs e)
        {
            //foreach (var item in this.displays)
            //    item.Value.Capture();
        }

        #endregion

        #region Menu event handler routines

        private void TsmiFileExit_Click(object sender, EventArgs e) => Close();

        private void TsmiVideoStart_Click(object sender, EventArgs e) => Start();

        private void TsmiVideoStop_Click(object sender, EventArgs e) => Stop();

        private void TsmiVideoDevice_Click(object? sender, EventArgs e) => ToggleDevice(sender as ToolStripMenuItem);

        #endregion

        private class ImageDisplay :
            IDisposable
        {
            #region Fields

            private readonly PictureBox picture;
            private readonly ToolStripStatusLabel? toolStripStatusLabel = null;

            private readonly System.Windows.Forms.Timer timer;

            private VideoCapture? capture;
            private Bitmap? image1;
            private Bitmap? image2;
            private Mat? frame;

            private bool toggle = false;

            #endregion

            #region Properties

            public bool Enabled { get; set; } = true;

            public bool IsDisposed { get; private set; } = false;

            public bool IsRunning { get; private set; } = false;

            #endregion

            #region Constructors

            public ImageDisplay(PictureBox picture)
            {
                this.picture = picture;

                this.timer = new()
                {
                    Enabled = false,
                    Interval = 17, // mS
                };
                this.timer.Tick += Timer_Tick;
            }

            public ImageDisplay(PictureBox picture, ToolStripStatusLabel? toolStripStatusLabel)
                : this(picture) => this.toolStripStatusLabel = toolStripStatusLabel;

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

            private void Capture()
            {
                if (!this.IsRunning) return;
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

            public void Start(int index)
            {
                DisposeCameraResources();

                this.capture = new VideoCapture(index);
                this.capture.Open(index);

                this.timer.Enabled = true;

                this.IsRunning = true;
            }

            public void Stop()
            {
                this.timer.Enabled = false;

                this.IsRunning = false;

                DisposeCaptureResources();
            }

            #endregion

            #region Support routines

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
}