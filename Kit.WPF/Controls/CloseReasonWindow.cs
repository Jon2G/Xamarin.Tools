using System;
using System.Windows;
using System.Windows.Interop;

namespace Kit.WPF.Controls
{
    public class CloseReasonWindow : ObservableWindow
    {
        public enum ECloseReason
        {
            /// <summary>
            /// La ventana no se ha cerrado
            /// </summary>
            NotClosed = -1,
            /// <summary>
            /// El programa actual cerro la ventana desde Close()
            /// </summary>
            UserClosed,
            /// <summary>
            /// Se cerro debido a que windows se apagará
            /// </summary>
            Shutdown,
            /// <summary>
            /// El usuario la cerro desde la X del menú natural de Windows
            /// </summary>
            SystemMenuClosedByUser
        }
        public ECloseReason CloseReason { get; private set; }
        public CloseReasonWindow() : base()
        {
            this.CloseReason = ECloseReason.NotClosed;
            Loaded += delegate
            {
                HwndSource source = (HwndSource)PresentationSource.FromDependencyObject(this);
                source.AddHook(WindowProc);
            };
        }
        #region WinProc
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x10 || msg == 0x11 || msg == 0x16 || msg == 0x112)
            {
                CloseReasonWindow window = (CloseReasonWindow)HwndSource.FromHwnd(hwnd).RootVisual;
                switch (msg)
                {
                    case 0x10:
                        if (window.CloseReason != ECloseReason.SystemMenuClosedByUser)
                        {
                            window.CloseReason = ECloseReason.UserClosed;
                        }
                        //Close
                        break;
                    case 0x11:
                    case 0x16:
                        window.CloseReason = ECloseReason.Shutdown;
                        //Console.WriteLine("Close reason: WindowsShutDown");
                        break;

                    case 0x112:
                        if (((ushort)wParam & 0xfff0) == 0xf060)
                            window.CloseReason = ECloseReason.SystemMenuClosedByUser;
                        //Console.WriteLine("Close reason: User closing from menu");
                        //Console.WriteLine("Close reason: Clicking X");
                        break;
                }
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}
