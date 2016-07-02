using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TeX2img {
    class Converter : IDisposable {
        /* 空ページの扱い：
         * 生成されるEPSファイルはBoundingBoxが0 0 0 0かもしれない。
         * 変換に渡されるEPSファイルのBoundingBoxは必ず幅を持つようにする。
         */

        #region プロパティ，コンストラクタetc...
        // 拡張子たち
        public static readonly string[] bmpExtensions = new string[] { ".jpg", ".png", ".bmp", ".gif", ".tiff" };
        public static readonly string[] vectorExtensions = new string[] { ".eps", ".pdf", ".emf", ".wmf", ".svg", ".svgz" };
        public static string[] imageExtensions {
            get { return bmpExtensions.Concat(vectorExtensions).ToArray(); }
        }

        IOutputController controller_;
        string workingDir;
        string InputFile, OutputFile;// フルパス
        List<string> outputFileNames;// 出力されたファイル一覧
        public List<string> OutputFileNames { get { return outputFileNames; } }

        // 結果等々
        bool error_ignored = false;
        List<string> warnngs = new List<string>();
        // フルパスを入れる
        public Converter(IOutputController controller, string inputTeXFilePath, string outputFilePath) {
            InputFile = inputTeXFilePath;
            OutputFile = outputFilePath;
            controller_ = controller;
            workingDir = Path.GetDirectoryName(inputTeXFilePath);
            tempFilesDeleter = new TempFilesDeleter(workingDir);
        }
        Dictionary<string, string> Environments = new Dictionary<string, string>();
        ~Converter() {
            Dispose();
        }
        private TempFilesDeleter tempFilesDeleter;
        public void Dispose() {
            tempFilesDeleter.Dispose();
        }
        // 変換
        public bool Convert() {
            warnngs.Clear();
            error_ignored = false;
            if (GetInputEncoding().CodePage == Encoding.UTF8.CodePage) {
                Environments["command_line_encoding"] = "utf8";
            }
            tempFilesDeleter.AddTeXFile(Path.Combine(workingDir, Path.GetFileNameWithoutExtension(InputFile)));
            if (Path.GetExtension(InputFile).ToLower() != ".tex") {
                tempFilesDeleter.AddFile(Path.Combine(workingDir, InputFile));
            }
            bool rv = generate(InputFile, OutputFile);

            return rv;
        }
        #endregion

        #region BoundingBox関連
        void enlargeBB(string inputEpsFileName, bool use_bp = true) {
            Func<BoundingBox, BoundingBox> func = bb => AddMargineToBoundingBox(bb, use_bp);
            rewriteBB(inputEpsFileName, func, func);
        }

        void rewriteBB(string inputEpsFileName, Func<BoundingBox, BoundingBox> bb, Func<BoundingBox, BoundingBox> hiresbb) {
            Regex regexBB = new Regex(@"^\%\%(HiRes|)BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
            byte[] inbuf;
            using (var fs = new FileStream(Path.Combine(workingDir, inputEpsFileName), FileMode.Open, FileAccess.Read)) {
                if (!fs.CanRead) return;
                inbuf = new byte[fs.Length];
                fs.Read(inbuf, 0, (int)fs.Length);
            }
            var s = System.Text.UTF8Encoding.UTF8.GetString(inbuf);
            byte[] outbuf = new byte[inbuf.Length + 200];
            byte[] tmpbuf;

            // 現在読んでいるinufの「行」の先頭
            int inp = 0;
            // inbufの現在読んでいる場所
            int q = 0;
            // outbufに書き込んだ量
            int outp = 0;
            bool bbfound = false, hiresbbfound = false;
            while (q < inbuf.Length) {
                if (q == inbuf.Length - 1 || inbuf[q] == '\r' || inbuf[q] == '\n') {
                    string line = System.Text.Encoding.ASCII.GetString(inbuf, inp, q - inp);
                    Match match = regexBB.Match(line);
                    if (match.Success) {
                        BoundingBox bbinfile = new BoundingBox(
                            System.Convert.ToDecimal(match.Groups[2].Value),
                            System.Convert.ToDecimal(match.Groups[3].Value),
                            System.Convert.ToDecimal(match.Groups[4].Value),
                            System.Convert.ToDecimal(match.Groups[5].Value));
                        string HiRes = match.Groups[1].Value;
                        if (HiRes == "") {
                            bbfound = true;
                            var newbb = bb(bbinfile);
                            line = String.Format("%%BoundingBox: {0} {1} {2} {3}", (int)newbb.Left, (int)newbb.Bottom, (int)newbb.Right, (int)newbb.Top);
                        } else {
                            hiresbbfound = true;
                            var newbb = hiresbb(bbinfile);
                            line = String.Format("%%HiResBoundingBox: {0} {1} {2} {3}", newbb.Left, newbb.Bottom, newbb.Right, newbb.Top);
                        }
                        tmpbuf = System.Text.Encoding.ASCII.GetBytes(line);
                        System.Array.Copy(tmpbuf, 0, outbuf, outp, tmpbuf.Length);
                        outp += tmpbuf.Length;
                        if (bbfound && hiresbbfound) {
                            System.Array.Copy(inbuf, q, outbuf, outp, inbuf.Length - q);
                            outp += inbuf.Length - q;
                            break;
                        }
                    } else {
                        System.Array.Copy(inbuf, inp, outbuf, outp, q - inp);
                        outp += q - inp;
                    }
                    inp = q;
                    while (q < inbuf.Length - 1 && (inbuf[q] == '\r' || inbuf[q] == '\n')) ++q;
                    System.Array.Copy(inbuf, inp, outbuf, outp, q - inp);
                    outp += q - inp;
                    inp = q;
                    if (q == inbuf.Length - 1) break;
                } else ++q;
            }
            using (FileStream wfs = new System.IO.FileStream(Path.Combine(workingDir, inputEpsFileName), FileMode.Create, FileAccess.Write)) {
                var ss = System.Text.UTF8Encoding.UTF8.GetString(outbuf, 0, outp);
                wfs.Write(outbuf, 0, outp);
            }
        }

        private BoundingBoxPair readBB(string inputEpsFileName) {
            Regex regex = new Regex(@"^\%\%(HiRes)?BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
            BoundingBox? bb = null;
            BoundingBox? hiresbb = null;
            using (StreamReader sr = new StreamReader(Path.Combine(workingDir, inputEpsFileName), Encoding.GetEncoding("shift_jis"))) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    Match match = regex.Match(line);
                    if (match.Success) {
                        var cb = new BoundingBox(
                            System.Convert.ToDecimal(match.Groups[2].Value),
                            System.Convert.ToDecimal(match.Groups[3].Value),
                            System.Convert.ToDecimal(match.Groups[4].Value),
                            System.Convert.ToDecimal(match.Groups[5].Value));
                        if (match.Groups[1].Value == "HiRes") {
                            hiresbb = cb;
                        } else {
                            bb = cb;
                        }
                        if (bb != null && hiresbb != null) break;
                    }
                }
            }
            if (bb == null && hiresbb == null) return null;
            if (hiresbb == null) hiresbb = bb.Value;
            else if (bb == null) bb = hiresbb.Value.HiresBBToBB();
            return new BoundingBoxPair(bb.Value, hiresbb.Value);
        }

        List<BoundingBoxPair> readPDFBox(string inputPDFFileName, List<int> pages, string boxname) {
            try {
                using (var mupdf = new MuPDF(Path.Combine(GetToolsPath(), "mudraw.exe"))) {
                    if (controller_ != null) controller_.appendOutput("Getting the size of PDFBox...\n");
                    var rv = new List<BoundingBoxPair>();
                    var doc = mupdf.Execute<int>("open_document", Path.Combine(workingDir, inputPDFFileName));
                    if (doc == 0) return null;
                    foreach (var p in pages) {
                        if (abort) {
                            if (controller_ != null) controller_.appendOutput(Properties.Resources.STOPCONVERTMSG + "\n");
                            return null;
                        }
                        int rotate = 0;
                        BoundingBox box = new BoundingBox(), media = new BoundingBox();
                        const int repeatTimes = 10;
                        for (int i = 1; i <= repeatTimes; ++i) {
                            try {
                                var page = mupdf.Execute<int>("load_page", doc, p - 1);
                                media = mupdf.Execute< BoundingBox>("pdfbox_page", page, "media");
                                box = mupdf.Execute<BoundingBox>("pdfbox_page", page, boxname);
                                rotate = mupdf.Execute<int>("rotate_page", page);
                                break;
                            }
                            catch (Exception) {
                                mupdf.ClearError();
                                if (i == repeatTimes) throw;
                            }
                        }
                        BoundingBox bb;
                        switch (rotate) {
                        default:
                        case 0:
                            bb = new BoundingBox(box.Left - media.Left, box.Bottom - media.Bottom, box.Right - media.Left, box.Top - media.Bottom);
                            break;
                        case 90:
                            bb = new BoundingBox(box.Bottom - media.Bottom, media.Right - box.Right, box.Top - media.Bottom, media.Right - box.Left);
                            break;
                        case 180:
                            bb = new BoundingBox(media.Right - box.Right, media.Top - box.Top, media.Right - box.Left, media.Top - box.Bottom);
                            break;
                        case 270:
                            bb = new BoundingBox(media.Top - box.Top, box.Left - media.Left, media.Top - box.Bottom, box.Right - media.Left);
                            break;
                        }
                        if (controller_ != null) controller_.appendOutput(bb.ToString() + " (Page " + p + ")\n");
                        rv.Add(new BoundingBoxPair(bb.HiresBBToBB(), bb));
                    }
                    if (controller_ != null) controller_.appendOutput("\n");
                    return rv;
                }
            }
            catch (Exception e) {
                if (controller_ != null) controller_.appendOutput(e.Message + "\n");
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
#endif
                return null;
            }
        }

        BoundingBoxPair readPDFBB(string inputPDFFileName, int page) {
            var bbs = readPDFBB(inputPDFFileName, page, page);
            if (bbs != null) return bbs[0];
            else return new BoundingBoxPair();
        }

        List<BoundingBoxPair> readPDFBB(string inputPDFFileName, int firstpage, int lastpage) {
            System.Diagnostics.Debug.Assert(lastpage >= firstpage);
            string arg;
            var gspath = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = gspath;
                proc.StartInfo.Arguments = arg + "-q -dBATCH -dNOPAUSE -sDEVICE=bbox ";
                /*
                if(Properties.Settings.Default.pagebox != "media") {
                    var box = Properties.Settings.Default.pagebox;
                    proc.StartInfo.Arguments += "-dUse" + Char.ToUpper(box[0]) + box.Substring(1) + "Box ";
                }*/
                proc.StartInfo.Arguments += "-dFirstPage=" + firstpage.ToString() + " -dLastPage=" + lastpage.ToString() + " \"" + inputPDFFileName + "\"";
                var rv = new List<BoundingBoxPair>();
                Regex regexBB = new Regex(@"^\%\%(HiRes)?BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
                BoundingBox? bb = null;
                BoundingBox? hiresbb = null;
                Action<string> err_read = line => {
                    if (controller_ != null) controller_.appendOutput(line + "\n");
                    var match = regexBB.Match(line);
                    if (match.Success) {
                        var currentbb = new BoundingBox(
                            System.Convert.ToDecimal(match.Groups[2].Value),
                            System.Convert.ToDecimal(match.Groups[3].Value),
                            System.Convert.ToDecimal(match.Groups[4].Value),
                            System.Convert.ToDecimal(match.Groups[5].Value));
                        if (match.Groups[1].Value == "HiRes") {
                            hiresbb = currentbb;
                        } else {
                            bb = currentbb;
                        }
                        if (bb != null && hiresbb != null) {
                            rv.Add(new BoundingBoxPair(bb.Value, hiresbb.Value));
                            bb = null; hiresbb = null;
                        }
                    }
                };

                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.GET_BOUNDINGBOX, s => { System.Diagnostics.Debug.WriteLine(s); }, err_read);
                    if (controller_ != null) controller_.appendOutput("\n");
                    if (rv.Count != lastpage - firstpage + 1) return null;
                    else return rv;
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError(gspath, "Ghostscript");
                    return null;
                }
                catch (TimeoutException) { return null; }
            }
        }

        BoundingBox AddMargineToBoundingBox(BoundingBox bb, bool use_bp) {
            decimal margindevide = use_bp ? 1 : Properties.Settings.Default.resolutionScale;
            return new BoundingBox(
                bb.Left - Properties.Settings.Default.leftMargin / margindevide,
                bb.Bottom - Properties.Settings.Default.bottomMargin / margindevide,
                bb.Right + Properties.Settings.Default.rightMargin / margindevide,
                bb.Top + Properties.Settings.Default.topMargin / margindevide);
        }

        BoundingBoxPair AddMargineToBoundingBox(BoundingBoxPair bb, bool use_bp) {
            return new BoundingBoxPair(AddMargineToBoundingBox(bb.bb, use_bp), AddMargineToBoundingBox(bb.hiresbb, use_bp));
        }
        #endregion

        #region TeX -> PDF
        // ファイル名はフルパスではなくファイル名のみで与える．
        private bool tex2dvi(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
            ProcessStartInfo startinfo = GetProcessStartInfo();
            startinfo.FileName = setProcStartInfo(Properties.Settings.Default.platexPath, out arg);
            if (Properties.Settings.Default.platexPath == "") {
                string latex = "pdflatex";
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "ja-JP") latex = "platex";
                if (controller_ != null) controller_.showPathError(latex, String.Format(Properties.Resources.TEX_DISTRIBUTION, latex));
                return false;
            }
            startinfo.Arguments = arg;
            //if(IspTeX(startinfo.FileName)) {
            if (Properties.Settings.Default.encode.Substring(0, 1) != "_") startinfo.Arguments += "-no-guess-input-enc -kanji=" + Properties.Settings.Default.encode + " ";
            //}
            startinfo.Arguments += "-interaction=nonstopmode " + baseName + ".tex";
            startinfo.StandardOutputEncoding = GetOutputEncoding();

            try {
                error_ignored = false;
                if (Properties.Settings.Default.guessLaTeXCompile) {
                    var analyzer = new AnalyzeLaTeXCompile(Path.Combine(workingDir, fileName));
                    analyzer.UseBibtex = analyzer.UseMakeIndex = false;
                    int i = 0;
                    while (analyzer.Check() != AnalyzeLaTeXCompile.Program.Done) {
                        using (var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            printCommandLine(proc);
                            ReadOutputs(proc, Properties.Resources.COMPILE_TEX);
                            if (proc.ExitCode != 0) {
                                if (!Properties.Settings.Default.ignoreErrorFlag) {
                                    if (controller_ != null) controller_.showGenerateError();
                                    return false;
                                } else {
                                    error_ignored = true;
                                }
                            }
                            ++i;
                            if (i == Properties.Settings.Default.LaTeXCompileMaxNumber) break;
                        }
                    }
                } else {
                    for (int i = 0; i < Properties.Settings.Default.LaTeXCompileMaxNumber; ++i) {
                        using (var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            printCommandLine(proc);
                            ReadOutputs(proc, Properties.Resources.COMPILE_TEX);
                            if (proc.ExitCode != 0) {
                                if (!Properties.Settings.Default.ignoreErrorFlag) {
                                    if (controller_ != null) controller_.showGenerateError();
                                    return false;
                                } else {
                                    error_ignored = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Win32Exception) {
                string latex = "pdflatex";
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "ja-JP") latex = "platex";
                if (controller_ != null) controller_.showPathError(startinfo.FileName, String.Format(Properties.Resources.TEX_DISTRIBUTION, latex));
                return false;
            }
            catch (TimeoutException) { return false; }

            return true;
        }

        private bool dvi2pdf(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.dvipdfmxPath, out arg);
                if (Properties.Settings.Default.dvipdfmxPath == "") {
                    if (controller_ != null) controller_.showPathError("dvipdfmx.exe", String.Format(Properties.Resources.TEX_DISTRIBUTION, "dvipdfmx"));
                    return false;
                }
                //proc.StartInfo.Arguments = arg + " -vv -o " + baseName + ".pdf " + baseName + ".dvi";
                proc.StartInfo.Arguments = arg + baseName + ".dvi";

                try {
                    // 出力は何故か標準エラー出力から出てくる
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.DVITOPDF);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError(proc.StartInfo.FileName, String.Format(Properties.Resources.TEX_DISTRIBUTION, "dvipdfmx"));
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (proc.ExitCode != 0/* || !File.Exists(Path.Combine(workingDir, baseName + ".pdf"))*/) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Ghostscript使う系
        // origbbには，GhostscriptのsDevice=bboxで得られた値を入れておく。（nullならばここで取得する。）
        // versionはinputFileNameのpdfバージョン
        private bool pdf2eps(string inputFileName, string outputFileName, int resolution, int page, int version, BoundingBoxPair origbb = null) {
            string arg;
            tempFilesDeleter.AddFile(outputFileName);
            var tmppdf = TempFilesDeleter.GetTempFileName(".pdf", workingDir);
            tempFilesDeleter.AddFile(tmppdf);
            // あらかじめpdf2writeにかけておくと透明にちょっと強くなる
            if (!pdf2pdf(inputFileName, tmppdf, resolution, version, page)) return false;
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if (proc.StartInfo.FileName == "") {
                    if (controller_ != null) controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                if (arg != "") arg += " ";
                proc.StartInfo.Arguments = arg + "-dNOPAUSE -dBATCH -sDEVICE=" + Properties.Settings.Default.gsDevice + " -dFirstPage=1 -dLastPage=1";
                if (IsNewGhostscript()) proc.StartInfo.Arguments += " -dNoOutputFonts";
                else proc.StartInfo.Arguments += " -dNOCACHE";
                proc.StartInfo.Arguments += " -dEPSCrop -sOutputFile=\"" + outputFileName + "\" -r" + resolution + " \"" + tmppdf + "\"";

                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.PDFTOEPS);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError(Properties.Settings.Default.gsPath, "Ghostscript");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, outputFileName))) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
                // BoundingBoxをあらかじめ計測した物に取り替える。
                BoundingBoxPair bb;
                if (origbb == null) bb = readPDFBB(inputFileName, page);
                else bb = origbb;
                if (bb == null) return false;
                Func<BoundingBox, BoundingBox> bbfunc = (b) => bb.bb;
                Func<BoundingBox, BoundingBox> hiresbbfunc = (b) => bb.hiresbb;
                rewriteBB(outputFileName, bbfunc, hiresbbfunc);
            }
            return true;
        }

        bool ps2pdf(string filename, string output, int version = -1) {
            return gs_pdfwrite("\"" + filename + "\"", output, "", Properties.Resources.PSTOPDF, 0, version, "");
        }

        bool eps2pdf(List<string> filename, string output, int resolution, int version) {
            return gs_pdfwrite(String.Join(" ", filename.Select(d => "\"" + d + "\"").ToArray()), output, "-dEPSCrop", Properties.Resources.EPSTOPDF, resolution, version, "");
        }

        bool pdf2pdf(string input, string output, int resolution, int version, int page = 0) {
			string pageopt = "";
			if(page != 0) pageopt = " -dFirstPage=" + page.ToString() + " -dLastPage=" + page.ToString();
            return gs_pdfwrite("\"" + input + "\"", output, (IsNewGhostscript() ? "-dNoOutputFonts" : "-dNOCACHE") + pageopt, Properties.Resources.EXEC_GS, resolution, version, "");
        }

        bool dashtoline(string input, int version) {
            var tmppdf = TempFilesDeleter.GetTempFileName(".pdf", workingDir);
            tempFilesDeleter.AddFile(tmppdf);
            File.Move(Path.Combine(workingDir, input), Path.Combine(workingDir, tmppdf));
            return gs_pdfwrite("\"" + tmppdf + "\"", input, "", Properties.Resources.EXEC_GS, 0, version, "/oldstroke /stroke load def /stroke {.dashpath [] 0 setdash oldstroke} def");
        }

        bool gs_pdfwrite(string input, string output, string option, string msg, int resolution, int version, string cmd) {
            using (var proc = GetProcess()) {
                string arg;
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if (proc.StartInfo.FileName == "") {
                    if (controller_ != null) controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                proc.StartInfo.Arguments = arg + "-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -dAutoRotatePages=/None ";
                if (version > 0) proc.StartInfo.Arguments += "-dCompatibilityLevel=1." + (version - 10).ToString() + " ";
                if (resolution > 0) proc.StartInfo.Arguments += "-r" + resolution.ToString() + " ";
                if (option != "") proc.StartInfo.Arguments += option + " ";
                proc.StartInfo.Arguments += "-sOutputFile=\"" + output + "\" -c \".setpdfwrite";
                if (cmd != "") proc.StartInfo.Arguments += " " + cmd;
                proc.StartInfo.Arguments += "\" -f " + input;
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, msg);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError(Properties.Settings.Default.gsPath, "Ghostscript ");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, output))) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
                return true;
            }
        }

        private bool pdf2img(string inputFileName, string outputFileName, int page = 0) {
            string device;
            switch (Path.GetExtension(outputFileName)) {
            case ".png":
                device = Properties.Settings.Default.transparentPngFlag ? "pngalpha" : "png16m";
                break;
            case ".bmp":
                device = "bmp16m";
                break;
            default:
                device = "jpeg";
                break;
            }
            return pdf2img(inputFileName, outputFileName, device, page);
        }

        bool pdf2img(string input, string output, string device, int page = 0) {
            using (var proc = GetProcess()) {
                tempFilesDeleter.AddFile(output);
                string arg;
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if (proc.StartInfo.FileName == "") {
                    if (controller_ != null) controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                if (arg != "") arg += " ";
                proc.StartInfo.Arguments += arg + "-dNOPAUSE -dBATCH -sDEVICE=" + device + " " +
                    ((page > 0) ? "-dFirstPage=" + page + " -dLastPage=" + page + " " : "") +
                    "-dTextAlphaBits=" + (Properties.Settings.Default.useMagickFlag ? "4" : "1") + " " +
                    "-r" + (72 * Properties.Settings.Default.resolutionScale).ToString() + " " +
                    "-sOutputFile=\"" + output + "\" \"" + input + "\"";
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_GS);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError(proc.StartInfo.FileName, "Ghostscript ");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (proc.ExitCode != 0) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
                return true;
            }
        }

        bool make_dummyeps(string filename) {
            try {
                using (var fw = new System.IO.StreamWriter(Path.Combine(workingDir, filename))) {
                    fw.WriteLine(@"%!PS-Adobe-3.0 EPSF-3.0");
                    fw.WriteLine(@"%%BoundingBox 0 0 10 10");
                }
            }
            catch (Exception) { return false; }
            return true;
        }
        #endregion

        #region pdftex
        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp, int page = 1, BoundingBoxPair origbb = null, int version = -1) {
            return pdfcrop(inputFileName, outputFileName, use_bp, new List<int>() { page }, new List<BoundingBoxPair>() { origbb }, true, true, true, version);
        }

        // origbbには，GhostscriptのsDevice=bboxで得られた値を入れておく。
        // 空ページはdeleteemptypages = trueならば消されるが，falseならばダミーのページが挿入される．
        // ついでに塗る．
        // crop = falseならば塗ることしかしない．
        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp, List<int> pages, List<BoundingBoxPair> origbb, bool drawback = true, bool deleteemptypages = false, bool crop = true, int version = -1) {
            var colorstr =
                ((double)Properties.Settings.Default.backgroundColor.R / 255).ToString() + " " +
                ((double)Properties.Settings.Default.backgroundColor.G / 255).ToString() + " " +
                ((double)Properties.Settings.Default.backgroundColor.B / 255).ToString();
            System.Diagnostics.Debug.Assert(pages.Count == origbb.Count);
            var tmpfile = TempFilesDeleter.GetTempFileName(".tex", workingDir);
            if (tmpfile == null) return false;
            tempFilesDeleter.AddTeXFile(tmpfile);
            tempFilesDeleter.AddFile(outputFileName);

            var bbBox = new List<BoundingBox>();
            for (int i = 0; i < pages.Count; ++i) {
                BoundingBoxPair bb;
                if (origbb[i] == null) {
                    bb = readPDFBB(inputFileName, pages[i]);
                } else {
                    bb = origbb[i];
                }
                if (bb == null) return false;
                var rect = AddMargineToBoundingBox(bb.hiresbb, use_bp);
                if (rect.IsEmpty && !deleteemptypages) rect = new BoundingBox(0, 0, 10, 10);// dummy
                bbBox.Add(rect);
            }
            using (var fw = new StreamWriter(Path.Combine(workingDir, tmpfile))) {
                fw.WriteLine(@"\pdfoutput=1\relax");
                if (version != -1) fw.WriteLine(@"\pdfminorversion=" + (version - 10).ToString() + @"\relax");
                for (int i = 0; i < pages.Count; ++i) {
                    var box = bbBox[i];
                    if (!box.IsEmpty) {
                        var page = pages[i];
                        fw.WriteLine(@"\pdfximage page " + page.ToString() + " mediabox{" + inputFileName + @"}");
                        fw.Write(@"\setbox0=\hbox{");
                        if (drawback) {
                            fw.Write(@"\pdfliteral{q " + colorstr + " rg n " +
                                box.Left.ToString() + " " + box.Bottom.ToString() + " " +
                                box.Width.ToString() + " " + box.Height.ToString() + " re f Q}");
                        }
                        fw.WriteLine(@"\pdfrefximage\pdflastximage}\relax");
                        if (crop) {
                            fw.WriteLine(@"\pdfhorigin=" + (-box.Left).ToString() + @"bp\relax");
                            fw.WriteLine(@"\pdfvorigin=" + box.Bottom.ToString() + @"bp\relax");
                            fw.WriteLine(@"\pdfpagewidth=" + (box.Right - box.Left).ToString() + @"bp\relax");
                            fw.WriteLine(@"\pdfpageheight=" + (box.Top - box.Bottom).ToString() + @"bp\relax");
                            fw.WriteLine(@"\ht0=\pdfpageheight\relax");
                        } else {
                            fw.WriteLine(@"\pdfhorigin=0pt\relax");
                            fw.WriteLine(@"\pdfvorigin=0pt\relax");
                            fw.WriteLine(@"\pdfpagewidth=\wd0\relax");
                            fw.WriteLine(@"\pdfpageheight=\ht0\relax");
                        }
                        fw.WriteLine(@"\shipout\box0\relax");
                    }
                }
                fw.WriteLine(@"\bye");
            }
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = GetpdftexPath();
                proc.StartInfo.Arguments = "-no-shell-escape -interaction=nonstopmode \"" + tmpfile + "\"";
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_PDFTEX);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showPathError("pdftex.exe", String.Format(Properties.Resources.TEX_DISTRIBUTION,"pdftex"));
                    return false;
                }
                catch (TimeoutException) { return false; }
            }
            if (File.Exists(Path.Combine(workingDir, Path.GetFileNameWithoutExtension(tmpfile) + ".pdf"))) {
                File.Delete(Path.Combine(workingDir, outputFileName));
                File.Move(Path.Combine(workingDir, Path.GetFileNameWithoutExtension(tmpfile) + ".pdf"), Path.Combine(workingDir, outputFileName));
            } else {
                if (controller_ != null) controller_.showGenerateError();
                return false;
            }
            return true;
        }
        #endregion

        #region 内部で変換
        bool png2img(string inputFileName, string outputFileName) {
            tempFilesDeleter.AddFile(outputFileName);
            System.Drawing.Imaging.ImageFormat format;
            var extension = Path.GetExtension(outputFileName).ToLower();
            switch (extension) {
            case ".png":
                format = System.Drawing.Imaging.ImageFormat.Png;
                break;
            case ".jpg":
            case ".jpeg":
                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                break;
            case ".gif":
                format = System.Drawing.Imaging.ImageFormat.Gif;
                break;
            case ".tif":
            case ".tiff":
                format = System.Drawing.Imaging.ImageFormat.Tiff;
                break;
            case ".bmp":
            default:
                format = System.Drawing.Imaging.ImageFormat.Bmp;
                break;
            }
            if (controller_ != null) controller_.appendOutput("TeX2img: Convert " + inputFileName + " to " + outputFileName + "\n");
            try {
                using (var bitmap = new System.Drawing.Bitmap(Path.Combine(workingDir, inputFileName))) {
                    if (Properties.Settings.Default.transparentPngFlag && extension != ".gif") {
                        bitmap.MakeTransparent();
                    }
                    bitmap.Save(Path.Combine(workingDir, outputFileName), format);
                }
                return true;
            }
            catch (FileNotFoundException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }
        #endregion

        #region mudraw系
        bool pdf2img_mudraw(string inputFileName, string outputFileName, int page = 0) {
            return pdf2img_mudraw(inputFileName, outputFileName, page == 0 ? new List<int>() : new List<int> { page });
        }

        bool pdf2img_mudraw(string inputFileName, string outputFileName, List<int> pages) {
            tempFilesDeleter.AddFile(outputFileName);
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "mudraw.exe");
                proc.StartInfo.Arguments = "-l -o \"" + outputFileName + "\" \"" + inputFileName + "\"";
                if (pages.Count > 0) proc.StartInfo.Arguments += " " + String.Join(",", pages.Select(d => d.ToString()).ToArray());
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_MUDRAW);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showToolError("mudraw.exe");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (outputFileName.Contains("%d")) {
                    var r = outputFileName.IndexOf("%d");
                    var pre = outputFileName.Substring(0, r);
                    var aft = outputFileName.Substring(r + 2);
                    if (pages.Count > 0) {
                        bool rv = true;
                        foreach (var p in pages) {
                            var f = Path.Combine(workingDir, pre + p.ToString() + aft);
                            if (File.Exists(f)) tempFilesDeleter.AddFile(f);
                            else rv = false;
                        }
                        return rv;
                    } else {
                        for (int i = 1; ; ++i) {
                            var f = Path.Combine(workingDir, pre + i.ToString() + aft);
                            if (File.Exists(f)) tempFilesDeleter.AddFile(f);
                            else break;
                        }
                    }
                } else {
                    if (!File.Exists(Path.Combine(workingDir, outputFileName))) {
                        if (controller_ != null) controller_.showToolError("mudraw.exe");
                        return false;
                    }
                }
                return true;
            }
        }

        void DeleteHeightAndWidthFromSVGFile(string svgFile) {
            var fullpath = Path.Combine(workingDir, svgFile);
            var xml = new System.Xml.XmlDocument();
            xml.XmlResolver = null;
            xml.Load(fullpath);
            foreach (System.Xml.XmlNode node in xml.GetElementsByTagName("svg")) {
                var attr = node.Attributes["width"];
                if (attr != null) node.Attributes.Remove(attr);
                attr = node.Attributes["height"];
                if (attr != null) node.Attributes.Remove(attr);
            }
            xml.Save(fullpath);
        }
        #endregion

        #region pdfiumdraw
        bool pdf2img_pdfium(string inputFilename, string outputFileName, int pages = 0) {
            return pdf2img_pdfium(inputFilename, outputFileName, pages == 0 ? null : new List<int> { pages });
        }

        bool pdf2img_pdfium(string inputFilename, string outputFileName, List<int> pages) {
            System.Diagnostics.Debug.Assert(pages == null || pages.Count > 0);
            var type = Path.GetExtension(outputFileName).Substring(1).ToLower();
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                if (type == "emf") {
                    proc.StartInfo.Arguments = "--extent=50 ";
                    if (!Properties.Settings.Default.transparentPngFlag)
                        proc.StartInfo.Arguments += "--backcolor=" + String.Format("{0:X2}{1:X2}{2:X2}",
                            Properties.Settings.Default.backgroundColor.R,
                            Properties.Settings.Default.backgroundColor.G,
                            Properties.Settings.Default.backgroundColor.B) + " ";
                } else if (type != "pdf") {
                    proc.StartInfo.Arguments = "--scale=" + Properties.Settings.Default.resolutionScale.ToString() + " ";
                }
                proc.StartInfo.Arguments +=
                    "--" + type + " " + (Properties.Settings.Default.transparentPngFlag ? "--transparent " : "") +
                    (pages != null ? "--pages=" + String.Join(",", pages.Select(i => i.ToString()).ToArray()) + " " : "") +
                    "--output=\"" + outputFileName + "\" \"" + inputFilename + "\"";
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_PDFIUMDRAW);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showToolError("pdfiumdraw.exe");
                    return false;
                }
                catch (TimeoutException) { return false; }
            }
            // 簡易チェック
            if (outputFileName.Contains("%d")) {
                var r = outputFileName.IndexOf("%d");
                var pre = outputFileName.Substring(0, r);
                var aft = outputFileName.Substring(r + 2);
                if (pages == null) {
                    for (int i = 1; ; ++i) {
                        var f = Path.Combine(workingDir, pre + i.ToString() + aft);
                        if (File.Exists(f)) tempFilesDeleter.AddFile(f);
                        else break;
                    }
                } else {
                    bool rv = true;
                    foreach (var p in pages) {
                        var f = Path.Combine(workingDir, pre + p.ToString() + aft);
                        tempFilesDeleter.AddFile(f);
                        if (!File.Exists(f)) rv = false;
                    }
                    if (!rv && controller_ != null) controller_.showGenerateError();
                    return rv;
                }
            } else {
                if (!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                } else tempFilesDeleter.AddFile(Path.Combine(workingDir, outputFileName));
            }
            return true;
        }

        bool pdf2pdf_pdfium(string input, string output, int page) {
            return pdf2pdf_pdfium(input, output, new List<int> { page });
        }

        bool pdf2pdf_pdfium(string input, string output, List<int> pagelist) {
            tempFilesDeleter.AddFile(output);
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                proc.StartInfo.Arguments = "--pdf --input-format=pdf --pages=" + String.Join(",", pagelist.Select(i => i.ToString()).ToArray()) +
                    " --output=\"" + output + "\" \"" + input + "\"";
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_PDFIUMDRAW);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showToolError("pdfiumdraw.exe");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (proc.ExitCode != 0) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
                return true;
            }
        }

        bool img2img_pdfium(string inputFileName, string outputFileName) {
            tempFilesDeleter.AddFile(outputFileName);
            var inputtype = Path.GetExtension(inputFileName).Substring(1).ToLower();
            var type = Path.GetExtension(outputFileName).Substring(1).ToLower();
            using (var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                proc.StartInfo.Arguments =
                    "--" + type + " --input-format=" + inputtype +
                    " --output=\"" + outputFileName + "\" \"" + inputFileName + "\"";
                try {
                    printCommandLine(proc);
                    ReadOutputs(proc, Properties.Resources.EXEC_PDFIUMDRAW);
                }
                catch (Win32Exception) {
                    if (controller_ != null) controller_.showToolError("pdfiumdraw.exe");
                    return false;
                }
                catch (TimeoutException) { return false; }
                if (!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    if (controller_ != null) controller_.showToolError("pdfiumdraw.exe");
                    return false;
                } else {
                    return true;
                }
            }
        }
        #endregion

        #region 画像結合
        // http://dobon.net/vb/dotnet/graphics/createmultitiff.html
        bool tiffconcat(List<string> files, string output) {
            tempFilesDeleter.AddFile(output);
            var Compression = System.Drawing.Imaging.EncoderValue.CompressionLZW;
            if (files.Count == 0) return true;
            if (files.Count == 1) {
                File.Copy(Path.Combine(workingDir, files[0]), Path.Combine(workingDir, output));
            } else {
                System.Drawing.Imaging.ImageCodecInfo ici = null;
                foreach (var enc in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()) {
                    if (enc.MimeType == "image/tiff") {
                        ici = enc;
                        break;
                    }
                }
                if (ici == null) {
                    if (controller_ != null) controller_.appendOutput(Properties.Resources.FAIL_CONCAT_IMAGES);
                    return false;
                }
                var bitmaps = files.Select(f => new System.Drawing.Bitmap(Path.Combine(workingDir, f))).ToList();
                try {
                    using (var ep = new System.Drawing.Imaging.EncoderParameters(2)) {
                        ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)System.Drawing.Imaging.EncoderValue.MultiFrame);
                        ep.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)Compression);
                        bitmaps[0].Save(Path.Combine(workingDir, output), ici, ep);
                    }
                    for (int i = 1; i < files.Count; ++i) {
                        using (var ep = new System.Drawing.Imaging.EncoderParameters(2)) {
                            ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)System.Drawing.Imaging.EncoderValue.FrameDimensionPage);
                            ep.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)Compression);
                            bitmaps[0].SaveAdd(bitmaps[i], ep);
                        }
                    }
                    using (var ep = new System.Drawing.Imaging.EncoderParameters(1)) {
                        ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)System.Drawing.Imaging.EncoderValue.Flush);
                        bitmaps[0].SaveAdd(ep);
                    }
                }
                finally {
                    foreach (var b in bitmaps) b.Dispose();
                }
            }

            if (File.Exists(Path.Combine(workingDir, output))) {
                if (controller_ != null) controller_.appendOutput("TeX2img: Concatinate TIFF files");
                return true;
            } else return false;
        }

        bool gifconcat(List<string> files, string output, uint delay, uint loop) {
            if (controller_ != null) controller_.appendOutput("TeX2img: Making GIF annimation...");
            tempFilesDeleter.AddFile(output);
            int width = 0;
            int height = 0;
            foreach (var f in files) {
                using (var bmp = new System.Drawing.Bitmap(Path.Combine(workingDir, f))) {
                    if (bmp.Width > width) width = bmp.Width;
                    if (bmp.Height > height) height = bmp.Height;
                }
            }
            using (var fw = new FileStream(Path.Combine(workingDir, output), FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fw)) {
                for (int i = 0; i < files.Count; ++i) {
                    using (var fr = new FileStream(Path.Combine(workingDir, files[i]), FileMode.Open, FileAccess.Read))
                    using (var reader = new BinaryReader(fr)) {
                        byte[] bytes = null;
                        byte b = 0;
                        bytes = reader.ReadBytes(13);//Global Color Tableの前まで
                        int ColorTableSize = 0;
                        if ((bytes[10] & 0x80) != 0) ColorTableSize = bytes[10] & 0x07;//(int) Math.Pow(2, bytes[10] & 0x07 + 1);
                        byte[] ColorTable = null;
                        // Global Color Tableの読み込み
                        if (ColorTableSize > 0) ColorTable = reader.ReadBytes(((int)Math.Pow(2, ColorTableSize)) * 3);
                        if (i == 0) {
                            bytes[4] = 0x39;// GIF89aを強制
                            var dimbytes = BitConverter.GetBytes(width);
                            bytes[6] = dimbytes[0]; bytes[7] = dimbytes[1];
                            dimbytes = BitConverter.GetBytes(height);
                            bytes[8] = dimbytes[0]; bytes[9] = dimbytes[1];
                            bytes[10] &= 0x78;// Global Color Tableを無効化
                            writer.Write(bytes);
                            // Netscape Apprication Extension
                            bytes = BitConverter.GetBytes(loop);
                            writer.Write(new byte[] {
                                0x21, 0xFF, 0x0B, (byte) 'N', (byte) 'E', (byte) 'T', (byte) 'S' ,
                                (byte)'C',(byte)'A',(byte)'P',(byte)'E',(byte)'2',(byte)'.',(byte)'0',
                                0x03,0x01,bytes[0],bytes[1],0x00});
                        }
                        // Graphic Control Extension
                        b = reader.ReadByte();
                        writer.Write((byte)0x21);
                        var delaybytes = BitConverter.GetBytes(delay);
                        if (b == 0x21) {
                            bytes = reader.ReadBytes(7);
                            if (bytes[0] != 0xF9) return false;
                            bytes[2] &= 0xE3; bytes[2] |= 0x08;// 処分方法を背景色塗りつぶしに変更
                            bytes[3] = delaybytes[0];
                            bytes[4] = delaybytes[1];
                            b = reader.ReadByte();
                        } else bytes = new byte[] { 0xF9, 0x04, 0x08, delaybytes[0], delaybytes[1], 0x00, 0x00 };
                        writer.Write(bytes);
                        // Image Descriptor
                        if (b != 0x2C) return false;
                        writer.Write(b);
                        bytes = reader.ReadBytes(9);
                        if (ColorTable == null && (bytes[8] & 0x80) == 0) return false;
                        if ((bytes[8] & 0x80) != 0) {
                            ColorTableSize = (bytes[8] & 7);
                            ColorTable = reader.ReadBytes(((int)Math.Pow(2, ColorTableSize)) * 3);
                        } else {
                            // Local Color Tableを使う指定とそのサイズを入れる
                            bytes[8] &= 0xF8;
                            bytes[8] = (byte)(bytes[8] | 0x80 | ColorTableSize);
                        }
                        writer.Write(bytes);
                        writer.Write(ColorTable);// Local Color Table
                        bytes = reader.ReadBytes((int)(fr.Length - fr.Position) - 1);// Trailer以外の残り
                        writer.Write(bytes);
                    }
                }
                writer.Write((byte)0x3B);// Trailer
            }
            if (controller_ != null) controller_.appendOutput(" done\n");
            return true;
        }

        bool svgconcat(List<string> files, string output, uint delay, uint loop) {
            if (controller_ != null) controller_.appendOutput("TeX2img: Making animation svg...");
            try {
                var outxml = new System.Xml.XmlDocument();
                outxml.XmlResolver = null;
                outxml.AppendChild(outxml.CreateXmlDeclaration("1.0", "utf-8", "no"));
                outxml.AppendChild(outxml.CreateDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null));
                var svg = outxml.CreateElement("svg", "http://www.w3.org/2000/svg");
                var attr = outxml.CreateAttribute("xmlns:xlink");
                attr.Value = "http://www.w3.org/1999/xlink";
                svg.Attributes.Append(attr);
                attr = outxml.CreateAttribute("version");
                attr.Value = "1.1";
                svg.Attributes.Append(attr);
                outxml.AppendChild(svg);
                var defs = outxml.CreateElement("defs", "http://www.w3.org/2000/svg");
                svg.AppendChild(defs);
                var idreg = new System.Text.RegularExpressions.Regex(@"(?<!\&)#");
                foreach (var f in files) {
                    var id = Path.GetFileNameWithoutExtension(f);
                    var xml = new System.Xml.XmlDocument();
                    xml.XmlResolver = null;
                    xml.Load(Path.Combine(workingDir, f));
                    foreach (System.Xml.XmlNode tag in xml.GetElementsByTagName("*")) {
                        foreach (System.Xml.XmlAttribute a in tag.Attributes) {
                            if (a.Name.ToLower() == "id") a.Value = id + "-" + a.Value;
                            else a.Value = idreg.Replace(a.Value, "#" + id + "-");
                        }
                    }
                    foreach (System.Xml.XmlNode tag in xml.GetElementsByTagName("svg")) {
                        var idattr = xml.CreateAttribute("id");
                        idattr.Value = id;
                        tag.Attributes.Append(idattr);
                    }
                    foreach (System.Xml.XmlNode n in xml.ChildNodes) {
                        if (n.NodeType != System.Xml.XmlNodeType.DocumentType && n.NodeType != System.Xml.XmlNodeType.XmlDeclaration) {
                            defs.AppendChild(outxml.ImportNode(n, true));
                        }
                    }
                }
                var use = outxml.CreateElement("use", "http://www.w3.org/2000/svg");
                svg.AppendChild(use);
                var animate = outxml.CreateElement("animate", "http://www.w3.org/2000/svg");
                use.AppendChild(animate);
                attr = outxml.CreateAttribute("attributeName");
                attr.Value = "xlink:href"; animate.Attributes.Append(attr);
                attr = outxml.CreateAttribute("begin");
                attr.Value = "0s"; animate.Attributes.Append(attr);
                attr = outxml.CreateAttribute("dur");
                attr.Value = ((decimal)(delay * files.Count) / 100).ToString() + "s";
                animate.Attributes.Append(attr);
                attr = outxml.CreateAttribute("repeatCount");
                attr.Value = loop > 0 ? loop.ToString() : "indefinite";
                animate.Attributes.Append(attr);
                attr = outxml.CreateAttribute("values");
                attr.Value = String.Join(";", files.Select(d => "#" + Path.GetFileNameWithoutExtension(d)).ToArray());
                animate.Attributes.Append(attr);
                outxml.Save(Path.Combine(workingDir, output));
                if (controller_ != null) controller_.appendOutput(" done\n");
                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        // 1 file1が生成，-1 file2が生成，0 生成に失敗
        static int IsGenerated(string file1, string file2) {
            if (!File.Exists(file1)) {
                if (!File.Exists(file2)) return 0;
                else return -1;
            } else {
                if (File.Exists(file2) && System.IO.File.GetLastWriteTime(file2) > System.IO.File.GetLastWriteTime(file1)) {
                    return -1;
                }
            }
            return 1;
        }

        // 変換の実体
        bool generate(string inputTeXFilePath, string outputFilePath) {
            abort = false;
            outputFileNames = new List<string>();
            string extension = Path.GetExtension(outputFilePath).ToLower();
            string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);
            string inputextension = Path.GetExtension(inputTeXFilePath).ToLower();
            // とりあえずPDFを作る
            int generated;
            if (inputextension == ".tex") {
                if (!tex2dvi(tmpFileBaseName + ".tex")) return false;
                generated = IsGenerated(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), Path.Combine(workingDir, tmpFileBaseName + ".dvi"));
                if (generated == 0) {
                    if (controller_ != null) controller_.showGenerateError();
                    return false;
                }
                if (generated == -1) {
                    if (!dvi2pdf(tmpFileBaseName + ".dvi")) return false;
                }
            }
            generated = IsGenerated(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), Path.Combine(workingDir, tmpFileBaseName + ".ps"));
            if (inputextension == ".ps" || inputextension == ".eps") {
                if (!ps2pdf(tmpFileBaseName + inputextension, tmpFileBaseName + ".pdf")) return false;
            } else if (generated == -1) {
                if (!ps2pdf(tmpFileBaseName + ".ps", tmpFileBaseName + ".pdf")) return false;
            }

            // ページ数を取得
            int page, version;
            if (!pdfinfo(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), out page, out version)) {
                controller_.showError(Properties.Resources.FAIL_PDFPAGES);
                return false;
            }

            // boundingBoxを取得
            var bbs = new List<BoundingBoxPair>();
            if (Properties.Settings.Default.keepPageSize) {
                bbs = readPDFBox(tmpFileBaseName + ".pdf", new List<int>(Enumerable.Range(1, page)), Properties.Settings.Default.pagebox);
            } else {
                bbs = readPDFBB(tmpFileBaseName + ".pdf", 1, page);
            }
            if (bbs == null) {
                if (!abort) controller_.showError(Properties.Resources.FAIL_GETBOUNDINGBOX);
                return false;
            }

            // 空白ページの検出
            var emptyPages = new List<int>();
            for (int i = 1; i <= page; ++i) {
                if (bbs[i - 1].bb.IsEmpty) {
                    if (Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin == 0 || Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin == 0) {
                        warnngs.Add(String.Format(Properties.Resources.SKIPPED_BACAUSE_EMPTY, i));
                        emptyPages.Add(i);
                    } else {
                        warnngs.Add(String.Format(Properties.Resources.EMPTYMSG, i));
                    }
                }
            }
            if (emptyPages.Count == page) {
                controller_.appendOutput(Properties.Resources.ALLPAGE_EMPTYMSG);
                return false;
            }
            var pagelist = Enumerable.Range(1, page).Where(i => !emptyPages.Contains(i)).ToList();
            int gsresolution;
            if (Properties.Settings.Default.useLowResolution) gsresolution = 72 * Properties.Settings.Default.resolutionScale;
            else gsresolution = 20016;

            Func<bool, bool,bool> modify_pdf = (bool paint, bool resize) => {
                var tmppdf = TempFilesDeleter.GetTempFileName(".pdf", workingDir);
                tempFilesDeleter.AddFile(tmppdf);
                if (!pdfcrop(tmpFileBaseName + ".pdf", tmppdf,
                    vectorExtensions.Contains(extension) || Properties.Settings.Default.yohakuUnitBP,
                    Enumerable.Range(1, page).ToList(), bbs, paint, false, resize, version))
                    return false;
                tmpFileBaseName = Path.GetFileNameWithoutExtension(tmppdf);
                return true;
            };

            // サイズ調整もする
            Func<bool,bool> make_pdf_without_text = (bool paint) => {
                if (IsNewGhostscript()) {
                    if (!modify_pdf(paint, true)) return false;
                    // 新しい場合はpdfwrite
                    var tmppdf = TempFilesDeleter.GetTempFileName(".pdf", workingDir);
                    tempFilesDeleter.AddFile(tmppdf);
                    if (!pdf2pdf(tmpFileBaseName + ".pdf", tmppdf, gsresolution, version)) return false;
                    tmpFileBaseName = Path.GetFileNameWithoutExtension(tmppdf);
                } else {
                    if (!modify_pdf(paint, false)) return false;
                    // 古いときはeps経由
                    for (int i = 1; i <= page; ++i) {
                        if (emptyPages.Contains(i)) {
                            make_dummyeps(tmpFileBaseName + "-" + i + ".eps");
                        } else {
                            if (!pdf2eps(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".eps", gsresolution, i, version, bbs[i - 1])) return false;
                        }
                    }
                    if (!eps2pdf(Enumerable.Range(1, page).Select(d => tmpFileBaseName + "-" + d + ".eps").ToList(), tmpFileBaseName + ".pdf", gsresolution, version)) return false;
                }
                return true;
            };

            var generate_actions = new Dictionary<string, Func<bool>>();
            generate_actions[".pdf"] = () => {
                if (Properties.Settings.Default.outlinedText) {
                    if (!make_pdf_without_text(!Properties.Settings.Default.transparentPngFlag)) return false;
                } else if (!modify_pdf(!Properties.Settings.Default.transparentPngFlag, true)) return false;
                if (!Properties.Settings.Default.mergeOutputFiles) {
                    foreach (var i in pagelist) {
                        if (!pdf2pdf_pdfium(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i.ToString() + ".pdf", i)) return false;
                    }
                } else {
                    if (!pdf2pdf_pdfium(tmpFileBaseName + ".pdf", tmpFileBaseName + "-1.pdf", pagelist)) return false;
                    page = 1;
                }
                return true;
            };
            generate_actions[".eps"] = () => {
                if (!Properties.Settings.Default.transparentPngFlag) {
                    var tmppdf = TempFilesDeleter.GetTempFileName(".pdf", workingDir);
                    tempFilesDeleter.AddFile(tmppdf);
                    // ここではサイズ調整をしない
                    if (!pdfcrop(tmpFileBaseName + ".pdf", tmppdf, true, Enumerable.Range(1, page).ToList(), bbs, true, false, false)) return false;
                    tmpFileBaseName = Path.GetFileNameWithoutExtension(tmppdf);
                }
                foreach (var i in pagelist) {
                    if (!pdf2eps(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".eps", gsresolution, i, version, bbs[i - 1])) return false;
                    enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                }
                return true;
            };
            // extensionは内部で使わない
            generate_actions[".svg"] = () => {
                if (Properties.Settings.Default.outlinedText || (Properties.Settings.Default.mergeOutputFiles && page > 1)) {
                    if (!make_pdf_without_text(!Properties.Settings.Default.transparentPngFlag)) return false;
                } else if (!modify_pdf(!Properties.Settings.Default.transparentPngFlag, true)) return false;
                if (!pdf2img_mudraw(tmpFileBaseName + ".pdf", tmpFileBaseName + "-%d.svg", pagelist)) return false;
                if (Properties.Settings.Default.deleteDisplaySize) {
                    foreach(var i in pagelist) {
                        DeleteHeightAndWidthFromSVGFile(tmpFileBaseName + "-" + i.ToString() + ".svg");
                    }
                }
                if (Properties.Settings.Default.mergeOutputFiles && page > 1) {
                    var temp = TempFilesDeleter.GetTempFileName(".svg", workingDir);
                    var temp1 = Path.GetFileNameWithoutExtension(temp) + "-1.svg";
                    tempFilesDeleter.AddFile(temp1);
                    if(svgconcat(pagelist.Select(d=>tmpFileBaseName + "-" + d.ToString() + ".svg").ToList(), temp1, Properties.Settings.Default.animationDelay, Properties.Settings.Default.animationLoop)) {
                        page = 1;
                        tmpFileBaseName = Path.GetFileNameWithoutExtension(temp);
                    } else warnngs.Add(Properties.Resources.FAIL_CONCAT_IMAGES);
                }
                return true;
            };
            generate_actions[".svgz"] = () => {
                if (!generate_actions[".svg"]()) return false;
                for(int i = 1; i <= page; ++i) {
                    if(File.Exists(Path.Combine(workingDir,tmpFileBaseName + "-" + i.ToString() + ".svg"))) {
                        tempFilesDeleter.AddFile(tmpFileBaseName + "-" + i.ToString() + ".svgz");
                        using (var ins = new FileStream(Path.Combine(workingDir, tmpFileBaseName + "-" + i.ToString() + ".svg"), FileMode.Open))
                        using (var outs = new FileStream(Path.Combine(workingDir, tmpFileBaseName + "-" + i.ToString() + ".svgz"), FileMode.Create))
                        using (var gzip = new System.IO.Compression.GZipStream(outs, System.IO.Compression.CompressionMode.Compress)) {
                            var bytes = new byte[ins.Length];
                            ins.Read(bytes, 0, (int)ins.Length);
                            gzip.Write(bytes, 0, (int)ins.Length);
                        }
                    }
                }
                return true;
            };
            generate_actions[".emf"] = () => {
                if (Properties.Settings.Default.outlinedText) {
                    if (!make_pdf_without_text(false)) return false;
                }else if (!modify_pdf(false, true)) return false;
                if (!dashtoline(tmpFileBaseName + ".pdf", version)) return false;
                if (!pdf2img_pdfium(tmpFileBaseName + ".pdf", tmpFileBaseName + "-%d" + extension, pagelist)) return false;
                return true;
            };
            generate_actions[".wmf"] = generate_actions[".emf"];

            Func<string, bool> generate_bitmap = (ext) => {
                if (!modify_pdf(!Properties.Settings.Default.transparentPngFlag, true)) return false;
                foreach (var i in pagelist) {
                    if (!pdf2img(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ext, i)) return false;
                }
                return true;
            };

            generate_actions[".png"] = () => generate_bitmap(".png");
            generate_actions[".jpg"] = () => generate_bitmap(".jpg");
            generate_actions[".bmp"] = () => generate_bitmap(".bmp");
            generate_actions[".tiff"] = () => {
                if (!generate_bitmap(".png")) return false;
                foreach (var i in pagelist) {
                    if (!img2img_pdfium(tmpFileBaseName + "-" + i + ".png", tmpFileBaseName + "-" + i + extension)) return false;
                }
                if (Properties.Settings.Default.mergeOutputFiles && page > 1) {
                    var temp = TempFilesDeleter.GetTempFileName(".tiff", workingDir);
                    var temp1 = Path.GetFileNameWithoutExtension(temp) + "-1.tiff";
                    if(tiffconcat(pagelist.Select(d => tmpFileBaseName + "-" + d.ToString() + ".tiff").ToList(), temp1)){ 
                        page = 1;
                        tmpFileBaseName = Path.GetFileNameWithoutExtension(temp);
                    } else warnngs.Add(Properties.Resources.FAIL_CONCAT_IMAGES);
                }
                return true;
            };
            generate_actions[".gif"] = () => {
                if (Properties.Settings.Default.transparentPngFlag) {
                    if (!modify_pdf(false, true)) return false;
                    if (!pdf2img_pdfium(tmpFileBaseName + ".pdf", tmpFileBaseName + "-%d" + extension, pagelist)) return false;
                } else {
                    if (!generate_bitmap(".png")) return false;
                    foreach (var i in pagelist) {
                        if (!img2img_pdfium(tmpFileBaseName + "-" + i + ".png", tmpFileBaseName + "-" + i + extension)) return false;
                    }
                }
                if (Properties.Settings.Default.mergeOutputFiles && page > 1) {
                    var temp = TempFilesDeleter.GetTempFileName(".gif", workingDir);
                    var temp1 = Path.GetFileNameWithoutExtension(temp) + "-1.gif";
                    if (gifconcat(pagelist.Select(d => tmpFileBaseName + "-" + d.ToString() + ".gif").ToList(), temp1, Properties.Settings.Default.animationDelay, Properties.Settings.Default.animationLoop)) {
                        page = 1;
                        tmpFileBaseName = Path.GetFileNameWithoutExtension(temp);
                    } else warnngs.Add(Properties.Resources.FAIL_CONCAT_IMAGES);
                }
                return true;
            };

            try {
                if (!generate_actions[extension]()) return false;
            }
            // 適当．後で考える．
            catch(Exception e) {
                warnngs.Add(e.Message);
            }

            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (outputDirectory != "" && !Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            // 出力ファイルをターゲットディレクトリにコピー
            if (page == 1) {
                string generatedFile = Path.Combine(workingDir, tmpFileBaseName + "-1" + extension);
                if (File.Exists(generatedFile)) {
                    try {
                        File.Delete(outputFilePath);
                        File.Move(generatedFile, outputFilePath);
                    }
                    catch (UnauthorizedAccessException) {
                        if (controller_ != null) controller_.showUnauthorizedError(outputFilePath);
                    }
                    catch (IOException) {
                        if (controller_ != null) controller_.showIOError(outputFilePath);
                    }

                    outputFileNames.Add(outputFilePath);
                }
            } else {
                string outputFilePathBaseName = Path.Combine(Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath));
                for (int i = 1; i <= page; ++i) {
                    string generatedFile = Path.Combine(workingDir, tmpFileBaseName + "-" + i + extension);
                    if (File.Exists(generatedFile)) {
                        try {
                            File.Delete(outputFilePathBaseName + "-" + i + extension);
                            File.Move(generatedFile, outputFilePathBaseName + "-" + i + extension);
                        }
                        catch (UnauthorizedAccessException) {
                            if (controller_ != null) controller_.showUnauthorizedError(outputFilePath);
                        }
                        catch (IOException) {
                            if (controller_ != null) controller_.showIOError(outputFilePath);
                        }
                        outputFileNames.Add(outputFilePathBaseName + "-" + i + extension);
                    }
                }
            }

            // ソース埋め込み
            if (Properties.Settings.Default.embedTeXSource && inputextension == ".tex") {
                try {
                    using (var source = new FileStream(inputTeXFilePath, FileMode.Open, FileAccess.Read)) {
                        var buf = new byte[source.Length];
                        source.Read(buf, 0, (int)source.Length);
                        // エンコードの決定
                        var enc = KanjiEncoding.CheckBOM(buf);
                        if (enc == null) {
                            var encs = KanjiEncoding.GuessKajiEncoding(buf);
                            var inpenc = GetInputEncoding();
                            if (encs.Count() != 0) enc = encs[0];
                            else enc = inpenc;
                            foreach (var e in encs) {
                                if (inpenc.CodePage == e.CodePage) {
                                    enc = inpenc;
                                    break;
                                }
                            }
                        }
                        var srctext = enc.GetString(buf);
                        foreach (var f in outputFileNames) {
                            TeXSource.EmbedSource(f, srctext);
                        }
                    }
                }
                // 例外は無視
                catch (Exception) { }
            }
            if (controller_ != null) {
                foreach (var w in warnngs) controller_.appendOutput("TeX2img: " + w + "\n");
                if (error_ignored) controller_.errorIgnoredWarning();
            }
            if (Properties.Settings.Default.previewFlag) {
                if (outputFileNames.Count > 0) Process.Start(outputFileNames[0]);
            }
            return true;
        }

        #region PDF情報
        bool pdfinfo(string file, out int page, out int version) {
            page = -1;version = -1;
            using (var mupdf = new MuPDF(Path.Combine(GetToolsPath(), "mudraw.exe"))) {
                try {
                    int doc = mupdf.Execute<int>("open_document", Path.Combine(workingDir, file));
                    const int repeatTimes = 10;
                    for (int i = 1; i <= repeatTimes; ++i) {
                        try {
                            if (page == -1) page = mupdf.Execute<int>("count_pages", doc);
                            if (version == -1) version = mupdf.Execute<int>("version_document", doc);
                            return true;
                        }
                        catch (Exception) {
                            mupdf.ClearError();
                            if (i == repeatTimes) throw;
                        }
                    }
                }
                catch (Exception e) {
                    if (controller_ != null) controller_.appendOutput(Properties.Resources.FAIL_PDFPAGES + "\n" + e.Message);
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region ユーティリティー的な
        public static string which(string name) {
            string separator, fullPath;
            var extensions = new List<string> { "" };
            var pathext = Environment.GetEnvironmentVariable("PATHEXT");
            if (pathext == null) return "";
            var pathexts = pathext.Split(';').Select(s => s.ToLower()).ToList();
            extensions.AddRange(pathexts);
            var pathenv = Environment.GetEnvironmentVariable("PATH");
            var extname = Path.GetExtension(name);

            if (pathenv == null) return "";
            if (extensions == null) return "";
            foreach (string path in pathenv.Split(';')) {
                if (path.Length > 0 && path[path.Length - 1] != '\\') separator = "\\";
                else separator = "";
                foreach (var extension in extensions) {
                    fullPath = path + separator + name + extension;
                    if (File.Exists(fullPath) && pathexts.Contains(Path.GetExtension(fullPath).ToLower())) {
                        return fullPath;
                    }
                }
            }
            return string.Empty;
        }

        string GetpdftexPath() {
            var f = Path.Combine(Path.GetDirectoryName(setProcStartInfo(Properties.Settings.Default.platexPath)), "pdftex.exe");
            if (File.Exists(f)) return f;
            return which("pdftex");
        }

        ProcessStartInfo GetProcessStartInfo() {
            var rv = new ProcessStartInfo() {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDir,
            };
            foreach (var e in Environments) {
                try { rv.EnvironmentVariables.Add(e.Key, e.Value); }
                catch (ArgumentException) { }
            }
            return rv;
        }

        Process GetProcess() {
            return new Process() { StartInfo = GetProcessStartInfo() };
        }

        public bool CheckFormat() {
            string extension = Path.GetExtension(OutputFile).ToLower();
            if (!imageExtensions.Contains(extension)) {
                if (controller_ != null) controller_.showExtensionError(OutputFile);
                return false;
            }
            return true;
        }

        public bool CheckInputFormat() {
            string extension = Path.GetExtension(InputFile).ToLower();
            if (!new string[] { ".tex", ".pdf", ".ps", ".eps" }.Contains(extension)) {
                if (controller_ != null) controller_.showExtensionError(InputFile);
                return false;
            }
            return true;
        }

        // pTeX or upTeX
        static bool IspTeX(string latex) {
            var l = Path.GetFileNameWithoutExtension(latex).ToLower();
            return (l == "platex" || l == "uplatex" || l == "ptex" || l == "uptex");
        }
        static bool IsupTeX(string latex) {
            var l = Path.GetFileNameWithoutExtension(latex).ToLower();
            return (l == "uplatex" || l == "uptex");
        }

        // Ghostscriptが新しいか
        static bool IsNewGhostscript() {
            return Properties.Settings.Default.gsDevice == "eps2write";
        }

        public static Encoding GetInputEncoding() {
            switch (Properties.Settings.Default.encode) {
            case "_sjis":
            case "sjis": return Encoding.GetEncoding("shift_jis");
            case "euc": return Encoding.GetEncoding("euc-jp");
            case "jis": return Encoding.GetEncoding("iso-2022-jp");
            default: // "utf8" "_utf8"
                return Encoding.UTF8;
            }
        }

        public static Encoding GetOutputEncoding() {
            string arg;
            string latex = setProcStartInfo(Properties.Settings.Default.platexPath, out arg);
            return GetOutputEncoding(latex, arg);
        }

        public static Encoding GetOutputEncoding(string latex, string arg) {
            if (IspTeX(latex)) {
                if (arg.Contains("-sjis-terminal")) return Encoding.GetEncoding("shift_jis");
                switch (Properties.Settings.Default.encode) {
                case "sjis": return Encoding.GetEncoding("shift_jis");
                case "utf8": return Encoding.UTF8;
                case "jis": return Encoding.GetEncoding("iso-2022-jp");
                case "euc": return Encoding.GetEncoding("euc-jp");
                case "_utf8":
                case "_sjis":
                default:
                    if (!IsupTeX(latex) && !arg.Contains("-kanji")) return Encoding.GetEncoding("shift_jis");
                    else return Encoding.UTF8;
                }
            } else return Encoding.UTF8;
        }

        public static string setProcStartInfo(String path) {
            string dummy;
            return setProcStartInfo(path, out dummy);
        }
        // path に指定されたオプション引数を解釈する
        // 戻り値 = FileName
        public static string setProcStartInfo(String path, out string Arguments) {
            string FileName = path;
            Arguments = "";
            if (path.IndexOf("\"") != -1) {
                // "がないならば**"***"**(SPACE)という並びを探す 
                var m = Regex.Match(path, "^([^\" ]*(\"[^\"]*\")*[^\" ]*) (.*)$");
                if (m.Success) {
                    FileName = m.Groups[1].Value;
                    Arguments = m.Groups[3].Value;
                    if (Arguments != "") Arguments += " ";
                }
                FileName = FileName.Replace("\"", "");
            } else {
                // そうでなければスペースで切って後ろから解析。
                var splitted = path.Split(new char[] { ' ' });
                for (int i = splitted.Count(); i >= 0; --i) {
                    var file = String.Join(" ", splitted, 0, i);
                    if (file.EndsWith(" ")) continue;// File.Existsは末尾の空白を削除してから存在チェックをする
                    try {
                        if (File.Exists(file) || File.Exists(file + ".exe") || (Path.GetDirectoryName(file) == "" && which(file) != "")) {
                            FileName = file;
                            Arguments = String.Join(" ", splitted, i, splitted.Count() - i);
                            if (Arguments != "") Arguments += " ";
                            break;
                        }
                    }
                    catch (Exception) { }
                }
            }
            return FileName;
        }

        volatile bool abort = false;
        public void Abort() {
            abort = true;
        }

        private void printCommandLine(Process proc) {
            if (controller_ != null) controller_.appendOutput(proc.StartInfo.WorkingDirectory + ">\"" + proc.StartInfo.FileName + "\" " + proc.StartInfo.Arguments + "\n");
        }

        // Error -> 同期，Output -> 非同期
        // でとりあえずデッドロックしなくなったのでこれでよしとする。
        // 両方非同期で駄目な理由がわかりません……。
        //
        // 非同期だと全部読み込んだかわからない気がしたので，スレッドを作成することにした。
        //
        // 結局どっちもスレッドを回すことにしてみた……。
        void ReadOutputs(Process proc, string freezemsg) {
            Action<string> read_func = (s) => { if (controller_ != null) controller_.appendOutput(s + "\n"); };
            ReadOutputs(proc, freezemsg, read_func, read_func);
        }

        void ReadOutputs(Process proc, string freezemsg, Action<string> stdOutRead, Action<string> stdErrRead) {
            proc.Start();
            object syncObj = new object();
            var readThread = new Action<StreamReader, Action<string>>((sr, action) => {
                try {
                    while (!sr.EndOfStream) {
                        if (abort) return;
                        var str = sr.ReadLine();
                        if (str != null) {
                            lock (syncObj) { action(str); }
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException) { return; }
            });
            var ReadStdOutThread = readThread.BeginInvoke(proc.StandardOutput, stdOutRead, null, null);
            var ReadStdErrThread = readThread.BeginInvoke(proc.StandardError, stdErrRead, null, null);
            while (true) {
                proc.WaitForExit(Properties.Settings.Default.timeOut <= 0 ? 100 : Properties.Settings.Default.timeOut);
                if (proc.HasExited) {
                    break;
                } else {
                    bool kill = false;
                    if (Properties.Settings.Default.timeOut > 0) {
                        if (Properties.Settings.Default.batchMode == Properties.Settings.BatchMode.Default && controller_ != null) {
                            // プロセスからの読み取りを一時中断するためのlock。
                            // でないと特にCUI時にメッセージが混ざってわけがわからなくなる。
                            lock (syncObj) {
                                kill = !controller_.askYesorNo(String.Format(Properties.Resources.FREEZMSG, freezemsg));
                            }
                        } else kill = (Properties.Settings.Default.batchMode == Properties.Settings.BatchMode.Stop);
                    }
                    if (kill || abort) {
                        //proc.Kill();
                        KillChildProcesses(proc);
                        if (!ReadStdOutThread.IsCompleted || !ReadStdErrThread.IsCompleted) {
                            System.Threading.Thread.Sleep(500);
                            abort = true;
                        }
                        if (controller_ != null) controller_.appendOutput(Properties.Resources.STOPCONVERTMSG + "\n");
                        readThread.EndInvoke(ReadStdOutThread);
                        readThread.EndInvoke(ReadStdErrThread);
                        throw new System.TimeoutException();
                    } else continue;
                }
            }
            // 残っているかもしれないのを読む。
            while (!ReadStdOutThread.IsCompleted || !ReadStdErrThread.IsCompleted) {
                System.Threading.Thread.Sleep(300);
            }
            readThread.EndInvoke(ReadStdOutThread);
            readThread.EndInvoke(ReadStdErrThread);
            if (controller_ != null) controller_.appendOutput("\n");
            if (abort) throw new System.TimeoutException();
        }

        public static void KillChildProcesses(Process proc) {
            // taskkillを起動するのが早そう。
            using (var p = new Process()) {
                try {
                    p.StartInfo.FileName = "taskkill.exe";
                    p.StartInfo.Arguments = "/PID " + proc.Id.ToString() + " /T /F";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit(3000);
                    if (!p.HasExited) {
                        p.Kill();
                        proc.Kill();
                    }
                }
                catch (Win32Exception) { proc.Kill(); }
            }
        }
        public void AddInputPath(string path) {
            if (!Environments.ContainsKey("TEXINPUTS")) {
                string env;
                try {
                    env = Environment.GetEnvironmentVariable("TEXINPUTS");
                    if (env == null) env = "";
                }
                catch (System.Security.SecurityException) {
                    env = "";
                }
                if (!env.EndsWith(";")) env += ";";
                Environments["TEXINPUTS"] = env;
            }
            Environments["TEXINPUTS"] += path + ";";
        }

        public static string GetToolsPath() {
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location)), ShortToolPath);
        }
        public static readonly string ShortToolPath = "";
        #endregion
    }
}