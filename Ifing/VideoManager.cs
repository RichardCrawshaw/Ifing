using DirectShowLib;

namespace Ifing
{
    internal class VideoManager :
        IVideoManager
    {
        #region Fields

        private readonly IPresenter presenter;

        private readonly List<DsDevice> devices = new();
        private readonly Dictionary<int, ImageDisplay> displays = new();

        #endregion

        #region Properties

        public int Count => this.devices.Count;

        public List<PictureBox> Images { get; } = new();

        public bool IsDisposed { get; private set; }

        public bool? IsRunning { get; private set; } = false;

        public List<ToolStripMenuItem> MenuItems { get; } = new();

        #endregion

        #region Indexers

        public ImageDisplay this[int index] => this.displays[index];

        #endregion

        #region Constructors

        public VideoManager(IPresenter presenter)
        {
            this.presenter = presenter;
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    this.displays?.Values
                        .ToList()
                        .ForEach(n => n?.Dispose());
                    this.devices?
                        .ForEach(n => n.Dispose());
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.displays?.Clear();
                this.devices?.Clear();

                this.IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VideoManager()
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

        public void Capture()
        {
            var tasks =
                this.displays.Values
                    .Select(n => Task.Run(() => n.Capture()));
            Task.WhenAll(tasks).Wait();
        }

        public void Initialise()
        {
            // Get a list of video input devices.
            this.devices.AddRange(
                DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));

            // Initialise each video device and create a menu item for it.
            Enumerable
                .Range(0, this.devices.Count)
                .ToList()
                .ForEach(n => this.displays[n] = Initialise(n, this.devices[n]));

            // Ensure that each device has a unique name presented to the user.
            foreach (var deviceName in this.MenuItems.Select(n => n.Text).Distinct())
            {
                if (this.MenuItems.Count(n => n.Text == deviceName) > 1)
                {
                    var index = 1;
                    foreach (var tsmi in this.MenuItems.Where(n => n.Text == deviceName))
                        tsmi.Text = $"{tsmi.Text} #{index++}";
                }
            }
        }

        public async Task StartAsync()
        {
            if (this.IsRunning ?? true) return;
            this.IsRunning = null;

            Start(true);

            var tasks =
                this.displays
                    .Select(item => item.Value.StartAsync(item.Key));

            await
                Task.WhenAll(tasks)
                    .ContinueWith(t => Start(false));
        }

        public void Stop()
        {
            if (!(this.IsRunning ?? false)) return;
            this.IsRunning = null;

            Stop(true);

            foreach (var kvp in this.displays)
                kvp.Value.Stop();
            foreach (ToolStripMenuItem tsmi in this.MenuItems)
                tsmi.Checked = false;

            Stop(false);
        }

        #endregion

        #region Support routines

        private static PictureBox Initialise(int index)
        {
            var picture = new PictureBox
            {
                BackColor = SystemColors.ActiveCaptionText,
                Name = $"picture{index}",
                Size = new System.Drawing.Size(814, 435),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Tag = index,
            };

            return picture;
        }

        private static ToolStripMenuItem Initialise(int index, string name)
        {
            var tsmi = new ToolStripMenuItem
            {
                Name = $"tsmiVideoDevicesDevice{index}",
                Text = name,
                Tag = index,
            };

            return tsmi;
        }

        private ImageDisplay Initialise(int index, DsDevice device)
        {
            var picture = Initialise(index);
            this.Images.Add(picture);

            var tsmi = Initialise(index, device.Name);
            this.MenuItems.Add(tsmi);

            var imageDisplay = new ImageDisplay(this.presenter, picture, index, tsmi);

            return imageDisplay;
        }

        private void Start(bool flag)
        {
            this.presenter.Start(flag);

            if (!flag)
            {
                this.presenter.Start();

                this.IsRunning = true;
            }
        }

        private void Stop(bool flag)
        {
            if (flag)
                this.presenter.Stop();

            this.presenter.Stop(flag);

            if (!flag)
                this.IsRunning = false;
        }

        #endregion
    }
}
