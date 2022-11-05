namespace Ifing
{
    internal interface IVideoManager :
        IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the count of available video devices.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="PictureBox"/> controls, one for each video
        /// device.
        /// </summary>
        List<PictureBox> Images { get; }

        /// <summary>
        /// Gets whether the current <see cref="IVideoManager"/> has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets whether the current <see cref="IVideoManager"/> is running or not.
        /// </summary>
        bool? IsRunning { get; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="ToolStripMenuItem"/> objects, one for each
        /// video device.
        /// </summary>
        List<ToolStripMenuItem> MenuItems { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the <see cref="ImageDisplay"/> object that corresponds to the specified 
        /// <paramref name="index"/>.
        /// </summary>
        /// <param name="index">An <see cref="int"/> that contains the index of the required element.</param>
        /// <returns>An <see cref="ImageDisplay"/> object.</returns>
        ImageDisplay this[int index] { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Captures video from all the video devices.
        /// </summary>
        void CaptureVideo();

        /// <summary>
        /// Initialises the video devices and their corresponding <see cref="Control"/> objects.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Asynchronously starts the current <see cref="IVideoManager"/> instance.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task StartAsync();

        /// <summary>
        /// Stops the current <see cref="IVideoManager"/> instance.
        /// </summary>
        void Stop();

        #endregion
    }
}