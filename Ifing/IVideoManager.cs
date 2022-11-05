namespace Ifing
{
    internal interface IVideoManager :
        IDisposable
    {
        int Count { get; }
        List<PictureBox> Images { get; }
        bool IsDisposed { get; }
        bool? IsRunning { get; }
        List<ToolStripMenuItem> MenuItems { get; }

        ImageDisplay this[int index] { get; }

        void CaptureVideo();
        void Dispose();
        void Initialise();
        Task StartAsync();
        void Stop();
    }
}