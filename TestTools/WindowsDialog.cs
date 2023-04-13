using System;
using System.Runtime.InteropServices;

namespace TestTools
{
    public static class WindowsDialog
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIdDlgItem);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SETTEXT = 0x000c;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private static string _dialogTitle;

        private static IntPtr OpenDialogHandler => GetWindowHandleByClassName("#32770", _dialogTitle);

        public static IntPtr GetDialogHandle()
        {
            var hWnd = OpenDialogHandler;
            return hWnd;
        }

        public static bool IsOpenDialogActive(string title)
        {
            _dialogTitle = title;
            return OpenDialogHandler != IntPtr.Zero;
        }

        public static void SetTextToOpenDialog(string txt)
        {
            var btnHwnd = IntPtr.Zero;
            var dt1 = DateTime.Now;
            var dt2 = DateTime.Now;
            while (btnHwnd == IntPtr.Zero && (dt2 - dt1).TotalSeconds < ConfigSettingsReader.DefaultTimeOut())
            {
                btnHwnd = GetDialogHandle();
            }
            SetDialogText(btnHwnd, 1148, txt);
        }

        private static IntPtr GetWindowHandleByClassName(string className, string title)
        {
            return FindWindow(className, title);
        }

        private static void SetDialogText(IntPtr hWnd, int ctlId, string txt)
        {
            var boxHwnd = IntPtr.Zero;
            while (boxHwnd == IntPtr.Zero)
                boxHwnd = GetDlgItem(hWnd, ctlId);
            var btnHwnd = IntPtr.Zero;
            while (btnHwnd == IntPtr.Zero)
                btnHwnd = GetDlgItem(hWnd, 1);
            SendMessage(boxHwnd, WM_SETTEXT, IntPtr.Zero, txt);
            var length = 0;
            while (length != txt.Length)
            {
                boxHwnd = GetDlgItem(hWnd, ctlId);
                length = SendMessage(boxHwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            }
            SendMessage(btnHwnd, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(btnHwnd, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        public static void CancelButonClick()
        {
            if (string.IsNullOrEmpty(_dialogTitle)) return;
            var btnHwnd = IntPtr.Zero;
            var dt1 = DateTime.Now;
            var dt2 = DateTime.Now;
            while (btnHwnd == IntPtr.Zero && (dt2 - dt1).TotalSeconds < 5)
            {
                btnHwnd = GetDlgItem(GetDialogHandle(), 2);
                dt2 = DateTime.Now;
            }
            if (btnHwnd == IntPtr.Zero)
            {
                _dialogTitle = "";
                return;
            }
            SendMessage(btnHwnd, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(btnHwnd, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
            _dialogTitle = "";
        }
    }
}
