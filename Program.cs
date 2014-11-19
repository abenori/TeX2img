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

        // オプション引数，動作，ヘルプをまとめて扱うためのクラス
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
            bool? savesetting = null;
            bool nogui = false;
            bool quiet = false;
            bool version = false;
            bool help = false;
            bool? preview = null;

            var options = new OptionWithHelpCollection(){
                {"platex=", val => {Properties.Settings.Default.platexPath=val;},"platex のパス"},
                {"dvipdfmx=",val =>{Properties.Settings.Default.dvipdfmxPath=val;},"dvpdfmx のパス"},
                {"gs=",val => {Properties.Settings.Default.gsPath = val;},"Ghostscript のパス"},
                {"gsdevice=",
                    val => {Properties.Settings.Default.gsDevice = GetStringsFromArray("gsdevice",val,new string[]{"epswrite","eps2write"});},
                    "Ghostscript の device の値（epswrite/eps2write）"
                },
                {"exit", val => {exit = true;},"設定の保存のみを行い終了する．"},
                {"nogui",val => {nogui = true;},null},
                {"savesettings=",val => {savesetting = GetTrueorFalse("savesettings",val);},"設定の保存を行うかを指定する（true/false）．"},
                {"resolution=",val => {Properties.Settings.Default.resolutionScale = GetNumberWithErrorHandling(val,"resolution");},"解像度レベル"},
                {"left-margin=",val => {Properties.Settings.Default.leftMargin = GetNumberWithErrorHandling(val,"left-margin");},"左余白"},
                {"right-margin=",val => {Properties.Settings.Default.rightMargin = GetNumberWithErrorHandling(val,"right-margin");},"右余白"},
                {"top-margin=",val => {Properties.Settings.Default.topMargin = GetNumberWithErrorHandling(val,"top-margin");},"上余白"},
                {"bottom-margin=",val => {Properties.Settings.Default.bottomMargin = GetNumberWithErrorHandling(val,"bottom-margin");},"下余白"},
                {"unit=",val => {
					switch(val){
                    case "bp": Properties.Settings.Default.yohakuUnitBP = true; return;
                    case "px": Properties.Settings.Default.yohakuUnitBP = false; return;
                    default: throw new NDesk.Options.OptionException("bp か bx のいずれかを指定してください．", "unit");
                    }
                },"余白の単位（bp/px）"},
                {"kanji=",val => {
                    string v = GetStringsFromArray("kanji", val, new string[] { "utf8", "sjis", "jis", "euc", "no" });
                    if(v == "no") Properties.Settings.Default.encode = "_utf8";
                    else Properties.Settings.Default.encode = val;
                },"文字コードの指定（utf8/sjis/jis/euc/no)"},
                {"ignore-errors=",val => {Properties.Settings.Default.ignoreErrorFlag = GetTrueorFalse("ignore-errors",val);},"少々のエラーは無視する（true/false）"},
                {"low-resolution=",val => {Properties.Settings.Default.useLowResolution = GetTrueorFalse("low-resolution",val);},"低解像度で処理する（true/false）"},
                {"quiet",val => {quiet = true;},"Quiet モード"},
                {"no-delete=",val => {Properties.Settings.Default.deleteTmpFileFlag = !GetTrueorFalse("no-delete",val);},"一時ファイルを削除しない（true/false）"},
                {"num=",val => {Properties.Settings.Default.LaTeXCompileMaxNumber = GetNumberWithErrorHandling(val,"num");},"LaTeX ソースコンパイルの（最大）回数"},
                {"guess-compile=",val => {Properties.Settings.Default.guessLaTeXCompile = GetTrueorFalse("guess-cimpile",val);},"LaTeX ソースコンパイル回数を推定（true/false）"},
                {"imagemagick=",val => {Properties.Settings.Default.useMagickFlag = GetTrueorFalse("imagemagick",val);},"ImageMagick を使う（true/false）"},
                {"transparent=",val => {Properties.Settings.Default.transparentPngFlag = GetTrueorFalse("transparent",val);},"透過 PNG を作る（true/false）"},
                {"preview",val => {preview = true;},"生成されたファイルを開く"},
                {"help",val => {help = true;},"このメッセージを表示する"},
                {"version",val => {version = true;},"バージョン情報を表示する"}
            };

            var opt = options.GenerateOption();
            List<string> files;
            try { files = opt.Parse(Environment.GetCommandLineArgs()); }
            //try { files = opt.Parse(new string[]{"TeX2img","/nogui","/platex"}); }
            catch(NDesk.Options.OptionException e) {
                if(e.OptionName != null) Console.WriteLine("オプション " + e.OptionName + " への入力が不正です．");
                Environment.ExitCode = 1;
                return;
            }
            // files[0]はTeX2img本体なので消しておく
            if(files.Count != 0)files.RemoveAt(0);
            Properties.Settings.Default.SaveSettings = savesetting ?? !nogui;
            if(preview != null) Properties.Settings.Default.previewFlag = (bool)preview;

            // すぐに終了
            if(exit) {
                Properties.Settings.Default.Save();
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
            }
            // filesのチェック
            string err = "";
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                if(!File.Exists(files[2 * i])) {
                    err += "ファイル " + files[2 * i] + " は見つかりませんでした．\n";
                }
                if(!Converter.CheckFormat(files[2 * i + 1],null)) {
                    err += "ファイル " + files[2 * i + 1] + " の拡張子は eps/png/jpg/pdf のいずれでもありません．\n";
                }
            }
            if(files.Count % 2 != 0) {
                err += "ファイル " + files[files.Count - 1] + " に対応する出力ファイルが指定されていません．\n";
            }
            if(err != ""){
                err = err.Remove(err.Length - 1);// 最後の改行を削除
                if(nogui)Console.WriteLine(err);
                else MessageBox.Show(err,"TeX2img");
                Environment.ExitCode = 2;
                return;
            }

            if(nogui) {
                if(preview == null) {
                    preview = Properties.Settings.Default.previewFlag;
                    Properties.Settings.Default.previewFlag = false;
                }
                CUIExec(new CUIOutput(quiet), files);
                Properties.Settings.Default.showOutputWindowFlag = (bool) preview;
            } else {
                // Azuki.dllの存在チェック
                string[] chkfiles = new string[] { "Azuki.dll" };
                foreach(var f in chkfiles) {
                    if(!System.IO.File.Exists(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), f))) {
                        if(Converter.which(f) == "") {
                            MessageBox.Show(f + " が見つからないため，起動することができませんでした．", "TeX2img");
                            Environment.ExitCode = -1;
                            return;
                        }
                    }
                }
                Application.Run(new MainForm(files));
            }
            Properties.Settings.Default.Save();
        }

        static int GetNumberWithErrorHandling(string str, string optioname) {
            try { return Int32.Parse(str); }
            catch(FormatException) {
                Console.WriteLine(optioname + " に数値以外が指定されています．");
                throw new NDesk.Options.OptionException();
            }
            catch(OverflowException) {
                Console.WriteLine(optioname + " に指定された数値が大きすぎ / 小さすぎます．");
                throw new NDesk.Options.OptionException();
            }
        }
        static string GetStringsFromArray(string optionname,string val,string[] possibleargs){
			foreach(var s in possibleargs){
				if(val.ToLower() == s)return s;
			}
            throw new NDesk.Options.OptionException("引数が不正です．", optionname);
		}
        static bool GetTrueorFalse(string optionname, string val) {
            switch(val.ToLower()) {
            case "true": return true;
            case "false": return false;
            default: throw new NDesk.Options.OptionException("引数が不正です．", optionname);
            }
        }


        // CUIモード
        static void CUIExec(IOutputController Output, List<string> files) {
            Converter conv = new Converter(Output);
            if(files.Count == 0) {
                Console.WriteLine("入力ファイルが存在しません．");
                return;
            }
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

