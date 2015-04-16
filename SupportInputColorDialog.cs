using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TeX2img {
    // ColorDialogを拡張する．
    class SupportInputColorDialog : ColorDialog {
        public enum ControlSequence { color, textcolor, colorbox, definecolor };
        public ControlSequence CheckedControlSequence;
        const int ID_SAMPLETEXTBOX = 1000;
        const int ID_CURRENTTEXTCOLOR = 1001;
        const int ID_CHECKBOX_COLOR = 1002;
        const int ID_CHECKBOX_TEXTCOLOR = 1003;
        const int ID_CHECKBOX_COLORBOX = 1004;
        const int ID_CHECKBOX_DEFINECOLOR = 1005;
        System.Drawing.Size OriginalSize;
        System.Drawing.Size OriginalClientSize;
        System.Drawing.Size Size {
            get { return new System.Drawing.Size(OriginalSize.Width, OriginalSize.Height + 50); }
        }
        System.Drawing.Size ClientSize {
            get { return new System.Drawing.Size(OriginalClientSize.Width, OriginalClientSize.Height + 50); }
        }
        IntPtr brush = PInvoke.CreateSolidBrush(ToRGB(Properties.Settings.Default.editorNormalColorBack));
        protected override void Dispose(bool disposing) {
            PInvoke.DeleteObject(brush);
            base.Dispose(disposing);
        }
        public SupportInputColorDialog() : base(){
            AllowFullOpen = true;
            FullOpen = true;
        }
        public new bool FullOpen {
            get { return base.FullOpen; }
            private set { base.FullOpen = value; }
        }
        public new bool AllowFullOpen {
            get { return base.AllowFullOpen; }
            private set { base.AllowFullOpen = value; }
        }
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {
            PInvoke.RECT rect;
            IntPtr dlg;
            switch(msg) {
            case PInvoke.WM_INITDIALOG:
                var dialogFont = PInvoke.SendMessage(hWnd, PInvoke.WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
                // 全体のサイズを広げる
                PInvoke.GetClientRect(hWnd, out rect);
                OriginalClientSize.Width = rect.Right - rect.Left;
                OriginalClientSize.Height = rect.Bottom - rect.Top;
                PInvoke.GetWindowRect(hWnd, out rect);
                OriginalSize.Width = rect.Right - rect.Left;
                OriginalSize.Height = rect.Bottom - rect.Top;
                PInvoke.MoveWindow(hWnd, rect.Left, rect.Top, Size.Width, Size.Height, true);
                // 選択されている色の表示エリアの高さを半分に
                dlg = PInvoke.GetDlgItem(hWnd, PInvoke.COLOR_CURRENT);
                GetControlRect(hWnd, dlg, out rect);
                PInvoke.MoveWindow(dlg, rect.Left, rect.Top, rect.Right - rect.Left, (rect.Bottom - rect.Top) / 2-2, true);
                // その下にテキストのサンプル
                dlg = PInvoke.CreateWindowEx(0,"EDIT","サンプル",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_BORDER | PInvoke.WindowStyles.ES_CENTER,
                    rect.Left,(rect.Top + rect.Bottom)/2+4,rect.Right - rect.Left,(rect.Bottom - rect.Top)/2-2,
                    hWnd,new IntPtr(ID_CURRENTTEXTCOLOR),Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule), IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.EM_SETREADONLY, new IntPtr(1), IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                // OK,キャンセル,色の追加を下に移動．
                dlg = PInvoke.GetDlgItem(hWnd,PInvoke.IDOK);
                var okButton = dlg;
                GetControlRect(hWnd, dlg, out rect);
                var RadioBtnPos = new System.Drawing.Point(rect.Left, rect.Top + 5);
                PInvoke.MoveWindow(dlg, rect.Left, rect.Top + 50, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
                dlg = PInvoke.GetDlgItem(hWnd,PInvoke.IDCANCEL);
                GetControlRect(hWnd, dlg, out rect);
                PInvoke.MoveWindow(dlg, rect.Left, rect.Top + 50, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
                dlg = PInvoke.GetDlgItem(hWnd, PInvoke.COLOR_ADD);
                GetControlRect(hWnd,dlg, out rect);
                PInvoke.MoveWindow(dlg, rect.Left, rect.Top + 50, OriginalClientSize.Width - 10 - rect.Left, rect.Bottom - rect.Top, true);
                // ラジオボタン四つ
                var instance = Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule);
                dlg = PInvoke.CreateWindowEx(0, "BUTTON", "\\color",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_TABSTOP | PInvoke.WindowStyles.BS_AUTORADIOBUTTON | PInvoke.WindowStyles.WS_GROUP,
                    RadioBtnPos.X, RadioBtnPos.Y, ClientSize.Width / 4, 15,
                    hWnd, new IntPtr(ID_CHECKBOX_COLOR), instance, IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                PInvoke.SendMessage(dlg, PInvoke.BM_SETCHECK, new IntPtr(1), IntPtr.Zero);
                // タブオーダーをCOLOR_CUSTOM1の後に持ってくる．
                // http://stackoverflow.com/questions/50236/how-do-you-programmatically-change-the-tab-order-in-a-win32-dialog
                PInvoke.SetWindowPos(dlg, PInvoke.GetDlgItem(hWnd,PInvoke.COLOR_CUSTOM1), 0, 0, 0, 0, PInvoke.SetWindowPosFlags.SWP_NOMOVE | PInvoke.SetWindowPosFlags.SWP_NOSIZE);
                var prevdlg = dlg;
                dlg = PInvoke.CreateWindowEx(0, "BUTTON", "\\textcolor",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_TABSTOP | PInvoke.WindowStyles.BS_AUTORADIOBUTTON,
                    RadioBtnPos.X + ClientSize.Width / 4, RadioBtnPos.Y, ClientSize.Width / 4, 15,
                    hWnd, new IntPtr(ID_CHECKBOX_TEXTCOLOR), instance, IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                PInvoke.SetWindowPos(dlg, prevdlg, 0, 0, 0, 0, PInvoke.SetWindowPosFlags.SWP_NOMOVE | PInvoke.SetWindowPosFlags.SWP_NOSIZE);
                prevdlg = dlg;
                dlg = PInvoke.CreateWindowEx(0, "BUTTON", "\\colorbox",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_TABSTOP | PInvoke.WindowStyles.BS_AUTORADIOBUTTON,
                    RadioBtnPos.X + ClientSize.Width / 2, RadioBtnPos.Y, ClientSize.Width / 4, 15,
                    hWnd, new IntPtr(ID_CHECKBOX_COLORBOX), instance, IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                PInvoke.SetWindowPos(dlg, prevdlg, 0, 0, 0, 0, PInvoke.SetWindowPosFlags.SWP_NOMOVE | PInvoke.SetWindowPosFlags.SWP_NOSIZE);
                prevdlg = dlg;
                dlg = PInvoke.CreateWindowEx(0, "BUTTON", "\\definecolor",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_TABSTOP | PInvoke.WindowStyles.BS_AUTORADIOBUTTON,
                    RadioBtnPos.X + ClientSize.Width * 3 / 4, RadioBtnPos.Y, ClientSize.Width / 4, 15,
                    hWnd, new IntPtr(ID_CHECKBOX_DEFINECOLOR), instance, IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                PInvoke.SetWindowPos(dlg, prevdlg, 0, 0, 0, 0, PInvoke.SetWindowPosFlags.SWP_NOMOVE | PInvoke.SetWindowPosFlags.SWP_NOSIZE);
                // 入力される命令を出すエディトボックス
                dlg = PInvoke.CreateWindowEx(0, "EDIT", "",
                    PInvoke.WindowStyles.WS_CHILD | PInvoke.WindowStyles.WS_VISIBLE | PInvoke.WindowStyles.WS_BORDER,
                    RadioBtnPos.X, RadioBtnPos.Y + 20, ClientSize.Width - 10 - RadioBtnPos.X, 20,
                    hWnd, new IntPtr(ID_SAMPLETEXTBOX), instance, IntPtr.Zero);
                PInvoke.SendMessage(dlg, PInvoke.WM_SETFONT, dialogFont, new IntPtr(1));
                PInvoke.SendMessage(dlg, PInvoke.EM_SETREADONLY, new IntPtr(1), IntPtr.Zero);
                PInvoke.SetWindowText(dlg, GetControlSequenceString(hWnd));
                // ウィンドウ広げたら出てきた．消しておく．
                PInvoke.ShowWindow(PInvoke.GetDlgItem(hWnd, PInvoke.COLOR_SOLID), PInvoke.ShowWindowCommands.Hide);
                // 常にFullOpen = trueなので不要だから消しておく．
                PInvoke.ShowWindow(PInvoke.GetDlgItem(hWnd, PInvoke.COLOR_MIX), PInvoke.ShowWindowCommands.Hide);
                break;
            case PInvoke.WM_COMMAND:
                //System.Diagnostics.Debug.WriteLine("WM_COMMAND: id = " + (wparam.ToInt32() & 0xFFFF).ToString());
                switch(wparam.ToInt32() & 0xFFFF) {
                case PInvoke.COLOR_RED:
                case PInvoke.COLOR_BLUE:
                case PInvoke.COLOR_GREEN:
                    OnSelectedColorChanged(GetCurrentColor(hWnd),GetControlSequence(hWnd));
                    dlg = PInvoke.GetDlgItem(hWnd, ID_CURRENTTEXTCOLOR);
                    PInvoke.InvalidateRect(dlg, IntPtr.Zero, true);
                    PInvoke.SetWindowText(PInvoke.GetDlgItem(hWnd, ID_SAMPLETEXTBOX), GetControlSequenceString(hWnd));
                    break;
                case ID_CHECKBOX_COLOR:
                case ID_CHECKBOX_COLORBOX:
                case ID_CHECKBOX_DEFINECOLOR:
                case ID_CHECKBOX_TEXTCOLOR:
                    PInvoke.SetWindowText(PInvoke.GetDlgItem(hWnd, ID_SAMPLETEXTBOX), GetControlSequenceString(hWnd));
                    break;
                case PInvoke.IDOK:
                    CheckedControlSequence = GetControlSequence(hWnd);
                    break;
                default:
                    break;
                }
                break;
            case PInvoke.WM_CTLCOLORSTATIC:
                if(lparam == PInvoke.GetDlgItem(hWnd, ID_CURRENTTEXTCOLOR)) {
                    PInvoke.SetTextColor(wparam, ToRGB(GetCurrentColor(hWnd)));
                    return PInvoke.GetStockObject(PInvoke.StockObjects.WHITE_BRUSH);
                }
                if(lparam == PInvoke.GetDlgItem(hWnd, ID_SAMPLETEXTBOX)) {
                    PInvoke.SetTextColor(wparam, ToRGB(Properties.Settings.Default.editorNormalColorFont));
                    return brush;
                }
                break;
            default:
                break;
            }
            return base.HookProc(hWnd, msg, wparam, lparam);
        }
        System.Drawing.Color GetCurrentColor(IntPtr hWnd) {
            /*
            System.Diagnostics.Debug.WriteLine(
                    "Red = " + GetWindowText(hWnd, PInvoke.COLOR_RED) +
                    ", Green = " + GetWindowText(hWnd, PInvoke.COLOR_GREEN) +
                    ", Blue = " + GetWindowText(hWnd, PInvoke.COLOR_BLUE)
                    );
             */ 
            int red, blue, green;
            if(!Int32.TryParse(GetWindowText(hWnd, PInvoke.COLOR_RED),out red))red = 0;
            if(!Int32.TryParse(GetWindowText(hWnd, PInvoke.COLOR_GREEN),out green))green = 0;
            if(!Int32.TryParse(GetWindowText(hWnd, PInvoke.COLOR_BLUE),out blue))blue = 0;
            return System.Drawing.Color.FromArgb(red, green, blue);
        }
        ControlSequence GetControlSequence(IntPtr hWnd) {
            var chklist = new Dictionary<int,ControlSequence>();
            chklist[ID_CHECKBOX_COLOR] = ControlSequence.color;
            chklist[ID_CHECKBOX_COLORBOX] = ControlSequence.colorbox;
            chklist[ID_CHECKBOX_DEFINECOLOR] = ControlSequence.definecolor;
            chklist[ID_CHECKBOX_TEXTCOLOR] = ControlSequence.textcolor;
            foreach(var chk in chklist){
                if(PInvoke.SendMessage(PInvoke.GetDlgItem(hWnd, chk.Key), PInvoke.BM_GETCHECK, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero) {
                    return chk.Value;
                }
            }
            return ControlSequence.color;
        }
        string GetControlSequenceString(IntPtr hWnd) {
            var color = GetCurrentColor(hWnd);
            var colstring = "{" + ((double) color.R / (double) 255).ToString() + "," + ((double) color.G / (double) 255).ToString() + "," + ((double) color.B / (double) 255).ToString() + "}";
            var cs = GetControlSequence(hWnd);
            switch(cs) {
            case ControlSequence.colorbox:
                return "\\colorbox[rgb]" + colstring + "{}";
            case ControlSequence.textcolor:
                return "\\textcolor[rgb]" + colstring + "{}";
            case ControlSequence.definecolor:
                return "\\definecolor{}{rgb}" + colstring;
            case ControlSequence.color:
            default:
                return "\\color[rgb]" + colstring;
            }
        }
        static int ToRGB(System.Drawing.Color color) {
            return color.B << 16 | color.G << 8 | color.R;
        }
        public class SelectedColorChangedEventArgs : EventArgs {
            public SelectedColorChangedEventArgs(System.Drawing.Color c, ControlSequence cs) { Color = c; CS = cs; }
            public System.Drawing.Color Color { get; private set;}
            public ControlSequence CS { get; private set; }
        }
        public delegate void SelectedColorChangedEventHandler(object sender, SelectedColorChangedEventArgs e);
        public event SelectedColorChangedEventHandler SelectedColorChanged = (s, e) => { };
        protected void OnSelectedColorChanged(System.Drawing.Color c,ControlSequence cs) {
            SelectedColorChanged(this, new SelectedColorChangedEventArgs(c, cs));
        }
        static void GetControlRect(IntPtr hWnd, IntPtr controlHWND, out PInvoke.RECT rect) {
            PInvoke.GetWindowRect(controlHWND, out rect);
            var pt = new PInvoke.POINT(rect.Left, rect.Top);
            PInvoke.ScreenToClient(hWnd, ref pt);
            rect.Right = rect.Right - rect.Left + pt.X;
            rect.Bottom = rect.Bottom - rect.Top + pt.Y;
            rect.Left = pt.X;
            rect.Top = pt.Y;
        }
        static string GetWindowText(IntPtr hWnd, int id) {
            return GetWindowText(PInvoke.GetDlgItem(hWnd, id));
        }
        static string GetWindowText(IntPtr hWnd) {
            var rv = new StringBuilder(4096);
            PInvoke.GetWindowText(hWnd, rv, 4095);
            return rv.ToString();
        }

        class PInvoke {
            public const int WM_INITDIALOG = 0x0110;
            public const int WM_COMMAND = 0x0111;
            public const int WM_SIZE = 0x0005;
            public const int WM_CTLCOLOREDIT = 0x0133;
            public const int WM_CTLCOLORSTATIC = 0x0138;
            public const int WM_SETFONT = 0x0030;
            public const int WM_GETFONT = 0x0031;

            public const int BM_GETCHECK = 0x00F0;
            public const int BM_SETCHECK = 0x00F1;
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
            public static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
            [Flags()]
            public enum SetWindowPosFlags : uint {
                SWP_ASYNCWINDOWPOS = 0x4000,
                SWP_DEFERERASE = 0x2000,
                SWP_DRAWFRAME = 0x0020,
                SWP_FRAMECHANGED = 0x0020,
                SWP_HIDEWINDOW = 0x0080,
                SWP_NOACTIVATE = 0x0010,
                SWP_NOCOPYBITS = 0x0100,
                SWP_NOMOVE = 0x0002,
                SWP_NOOWNERZORDER = 0x0200,
                SWP_NOREDRAW = 0x0008,
                SWP_NOREPOSITION = 0x0200,
                SWP_NOSENDCHANGING = 0x0400,
                SWP_NOSIZE = 0x0001,
                SWP_NOZORDER = 0x0004,
                SWP_SHOWWINDOW = 0x0040,
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
            [DllImport("user32.dll")]
            public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
            [DllImport("user32.dll")]
            public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowEx(WindowStylesEx dwExStyle, string lpClassName, string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool SetWindowText(IntPtr hwnd, String lpString);
            [DllImport("user32.dll")]
            public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateSolidBrush(int crColor);
            [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject([In] IntPtr hObject);
            [DllImport("user32.dll")]
            public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);
            public enum ShowWindowCommands {
                Hide = 0,
                Normal = 1,
                ShowMinimized = 2,
                Maximize = 3, // is this the right value?
                ShowMaximized = 3,
                ShowNoActivate = 4,
                Show = 5,
                Minimize = 6,
                ShowMinNoActive = 7,
                ShowNA = 8,
                Restore = 9,
                ShowDefault = 10,
                ForceMinimize = 11
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT {
                public int X;
                public int Y;
                public POINT(int x, int y) {
                    this.X = x;
                    this.Y = y;
                }
                public POINT(System.Drawing.Point pt) : this(pt.X, pt.Y) { }
                public static implicit operator System.Drawing.Point(POINT p) {
                    return new System.Drawing.Point(p.X, p.Y);
                }
                public static implicit operator POINT(System.Drawing.Point p) {
                    return new POINT(p.X, p.Y);
                }
            }
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
            public const int EM_SETREADONLY = 0x00CF;
            [DllImport("gdi32.dll")]
            public static extern uint SetTextColor(IntPtr hdc, int crColor);
            [Flags]
            public enum WindowStylesEx : uint {
                WS_EX_ACCEPTFILES = 0x00000010,
                WS_EX_APPWINDOW = 0x00040000,
                WS_EX_CLIENTEDGE = 0x00000200,
                WS_EX_COMPOSITED = 0x02000000,
                WS_EX_CONTEXTHELP = 0x00000400,
                WS_EX_CONTROLPARENT = 0x00010000,
                WS_EX_DLGMODALFRAME = 0x00000001,
                WS_EX_LAYERED = 0x00080000,
                WS_EX_LAYOUTRTL = 0x00400000,
                WS_EX_LEFT = 0x00000000,
                WS_EX_LEFTSCROLLBAR = 0x00004000,
                WS_EX_LTRREADING = 0x00000000,
                WS_EX_MDICHILD = 0x00000040,
                WS_EX_NOACTIVATE = 0x08000000,
                WS_EX_NOINHERITLAYOUT = 0x00100000,
                WS_EX_NOPARENTNOTIFY = 0x00000004,
                WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
                WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
                WS_EX_RIGHT = 0x00001000,
                WS_EX_RIGHTSCROLLBAR = 0x00000000,
                WS_EX_RTLREADING = 0x00002000,
                WS_EX_STATICEDGE = 0x00020000,
                WS_EX_TOOLWINDOW = 0x00000080,
                WS_EX_TOPMOST = 0x00000008,
                WS_EX_TRANSPARENT = 0x00000020,
                WS_EX_WINDOWEDGE = 0x00000100
            }
            [Flags()]
            public enum WindowStyles : uint {
                WS_BORDER = 0x800000,
                WS_CAPTION = 0xc00000,
                WS_CHILD = 0x40000000,
                WS_CLIPCHILDREN = 0x2000000,
                WS_CLIPSIBLINGS = 0x4000000,
                WS_DISABLED = 0x8000000,
                WS_DLGFRAME = 0x400000,
                WS_GROUP = 0x20000,
                WS_HSCROLL = 0x100000,
                WS_MAXIMIZE = 0x1000000,
                WS_MAXIMIZEBOX = 0x10000,
                WS_MINIMIZE = 0x20000000,
                WS_MINIMIZEBOX = 0x20000,
                WS_OVERLAPPED = 0x0,
                WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
                WS_POPUP = 0x80000000u,
                WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
                WS_SIZEFRAME = 0x40000,
                WS_SYSMENU = 0x80000,
                WS_TABSTOP = 0x10000,
                WS_VISIBLE = 0x10000000,
                WS_VSCROLL = 0x200000,
                BS_RADIOBUTTON = 0x0004,
                BS_AUTORADIOBUTTON = 0x0009,
                ES_CENTER = 0x0001,
            }
            [DllImport("gdi32.dll")]
            public static extern IntPtr GetStockObject(StockObjects fnObject);
            public enum StockObjects {
                WHITE_BRUSH = 0,
                LTGRAY_BRUSH = 1,
                GRAY_BRUSH = 2,
                DKGRAY_BRUSH = 3,
                BLACK_BRUSH = 4,
                NULL_BRUSH = 5,
                HOLLOW_BRUSH = NULL_BRUSH,
                WHITE_PEN = 6,
                BLACK_PEN = 7,
                NULL_PEN = 8,
                OEM_FIXED_FONT = 10,
                ANSI_FIXED_FONT = 11,
                ANSI_VAR_FONT = 12,
                SYSTEM_FONT = 13,
                DEVICE_DEFAULT_FONT = 14,
                DEFAULT_PALETTE = 15,
                SYSTEM_FIXED_FONT = 16,
                DEFAULT_GUI_FONT = 17,
                DC_BRUSH = 18,
                DC_PEN = 19,
            }

            public const int COLOR_MIX = 719;
            public const int COLOR_RED = 706;
            public const int COLOR_GREEN = 707;
            public const int COLOR_BLUE = 708;
            public const int COLOR_ADD = 712;
            public const int COLOR_CURRENT = 709;
            public const int COLOR_SOLID = 713;
            public const int COLOR_CUSTOM1 = 721;

            public const int IDOK = 1;
            public const int IDCANCEL = 2;
        }
    }
}
