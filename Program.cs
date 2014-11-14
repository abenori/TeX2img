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

        struct OptionWithHelp {
            public string option, help;
            public Action<string> action;
            public bool withvalue;
            public OptionWithHelp(string o, Action<string> a, string h) {
                System.Diagnostics.Debug.Assert(o != "");
                option = o; action = a; help = h;
                withvalue = (option.Substring(option.Length - 1, 1) == "=");
            }
        }
        class OptionWithHelpCollection : List<OptionWithHelp>{
            public void Add(string o, Action<string> a, string h) {
                Add(new OptionWithHelp(o, a, h));
            }
            public NDesk.Options.OptionSet GenerateOption() {
                NDesk.Options.OptionSet opt = new NDesk.Options.OptionSet();
                foreach(var oh in this) opt.Add(oh.option, oh.action);
                return opt;
            }
            public string GenerateHelp() {
                string rv = "";
                int maxlength = 0;
                foreach(var oh in this)if(oh.help != null)maxlength = Math.Max(maxlength, oh.option.Length + (oh.withvalue ? 5 : 0));
                maxlength += 2;
                foreach(var oh in this) {
                    if(oh.help != null) {
                        string opstr = "/" + oh.option;
                        if(oh.withvalue) opstr = opstr.Remove(opstr.Length - 1) + " <VAL>";
                        rv += "  " + opstr;
                        rv += new string(' ', maxlength - opstr.Length);
                        rv += oh.help.Replace("\n", "\n" + new string(' ', maxlength + 1)) + "\n";
                    }
                }
                if(rv.EndsWith("\n")) rv = rv.Remove(rv.Length - 1, 1);
                return rv;
            }
        }


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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // コマンドライン解析
            bool exit = false;
            bool? nosavesetting = null;
            bool nogui = false;
            bool quiet = false;
            bool version = false;
            bool help = false;

            OptionWithHelpCollection options = new OptionWithHelpCollection(){
                {"platex=", val => {Properties.Settings.Default.platexPath=val;},"platex のパスを設定"},
                {"dvipdfmx=",val =>{Properties.Settings.Default.dvipdfmxPath=val;},"dvpdfmx のパスを設定"},
                {"gs=",val => {Properties.Settings.Default.gsPath = val;},"Ghostscript のパスを設定"},
                {"exit", val => {exit = true;},"設定の保存のみを行い終了する．"},
                {"nosavesettings",val => {nosavesetting = true;},"設定の保存を行わない．"},
                {"nogui",val => {nogui = true;},"CUI モード"},
                {"savesettings",val => {nosavesetting = false;},"設定の保存を行う．"},
                {"resolution=",val => {Properties.Settings.Default.resolutionScale = GetNumberWithErrorHandling(val,"resolution");},"解像度レベルを設定"},
                {"left-margin=",val => {Properties.Settings.Default.leftMargin = GetNumberWithErrorHandling(val,"left-margin");},"左余白を設定"},
                {"right-margin=",val => {Properties.Settings.Default.rightMargin = GetNumberWithErrorHandling(val,"right-margin");},"右余白を設定"},
                {"top-margin=",val => {Properties.Settings.Default.topMargin = GetNumberWithErrorHandling(val,"top-margin");},"上余白を設定"},
                {"bottom-margin=",val => {Properties.Settings.Default.bottomMargin = GetNumberWithErrorHandling(val,"bottom-margin");},"下余白を設定"},
                {"unit=",val => {
                    if(val == "bp") Properties.Settings.Default.yohakuUnitBP = true;
                    else if(val == "px") Properties.Settings.Default.yohakuUnitBP = false;
                },"余白の単位（bp/px）を設定"},
                {"kanji=",val => {
                    if(val == "") Properties.Settings.Default.encode = "_utf8";
                    else Properties.Settings.Default.encode = val;
                },"文字コードの指定（utf8/sjis/jis/euc/)"},
                {"ignore-errors",val => {Properties.Settings.Default.ignoreErrorFlag = true;},"少々のエラーは無視する"},
                {"quiet",val => {quiet = true;},"Quiet モード"},
                {"no-delete",val => {Properties.Settings.Default.deleteTmpFileFlag = false;},"一時ファイルを削除しない"},
                {"help",val => {help = true;},"このメッセージを表示する"},
                {"version",val => {version = true;},"バージョン情報を表示する"}
            };

            var opt = options.GenerateOption();
            List<string> files;
            try { files = opt.Parse(Environment.GetCommandLineArgs()); }
            catch(NDesk.Options.OptionException e) {
                Console.WriteLine("オプション " + e.OptionName + " への入力が不正です．");
                return;
            }
            // files[0]はTeX2img本体なので消しておく
            files.RemoveAt(0);
            if(nosavesetting == null) Properties.Settings.Default.NoSaveSettings = nogui;
            else Properties.Settings.Default.NoSaveSettings = (bool)nosavesetting;

            // すぐに終了
            if(exit) {
                if(nosavesetting != false) Properties.Settings.Default.Save();
                return;
            }

            if(nogui) {
                if(version) {
                    ShowVersion();
                    return;
                }
                if(help) {
                    ShowHelp(options.GenerateHelp());
                    return;
                }
                bool Preview = Properties.Settings.Default.showOutputWindowFlag;
                Properties.Settings.Default.previewFlag = false;
                CUIExec(new CUIOutput(quiet), files);
                Properties.Settings.Default.showOutputWindowFlag = Preview;
            } else {
                Application.Run(new MainForm(files));
            }
            Properties.Settings.Default.Save();
        }

        static int GetNumberWithErrorHandling(string str, string optioname) {
            try { return Int32.Parse(str); }
            catch(FormatException) {
                Console.WriteLine(optioname + " に数値以外が指定されています．");
                Environment.Exit(-2);
            }
            catch(OverflowException) {
                Console.WriteLine(optioname + " に指定された数値が大きすぎ / 小さすぎます．");
                Environment.Exit(-2);
            }
            return 0;
        }

        // CUIモード
        static void CUIExec(IOutputController Output, List<string> files) {
            Converter conv = new Converter(Output);
            if(files.Count == 0) {
                Console.WriteLine("入力ファイルが存在しません．");
                return;
            }
            bool err = false;
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                if(!File.Exists(files[2 * i])) {
                    Console.WriteLine("ファイル " + files[2 * i] + " は見つかりませんでした．");
                    err = true;
                    continue;
                }
                if(!conv.CheckFormat(files[2 * i + 1])) {
                    err = true;
                }
            }
            if(err) return;
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                string file = files[2 * i];
                // 一時フォルダにコピー
                string tmpFilePath = Path.GetTempFileName();
                string tmpTeXFileName = Path.Combine(Path.GetDirectoryName(tmpFilePath),Path.GetFileNameWithoutExtension(tmpFilePath) + ".tex");
                File.Delete(tmpTeXFileName);
                File.Copy(file, tmpTeXFileName, true);

                // 変換！
                conv.Convert(tmpTeXFileName, files[2 * i + 1]);
            }
        }

        static void ShowVersion() {
            Console.WriteLine(
                Application.ProductName + " Version " + Application.ProductVersion
                );
        }

        static void ShowHelp(string optionhelp) {
            Console.WriteLine("使い方：TeX2imgc.exe [Options] Input Output\n\n" + optionhelp);
        }
    }
}

