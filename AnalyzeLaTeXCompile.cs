using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace TeX2img {
    public class AnalyzeLaTeXCompile {
        public enum Program { LaTeX, Bibtex, MakeIndex, Done };

        static string[] exts = {".aux",".toc",".lot",".lof"};
        Dictionary<string, byte[]> aux = new Dictionary<string,byte[]>();
        string FileName, FileNameWithoutExt;
        bool forcelatex, bibtex, makeindex;
        public bool UseBibtex = true, UseMakeIndex = true;
        public AnalyzeLaTeXCompile(string path) {
            FileName = path;
            FileNameWithoutExt = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            forcelatex = bibtex = makeindex = false;
            foreach(var ext in exts) aux[ext] = null;
        }

        public Program Check() {
            if(forcelatex) {
                forcelatex = false;
                return Program.LaTeX;
            }
            if(LaTeXCheck()) return Program.LaTeX;
            if(UseMakeIndex) {
                if(MakeIndexCheck()) {
                    forcelatex = true;
                    return Program.MakeIndex;
                }
            }
            if(UseBibtex) {
                if(BibTeXCheck()) {
                    forcelatex = true;
                    return Program.Bibtex;
                }
            }
            return Program.Done;
        }

        bool MakeIndexCheck() {
            if(makeindex) return false;
            if(File.Exists(FileNameWithoutExt + ".idx")) return true;
            else return false;
        }

        bool BibTeXCheck(){
            if(!aux.ContainsKey(".aux") || aux[".aux"] == null) return false;
            var bibcite = new System.Text.RegularExpressions.Regex("\\\\bibcite\\{(.*?)\\}");
            var citation = new System.Text.RegularExpressions.Regex("\\\\citation\\{(.*?)\\}");
            var bibs = new List<string>();
            var cits = new List<string>();
            bool existbibdata = false;
            Encoding[] encs = KanjiEncoding.GuessKajiEncoding(aux[".aux"]);
            Encoding enc;
            if(encs.Length == 0) enc = Encoding.GetEncoding("shift_jis");
            else enc = encs[0];
            using(var fs = new StringReader(enc.GetString(aux[".aux"]))) {
                while(true) {
                    string line = fs.ReadLine();
                    if(line == null) break;
                    if(!existbibdata && line.IndexOf("\\bibdata{") != -1) existbibdata = true;
                    var m = bibcite.Match(line);
                    if(m.Success) bibs.Add(m.Groups[1].Value);
                    m = citation.Match(line);
                    if(m.Success) cits.Add(m.Groups[1].Value);
                }
            }
            if(!existbibdata)return false;
            cits.Sort(); cits = new List<string>(cits.Distinct());
            bibs.Sort(); bibs = new List<string>(bibs.Distinct());
            return !(cits.SequenceEqual(bibs));
        }

        bool LaTeXCheck() {
            bool rv = false;
            foreach(var ext in exts) {
                byte[] buf;
                ReadAUXFile(ext, out buf);
                if(!rv) {
                    if(buf != null) {
                        if(aux[ext] == null) rv = true;
                        else if(!buf.SequenceEqual(aux[ext])) rv = true;
                    } else if(aux[ext] != null) rv = true;
                }
                aux[ext] = buf;
            }
            // 初回であると判定する
            if(aux[".aux"] == null) {
                aux[".aux"] = System.Text.Encoding.ASCII.GetBytes("\\relax \n");
                return true;
            }
            return rv;
        }
        
        void ReadAUXFile(string ext, out byte[] buf) {
            try {
                using(var fs = new FileStream(FileNameWithoutExt + ext, FileMode.Open, FileAccess.Read)) {
                    buf = new byte[fs.Length];
                    fs.Read(buf, 0, (int) fs.Length);
                }
            }
            catch { buf = null; }
        }
    }
}
