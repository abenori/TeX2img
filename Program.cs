using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel;

namespace TeX2img {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        /// 
        static bool exit = false;
        static bool quiet = false;
        static bool? preview = null;
        static bool nogui = false;
        static bool version = false;
        static bool help = false;

        static OptionSet options = new OptionSet(){
			{"platex=","platex のパス", val => {Properties.Settings.Default.platexPath=val;}},
			{"dvipdfmx=","dvipdfmx のパス",val =>{Properties.Settings.Default.dvipdfmxPath=val;}},
			{"gs=","Ghostscript のパス",val => {Properties.Settings.Default.gsPath = val;}},
			{"gsdevice=",
				"Ghostscript の device の値（epswrite/eps2write）",
				val => {Properties.Settings.Default.gsDevice = GetStringsFromArray("gsdevice",val,new string[]{"epswrite","eps2write"});}
			},
			{"kanji=","文字コードの指定（utf8/sjis/jis/euc/no)",val => {
				string v = GetStringsFromArray("kanji", val, new string[] { "utf8", "sjis", "jis", "euc", "no" });
				if(v == "no"){
					if(Properties.Settings.Default.encode != "_sjis") Properties.Settings.Default.encode = "_utf8";
				}else Properties.Settings.Default.encode = val;
			}},
			{"guess-compile","LaTeX ソースコンパイル回数を推定[-]",val => {Properties.Settings.Default.guessLaTeXCompile = (val != null);}},
			{"num=","LaTeX ソースコンパイルの（最大）回数",(int val) => {Properties.Settings.Default.LaTeXCompileMaxNumber = val;}},
			{"resolution=","解像度レベル",(int val) => {Properties.Settings.Default.resolutionScale = val;}},
            {"left-margin=","左余白",(int val) => {Properties.Settings.Default.leftMargin = val;}},
			{"right-margin=","右余白",(int val) => {Properties.Settings.Default.rightMargin = val;}},
			{"top-margin=","上余白",(int val) => {Properties.Settings.Default.topMargin = val;}},
			{"bottom-margin=","下余白",(int val) => {Properties.Settings.Default.bottomMargin = val;}},
			{"unit=","余白の単位（bp/px）",val => {
			    switch(val){
			    case "bp": Properties.Settings.Default.yohakuUnitBP = true; return;
			    case "px": Properties.Settings.Default.yohakuUnitBP = false; return;
			    default: throw new NDesk.Options.OptionException("bp, px のいずれかを指定してください．", "unit");
			    }
            }},
			{"transparent","透過 PNG を作る[-]",val => {Properties.Settings.Default.transparentPngFlag = (val != null);}},
			{"imagemagick","ImageMagick を使う[-]",val => {Properties.Settings.Default.useMagickFlag = (val != null);}},
			{"low-resolution","低解像度で処理する[-]",val => {Properties.Settings.Default.useLowResolution = (val!= null);}},
			{"ignore-errors","少々のエラーは無視する[-]",val => {Properties.Settings.Default.ignoreErrorFlag = (val != null);}},
            {"no-delete","一時ファイルを削除しない[-]",val => {Properties.Settings.Default.deleteTmpFileFlag = !(val != null);}},
			{"preview","生成されたファイルを開く",val => {preview = (val != null);}},
            {"no-embed-source","ソース情報を生成ファイルに保存しない[-]",val => {Properties.Settings.Default.embedTeXSource = !(val != null);}},
			{"savesettings","設定の保存を行う",val => {Properties.Settings.Default.SaveSettings = (val != null);}},
			{"quiet","Quiet モード",val => {quiet = true;}},
            {"batch=","Batch モード（stop/nonstop）", val => {
                switch(val) {
                case "nonstop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.NonStop; break;
                case "stop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.Stop; break;
                default: throw new NDesk.Options.OptionException("stop, nonstop のいずれかを指定してください．", "batch");
                }
            }},
			{"exit","設定の保存のみを行い終了する", val => {exit = true;}},
			{"help","このメッセージを表示する",val => {help = true;}},
			{"version","バージョン情報を表示する",val => {version = true;}}
        };

