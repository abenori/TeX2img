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
            if (ExtraEmbed.ContainsKey(ext)) {
                try { ExtraEmbed[ext](file, text); }
                catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.Data); }
            }

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
            /*
            try {
                using (var fs = AlternativeDataStream.ReadAlternativeDataStream(file, ADSName))
                using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception) { }*/
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
            { ".pdf",PDFEmbed },
        };
        static Dictionary<string, Func<string, string>> ExtraRead = new Dictionary<string, Func<string, string>>() {
            { ".pdf",PDFRead },
        };

        static void PDFEmbed(string file, string text) {
            var tmpdir = Path.GetTempPath();
//            tmpdir = @"C:\Users\Abe_Noriyuki\Desktop";
            using (var tempFileDeleter = new TempFilesDeleter(tmpdir)) {
                var targettmp = TempFilesDeleter.GetTempFileName(".pdf", tmpdir);
                try { File.Copy(file, Path.Combine(tmpdir,targettmp)); }
                catch(Exception e) { return; }
                tempFileDeleter.AddFile(targettmp);
                var tmp = TempFilesDeleter.GetTempFileName(".tex", tmpdir);
                tmp = Path.Combine(tmpdir, tmp);
                //tempFileDeleter.AddTeXFile(tmp);
                using (var fw = new StreamWriter(tmp)) {
                    fw.WriteLine(@"\pdfoutput=1\relax");
                    fw.WriteLine(
                        @"{\catcode`\^^J=12\relax\catcode`\^^M=12\relax\catcode`^=12\catcode`\%=12\relax" + 
                        @"\xdef\teximgannot{\pdfannot width 0pt height 0pt depth 0pt" +
                        @"{/Subtype /Text /F 35 /Contents (\pdfescapestring{\detokenize{" + ChangeReturnCode(PDFsrcHead + System.Environment.NewLine + text,"\n") + @"}})}}}%"
                    );
                    fw.WriteLine(@"\newcount\pagecnt\pagecnt=1\relax
\newcount\totalpage
\pdfximage{" + targettmp + @"}\totalpage =\pdflastximagepages
\advance\totalpage by 1\relax
\loop
\pdfximage page \the\pagecnt {" + targettmp + @"}
\setbox0 =\hbox{\ifnum\pagecnt = 1\relax\teximgannot\fi\pdfrefximage\pdflastximage}%
\pdfhorigin = 0pt\relax
\pdfvorigin = 0pt\relax
\pdfpagewidth =\wd0\relax
\pdfpageheight =\ht0\relax
\shipout\box0
\advance\pagecnt by 1
\ifnum\pagecnt <\totalpage\repeat
\bye"
                        );
                }
                using(var proc = new System.Diagnostics.Process()) {
                    string arg;
                    proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(tmp);
                    proc.StartInfo.FileName = Converter.setProcStartInfo(Properties.Settings.Default.pdftexPath, out arg);
                    //                    proc.StartInfo.Arguments = arg + " -no-shell-escape -interaction=nonstopmode \"" + Path.GetFileName(tmp) + "\"";
                    proc.StartInfo.Arguments = arg + " -no-shell-escape  \"" + Path.GetFileName(tmp) + "\"";
                    try { proc.Start(); }
                    catch (Exception) { return; }
                    proc.WaitForExit(5000);
                }
                var tmppdf = Path.ChangeExtension(tmp, ".pdf");
                if (File.Exists(tmppdf)) {
                    File.Delete(file);
                    File.Move(tmppdf, file);
                }
            }
        }

        static string PDFRead(string file) {
            var srcHead = PDFsrcHead + System.Environment.NewLine;
            var tmpdir = Path.GetTempPath();
            using (var tempFileDeleter = new TempFilesDeleter(tmpdir)) {
                var targettmp = TempFilesDeleter.GetTempFileName(".pdf");
                tempFileDeleter.AddFile(targettmp);
                try { File.Copy(file, Path.Combine(tmpdir, targettmp)); }
                catch (Exception) { return null; }
                var targetpre = Path.GetFileNameWithoutExtension(targettmp);
                using (var proc = new System.Diagnostics.Process()) {
                    proc.StartInfo.FileName = Path.Combine(Converter.GetToolsPath(), "pdfiumdraw.exe");
                    proc.StartInfo.Arguments = "--output-text-annots --output=\"" + targetpre + "-%d.txt\" \"" + targettmp + "\"";
                    proc.StartInfo.WorkingDirectory = tmpdir;
                    try { proc.Start(); }
                    catch (Exception) { return null; }
                }
                int i = 1;
                string rv = null;
                while (File.Exists(Path.Combine(tmpdir, targetpre + "-" + i.ToString() + ".txt"))){
                    string annot_txt_file = Path.Combine(tmpdir, targetpre + "-" + i.ToString() + ".txt");
                    tempFileDeleter.AddFile(annot_txt_file);
                    if (rv == null) {
                        using (var fr = new StreamReader(Path.Combine(tmpdir, targetpre + "-" + i.ToString() + ".txt"))) {
                            var text = ChangeReturnCode(fr.ReadToEnd());
                            if (text.StartsWith(srcHead)) {
                                return text.Substring(srcHead.Length);
                            }
                        }
                    }
                    ++i;
                }
                return rv;
            }
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
