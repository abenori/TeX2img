﻿using System;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest")]

namespace TeX2img.Properties {
    // このクラスでは設定クラスでの特定のイベントを処理することができます:
    //  SettingChanging イベントは、設定値が変更される前に発生します。
    //  PropertyChanged イベントは、設定値が変更された後に発生します。
    //  SettingsLoaded イベントは、設定値が読み込まれた後に発生します。
    //  SettingsSaving イベントは、設定値が保存される前に発生します。
    internal sealed partial class Settings {
        static Settings() {
            SystemDefaultCaltureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        }
        public Settings() {
            // // 設定の保存と変更のイベント ハンドラーを追加するには、以下の行のコメントを解除します:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // SettingChangingEvent イベントを処理するコードをここに追加してください。
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // SettingsSaving イベントを処理するコードをここに追加してください。
        }

        void SetValues() {
            editorFontColor["テキスト"] = new FontColor(editorNormalColorFont, editorNormalColorBack);
            editorFontColor["選択範囲"] = new FontColor(editorSelectedColorFont, editorSelectedColorBack);
            editorFontColor["コントロールシークエンス"] = new FontColor(editorCommandColorFont, editorCommandColorBack);
            editorFontColor["$"] = new FontColor(editorEquationColorFont, editorEquationColorBack);
            editorFontColor["中 / 大括弧"] = new FontColor(editorBracketColorFont, editorBracketColorBack);
            editorFontColor["コメント"] = new FontColor(editorCommentColorFont, editorCommentColorBack);
            editorFontColor["改行，EOF"] = new FontColor(editorEOFColorFont, editorNormalColorBack);
            editorFontColor["対応する括弧"] = new FontColor(editorMatchedBracketColorFont, editorMatchedBracketColorBack);
            editorFontColor["空白"] = new FontColor(editorSpaceColorFont, editorNormalColorBack);
            if(preambleTemplateCollection == null) {
                preambleTemplates = GetDefaultTemplate();
                preambleTemplateCollection = DictionaryToStringCollection(preambleTemplates);
            } else preambleTemplates = StringCollectionToDictionary(preambleTemplateCollection);
        }

        protected override void OnSettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e) {
            SetValues();
            base.OnSettingsLoaded(sender, e);
        }

        protected override void OnSettingsSaving(object sender, System.ComponentModel.CancelEventArgs e) {
            editorNormalColorFont = editorFontColor["テキスト"].Font;
            editorNormalColorBack = editorFontColor["テキスト"].Back;
            editorSelectedColorFont = editorFontColor["選択範囲"].Font;
            editorSelectedColorBack = editorFontColor["選択範囲"].Back;
            editorCommandColorFont = editorFontColor["コントロールシークエンス"].Font;
            editorCommandColorBack = editorFontColor["コントロールシークエンス"].Back;
            editorEquationColorFont = editorFontColor["$"].Font;
            editorEquationColorBack = editorFontColor["$"].Back;
            editorBracketColorFont = editorFontColor["中 / 大括弧"].Font;
            editorBracketColorBack = editorFontColor["中 / 大括弧"].Back;
            editorCommentColorFont = editorFontColor["コメント"].Font;
            editorCommentColorBack = editorFontColor["コメント"].Back;
            editorEOFColorFont = editorFontColor["改行，EOF"].Font;
            editorMatchedBracketColorFont = editorFontColor["対応する括弧"].Font;
            editorMatchedBracketColorBack = editorFontColor["対応する括弧"].Back;
            editorSpaceColorFont = editorFontColor["空白"].Font;
            preambleTemplateCollection = DictionaryToStringCollection(preambleTemplates);
            base.OnSettingsSaving(sender, e);
        }

        public override void Save() {
            if (SaveSettings) base.Save();
        }

