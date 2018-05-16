using System;
using System.Windows.Forms;
using SlimDX.Windows;

namespace ModelEx
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MainWindow form = new MainWindow();
            MessagePump.Run(form, () => { });
            return;
        }
    }
}