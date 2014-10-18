using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

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
                {"platex=", val => {SettingData.PlatexPath=val;}},
                {"dvipdfmx=",val =>{SettingData.DvipdfmxPath=val;}},
                {"gs=",val => {SettingData.GsPath = val;}},
                {"exit", val => {exit = true;}},
                {"nosavesettings",val => {nosavesetting = true;}},
                {"nogui",val => {nogui = true;}},
                {"savesettings",val => {nosavesetting = false;}}
            };

            List<string> files = opt.Parse(Environment.GetCommandLineArgs());
            
            // すぐに終了
            if(exit) {
                if(nosavesetting != false) SettingData.SaveSettings();
                return;
            }

            if(nogui) {
                // CUIモード
                for(int i = 0 ; i < files.Count / 2 ;++i){
                    string file = files[2 * i];
                    if(!File.Exists(file)) {
                        Console.WriteLine(file + " は見つかりませんでした．");
                        continue;
                    }
                    // 一時フォルダにコピー
                    string tmpFilePath = Path.GetTempFileName();
                    string tmpTeXFileName = Path.GetDirectoryName(tmpFilePath) + "\\" + Path.GetFileNameWithoutExtension(tmpFilePath) + ".tex";
                    File.Delete(tmpTeXFileName);
                    File.Copy(file, tmpTeXFileName,true);

                    // 変換！
                    Converter conv = new Converter(SettingData, new CUIOutput());
                    conv.Convert(tmpTeXFileName,files[2*i + 1]);
                    return;
                }
                if(nosavesetting == true) SettingData.SaveSettings();

            } else {
                Application.Run(new MainForm(SettingData,files));
                if(nosavesetting != false) SettingData.SaveSettings();
            }
        }
    }
}