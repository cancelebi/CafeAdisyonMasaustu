using CafeAdisyonMasaustu.Views;
using System;
using System.Windows.Forms;

namespace CafeAdisyonMasaustu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GirisForm());
        }
    }
}