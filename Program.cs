using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace TeX2img {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>

        [STAThread]
        static void Main() {
            // Azuki.dllの存在チェック
            string[] chkfiles = new string[] { "Azuki.dll" };
            foreach(var f in chkfiles) {
                if(!System.IO.File.Exists(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), f))) {
                    if(Converter.which(f) == "") {
                        MessageBox.Show(f + " が見つからないため，起動することができませんでした．", "TeX2img");
                        Environment.Exit(-1);
                    }
                }
            }
            // アップデートしていたら前バージョンの設定を読み込む
            if(!Properties.Settings.Default.IsUpgraded) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }
            // 設定読みだし
            SettingForm.Settings SettingData = new SettingForm.Settings();
            SettingData.LoadSetting();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // コマンドライン解析
            bool exit = false;
            bool? nosavesetting = null;
            bool nogui = false;
            var opt = new NDesk.Options.OptionSet(){
                {"platex=", (val) => {SettingData.PlatexPath=val;}},
                {"dvipdfmx=",(val) =>{SettingData.DvipdfmxPath=val;}},
                {"gs=",(val) => {SettingData.GsPath = val;}},
                {"exit", (val) => {exit = true;}},
                {"nosavesettings",(val) => {nosavesetting = true;}},
                {"nogui",(val) => {nogui = true;}},
                {"savesettings",(val) => {nosavesetting = false;}}
            };

            List<string> files = opt.Parse(Environment.GetCommandLineArgs());

            // すぐに終了
            if(exit) {
                if(nosavesetting != false) SettingData.SaveSettings();
                return;
            }

            if(nogui) {
                CUIExec(SettingData, files);
                if(nosavesetting == true) SettingData.SaveSettings();
            } else {
                Application.Run(new MainForm(SettingData, files));
                if(nosavesetting != false) SettingData.SaveSettings();
            }
        }

        // CUIモード
        static void CUIExec(SettingForm.Settings SettingData, List<string> files) {
            bool consoleattached = false;
            // 親プロセスのコンソールにアタッチする
            // どうもうまく行かないなぁ．
            if(Win32.AttachConsole(System.UInt32.MaxValue)) {
                System.IO.StreamWriter stdout = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
                stdout.AutoFlush = true;
                System.Console.SetOut(stdout);
                Console.WriteLine();
                // これがないと化けるのだけど，これがあると挙動がおかしい……
                // 英語モードに勝手になって，表示できないからか落ちる．
                Console.OutputEncoding = new System.Text.UTF8Encoding();
                consoleattached = true;
            }
                       
            Converter conv = new Converter(SettingData, new CUIOutput());
            bool err = false;
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                if(!File.Exists(files[2 * i + 1])) {
                    Console.WriteLine(files[2 * i + 1] + " は見つかりませんでした．");
                    err = true;
                    continue;
                }
                if(!conv.CheckFormat(files[2 * i + 2])) {
                    err = true;
                }
            }
            if(err) return;
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                string file = files[2 * i + 1];
                // 一時フォルダにコピー
                string tmpFilePath = Path.GetTempFileName();
                string tmpTeXFileName = Path.Combine(Path.GetDirectoryName(tmpFilePath),Path.GetFileNameWithoutExtension(tmpFilePath) + ".tex");
                File.Delete(tmpTeXFileName);
                File.Copy(file, tmpTeXFileName, true);

                // 変換！
                conv.Convert(tmpTeXFileName, files[2 * i + 2]);
            }
            if(consoleattached) Win32.FreeConsole();
            Environment.Exit(-1);
        }
    }
    public class Win32 {
        [DllImport("kernel32.dll")]
        public static extern Boolean AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
    }
}

