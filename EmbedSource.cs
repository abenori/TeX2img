using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TeX2img {
    public static class EmbedSource {
        // ADS名
        public const string ADSName = "TeX2img.source";
        public static string mudraw = Path.Combine(Converter.GetToolsPath(), "mudraw.exe");
        public const string PDFsrcHead = "%%TeX2img Document";

        public static void Embed(string file, string text) {
            // Alternative Data Streamにソースを書き込む
            try {
                using (var fs = AlternativeDataStream.WriteAlternativeDataStream(file, ADSName))
                using (var ws = new StreamWriter(fs, new UTF8Encoding(false))) {
                    ws.Write(text);
                }
            }
            // 例外は無視
            catch (IOException) { }
            catch (NotImplementedException) { }
            var ext = Path.GetExtension(file);
            if (ExtraEmbed.ContainsKey(ext)) ExtraEmbed[ext](file, text);
        }

        public static string Read(string file) {
            try {
                using (var fs = AlternativeDataStream.ReadAlternativeDataStream(file, ADSName))
                using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception) {
                var ext = Path.GetExtension(file).ToLower();
                if (ExtraRead.ContainsKey(ext)) {
                    return ExtraRead[ext](file);
                } else throw;
            }
        }

        static Dictionary<string, Action<string, string>> ExtraEmbed = new Dictionary<string, Action<string, string>>() {
            { ".pdf",PDFEmbed }
        };
        static Dictionary<string, Func<string, string>> ExtraRead = new Dictionary<string, Func<string, string>>() {
            { ".pdf",PDFRead }
        };

        static void PDFEmbed(string file,string text) {
            var tmp = Converter.GetTempFileName(".pdf", Path.GetDirectoryName(file));
            tmp = Path.Combine(Path.GetDirectoryName(file), tmp);
            using (var mupdf = new MuPDF(mudraw)) { 
                var doc = (int)mupdf.Execute("open_document", typeof(int), file);
                if (doc == 0) return;
                var page = (int)mupdf.Execute("load_page", typeof(int), doc, 0);
                if (page == 0) return;
                var annot = (int)mupdf.Execute("create_annot", typeof(int), page, "Text");
                if (annot == 0) return;
                mupdf.Execute("set_annot_contents", annot, PDFsrcHead + System.Environment.NewLine + text);
                mupdf.Execute("set_annot_flag",annot, 35);
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
                var page = (int)mupdf.Execute("load_page", typeof(int), doc,0);
                if (page == 0) return null;
                var annot = (int)mupdf.Execute("first_annot", typeof(int), page);
                while(annot != 0) {
                    var rect = mupdf.Execute("bound_annot",
                        new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) },
                        annot);
                    if(((int)rect[0] ==  (int)rect[2]) && ((int)rect[1] == (int)rect[3])) { 
                        if((string)mupdf.Execute("annot_type",typeof(string),annot) == "Text") {
                            var text = (string)mupdf.Execute("annot_contents", typeof(string), annot);
                            text = MainForm.ChangeReturnCode(text);
                            if(text.StartsWith(srcHead)) {
                                return text.Substring(srcHead.Length);
                            }
                        }
                    }
                    annot = (int)mupdf.Execute("next_annot", typeof(int), annot);
                }
            }
            return null;
        }

    }
}
