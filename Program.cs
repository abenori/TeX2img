using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TeX2img {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main() {
            // Azuki.dllの存在チェック
            if(!System.IO.File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Azuki.dll")) {
                if(Converter.which("Azuki.dll") == "") {
                    MessageBox.Show("Azuki.dll が見つからないため，起動することができませんでした．", "TeX2img");
                    Environment.Exit(-1);
                }
            }
            // アップデートしていたら前バージョンの設定を読み込む
            if(!Properties.Settings.Default.IsUpgraded) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}