using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace TeX2img {
    public class AnalyzeLaTeXCompile {
        public enum Program { LaTeX, Bibtex, MakeIndex, Done };

        static string[] exts = { ".aux", ".toc", ".lot", ".lof" };
        Dictionary<string, byte[]> aux = new Dictionary<string, byte[]>();
        string FileName, FileNameWithoutExt;
        bool forcelatex, bibtex, makeindex, firstlatex;
        public bool UseBibtex = true, UseMakeIndex = true;
        public AnalyzeLaTeXCompile(string path) {
            FileName = path;
            FileNameWithoutExt = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            forcelatex = bibtex = makeindex = false;
            firstlatex = true;
            foreach (var ext in exts) aux[ext] = null;
        }

        public Program Check() {
            if (forcelatex) {
                forcelatex = false;
                return Program.LaTeX;
            }
            if (LaTeXCheck()) return Program.LaTeX;
            if (UseMakeIndex) {
                if (MakeIndexCheck()) {
                    forcelatex = true;
                    return Program.MakeIndex;
                }
            }
            if (UseBibtex) {
                if (BibTeXCheck()) {
                    forcelatex = true;
                    return Program.Bibtex;
                }
            }
            return Program.Done;
        }

        bool MakeIndexCheck() {
            if (makeindex) return false;
            if (File.Exists(FileNameWithoutExt + ".idx")) return true;
            else return false;
        }

        bool BibTeXCheck() {
            if (!aux.ContainsKey(".aux") || aux[".aux"] == null) return false;
            var bibcite = new System.Text.RegularExpressions.Regex("\\\\bibcite\\{(.*?)\\}");
            var citation = new System.Text.RegularExpressions.Regex("\\\\citation\\{(.*?)\\}");
            var bibs = new List<string>();
            var cits = new List<string>();
            bool existbibdata = false;
            Encoding[] encs = KanjiEncoding.GuessKajiEncoding(aux[".aux"]);
            Encoding enc;
            if (encs.Length == 0) enc = Encoding.GetEncoding("shift_jis");
            else enc = encs[0];
            using (var fs = new StringReader(enc.GetString(aux[".aux"]))) {
                while (true) {
                    string line = fs.ReadLine();
                    if (line == null) break;
                    if (!existbibdata && line.IndexOf("\\bibdata{") != -1) existbibdata = true;
                    var m = bibcite.Match(line);
                    if (m.Success) bibs.Add(m.Groups[1].Value);
                    m = citation.Match(line);
                    if (m.Success) cits.Add(m.Groups[1].Value);
                }
            }
            if (!existbibdata) return false;
            cits.Sort(); cits = new List<string>(cits.Distinct());
            bibs.Sort(); bibs = new List<string>(bibs.Distinct());
            return !(cits.SequenceEqual(bibs));
        }

        // 0 = null, 空 = 1, \relaxのみ = 2, \relax+\gdef\@abspage@last = 3, それ以外 - 4
        int AuxStatus(byte[] aux) {
            if (aux == null) return 0;
            var lines = System.Text.Encoding.UTF8.GetString(aux).Replace("\r\n", "\n").Replace("\r", "\n").Split(new char[] { '\n' });
            if (lines.Length == 0) return 1;
            lines[0] = lines[0].TrimEnd();
            if (lines[0] != "\\relax") return 4;
            if (lines.Length == 1) return 2;
            lines[1] = lines[1].TrimEnd();
            int possible_status;
            if (lines[1] == "") possible_status = 2;
            else if (System.Text.RegularExpressions.Regex.Match(lines[1], "\\\\gdef *\\\\@abspage@last\\{[0-9]+\\}").Success) possible_status = 3;
            else return 4;
            for (int i = 2; i < lines.Length; ++i) {
                if (lines[i].TrimEnd() != "") return 4;
            }
            return possible_status;
        }

        bool LaTeXCheck() {
            bool rv = false;
            // 前回.auxがない -> 二回目実行するかを判断中．
            foreach (var ext in exts) {
                byte[] buf;
                ReadAUXFile(ext, out buf);
                if (!rv) {
                    if (ext == ".aux") {
                        int old_status = AuxStatus(aux[".aux"]);
                        int new_status = AuxStatus(buf);
                        if (new_status == 4) {
                            if (old_status == 4) {
                                if (!buf.SequenceEqual(aux[ext])) rv = true;
                            }else rv = true;
                        }else if(new_status == 1 || new_status == 2 || new_status == 3) {
                            if (old_status == 4) rv = true;
                        }
                    } else {
                        if (buf != null) {
                            if (aux[ext] == null) rv = true;
                            else if (!buf.SequenceEqual(aux[ext])) rv = true;
                        } else if (aux[ext] != null) {
                            rv = true;
                        }
                    }
                }
                aux[ext] = buf;
            }
            if (firstlatex) {
                firstlatex = false;
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
