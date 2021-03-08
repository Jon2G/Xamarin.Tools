using System.Diagnostics;
using Kit;
using Kit.Enums;
using Kit.Forms.Controls.CrossImage;
using Kit.Forms.Services;
using Kit.Services.Interfaces;
using Tools.UWP.Services;
using CustomMessageBoxService = Tools.UWP.Services.CustomMessageBoxService;

namespace Tools.UWP
{
    public class ToolsImplementation : AbstractTools
    {
        public override void Init()
        {
            base.Init(new DeviceInfo(), new CustomMessageBoxService(), new SynchronizeInvoke(),
                new ScreenManagerService(), new ImageExtensions(), null);
         
        }

        public override RuntimePlatform RuntimePlatform { get=>RuntimePlatform.UWP; }

        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get => Kit.Tools.Instance as ToolsImplementation;
        }
        private bool? _IsInDesingMode;
        public new bool IsInDesingMode
        {
            get
            {
                if (_IsInDesingMode is null)
                {
                    _IsInDesingMode = Designing();
                }
                return (bool)_IsInDesingMode;
            }
        }
        private bool Designing()
        {
            string name = Process.GetCurrentProcess().ProcessName;
            name = name?.Trim()?.ToUpper();
            if (name == "XDESPROC" || name == "DEVENV")
            {
                return true;
            }
            // MessageBox.Show(name);
            return false;
        }
        #endregion
    }
}
