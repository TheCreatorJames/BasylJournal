using System;
using System.Windows.Forms;

namespace BasylJournal
{
    /// <summary>
    /// See the BasylJournalForm for the description of this application.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BasylJournalForm());
        }
    }
}
