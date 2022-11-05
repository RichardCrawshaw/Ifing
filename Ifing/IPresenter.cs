namespace Ifing
{
    internal interface IPresenter
    {
        /// <summary>
        /// Check or uncheck the specified <paramref name="menuItem"/> according to the specified
        /// <paramref name="isChecked"/> flag.
        /// </summary>
        /// <param name="menuItem">A <see cref="ToolStripMenuItem"/> object.</param>
        /// <param name="isChecked">A <see cref="bool"/>.</param>
        void CheckMenuItem(ToolStripMenuItem menuItem, bool isChecked);

        /// <summary>
        /// Resize the specified <paramref name="pictureBox"/> to the specified <paramref name="width"/>
        /// and <paramref name="height"/>.
        /// </summary>
        /// <param name="pictureBox">A <see cref="PictureBox"/> control.</param>
        /// <param name="width">An <see cref="int"/> containing the new width.</param>
        /// <param name="height">An <see cref="int"/> containing the new height.</param>
        void ResizePictureBox(PictureBox pictureBox, int width, int height);

        /// <summary>
        /// Start the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Update the current <see cref="IPresenter"/> for pre- or post start, according to the 
        /// specified <paramref name="flag"/>.
        /// </summary>
        /// <param name="flag">A <see cref="bool"/> that indicates whether the pre- (true) or post- (false) update is to be done.</param>
        void Start(bool flag);

        /// <summary>
        /// Stop the timer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Update the current <see cref="IPresenter"/> for pre- or post stop, according to the
        /// specified <paramref name="flag"/>.
        /// </summary>
        /// <param name="flag">A <see cref="bool"/> that indicates whether the pre- (true) or post- (false) update is to be done.</param>
        void Stop(bool flag);
    }
}
