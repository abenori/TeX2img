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
        static System.Globalization.CultureInfo enUS = new System.Globalization.CultureInfo("en-US");

        static OptionSet GetOptiontSet() {
            return new OptionSet(){
            {"latex=",Properties.Resources.CMDLINE_LATEX, val => Properties.Settings.Default.platexPath=val,()=>Properties.Settings.Default.platexPath},
            {"platex=",val => Properties.Settings.Default.platexPath=val},
            {"dvidriver=",Properties.Resources.CMDLINE_DVIDRIVER,val =>Properties.Settings.Default.dvipdfmxPath=val,()=>Properties.Settings.Default.dvipdfmxPath},
            {"dvipdfmx=",val =>Properties.Settings.Default.dvipdfmxPath=val},
            {"gs=",Properties.Resources.CMDLINE_GS,val => Properties.Settings.Default.gsPath = val,()=>Properties.Settings.Default.gsPath},
            {"oldgs",
                Properties.Resources.CMDLINE_OLDGS + "[-]",
                val => {Properties.Settings.Default.gsDevice = (val != null) ? "epswrite" : "eps2write"; },
                ()=>Properties.Settings.Default.gsDevice == "epswrite"
            },
            {"kanji=",Properties.Resources.CMDLINE_KANJI,val => {
                string v = GetStringsFromArray("kanji", val, new string[] { "utf8", "sjis", "jis", "euc", "no" });
                if(v == "no"){
                    if(Properties.Settings.Default.encode != "_sjis") Properties.Settings.Default.encode = "_utf8";
                }else Properties.Settings.Default.encode = val;
            },()=>Properties.Settings.Default.encode.StartsWith("_") ? "no" : Properties.Settings.Default.encode},
            {"guess-compile",Properties.Resources.CMDLINE_GUESS_COMPILE + "[-]",val => Properties.Settings.Default.guessLaTeXCompile = (val != null),()=>Properties.Settings.Default.guessLaTeXCompile},
            {"num=",Properties.Resources.CMDLINE_NUM,(int val) => {Properties.Settings.Default.LaTeXCompileMaxNumber = val;},()=>Properties.Settings.Default.LaTeXCompileMaxNumber},
            {"resolution=",Properties.Resources.CMDLINE_RESOLUTION,(int val) => Properties.Settings.Default.resolutionScale = val,()=>Properties.Settings.Default.resolutionScale},
            {"left-margin=",Properties.Resources.CMDLINE_LEFT_MARGIN,(int val) => Properties.Settings.Default.leftMargin = val,()=>Properties.Settings.Default.leftMargin},
            {"top-margin=",Properties.Resources.CMDLINE_TOP_MARGIN,(int val) => Properties.Settings.Default.topMargin = val,()=>Properties.Settings.Default.topMargin},
            {"right-margin=",Properties.Resources.CMDLINE_RIGHT_MARGIN,(int val) => Properties.Settings.Default.rightMargin = val,()=>Properties.Settings.Default.rightMargin},
            {"bottom-margin=",Properties.Resources.CMDLINE_BOTTOM_MARGIN,(int val) => Properties.Settings.Default.bottomMargin = val,()=>Properties.Settings.Default.bottomMargin},
            {"margins=",Properties.Resources.CMDLINE_MARGINS,val=>{
                var list = val.Split(new char[] { ' ' }).ToList();
                list.RemoveAll((s) => (s == ""));
                try {
                    if(list.Count == 1) Properties.Settings.Default.leftMargin = Properties.Settings.Default.topMargin = Properties.Settings.Default.rightMargin = Properties.Settings.Default.bottomMargin = Int32.Parse(list[0], enUS);
                    else if(list.Count == 2) {
                        Properties.Settings.Default.leftMargin = Properties.Settings.Default.rightMargin = Int32.Parse(list[0], enUS);
                        Properties.Settings.Default.topMargin = Properties.Settings.Default.bottomMargin = Int32.Parse(list[1], enUS);
                    } else if(list.Count == 4) {
                        Properties.Settings.Default.leftMargin = Int32.Parse(list[0], enUS);
                        Properties.Settings.Default.topMargin = Int32.Parse(list[1], enUS);
                        Properties.Settings.Default.rightMargin = Int32.Parse(list[2], enUS);
                        Properties.Settings.Default.bottomMargin = Int32.Parse(list[3], enUS);
                    }else throw new Mono.Options.OptionException(Properties.Resources.CMDLINE_ERROR_INVALID_MARGIN,"margins");
                }
                catch(FormatException e) {
                    throw new Mono.Options.OptionException(e.Message,"margins");
                }
            } },
            {"unit=",Properties.Resources.CMDLINE_UNIT,val => {
                switch(val){
                case "bp": Properties.Settings.Default.yohakuUnitBP = true; return;
                case "px": Properties.Settings.Default.yohakuUnitBP = false; return;
                default: throw new Mono.Options.OptionException(Properties.Resources.CMDLINE_ERROR_INVALID_UNIT, "unit");
                }
            },()=>Properties.Settings.Default.yohakuUnitBP ? "bp" : "px"},
            {"keep-page-size",Properties.Resources.CMDLINE_KEEP_PAGE_SIZE + "[-]",val=>Properties.Settings.Default.keepPageSize=(val != null),()=>Properties.Settings.Default.keepPageSize},
            {"pagebox=",val=>Properties.Settings.Default.pagebox = GetStringsFromArray("pagebox",val,new string[]{"media","crop","bleed","trim","art"})},// hidden
            {"merge-output-files",Properties.Resources.CMDLINE_MERGE_OUTPUT_FILES + "[-]",val=>Properties.Settings.Default.mergeOutputFiles = (val != null),()=>Properties.Settings.Default.mergeOutputFiles},
            {"animation-delay=",Properties.Resources.CMDLINE_ANIMATION_DELAY,(double sec)=>{Properties.Settings.Default.animationDelay = (uint)(sec*100);},()=>Properties.Settings.Default.animationDelay/100m},
            {"animation-loop=",Properties.Resources.CMDLINE_ANIMATION_LOOP,(uint val)=>Properties.Settings.Default.animationLoop = val,()=>Properties.Settings.Default.animationLoop},
            {"background-color=",Properties.Resources.CMDLINE_BACKGROUND_COLOR,val=> {
                if(val.ToLower() == "transparent") {
                    Properties.Settings.Default.transparentPngFlag = true;
                }
                try {
                    Properties.Settings.Default.backgroundColor =  System.Drawing.ColorTranslator.FromHtml(val);
                    return;
                }catch(Exception) { }
                try {
                    Properties.Settings.Default.backgroundColor =  System.Drawing.ColorTranslator.FromHtml("#" + val);
                    return;
                }catch(Exception) { }
                var list = val.Split(new char[] { ' ' }).ToList();
                list.RemoveAll((s) => (s == ""));
                if(list.Count == 1)throw new Mono.Options.OptionException(String.Format(Properties.Resources.INVALID_COLOR_NAME,val),"background-color");
                try {
                    if(list.Count != 3)throw new Exception(Properties.Resources.INVALID_INPUT);
                    Properties.Settings.Default.backgroundColor = System.Drawing.Color.FromArgb(0,
                        Int32.Parse(list[0], enUS),Int32.Parse(list[1], enUS),Int32.Parse(list[2], enUS));
                }
                catch(Exception e) {throw new Mono.Options.OptionException(e.Message,"background-color"); }
            },()=> {var c = Properties.Settings.Default.backgroundColor;
                var r = String.Format("{0,2:X2}{1,2:X2}{2,2:X2}",c.R,c.G,c.B);
                if(c.IsNamedColor)r += " (" + c.Name + ")";
                if(Properties.Settings.Default.transparentPngFlag)r += " " + Properties.Resources.TRANSPARENT + " or " + Properties.Resources.WHITE;
                return r; } },
            {"transparent",Properties.Resources.CMDLINE_TRANSPARENT + "[-]",val=>Properties.Settings.Default.transparentPngFlag= (val != null) ,()=>Properties.Settings.Default.transparentPngFlag},
            { "with-text",Properties.Resources.CMDLINE_WITH_TEXT + "[-]",val =>Properties.Settings.Default.outlinedText = !(val != null),()=>!Properties.Settings.Default.outlinedText},
            {"delete-display-size",Properties.Resources.CMDLINE_DELETE_DISPLAY_SIZE + "[-]",val => Properties.Settings.Default.deleteDisplaySize = (val != null),()=>Properties.Settings.Default.deleteDisplaySize},
            {"antialias",Properties.Resources.CMDLINE_ANTIALIAS + "[-]",val => Properties.Settings.Default.useMagickFlag = (val != null),()=>Properties.Settings.Default.useMagickFlag},
            {"low-resolution",Properties.Resources.CMDLINE_LOW_RESOLUTION + "[-]",val => Properties.Settings.Default.useLowResolution = (val!= null),()=>Properties.Settings.Default.useLowResolution},
            {"ignore-errors",Properties.Resources.CMDLINE_IGNORE_ERRORS + "[-]",val => Properties.Settings.Default.ignoreErrorFlag = (val != null),()=>Properties.Settings.Default.ignoreErrorFlag},
            {"delete-tmpfiles",Properties.Resources.CMDLINE_DELETE_TMPFILES + "[-]",val => Properties.Settings.Default.deleteTmpFileFlag = (val != null),()=>Properties.Settings.Default.deleteTmpFileFlag},
            {"preview",Properties.Resources.CMDLINE_PREVIEW + "[-]",val => preview = (val != null),()=>preview==null?false:preview.Value},
            {"embed-source",Properties.Resources.CMDLINE_EMBED_SOURCE + "[-]",val => Properties.Settings.Default.embedTeXSource = (val != null),()=>Properties.Settings.Default.embedTeXSource},
            {"copy-to-clipboard",Properties.Resources.CMDLINE_COPY_TO_CLIPBOARD + "[-]",val =>  Properties.Settings.Default.setFileToClipBoard = (val != null),()=>Properties.Settings.Default.setFileToClipBoard},
            {"workingdir=",Properties.Resources.CMDLINE_WORKINGDIR,val=> Properties.Settings.Default.workingDirectory = GetStringsFromArray("workingdir", val, new string[] { "tmp","file","current" }),()=>Properties.Settings.Default.workingDirectory },
            {"savesettings",Properties.Resources.CMDLINE_SAVESETTINGS + "[-]",val => Properties.Settings.Default.SaveSettings = (val != null),()=>Properties.Settings.Default.SaveSettings},
            {"quiet",Properties.Resources.CMDLINE_QUIET + "[-]",val => quiet = (val != null),()=>quiet},
            {"timeout=",Properties.Resources.CMDLINE_TIMEOUT, (int val) => {
                if(val <= 0) throw new Mono.Options.OptionException(Properties.Resources.CMDLINE_ERROR_INVALID_TIMEOUT, "timeout");
                Properties.Settings.Default.timeOut = val * 1000;
            },()=>Properties.Settings.Default.timeOut/1000 + " " + Properties.Resources.SECONDS},
            // TODO: defaultに対応するオプションがあった方がよい？
            {"batch=",Properties.Resources.CMDLINE_BATCH, val => {
                switch(val) {
                case "nonstop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.NonStop; break;
                case "stop": Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.Stop; break;
                default: throw new Mono.Options.OptionException(Properties.Resources.CMDLINE_ERROR_INVALID_BATCHTYPE, "batch");
                }
            },()=>{
                switch(Properties.Settings.Default.batchMode){
                case Properties.Settings.BatchMode.NonStop: return"nonstop";
                case Properties.Settings.BatchMode.Stop: return "stop";
                default: return "deafult";
            }}},
            {"exit",Properties.Resources.CMDLINE_EXIT, val => {exit = true;}},
            {"load-defaults",Properties.Resources.CMDLINE_LOAD_DEFAULTS,val => {if(val != null)Properties.Settings.Default.ReloadDefaults();}},
            {"help",Properties.Resources.CMDLINE_HELP,val => {help = true;}},
            {"version",Properties.Resources.CMDLINE_VERSION,val => {version = true;}},
            {"language=", "Language (system/ja/en)",val => {
                switch(val) {
                    case "system": Properties.Settings.Default.language = "";break;
                    case "ja": Properties.Settings.Default.language = "ja-JP";break;
                    case "en": Properties.Settings.Default.language = "en-US";break;
                    default:throw new Mono.Options.OptionException("","language");
                }
                Properties.Settings.SetLanguage(Properties.Settings.Default.language);
            } }
        };
        }

        static string GetStringsFromArray(string optionname, string val, string[] possibleargs) {
            if (possibleargs.Contains(val)) return val;
            throw new Mono.Options.OptionException(String.Format(Properties.Resources.NOTPOSSIBLEARG, String.Join(", ", possibleargs)), optionname);
        }

        static int TeX2imgMain(List<string> cmds) {
            Properties.Settings.Default.SaveSettings = !nogui;
            // オプション解析
            List<string> files;
            var options = GetOptiontSet();
            try { files = options.Parse(cmds); }
            catch (Mono.Options.OptionException e) {
                if (e.OptionName != null) {
                    var msg = String.Format(Properties.Resources.INVALID_INPUT_TO_OPTION, e.OptionName);
                    if (e.Message != "") msg += " : " + e.Message;
                    msg += "\n" + String.Format(Properties.Resources.SEEHELPMSG, "TeX2img" + (nogui ? "c" : "") + ".exe /help");
                    if (nogui) Console.WriteLine(msg);
                    else MessageBox.Show(msg, "TeX2img");
                }
                return -1;
            }
            // 各種バイナリのパスが設定されていなかったら推測する。
            // "/exit"が指定されている場合はメッセージ表示をしない。
            setPath(exit);
            if (help) {
                ShowHelp();
                return 0;
            }
            if (version) {
                ShowVersion();
                return 0;
            }
            // CUIモードの引数なしはエラー
            if (nogui && files.Count == 0) {
                Console.WriteLine(Properties.Resources.NOARGUMENT + "\n");
                ShowHelp();
                return -1;
            }

            if (preview != null) Properties.Settings.Default.previewFlag = (bool)preview;
            //Console.WriteLine(preview == null ? "null" : preview.ToString());

            // すぐに終了
            if (exit) {
                Properties.Settings.Default.Save();
                return 0;
            }
            // filesのチェック
            string err = "";
            for (int i = 0; i < files.Count / 2; ++i) {
                var chkconv = new Converter(null, files[2 * i], files[2 * i + 1]);
                if (!chkconv.CheckInputFormat()) {
                    err += String.Format(Properties.Resources.INVALID_EXTENSION, files[2 * i]) + "\n";
                }
                if (!File.Exists(files[2 * i])) {
                    err += String.Format(Properties.Resources.NOTEXIST, files[2 * i]);
                    if (files[2 * i].StartsWith("-") || files[2 * i].StartsWith("/")) err += Properties.Resources.MAY_BE_OPTION;
                    err += "\n";
                }
                if (!chkconv.CheckFormat()) {
                    err += String.Format(Properties.Resources.INVALID_EXTENSION, files[2 * i + 1]) + "\n";
                }
            }
            if (files.Count % 2 != 0) {
                err += String.Format(Properties.Resources.NOOUTPUT_FILE, files[files.Count - 1]);
                if (files[files.Count - 1].StartsWith("-") || files[files.Count - 1].StartsWith("/")) err += Properties.Resources.MAY_BE_OPTION;
                err += "\n";
            }
            if (err != "") {
                err = err.Remove(err.Length - 1);// 最後の改行を削除
                if (nogui) Console.WriteLine(err);
                else MessageBox.Show(err, "TeX2img");
                return -2;
            }

            if (nogui) {
                // CUIでオプション指定がないときは，設定によらずプレビューをしないようにする。
                if (preview == null) {
                    preview = Properties.Settings.Default.previewFlag;
                    Properties.Settings.Default.previewFlag = false;
                }
                int r = CUIExec(quiet, files);
                Properties.Settings.Default.previewFlag = (bool)preview;
                return r;
            } else {
                Application.Run(new MainForm(files));
                return 0;
            }
        }

        [STAThread]
        static void Main() {
            // アップデートしていたら前バージョンの設定を読み込む
            if (!Properties.Settings.Default.IsUpgraded) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.TeX2imgVersion = Application.ProductVersion;
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // ロケール
            Properties.Settings.SetLanguage(Properties.Settings.Default.language);

            // コマンドライン解析
            var cmds = new List<string>(Environment.GetCommandLineArgs());
            //cmds = new List<string> { "TeX2img.exe", "/nogui", "--margins=\"a b c d\"","test.tex","test.pdf" };
            // 一つ目がTeX2img本体ならば削除
            // abtlinstからCreateProcessで呼び出すとTeX2img本体にならなかったので，一応確認をする。
            if (cmds.Count > 0) {
                string filecmds0 = Path.GetFileName(cmds[0]).ToLower();
                string me = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToLower();
#if DEBUG
                string vshost = Path.Combine(Path.GetDirectoryName(me), Path.GetFileNameWithoutExtension(me) + ".vshost.exe").ToLower();
                if (vshost == filecmds0 || vshost == filecmds0 + ".exe") filecmds0 = me;
#endif
                if (filecmds0 == me || filecmds0 + ".exe" == me) cmds.RemoveAt(0);
            }

            // 二つ目でCUIモードか判定する。
            if (cmds.Count > 0 && (cmds[0] == "/nogui" || cmds[0] == "-nogui" || cmds[0] == "--nogui")) {
                nogui = true;
                cmds.RemoveAt(0);
            }
            var chkfiles = new List<string>() { "pdfiumdraw.exe", "mudraw.exe" };
            if (!nogui) chkfiles.Add("Azuki.dll");
            string mydir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            chkfiles = chkfiles.Where(f => !File.Exists(Path.Combine(mydir, f))).ToList();
            string chkfile_errmsg = null;
            if (chkfiles.Count != 0) {
                chkfile_errmsg = Properties.Resources.NOSYSTEMFILE + "\n" + String.Join("\n", chkfiles.ToArray());
            }
            if (chkfile_errmsg != null) {
                if (nogui) Console.WriteLine(chkfile_errmsg);
                else MessageBox.Show(chkfile_errmsg, "TeX2img");
                Environment.ExitCode = -3;
                return;
            }

            // GUIか否かで設定の変更をする
            if (!nogui) {
                //Properties.Settings.Default.batchMode = Properties.Settings.BatchMode.NonStop;
                Properties.Settings.Default.timeOut = 0;
            }
            // メイン
#if DEBUG
            int exitcode;
            exitcode = TeX2imgMain(cmds);
#else
            var exitcode = TeX2imgMain(cmds);
#endif
            Properties.Settings.Default.Save();
            Environment.ExitCode = exitcode;
        }

        static void setPath(bool nomsg) {
            if (Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                if (Properties.Settings.Default.platexPath == "") Properties.Settings.Default.platexPath = Properties.Settings.Default.GuessPlatexPath();
                if (Properties.Settings.Default.dvipdfmxPath == "") Properties.Settings.Default.dvipdfmxPath = Properties.Settings.Default.GuessDvipdfmxPath();
                if (Properties.Settings.Default.gsPath == "") Properties.Settings.Default.gsPath = Properties.Settings.Default.GuessGsPath();
                if (Properties.Settings.Default.platexPath == "" || Properties.Settings.Default.dvipdfmxPath == "" || Properties.Settings.Default.gsPath == "") {
                    if (!nomsg) {
                        var msg = Properties.Resources.FAIL_INIT_PATH;
                        if (nogui) Console.WriteLine(msg + "\n");
                        else MessageBox.Show(msg, "TeX2img");
                        (new SettingForm()).ShowDialog();
                    }
                } else {
                    if (!nomsg) {
                        var msg = String.Format(Properties.Resources.INIT_PATH, Properties.Settings.Default.platexPath, Properties.Settings.Default.dvipdfmxPath, Properties.Settings.Default.gsPath);
                        if (nogui) Console.WriteLine(msg + "\n");
                        else MessageBox.Show(msg, "TeX2img");
                    }
                }
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.gsDevice == "" && Properties.Settings.Default.gsPath != "") {
                Properties.Settings.Default.gsDevice = Properties.Settings.Default.GuessGsdevice();
                if (Properties.Settings.Default.gsDevice == "") Properties.Settings.Default.gsDevice = "epswrite";
                Properties.Settings.Default.Save();
            }
        }

        // CUIモード
        static int CUIExec(bool q, List<string> files) {
            IOutputController Output = new CUIOutput(q);
            if (files.Count == 0) {
                Console.WriteLine(Properties.Resources.NOINPUTFILE);
                return -5;
            }
            try {
                Directory.CreateDirectory(Path.GetTempPath());
            }
            catch (Exception) {
                Console.WriteLine(String.Format(Properties.Resources.FAIL_TMPFOLDER, Path.GetTempPath()));
                return -7;
            }

            int failnum = 0;

            var outFiles = new System.Collections.Specialized.StringCollection();
            for (int i = 0; i < files.Count / 2; ++i) {
                string file = Path.GetFullPath(files[2 * i]);
                string dir;
                if (Properties.Settings.Default.workingDirectory == "file") dir = Path.GetDirectoryName(file);
                else if (Properties.Settings.Default.workingDirectory == "current") dir = Directory.GetCurrentDirectory();
                else dir = Path.GetTempPath();
                string tmpTeXFileName = TempFilesDeleter.GetTempFileName(Path.GetExtension(file), dir);
                if (tmpTeXFileName == null) {
                    Console.WriteLine(String.Format(Properties.Resources.FAIL_TMPFILE, Path.GetTempPath()));
                    return -6;
                }
                tmpTeXFileName = Path.Combine(dir, tmpTeXFileName);
                // 一時フォルダにコピー
                File.Copy(file, tmpTeXFileName, true);
                (new FileInfo(tmpTeXFileName)).Attributes = FileAttributes.Normal;
                var output = Path.GetFullPath(files[2 * i + 1]);
                // 変換！
                try {
                    using (var converter = new Converter(Output, tmpTeXFileName, output)) {
                        converter.AddInputPath(Path.GetDirectoryName(file));
                        if (!converter.Convert()) ++failnum;
                        else outFiles.AddRange(converter.OutputFileNames.ToArray());
                    }
                    if (Properties.Settings.Default.setFileToClipBoard) Clipboard.SetFileDropList(outFiles);
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return failnum;
        }

        static void ShowVersion() {
            var msg = Application.ProductName + " Version " + Application.ProductVersion;
            if (nogui) Console.WriteLine(msg);
            else MessageBox.Show(msg, "TeX2img");
        }

        static void ShowHelp() {
            StringWriter sw = new StringWriter();
            var options = GetOptiontSet();
            options.WriteOptionDescriptions(sw);
            var msg = Properties.Resources.USAGE + ": TeX2img" + (nogui ? "c" : "") + ".exe [Options] Input Output\n\n" + sw.ToString();
            //if(msg.EndsWith("\n")) msg = msg.Remove(msg.Length - 1);
            if (nogui) Console.WriteLine(msg);
            else MessageBox.Show(msg, "TeX2img");
        }
    }
}
