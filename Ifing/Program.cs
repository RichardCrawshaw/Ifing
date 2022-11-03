namespace Ifing
{
    internal static class Program
    {
        // In Norse mythology, Ifing (Old Norse, Ífingr) is a river that separates Asgard,
        // the realm of the gods, from Jotunheim, the land of giants, according to stanza 16
        // of the poem Vafthrudnismal from the Poetic Edda:
        //      "Ifing the river is called, which divides the earth
        //      between the sons of giants and the gods;
        //              freely it will flow through all time,
        //      ice never forms on the river."
        // https://en.wikipedia.org/wiki/%C3%8Dfingr

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}