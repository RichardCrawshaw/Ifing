using DirectShowLib;

namespace Ifing
{
    public partial class MainForm : Form
    {
        #region Fields

        private readonly List<DsDevice> devices = new();
        private readonly Dictionary<int, ImageDisplay> displays = new();

        private bool? isRunning = false; // null indicates starting or stopping

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
            this.tssbVideoStart.Enabled =
            this.tsmiVideoStart.Enabled = 
            this.tsmiVideoStop.Enabled = 
                this.devices.Count > 0;

            // Initialise each video device and create a menu item for it.
            var deviceMenuItems =
                Enumerable
                    .Range(0, this.devices.Count)
                    .Select(n => Initialise(n, this.devices[n]))
                    .ToArray();

            // Ensure that each device has a unique name presented to the user.
            foreach (var deviceName in deviceMenuItems.Select(n => n.Text).Distinct())
            {
                if (deviceMenuItems.Count(n => n.Text == deviceName) > 1)
                {
                    var index = 1;
                    foreach (var tsmi in deviceMenuItems.Where(n => n.Text == deviceName))
                        tsmi.Text = $"{tsmi.Text} #{index++}";
                }
            }
            this.tsmiVideoDevices.DropDownItems.Clear();
            this.tsmiVideoDevices.DropDownItems.AddRange(deviceMenuItems);
        }

        private ToolStripMenuItem Initialise(int index, DsDevice device)
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

            this.displays[index] = new ImageDisplay(picture, this.timer, index);

            this.flowLayoutPanel.Controls.Add(picture);

            var tsmi = new ToolStripMenuItem
            {
                Name = $"tsmiVideoDevicesDevice{index}",
                Text = device.Name,
                Tag = index,
            };
            tsmi.Click += TsmiVideoDevice_Click;
            return tsmi;
        }

        private void Start()
        {
            if (this.isRunning ?? true) return;
            this.isRunning = null;

            Start(true);

            Task.Run(() =>
            {
                foreach (var item in this.displays)
                    item.Value.Start(item.Key);
                foreach (ToolStripMenuItem tsmi in this.tsmiVideoDevices.DropDownItems)
                    tsmi.Checked = true;
                Start(false);
            });
        }

        private void Start(bool flag)
        {
            if (flag)
            {
                this.tsmiVideoStart.Enabled = false;
                this.tssbVideoStart.Enabled = false;
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStart.Visible = false;
                    this.tsmiVideoStop.Visible = true;
                    this.tsmiVideoStop.Enabled = true;
                    this.tssbVideoStart.Text = "Stop video";
                    this.tssbVideoStart.Enabled = true;

                    this.timer.Start();
                });
                this.isRunning = true;
            }
        }

        private void Stop()
        {
            if (!(this.isRunning ?? false)) return;
            this.isRunning = null;

            Stop(true);

            foreach (var kvp in this.displays)
                kvp.Value.Stop();
            foreach (ToolStripMenuItem tsmi in this.tsmiVideoDevices.DropDownItems)
                tsmi.Checked = false;
            Stop(false);
        }

        private void Stop(bool flag)
        {
            if (flag)
            {
                this.timer.Stop();

                this.tsmiVideoStop.Enabled = false;
                this.tssbVideoStart.Enabled = false;
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStop.Visible = false;
                    this.tsmiVideoStart.Visible = true;
                    this.tsmiVideoStart.Enabled = true;
                    this.tssbVideoStart.Text = "Start video";
                    this.tssbVideoStart.Enabled = true;
                });
                this.isRunning = false;
            }
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

        private void ToggleDevice(ImageDisplay display)
        {
            display.Enabled = !display.Enabled;
            foreach (ToolStripMenuItem tsmi in this.tsmiVideoDevices.DropDownItems)
            {
                var displayIndex = tsmi.Tag as int?;
                if (displayIndex == display.DisplayIndex)
                    tsmi.Checked = display.Enabled;
            }
        }

        #endregion

        #region Control event handler routines

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) =>
            this.displays?.Values
                .ToList()
                .ForEach(n => n?.Dispose());

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => Stop();

        private void MainForm_Load(object sender, EventArgs e) => Initialise();

        private void Picture_DoubleClick(object? sender, EventArgs e) => ToggleDevice(sender as PictureBox);

        #endregion

        #region Menu event handler routines

        private void TsmiFileExit_Click(object sender, EventArgs e) => Close();

        private void TsmiVideoStart_Click(object sender, EventArgs e) => Start();

        private void TsmiVideoStop_Click(object sender, EventArgs e) => Stop();

        private void TsmiVideoDevice_Click(object? sender, EventArgs e) => ToggleDevice(sender as ToolStripMenuItem);

        #endregion

        #region Status bar event handler routines

        private void TssbVideoStart_ButtonClick(object sender, EventArgs e)
        {
            if (!this.isRunning.HasValue)
                return;
            else if (this.isRunning.Value)
                Stop();
            else
                Start();
        }

        #endregion
    }
}