        public void ReloadDefaults() {
            var me = this.GetType();
            var properties = me.GetProperties();
            foreach(var property in properties) {
                var defaults = System.Attribute.GetCustomAttributes(property, typeof(System.Configuration.DefaultSettingValueAttribute));
                foreach(var def in defaults) {
                    var valattr = def as System.Configuration.DefaultSettingValueAttribute;
                    var val = System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType).ConvertFrom(valattr.Value);
                    property.SetValue(this, val, null);
                }
            }
            preambleTemplateCollection = null;
            SetValues();
            //SaveSettings = true;
            batchMode = BatchMode.Default;
            timeOut = 10000;
        }

        #region TeX関連パスの推定
        static string FindPathWithHint(string hint, string preamble, string def) {
            var m = (new System.Text.RegularExpressions.Regex("%.*?(" + hint + "):(.*)$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase)).Match(preamble);
            string name;
            if(m.Success) name = m.Groups[2].Value.Trim();
            else name = "";
            if(name == "") name = def;
            var r = name.IndexOf(" ");
            if(r == -1) return Converter.which(name);
            else return "\"" + Converter.which(name.Substring(0, r)) + "\"" + name.Substring(r);
        }
        public string GuessPlatexPath(string hint) {
            string def = "latex";
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "ja-JP") def = "platex";
            return GuessPlatexPath(hint, def);
        }
        public string GuessPlatexPath(string hint,string def) {
            return FindPathWithHint("compiler|latex", hint, def);
        }
        public string GuessPlatexPath() {
			return GuessPlatexPath(preamble);
        }
        public string GuessDvipdfmxPath(string hint,string def = "dvipdfmx") {
            return FindPathWithHint("dvi *driver|dviware", hint, def);
        }
        public string GuessDvipdfmxPath() {
			return GuessDvipdfmxPath(preamble);
        }
        public string GuessGsPath() {
            return GuessGsPath(platexPath);
        }
        public string GuessGsPath(string platex) {
            string gs = "";
            try {
                if (platex.IndexOf("texlive") != -1) {
                    gs = System.IO.Path.GetDirectoryName(platex) + "\\rungs.exe";
                    if (System.IO.File.Exists(gs)) return gs;
                    gs = "";
                }
            }
            catch (Exception) { }
            if(gs == "") {
                gs = Converter.which("gswin32c.exe");
                if(gs == "") {
                    gs = Converter.which("gswin64c.exe");
                    if(gs == "") {
                        gs = Converter.which("rungs.exe");
                        if(gs == "") {
                            if(platex != "") {
                                try {
                                    gs = System.IO.Path.GetDirectoryName(platex) + "\\rungs.exe";
                                    if (!System.IO.File.Exists(gs)) gs = "";
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            return gs;
        }
        public string GuessPdftexPath() {
            return GuessPdftexPath(platexPath);
        }
        public string GuessPdftexPath(string platex) {
            var dir = Path.GetDirectoryName(platex);
            if(dir != "") {
                var pdftex = Path.Combine(dir, "pdftex.exe");
                if (System.IO.File.Exists(pdftex)) return pdftex;
            }
            return Converter.which("pdftex.exe");
        }

        public string GuessGsdevice() {
            return GuessGsdevice(gsPath);
        }

        public string GuessGsdevice(string gs) {
            string gsdevice = "";
            var enUS = new System.Globalization.CultureInfo("en-US");
            // Ghostscriptのバージョンを取得する．
            using(var proc = new System.Diagnostics.Process()) {
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = gs;
                proc.StartInfo.Arguments = "-v";
                //string errmsg = "";
                proc.ErrorDataReceived += ((s, e) => {});
                try {
                    proc.Start();
                    proc.BeginErrorReadLine();
                    string msg = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(2000);
                    proc.CancelErrorRead();
                    if(!proc.HasExited) proc.Kill();
                    var reg = new System.Text.RegularExpressions.Regex("Ghostscript ([0-9]+)\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var m = reg.Match(msg);
                    if(m.Success) {
                        int major = int.Parse(m.Groups[1].Value, enUS);
                        int minor = int.Parse(m.Groups[2].Value, enUS);
                        //System.Diagnostics.Debug.WriteLine("major = " + major.ToString() + ", minor = " + minor.ToString());
                        // 9.15以上ならばeps2write，そうでないならepwsrite
                        if(major > 9 || (major == 9 && minor >= 15)) gsdevice = "eps2write";
                        else gsdevice = "epswrite";
                    }
                }
                catch(System.InvalidOperationException) { }
                catch(System.FormatException) { }
                catch(System.ComponentModel.Win32Exception) { }
            }
            return gsdevice;
        }
        #endregion

        public class FontColor {
            public System.Drawing.Color Font { get; set; }
            public System.Drawing.Color Back { get; set; }
            public FontColor(System.Drawing.Color f, System.Drawing.Color b) { Font = f; Back = b; }
        }

        public class FontColorCollection : System.Collections.Generic.Dictionary<string, FontColor> {
            public new FontColor this[string key] {
                get {
                    if((key == "改行，EOF" || key == "空白" )&& base.ContainsKey(key)) base[key].Back = base["テキスト"].Back;
                    return base[key];
                }
                set {
                    base[key] = value;
                    if((key == "改行，EOF" || key == "空白") && base.ContainsKey(key)) base[key].Back = base["テキスト"].Back;
                }
            }
        }

        public FontColorCollection editorFontColor = new FontColorCollection();
        public bool SaveSettings = true;
        // Default：問い合わせて終了，NonStop：停まらない，Stop：強制終了
        public enum BatchMode { Default, NonStop, Stop };
        public BatchMode batchMode = BatchMode.Default;
        public int timeOut = 10000;//ミリ秒，0以下なら無限大
        public int[] ColorDialogCustomColors = new int[16]{
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF};

        public System.Collections.Generic.Dictionary<string, string> preambleTemplates;
        static System.Collections.Generic.Dictionary<string, string> StringCollectionToDictionary(System.Collections.Specialized.StringCollection sc) {
            var rv = new System.Collections.Generic.Dictionary<string, string>();
            if(sc.Count % 2 != 0) throw new System.IO.InvalidDataException("Broken dictionary");
            for(var i = 0 ; i < sc.Count ; i += 2) rv.Add(sc[i], sc[i + 1]);
            return rv;
        }
        static System.Collections.Specialized.StringCollection DictionaryToStringCollection(System.Collections.Generic.Dictionary<string, string> dic) {
            var rv = new System.Collections.Specialized.StringCollection();
            foreach(var d in dic) {
                rv.Add(d.Key);
                rv.Add(d.Value);
            }
            return rv;
        }
        public static System.Collections.Generic.Dictionary<string, string> GetDefaultTemplate() {
            var rv = new System.Collections.Generic.Dictionary<string, string>();

            #region デフォルトテンプレート
            bool ja = (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "ja-JP");
if (ja) { 
rv["pLaTeX"] = @"%latex: platex
%dvidriver: dvipdfmx
\documentclass[fleqn,papersize,dvipdfmx]{jsarticle}
\usepackage{amsmath,amssymb}
\usepackage{xcolor}
\pagestyle{empty}
";

rv["upLaTeX"] = @"%latex: uplatex
%dvidriver: dvipdfmx
\documentclass[fleqn,papersize,uplatex,dvipdfmx]{jsarticle}
\usepackage{amsmath,amssymb}
\usepackage{xcolor}
\pagestyle{empty}
";
}

rv["pdfLaTeX"] = @"%latex: pdflatex
\documentclass[fleqn]{article}
\usepackage{amsmath,amssymb}
\usepackage{xcolor}
\pagestyle{empty}
";

if (ja) {
rv["XeLaTeX（和文）"] = @"%latex: xelatex
\documentclass[fleqn]{bxjsarticle}
\usepackage{zxjatype}
\usepackage{amsmath,amssymb}
\usepackage{xcolor}
\pagestyle{empty}
";

rv["LuaLaTeX（和文）"] = @"%latex: lualatex
\documentclass[fleqn]{ltjsarticle}
\usepackage{amsmath,amssymb}
\usepackage{xcolor}
\pagestyle{empty}
";
}
rv["LaTeX + dvips"] = @"%latex: latex
%dvidriver: dvips
\documentclass[fleqn,dvips]{article}
\usepackage{amsmath,amssymb}
\usepackage{graphicx,xcolor}
\pagestyle{empty}
";

if (ja) { 
rv["pLaTeX + dvips"] = @"%latex: platex
%dvidriver: dvips
\documentclass[fleqn,papersize,dvips]{jsarticle}
\usepackage{amsmath,amssymb}
\usepackage{graphicx,color}
\pagestyle{empty}
";

rv["upLaTeX + dvips"] = @"%latex: uplatex
%dvidriver: dvips
\documentclass[fleqn,papersize,uplatex,dvips]{jsarticle}
\usepackage{amsmath,amssymb}
\usepackage{graphicx,color}
\pagestyle{empty}
";
}
            #endregion
            return rv;
        }

        public static System.Globalization.CultureInfo SystemDefaultCaltureInfo;
        public static System.Globalization.CultureInfo CurrentCulture;
        public static void SetLanguage(string lang) {
            if (lang == "") {
                System.Threading.Thread.CurrentThread.CurrentCulture = SystemDefaultCaltureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = SystemDefaultCaltureInfo;
                CurrentCulture = SystemDefaultCaltureInfo;
            } else {
                var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
                foreach(var c in cultures) {
                    if (c.Name == lang) {
                        System.Threading.Thread.CurrentThread.CurrentUICulture = c;
                        System.Threading.Thread.CurrentThread.CurrentCulture = c;
                        CurrentCulture = c;
                        return;
                    }
                }
            }
        }
        public static void SetCurrentLanguage() {
            if (CurrentCulture != null) {
                System.Threading.Thread.CurrentThread.CurrentCulture = CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = CurrentCulture;
            }
        }
    }
}
