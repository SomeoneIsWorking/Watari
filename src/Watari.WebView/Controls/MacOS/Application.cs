using System.Reflection.Metadata;
using Watari.WebView.Bridge.MacOS;

namespace Watari.WebView.Controls.MacOS
{
    internal class Application
    {
        public IntPtr Handle { get; }
        public Application()
        {
            Handle = AppliationBridge.Init();
        }

        public void RunLoop() => AppliationBridge.RunLoop(Handle);
        public void StopLoop() => AppliationBridge.StopLoop(Handle);
    }
}