        static string GetStringsFromArray(string optionname, string val, string[] possibleargs) {
            if(possibleargs.Contains(val)) return val;
            throw new NDesk.Options.OptionException(String.Join(", ",possibleargs) + " のいずれかを指定してください．", optionname);
        }

        static int TeX2imgMain(List<string> cmds) {
            if(cmds.Count > 0) {
                using(var meta = new System.Drawing.Imaging.Metafile(cmds[0]))
                using(var ws = new StreamWriter(cmds[0] + ".txt")) {
                    var header = meta.GetMetafileHeader();
                    ws.WriteLine("Size = " + header.MetafileSize.ToString());
                    ws.WriteLine("dpiX = " + header.DpiX.ToString() + ", dpiY = " + header.DpiY.ToString());
                    ws.WriteLine("logicaldpiY = " + header.LogicalDpiX.ToString() + ", logicaldpiY = " + header.LogicalDpiY.ToString());
                }
                return 0;
            }

            // 各種バイナリのパスが設定されていなかったら推測する．
            // "/exit"が指定されている場合はメッセージ表示をしない．
            setPath(cmds.Contains("/exit") || cmds.Contains("-exit") || cmds.Contains("--exit"));
            // CUIモードの引数なしはエラー
            if(nogui && cmds.Count == 0) {
                Console.WriteLine("引数がありません．\n");
                ShowHelp();
                return -1;
            }
            Properties.Settings.Default.SaveSettings = !nogui;

            // オプション解析
            List<string> files;
            try { files = options.Parse(cmds); }
            catch(NDesk.Options.OptionException e) {
                if(e.OptionName != null) {
                    var msg = "オプション " + e.OptionName + " への入力が不正です";
                    if(e.Message != "") msg += "：" + e.Message;
                    else msg += "．";
                    msg += "\nTeX2img" + (nogui ? "c" : "") + ".exe /help によるヘルプを参照してください．";
                    if(nogui) Console.WriteLine(msg);
                    else MessageBox.Show(msg, "TeX2img");
                }
                return -1;
            }
            if(help) {
                ShowHelp();
                return 0;
            }
            if(version) {
                ShowVersion();
                return 0;
            }

            if(preview != null) Properties.Settings.Default.previewFlag = (bool) preview;
            //Console.WriteLine(preview == null ? "null" : preview.ToString());

            // すぐに終了
            if(exit) {
                Properties.Settings.Default.Save();
                return 0;
            }
            // filesのチェック
            string err = "";
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                if(!File.Exists(files[2 * i])) {
                    err += "ファイル " + files[2 * i] + " は見つかりませんでした．\n";
                }
                if(!(new Converter(null,null,files[2 * i + 1])).CheckFormat()) {
                    err += "ファイル " + files[2 * i + 1] + " の拡張子は eps/png/jpg/pdf のいずれでもありません．\n";
                }
            }
            if(files.Count % 2 != 0) {
                err += "ファイル " + files[files.Count - 1] + " に対応する出力ファイルが指定されていません．\n";
            }
            if(err != "") {
                err = err.Remove(err.Length - 1);// 最後の改行を削除
                if(nogui) Console.WriteLine(err);
                else MessageBox.Show(err, "TeX2img");
                return -2;
            }

