using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TeX2img {
    public static class TeXSource {
        // ADS名
        public const string ADSName = "TeX2img.source";
        public static string mudraw = Path.Combine(Converter.GetToolsPath(), "mudraw.exe");
        public const string PDFsrcHead = "%%TeX2img Document";
        public const string MacTeX2imgName = "com.loveinequality.TeX2img";

        public static bool ParseTeXSourceFile(TextReader file, out string preamble, out string body) {
            preamble = null; body = null;
            var reg = new Regex(@"(?<preamble>^(.*\n)*?[^%]*?(\\\\)*)\\begin\{document\}\n?(?<body>(.*\n)*[^%]*)\\end\{document\}");
            var text = ChangeReturnCode(file.ReadToEnd(), "\n");
            var m = reg.Match(text);
            if (m.Success) {
                preamble = ChangeReturnCode(m.Groups["preamble"].Value);
                body = ChangeReturnCode(m.Groups["body"].Value);
                return true;
            } else {
                body = ChangeReturnCode(text);
                return true;
            }
        }
        public static string ChangeReturnCode(string str) {
            return ChangeReturnCode(str, System.Environment.NewLine);
        }
        public static string ChangeReturnCode(string str, string returncode) {
            string r = str;
            r = r.Replace("\r\n", "\n");
            r = r.Replace("\r", "\n");
            r = r.Replace("\n", returncode);
            return r;
        }

        public static void WriteTeXSourceFile(TextWriter sw, string preamble, string body) {
            sw.Write(ChangeReturnCode(preamble));
            if (!preamble.EndsWith("\n")) sw.WriteLine("");
            sw.WriteLine("\\begin{document}");
            sw.Write(ChangeReturnCode(body));
            if (!body.EndsWith("\n")) sw.WriteLine("");
            sw.WriteLine("\\end{document}");
        }

        #region ソース埋め込み
        public static void EmbedSource(string file, string text) {
            // 拡張子毎の処理（あれば）
            var ext = Path.GetExtension(file);
            if (ExtraEmbed.ContainsKey(ext)) ExtraEmbed[ext](file, text);

            // Alternative Data Streamにソースを書き込む
            try {
                using (var fs = AlternativeDataStream.WriteAlternativeDataStream(file, ADSName))
                using (var ws = new StreamWriter(fs, new UTF8Encoding(false))) {
                    ws.Write(text);
                }
            }
            // 例外は無視
            catch (IOException e) { System.Diagnostics.Debug.WriteLine(e.Data); }
            catch (NotImplementedException e) { System.Diagnostics.Debug.WriteLine(e.Data); }
        }

        public static string ReadEmbededSource(string file) {
            try {
                using (var fs = AlternativeDataStream.ReadAlternativeDataStream(file, ADSName))
                using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception) { }
            try {
                var ext = Path.GetExtension(file).ToLower();
                if (ExtraRead.ContainsKey(ext)) {
                    var s = ExtraRead[ext](file);
                    if (s != null) return s;
                }
            }
            catch (Exception) { }
            try { return ReadAppleDouble(file); }
            catch (Exception) { }
            return null;
        }

        static Dictionary<string, Action<string, string>> ExtraEmbed = new Dictionary<string, Action<string, string>>() {
            { ".pdf",PDFEmbed }
        };
        static Dictionary<string, Func<string, string>> ExtraRead = new Dictionary<string, Func<string, string>>() {
            { ".pdf",PDFRead }
        };

        static void PDFEmbed(string file,string text) {
            var tmpdir = Path.GetTempPath();
            var tmp = Converter.GetTempFileName(".pdf", tmpdir);
            tmp = Path.Combine(tmpdir, tmp);
            using (var mupdf = new MuPDF(mudraw)) { 
                var doc = (int)mupdf.Execute("open_document", typeof(int), file);
                if (doc == 0) return;
                var page = (int)mupdf.Execute("load_page", typeof(int), doc, 0);
                if (page == 0) return;
                var annot = (int)mupdf.Execute("create_annot", typeof(int), page, "Text");
                if (annot == 0) return;
                mupdf.Execute("set_annot_contents", annot, PDFsrcHead + System.Environment.NewLine + text);
                mupdf.Execute("set_annot_flag", annot, 35);
                mupdf.Execute("write_document", doc, tmp);
            }
            if (File.Exists(tmp)) {
                File.Delete(file);
                File.Move(tmp, file);
            }
        }
        static string PDFRead(string file) {
            var srcHead = PDFsrcHead + System.Environment.NewLine;
            using (var mupdf = new MuPDF(mudraw)) {
                var doc = (int)mupdf.Execute("open_document", typeof(int), file);
                if (doc == 0) return null;
                var page = (int)mupdf.Execute("load_page", typeof(int), doc, 0);
                if (page == 0) return null;
                var annot = (int)mupdf.Execute("first_annot", typeof(int), page);
                while (annot != 0) {
                    var rect = mupdf.Execute("bound_annot",
                        new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) },
                        annot);
                    if (((int)rect[0] == (int)rect[2]) && ((int)rect[1] == (int)rect[3])) {
                        if ((string)mupdf.Execute("annot_type", typeof(string), annot) == "Text") {
                            var text = (string)mupdf.Execute("annot_contents", typeof(string), annot);
                            text = ChangeReturnCode(text);
                            if (text.StartsWith(srcHead)) {
                                return text.Substring(srcHead.Length);
                            }
                        }
                    }
                    annot = (int)mupdf.Execute("next_annot", typeof(int), annot);
                }
            }
            return null;
        }

        static string ReadAppleDouble(string file) {
            var dir = Path.GetDirectoryName(file);
            var doubleFile = "._" + Path.GetFileName(file);
            string f = null;
            var tmpf = Path.Combine(dir, doubleFile);
            if (File.Exists(tmpf)) f = tmpf;
            tmpf = Path.Combine(Path.Combine(dir, "__MACOSX"), doubleFile);
            if (File.Exists(tmpf)) f = tmpf;
            if (f != null) {
                var apd = new AppleDouble(f);
                foreach (var entry in apd.Entries) {
                    if (entry.ID == 9) {
                        var finfo = (AppleDouble.FinderInfo)entry;
                        foreach (var attr in finfo.Attrs) {
                            if (attr.Name == MacTeX2imgName) {
                                return ChangeReturnCode(Encoding.UTF8.GetString(attr.Data));
                            }
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
