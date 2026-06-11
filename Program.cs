using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabbo.GEarth;

namespace G_Formatter
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            var syncContext = SynchronizationContext.Current;

            var ext = new GEarthExtension(new GEarthOptions
            {
                Name = "G-Formatter",
                Description = "Text styling in Wired made simple.",
                Author = "BigBenitocamelo",
                Version = "1.1.0"
            });

            var mainLogic = new ExtensionMain();

            ext.Connected += (e) =>
            {
                syncContext?.Post(_ => mainLogic.Start(), null);
            };

            ext.Disconnected += () =>
            {

                syncContext?.Post(_ =>
                {
                    mainLogic.Stop();
                    Application.Exit();
                }, null);
            };

            Task.Run(() => ext.Run());
            Application.Run(new ApplicationContext());
        }
    }
}