using System;
using System.Diagnostics;
using G_Formatter.UI;

namespace G_Formatter
{
    public class ExtensionMain
    {
        private string _name = "G-Formatter";
        private string _version = "1.1.0";
        private string _author = "BigBenitocamelo";
        private string _description = "Text styling in Wired made simple.";

        public string Name { get { return _name; } }
        public string Version { get { return _version; } }
        public string Author { get { return _author; } }
        public string Description { get { return _description; } }

        private FormatMenu _menu;

        public ExtensionMain() { }

        public void Start()
        {
            Log("G-Formatter initiated");
            Log("Showing toolbar");

            _menu = new FormatMenu();
            _menu.Show();
        }

        public void Stop()
        {
            Log("👋 G-Formatter detenido");
            if (_menu != null && !_menu.IsDisposed)
            {
                _menu.Close();
            }
        }

        private void Log(string message)
        {
            Debug.WriteLine("[G-Formatter] " + message);
        }
    }
}