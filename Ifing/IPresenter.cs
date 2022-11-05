namespace Ifing
{
    internal interface IPresenter
    {
        void CheckMenuItem(ToolStripMenuItem menuItem, bool isChecked);

        void ResizePictureBox(PictureBox pictureBox, int width, int height);

        void Start();

        void Start(bool flag);

        void Stop();

        void Stop(bool flag);
    }
}
