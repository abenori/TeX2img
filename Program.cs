﻿using System;
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
            {"latex=","LaTeX のパス", val => Properties.Settings.Default.platexPath=val,()=>Properties.Settings.Default.platexPath},
            {"platex=","/latex と同じ（obsolete）", val => Properties.Settings.Default.platexPath=val},
            {"dvidriver=","DVI driver のパス",val =>Properties.Settings.Default.dvipdfmxPath=val,()=>Properties.Settings.Default.dvipdfmxPath},
            {"dvipdfmx=","/dvidriver と同じ（obsolete）",val =>Properties.Settings.Default.dvipdfmxPath=val},
            {"gs=","Ghostscript のパス",val => Properties.Settings.Default.gsPath = val,()=>Properties.Settings.Default.gsPath},
            {"gsdevice=",
                "Ghostscript の device（epswrite/eps2write）",
                val => {Properties.Settings.Default.gsDevice = GetStringsFromArray("gsdevice",val,new string[]{"epswrite","eps2write"});},
                ()=>Properties.Settings.Default.gsDevice
            },
            {"kanji=","文字コード（utf8/sjis/jis/euc/no)",val => {
                string v = GetStringsFromArray("kanji", val, new string[] { "utf8", "sjis", "jis", "euc", "no" });
                if(v == "no"){
                    if(Properties.Settings.Default.encode != "_sjis") Properties.Settings.Default.encode = "_utf8";
                }else Properties.Settings.Default.encode = val;
            },()=>Properties.Settings.Default.encode.StartsWith("_") ? "no" : Properties.Settings.Default.encode},
            {"guess-compile","LaTeX ソースコンパイル回数を推定[-]",val => Properties.Settings.Default.guessLaTeXCompile = (val != null),()=>Properties.Settings.Default.guessLaTeXCompile},
            {"num=","LaTeX ソースコンパイルの（最大）回数",(int val) => {Properties.Settings.Default.LaTeXCompileMaxNumber = val;},()=>Properties.Settings.Default.LaTeXCompileMaxNumber},
            {"resolution=","解像度レベル",(int val) => Properties.Settings.Default.resolutionScale = val,()=>Properties.Settings.Default.resolutionScale},
            {"left-margin=","左余白",(int val) => Properties.Settings.Default.leftMargin = val,()=>Properties.Settings.Default.leftMargin},
            {"top-margin=","上余白",(int val) => Properties.Settings.Default.topMargin = val,()=>Properties.Settings.Default.topMargin},
            {"right-margin=","右余白",(int val) => Properties.Settings.Default.rightMargin = val,()=>Properties.Settings.Default.rightMargin},
            {"bottom-margin=","下余白",(int val) => Properties.Settings.Default.bottomMargin = val,()=>Properties.Settings.Default.bottomMargin},
            {"margins=","余白（一括/左右 上下/左 上 右 下）",val=>{
                var list = val.Split(new char[] { ' ' }).ToList();
                list.RemoveAll((s) => (s == ""));
                try {
                    if(list.Count == 1) Properties.Settings.Default.leftMargin = Properties.Settings.Default.topMargin = Properties.Settings.Default.rightMargin = Properties.Settings.Default.bottomMargin = Int32.Parse(list[0]);
                    else if(list.Count == 2) {
                        Properties.Settings.Default.leftMargin = Properties.Settings.Default.rightMargin = Int32.Parse(list[0]);
                        Properties.Settings.Default.topMargin = Properties.Settings.Default.bottomMargin = Int32.Parse(list[1]);
                    } else if(list.Count == 4) {
                        Properties.Settings.Default.leftMargin = Int32.Parse(list[0]);
                        Properties.Settings.Default.topMargin = Int32.Parse(list[1]);
                        Properties.Settings.Default.rightMargin = Int32.Parse(list[2]);
                        Properties.Settings.Default.bottomMargin = Int32.Parse(list[3]);
                    }else throw new NDesk.Options.OptionException("余白の指定が不正です．","margins");
                }
                catch(FormatException e) {
                    throw new NDesk.Options.OptionException(e.Message,"margins");
                }
            } },
            {"unit=","余白の単位（bp/px）",val => {
                switch(val){
                case "bp": Properties.Settings.Default.yohakuUnitBP = true; return;
                case "px": Properties.Settings.Default.yohakuUnitBP = false; return;
                default: throw new NDesk.Options.OptionException("bp, px のいずれかを指定してください。", "unit");
                }
            },()=>Properties.Settings.Default.yohakuUnitBP ? "bp" : "px"},
            {"keep-page-size","ページサイズを維持[-]",val=>Properties.Settings.Default.keepPageSize=(val != null),()=>Properties.Settings.Default.keepPageSize},
            {"pagebox=",val=>Properties.Settings.Default.pagebox = GetStringsFromArray("pagebox",val,new string[]{"media","crop","bleed","trim","art"})},// 隠しオプション
            {"merge-output-files","PDF / TIFF ファイルを単一ファイルに[-]",val=>Properties.Settings.Default.mergeOutputFiles = (val != null),()=>Properties.Settings.Default.mergeOutputFiles},
            {"animation-delay=",(double sec)=>{Properties.Settings.Default.animationDelay = (uint)(sec*100);}},
            {"animation-loop=",(uint val)=>Properties.Settings.Default.animationLoop = val},
            {"transparent","透過 PNG / TIFF / EMF[-]",val => Properties.Settings.Default.transparentPngFlag = (val != null),()=>Properties.Settings.Default.transparentPngFlag},
            {"with-text","PDF のテキスト情報を保持[-]",val =>Properties.Settings.Default.outlinedText = !(val != null),()=>!Properties.Settings.Default.outlinedText},
            {"delete-display-size","SVG の表示寸法を削除[-]",val => Properties.Settings.Default.deleteDisplaySize = (val != null),()=>Properties.Settings.Default.deleteDisplaySize},
            {"antialias","アンチエイリアス処理[-]",val => Properties.Settings.Default.useMagickFlag = (val != null),()=>Properties.Settings.Default.useMagickFlag},
            {"low-resolution","低解像度で処理[-]",val => Properties.Settings.Default.useLowResolution = (val!= null),()=>Properties.Settings.Default.useLowResolution},
            {"ignore-errors","少々のエラーは無視[-]",val => Properties.Settings.Default.ignoreErrorFlag = (val != null),()=>Properties.Settings.Default.ignoreErrorFlag},
            {"delete-tmpfiles","一時ファイルを削除[-]",val => Properties.Settings.Default.deleteTmpFileFlag = (val != null),()=>Properties.Settings.Default.deleteTmpFileFlag},
            {"preview","生成ファイルを開く[-]",val => preview = (val != null),()=>preview==null?false:preview.Value},
            {"embed-source","ソース情報を生成ファイルに保存[-]",val => Properties.Settings.Default.embedTeXSource = (val != null),()=>Properties.Settings.Default.embedTeXSource},
            {"copy-to-clipboard","生成ファイルをクリップボードにコピー[-]",val =>  Properties.Settings.Default.setFileToClipBoard = (val != null),()=>Properties.Settings.Default.setFileToClipBoard},
            {"savesettings","設定の保存を行う[-]",val => Properties.Settings.Default.SaveSettings = (val != null),()=>Properties.Settings.Default.SaveSettings},
            {"quiet","Quiet モード[-]",val => quiet = (val != null),()=>quiet},
            {"timeout=","タイムアウト時間を設定（秒）", (int val) => {
                if(val <= 0) throw new NDesk.Options.OptionException("タイムアウト時間は 0 より大きい値を指定してください。", "timeout");
                Properties.Settings.Default.timeOut = val * 1000;
            },()=>Properties.Settings.Default.timeOut/1000 + " 秒"},
            // TODO: defaultに対応するオプションがあった方がよい？
            {"batch=","Batch モード（stop/nonstop）", val => {
                switch(val) {
                case "nonstop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.NonStop; break;
                case "stop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.Stop; break;
                default: throw new NDesk.Options.OptionException("stop, nonstop のいずれかを指定してください。", "batch");
                }
            },()=>{
                switch(Properties.Settings.Default.batchMode){
                case Properties.Settings.BatchMode.NonStop: return"nonstop";
                case Properties.Settings.BatchMode.Stop: return "stop";
                default: return "deafult";
            }}},
            {"exit","設定の保存のみを行い終了する", val => {exit = true;}},
            {"load-defaults","現在の設定をデフォルトに戻す",val => {if(val != null)Properties.Settings.Default.ReloadDefaults();}},
            {"help","このメッセージを表示する",val => {help = true;}},
            {"version","バージョン情報を表示する",val => {version = true;}}
        };

        static string GetStringsFromArray(string optionname, string val, string[] possibleargs) {
            if(possibleargs.Contains(val)) return val;
            throw new NDesk.Options.OptionException(String.Join(", ", possibleargs) + " のいずれかを指定してください。", optionname);
        }

        static int TeX2imgMain(List<string> cmds) {
            Properties.Settings.Default.SaveSettings = !nogui;
            // オプション解析
            List<string> files;
            try { files = options.Parse(cmds); }
            catch(NDesk.Options.OptionException e) {
                if(e.OptionName != null) {
                    var msg = "オプション " + e.OptionName + " への入力が不正です";
                    if(e.Message != "") msg += "：" + e.Message;
                    else msg += "。";
                    msg += "\nTeX2img" + (nogui ? "c" : "") + ".exe /help によるヘルプを参照してください。";
                    if(nogui) Console.WriteLine(msg);
                    else MessageBox.Show(msg, "TeX2img");
                }
                return -1;
            }
            // 各種バイナリのパスが設定されていなかったら推測する。
            // "/exit"が指定されている場合はメッセージ表示をしない。
            setPath(exit);
            if(help) {
                ShowHelp();
                return 0;
            }
            if(version) {
                ShowVersion();
                return 0;
            }
            // CUIモードの引数なしはエラー
            if(nogui && files.Count == 0) {
                Console.WriteLine("引数がありません。\n");
                ShowHelp();
                return -1;
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
                var chkconv = new Converter(null, files[2 * i], files[2 * i + 1]);
                if(!chkconv.CheckInputFormat()) {
                    err += "ファイル " + files[2 * i + 1] + " の拡張子は .tex ではありません。\n";
                }
                if(!File.Exists(files[2 * i])) {
                    err += "ファイル " + files[2 * i] + " は見つかりませんでした。";
                    if(files[2 * i].StartsWith("-") || files[2 * i].StartsWith("/")) err += "オプション名のミスの可能性もあります。";
                    err += "\n";
                }
                if(!chkconv.CheckFormat()) {
                    err += "ファイル " + files[2 * i + 1] + " の拡張子は " + String.Join("/", Converter.imageExtensions) + " のいずれでもありません。\n";
                }
            }
            if(files.Count % 2 != 0) {
                err += "ファイル " + files[files.Count - 1] + " に対応する出力ファイルが指定されていません。";
                if(files[files.Count - 1].StartsWith("-") || files[files.Count - 1].StartsWith("/")) err += "オプション名のミスの可能性もあります。";
                err += "\n";
            }
            if(err != "") {
                err = err.Remove(err.Length - 1);// 最後の改行を削除
                if(nogui) Console.WriteLine(err);
                else MessageBox.Show(err, "TeX2img");
                return -2;
            }

            if(nogui) {
                // CUIでオプション指定がないときは，設定によらずプレビューをしないようにする。
                if(preview == null) {
                    preview = Properties.Settings.Default.previewFlag;
                    Properties.Settings.Default.previewFlag = false;
                }
                int r = CUIExec(quiet, files);
                Properties.Settings.Default.previewFlag = (bool) preview;
                return r;
            } else {
                Application.Run(new MainForm(files));
                return 0;
            }
        }

        [STAThread]
        static void Main() {
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
            //cmds = new List<string> { "TeX2img.exe", "/nogui", "--margins=\"a b c d\"","test.tex","test.pdf" };
            // 一つ目がTeX2img本体ならば削除
            // abtlinstからCreateProcessで呼び出すとTeX2img本体にならなかったので，一応確認をする。
            if(cmds.Count > 0) {
                string filecmds0 = Path.GetFileName(cmds[0]).ToLower();
                string me = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToLower();
#if DEBUG
                string vshost = Path.Combine(Path.GetDirectoryName(me), Path.GetFileNameWithoutExtension(me) + ".vshost.exe").ToLower();
                if(vshost == filecmds0 || vshost == filecmds0 + ".exe") filecmds0 = me;
#endif
                if(filecmds0 == me || filecmds0 + ".exe" == me) cmds.RemoveAt(0);
            }
            
            // 二つ目でCUIモードか判定する。
            if(cmds.Count > 0 && (cmds[0] == "/nogui" || cmds[0] == "-nogui" || cmds[0] == "--nogui")) {
                nogui = true;
                cmds.RemoveAt(0);
            }
            var chkfiles = new List<string>() { "pdfiumdraw.exe", "mudraw.exe" };
            if(!nogui) chkfiles.Add("Azuki.dll");
            string mydir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            chkfiles = chkfiles.Where(f => !File.Exists(Path.Combine(mydir, f))).ToList();
            string chkfile_errmsg = null;
            if(chkfiles.Count != 0) {
                chkfile_errmsg = "以下のファイルが見つからないため，起動することができませんでした。\n" + String.Join("\n", chkfiles.ToArray());
            }
            if(chkfile_errmsg != null) {
                if(nogui) Console.WriteLine(chkfile_errmsg);
                else MessageBox.Show(chkfile_errmsg, "TeX2img");
                Environment.ExitCode = - 3;
                return;
            }

            // GUIか否かで設定の変更をする
            if(!nogui) {
                //Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.NonStop;
                Properties.Settings.Default.timeOut = 0;
            }
            // メイン
            var exitcode = TeX2imgMain(cmds);
            Properties.Settings.Default.Save();
            Environment.ExitCode = exitcode;
        }

        static void setPath(bool nomsg) {
            if(Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                if(Properties.Settings.Default.platexPath == "") Properties.Settings.Default.platexPath = Properties.Settings.Default.GuessPlatexPath();
                if(Properties.Settings.Default.dvipdfmxPath == "") Properties.Settings.Default.dvipdfmxPath = Properties.Settings.Default.GuessDvipdfmxPath();
                if(Properties.Settings.Default.gsPath == "") Properties.Settings.Default.gsPath = Properties.Settings.Default.GuessGsPath();
                if(Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                    if(!nomsg) {
                        var msg = "LaTeX / DVI driver / Ghostscript のパス設定に失敗しました。\n環境設定画面で手動で設定してください。";
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
            if(files.Count == 0) {
                Console.WriteLine("入力ファイルが存在しません。");
                return -5;
            }
            try {
                Directory.CreateDirectory(Path.GetTempPath());
            }
            catch(Exception) {
                Console.WriteLine("一時フォルダ\n" + Path.GetTempPath() + "の作成に失敗しました。環境変数 TMP 及び TEMP を確認してください。");
                return -7;
            }

            int failnum = 0;

            var outFiles = new System.Collections.Specialized.StringCollection();
            for(int i = 0 ; i < files.Count / 2 ; ++i) {
                string file = Path.GetFullPath(files[2 * i]);
                string tmpTeXFileName = TempFilesDeleter.GetTempFileName(Path.GetExtension(file));
                if(tmpTeXFileName == null) {
                    Console.WriteLine("一時ファイル名の決定に失敗しました。作業フォルダ：\n" + Path.GetTempPath() + "\nを確認してください。");
                    return -6;
                }
                tmpTeXFileName = Path.Combine(Path.GetTempPath(), tmpTeXFileName);
                // 一時フォルダにコピー
                File.Copy(file, tmpTeXFileName, true);
                (new FileInfo(tmpTeXFileName)).Attributes = FileAttributes.Normal;
                var output = Path.GetFullPath(files[2 * i + 1]);
                // 変換！
                try {
                    using(var converter = new Converter(Output, tmpTeXFileName, output)) {
                        converter.AddInputPath(Path.GetDirectoryName(file));
                        if(!converter.Convert()) ++failnum;
                        else outFiles.AddRange(converter.OutputFileNames.ToArray());
                    }
                    if(Properties.Settings.Default.setFileToClipBoard) Clipboard.SetFileDropList(outFiles);
                }
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
    }
}
