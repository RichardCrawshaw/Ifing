namespace Ifing
{
    public partial class MainForm : Form,
        IPresenter
    {
        #region Fields

        private readonly VideoManager videoManager;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.videoManager = new VideoManager(this);
        }

        #endregion

        #region IPresenter methods

        /// <inheritdoc/>
        void IPresenter.CheckMenuItem(ToolStripMenuItem menuItem, bool isChecked) => 
            Invoke((MethodInvoker)delegate
            {
                menuItem.Checked = isChecked;
            });

        /// <inheritdoc/>
        void IPresenter.ResizePictureBox(PictureBox pictureBox, int width, int height) =>
            Invoke((MethodInvoker)delegate
            {
                pictureBox.Size = new Size(width, height);
            });

        /// <inheritdoc/>
        void IPresenter.Start() => 
            Invoke((MethodInvoker)delegate 
            { 
                this.timer.Start(); 
            });

        /// <inheritdoc/>
        void IPresenter.Start(bool flag)
        {
            if (flag)
            {
                Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStart.Enabled = false;
                    this.tssbVideoStart.Enabled = false;
                });
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStart.Visible = false;
                    this.tsmiVideoStop.Visible = true;
                    this.tsmiVideoStop.Enabled = true;
                    this.tssbVideoStart.Text = "Stop video";
                    this.tssbVideoStart.Enabled = true;
                });
            }
        }

        /// <inheritdoc/>
        void IPresenter.Stop() =>
            Invoke((MethodInvoker)delegate 
            { 
                this.timer.Stop(); 
            });

        /// <inheritdoc/>
        void IPresenter.Stop(bool flag)
        {
            if (flag)
            {
                Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStop.Enabled = false;
                    this.tssbVideoStart.Enabled = false;
                });
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    this.tsmiVideoStop.Visible = false;
                    this.tsmiVideoStart.Visible = true;
                    this.tsmiVideoStart.Enabled = true;
                    this.tssbVideoStart.Text = "Start video";
                    this.tssbVideoStart.Enabled = true;
                });
            }
        }

        #endregion

        #region Support routines

        private void Initialise()
        {
            this.videoManager.Initialise();

            this.videoManager.Images
                .ForEach(n => n.DoubleClick += Picture_DoubleClick);
            this.videoManager.MenuItems
                .ForEach(n => n.Click += TsmiVideoDevice_Click);

            this.flowLayoutPanel.Controls.AddRange(this.videoManager.Images.ToArray());

            this.toolStripStatusLabel1.Text = $"Cameras detected: {this.videoManager.Count}";
            this.tssbVideoStart.Enabled =
            this.tsmiVideoStart.Enabled =
            this.tsmiVideoStop.Enabled =
                this.videoManager.Count > 0;

            this.tsmiVideoDevices.DropDownItems.Clear();
            this.tsmiVideoDevices.DropDownItems.AddRange(this.videoManager.MenuItems.ToArray());
        }

        private async Task StartAsync() => await this.videoManager.StartAsync();

        private void Stop() => this.videoManager.Stop();

        private void ShutDown() => this.videoManager?.Dispose();

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
            ToggleDevice(this.videoManager[index.Value]);
        }

        private static void ToggleDevice(ImageDisplay display) => display.Enabled = !display.Enabled;

        #endregion

        #region Control event handler routines

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) => ShutDown();

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => Stop();

        private void MainForm_Load(object sender, EventArgs e) => Initialise();

        private void Picture_DoubleClick(object? sender, EventArgs e) => ToggleDevice(sender as PictureBox);

        #endregion

        #region Menu event handler routines

        private void TsmiFileExit_Click(object sender, EventArgs e) => Close();

        private async void TsmiVideoStart_Click(object sender, EventArgs e) => await StartAsync();

        private void TsmiVideoStop_Click(object sender, EventArgs e) => Stop();

        private void TsmiVideoDevice_Click(object? sender, EventArgs e) => ToggleDevice(sender as ToolStripMenuItem);

        #endregion

        #region Status bar event handler routines

        private async void TssbVideoStart_ButtonClick(object sender, EventArgs e)
        {
            if (!this.videoManager.IsRunning.HasValue)
                return;
            else if (this.videoManager.IsRunning.Value)
                Stop();
            else
                await StartAsync();
        }

        #endregion

        #region Timer event handler routines

        private void Timer_Tick(object sender, EventArgs e) => this.videoManager.CaptureVideo();

        #endregion
    }
}