            if(nogui) {
                // CUIでオプション指定がないときは，設定によらずプレビューをしないようにする．
                if(preview == null) {
                    preview = Properties.Settings.Default.previewFlag;
                    Properties.Settings.Default.previewFlag = false;
                }
                int r = CUIExec(quiet, files);
                Properties.Settings.Default.previewFlag = (bool) preview;
                Properties.Settings.Default.Save();
                return r;
            } else {
                // dllの存在チェック
                string[] chkfiles = { "Azuki.dll" };
                
                string mydir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                chkfiles = chkfiles.Where(f => !File.Exists(Path.Combine(mydir, f))).ToArray();
                if(chkfiles.Length != 0) {
                    MessageBox.Show("以下のファイルが見つからないため，起動することができませんでした．\n" + String.Join("\n", chkfiles), "TeX2img");
                    return -3;
                }
                Application.Run(new MainForm(files));
                Properties.Settings.Default.Save();
                return 0;
            }
        }

        [STAThread]
        static void Main() {
            /*
            byte[] buf = System.Text.Encoding.GetEncoding("utf-8").GetBytes("完璧");
            var encs = KanjiEncoding.GuessKajiEncoding(buf);
            foreach(var enc in encs) { System.Diagnostics.Debug.WriteLine(enc.EncodingName + "： " + enc.GetString(buf)); }
            return;
            */
            // アップデートしていたら前バージョンの設定を読み込む
            if(!Properties.Settings.Default.IsUpgraded) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.TeX2imgVersion = Application.ProductVersion;
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // コマンドライン解析
            var cmds = new List<string>(Environment.GetCommandLineArgs());
            //var cmds = new List<string> { "TeX2img.exe", "/nogui", "/help" };
            // 一つ目がTeX2img本体ならば削除
            // abtlinstからCreateProcessで呼び出すとTeX2img本体にならなかったので，一応確認をする．
            if(cmds.Count > 0) {
                string filecmds0 = Path.GetFullPath(cmds[0]).ToLower();
                string me = Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location).ToLower();
#if DEBUG
                string vshost = Path.Combine(Path.GetDirectoryName(me), Path.GetFileNameWithoutExtension(me) + ".vshost.exe").ToLower();
                if(vshost == filecmds0 || vshost == filecmds0 + ".exe") filecmds0 = me;
#endif
                if(filecmds0 == me || filecmds0 + ".exe" == me) cmds.RemoveAt(0);
            }
            // 二つ目でCUIモードか判定する．
            if(cmds.Count > 0 && (cmds[0] == "/nogui" || cmds[0] == "-nogui" || cmds[0] == "--nogui")) {
                nogui = true;
                cmds.RemoveAt(0);
            }
            // メイン
            Environment.ExitCode = TeX2imgMain(cmds);
        }

        static void setPath(bool nomsg) {
            if(Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                if(Properties.Settings.Default.platexPath == "") Properties.Settings.Default.platexPath = Properties.Settings.Default.GuessPlatexPath();
                if(Properties.Settings.Default.dvipdfmxPath == "") Properties.Settings.Default.dvipdfmxPath = Properties.Settings.Default.GuessDvipdfmxPath();
                if(Properties.Settings.Default.gsPath == "") Properties.Settings.Default.gsPath = Properties.Settings.Default.GuessGsPath();
                if(Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                    if(!nomsg) {
                        var msg = "platex / dvipdfmx / gs のパス設定に失敗しました。\n環境設定画面で手動で設定してください。";
                        if(nogui) Console.WriteLine(msg + "\n");
                        else MessageBox.Show(msg, "TeX2img");
                        (new SettingForm()).ShowDialog();
                    }
                } else {
                    if(!nomsg) {
                        var msg = String.Format("TeX 関連プログラムのパスを\n {0}\n {1}\n {2}\nに設定しました。\n違っている場合は環境設定画面で手動で変更してください。", Properties.Settings.Default.platexPath, Properties.Settings.Default.dvipdfmxPath, Properties.Settings.Default.gsPath);
                        if(nogui) Console.WriteLine(msg + "\n");
                        else MessageBox.Show(msg, "TeX2img");
                    }
                }
                Properties.Settings.Default.Save();
            }

            if(Properties.Settings.Default.gsDevice == "" && Properties.Settings.Default.gsPath != "") {
                Properties.Settings.Default.gsDevice = Properties.Settings.Default.GuessGsdevice();
                if(Properties.Settings.Default.gsDevice == "") Properties.Settings.Default.gsDevice = "epswrite";
                Properties.Settings.Default.Save();
            }
        }

        // CUIモード
        static int CUIExec(bool q, List<string> files) {
            IOutputController Output = new CUIOutput(q);
            //Console.WriteLine(Output.askYesorNo("テストのyes or no"));
            if(files.Count == 0) {
                Console.WriteLine("入力ファイルが存在しません．");
                return -5;
            }
            int failnum = 0;
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                string file = files[2 * i];
                // 一時フォルダにコピー
                string tmpFilePath = Path.GetTempFileName();
                string tmpTeXFileName = Path.Combine(Path.GetDirectoryName(tmpFilePath), Path.GetFileNameWithoutExtension(tmpFilePath) + ".tex");
                File.Delete(tmpTeXFileName);
                File.Copy(file, tmpTeXFileName, true);

                // 変換！
                try { if(!((new Converter(Output,tmpTeXFileName, files[2 * i + 1])).Convert())) ++failnum; }
                catch(Exception e) { Console.WriteLine(e.Message); }
            }
            return failnum;
        }

        static void ShowVersion() {
            var msg = Application.ProductName + " Version " + Application.ProductVersion;
            if(nogui) Console.WriteLine(msg);
            else MessageBox.Show(msg, "TeX2img");
        }

        static void ShowHelp() {
            StringWriter sw = new StringWriter();
            options.WriteOptionDescriptions(sw);
            var msg = "使い方：TeX2img" + (nogui ? "c" : "") + ".exe [Options] Input Output\n\n" + sw.ToString();
            //if(msg.EndsWith("\n")) msg = msg.Remove(msg.Length - 1);
            if(nogui) Console.WriteLine(msg);
            else MessageBox.Show(msg, "TeX2img");
        }
        
        public class OptionSet : NDesk.Options.OptionSet {
            // 次でエラーを出すようにする
            // "option"の指定の時に--option=abc
            // "option="の指定の時に--option-
            protected override bool Parse(string argument, NDesk.Options.OptionContext c) {
                if(c.Option == null) {
                    string f, n, s, v;
                    if(!GetOptionParts(argument, out f, out n, out s, out v)) return false;
                    if(Contains(n)) {
                        var p = this[n];
                        if(v != null && p.OptionValueType == NDesk.Options.OptionValueType.None) {
                            // メッセージはさぼり
                            throw new NDesk.Options.OptionException(c.OptionSet.MessageLocalizer(""), f + n);
                        }
                    } else {
                        string rn;
                        if(n.Length >= 1 && (n[n.Length - 1] == '-' || n[n.Length - 1] == '+') && Contains((rn = n.Substring(0, n.Length - 1)))) {
                            var p = this[rn];
                            if(p.OptionValueType == NDesk.Options.OptionValueType.Required) {
                                throw new NDesk.Options.OptionException(c.OptionSet.MessageLocalizer("An argument is required for the option '" + f + rn + "'"), f + rn);
                            }
                        }
                    }
                }
                return base.Parse(argument, c);
            }

            // OptionSet.WriteOptionDescriptionsがちょっと気にくわないので独自に
            // GetNames().Count == 1と仮定してある．
            public new void WriteOptionDescriptions(TextWriter output) {
                int maxlength = 0;
                foreach(var oh in this) {
                    if(oh.Description != null) {
                        int length = oh.GetNames()[0].Length;
                        if(oh.Description.EndsWith("[-]")) length += 3;
                        else if(oh.OptionValueType == NDesk.Options.OptionValueType.Optional) length += 7;
                        else if(oh.OptionValueType == NDesk.Options.OptionValueType.Required) length += 5;
                        maxlength = Math.Max(maxlength, length);
                    }
                }
                maxlength += 3;
                foreach(var oh in this) {
                    if(oh.Description != null) {
                        string opstr = "/" + oh.GetNames()[0];
                        string desc = oh.Description.Replace("\n", "\n" + new string(' ', maxlength + 1));
                        if(oh.OptionValueType == NDesk.Options.OptionValueType.Optional) opstr += "[=<VAL>]";
                        else if(oh.OptionValueType == NDesk.Options.OptionValueType.Required) opstr += "=<VAL>";
                        else if(desc.EndsWith("[-]")) {
                            opstr += "[-]";
                            desc = desc.Substring(0, desc.Length - 3);
                        }
                        output.WriteLine("  " + opstr + new string(' ', maxlength - opstr.Length) + desc);
                    }
                }
            }
        }

    }
}
