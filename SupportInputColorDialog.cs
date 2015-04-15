using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeX2img {
    class SupportInputColorDialog : ColorDialog {
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {

            return base.HookProc(hWnd, msg, wparam, lparam);
        }


    }

}
