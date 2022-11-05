namespace Ifing
{
    internal interface IPresenter
    {
        void CheckMenuItem(ToolStripMenuItem menuItem, bool isChecked);

        void Start();

        void Start(bool flag);

        void Stop();

        void Stop(bool flag);
    }
}
