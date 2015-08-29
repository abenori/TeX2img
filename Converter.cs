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
    class Converter : IDisposable{
        /* 空ページの扱い：
         * 生成されるEPSファイルはBoundingBoxが0 0 0 0かもしれない．
         * 変換に渡されるEPSファイルのBoundingBoxは必ず幅を持つようにする．
         */ 

        // ADS名
        public const string ADSName = "TeX2img.source";
        // 拡張子たち
        public static readonly string[] bmpExtensions = new string[] { ".jpg", ".png", ".bmp", ".gif", ".tiff" };
        public static readonly string[] vectorExtensions = new string[] { ".eps", ".pdf", ".emf", ".svg" };
        public static string[] imageExtensions {
            get { return bmpExtensions.Concat(vectorExtensions).ToArray(); }
        }

        IOutputController controller_;
        int epsResolution_ = 20016;
        string workingDir;
        string InputFile, OutputFile;
        List<string> outputFileNames;
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
        }
        Dictionary<string, string> Environments = new Dictionary<string, string>();
        ~Converter() {
            Dispose();
        }
        public void Dispose() {
            if(Properties.Settings.Default.deleteTmpFileFlag) {
                try {
                    foreach(var f in generatedTeXFilesWithoutExtension) {
                        File.Delete(f + ".tex");
                        File.Delete(f + ".dvi");
                        File.Delete(f + ".pdf");
                        File.Delete(f + ".log");
                        File.Delete(f + ".aux");
                        File.Delete(f + ".tmp");
                        File.Delete(f + ".out");
                        File.Delete(f + ".pdf");
                    }
                    foreach(var d in generatedImageFiles) {
                        File.Delete(d);
                    }
                }
                catch(UnauthorizedAccessException) {
                    controller_.appendOutput("一部の一時ファイルの削除に失敗しました．\r\n");
                }
            }
            generatedTeXFilesWithoutExtension.Clear();
            generatedImageFiles.Clear();
        }

        List<string> generatedImageFiles = new List<string>();
        List<string> generatedTeXFilesWithoutExtension = new List<string>();

        // 変換
        public bool Convert() {
            if(GetInputEncoding().CodePage == Encoding.UTF8.CodePage) {
                Environments["command_line_encoding"] = "utf8";
            }

            bool rv = generate(InputFile, OutputFile);

            return rv;
        }
        
        #region BoundingBox関連
        struct BoundingBox{
            private decimal left, right, bottom, top;
            public decimal Left { get { return left; } }
            public decimal Right { get { return right; } }
            public decimal Bottom { get { return bottom; } }
            public decimal Top { get { return top; } }
            public BoundingBox(decimal l, decimal b, decimal r, decimal t) {
                left = l; right = r; bottom = b; top = t;
            }
            public bool IsEmpty { get { return left >= right || bottom >= top; } }
        };
        
        class BoundingBoxPair {
            public BoundingBox bb, hiresbb;
            public BoundingBoxPair(BoundingBox b, BoundingBox h) {
                bb = b; hiresbb = h;
            }
        }

        void enlargeBB(string inputEpsFileName, bool use_bp = true) {
            Func<BoundingBox, BoundingBox> func = bb => AddMargineToBoundingBox(bb,use_bp);
            rewriteBB(inputEpsFileName, func, func);
        }

        void rewriteBB(string inputEpsFileName,Func<BoundingBox,BoundingBox> bb,Func<BoundingBox,BoundingBox> hiresbb) {
            Regex regexBB = new Regex(@"^\%\%(HiRes|)BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
            byte[] inbuf;
            using(var fs = new FileStream(Path.Combine(workingDir, inputEpsFileName), FileMode.Open, FileAccess.Read)) {
                if(!fs.CanRead) return;
                inbuf = new byte[fs.Length];
                fs.Read(inbuf, 0, (int) fs.Length);
            }
            byte[] outbuf = new byte[inbuf.Length + 200];
            byte[] tmpbuf;

            // 現在読んでいるinufの「行」の先頭
            int inp = 0;
            // inbufの現在読んでいる場所
            int q = 0;
            // outbufに書き込んだ量
            int outp = 0;
            bool bbfound = false, hiresbbfound = false;
            while(q < inbuf.Length) {
                if(q == inbuf.Length - 1 || inbuf[q] == '\r' || inbuf[q] == '\n') {
                    string line = System.Text.Encoding.ASCII.GetString(inbuf, inp, q - inp);
                    Match match = regexBB.Match(line);
                    if(match.Success) {
                        BoundingBox bbinfile = new BoundingBox(
                            System.Convert.ToDecimal(match.Groups[2].Value),
                            System.Convert.ToDecimal(match.Groups[3].Value),
                            System.Convert.ToDecimal(match.Groups[4].Value),
                            System.Convert.ToDecimal(match.Groups[5].Value));
                        string HiRes = match.Groups[1].Value;
                        if(HiRes == "") {
                            bbfound = true;
                            var newbb = bb(bbinfile);
                            line = String.Format("%%BoundingBox: {0} {1} {2} {3}", (int) newbb.Left, (int) newbb.Bottom, (int) newbb.Right, (int) newbb.Top);
                        } else {
                            hiresbbfound = true;
                            var newbb = hiresbb(bbinfile);
                            line = String.Format("%%HiResBoundingBox: {0} {1} {2} {3}", newbb.Left, newbb.Bottom, newbb.Right, newbb.Top);
                        }
                        tmpbuf = System.Text.Encoding.ASCII.GetBytes(line);
                        System.Array.Copy(tmpbuf, 0, outbuf, outp, tmpbuf.Length);
                        outp += tmpbuf.Length;
                        if(bbfound && hiresbbfound) {
                            System.Array.Copy(inbuf, q, outbuf, outp, inbuf.Length - q);
                            outp += inbuf.Length - q;
                            break;
                        }
                    } else {
                        System.Array.Copy(inbuf, inp, outbuf, outp, q - inp);
                        outp += q - inp;
                    }
                    inp = q;
                    while(q < inbuf.Length - 1 && (inbuf[q] == '\r' || inbuf[q] == '\n')) ++q;
                    System.Array.Copy(inbuf, inp, outbuf, outp, q - inp);
                    outp += q - inp;
                    inp = q;
                    if(q == inbuf.Length - 1) break;
                } else ++q;
            }
            using(FileStream wfs = new System.IO.FileStream(Path.Combine(workingDir, inputEpsFileName), FileMode.Open, FileAccess.Write)) {
                wfs.Write(outbuf, 0, outp);
            }
        }

        private BoundingBoxPair readBB(string inputEpsFileName) { 
            Regex regex = new Regex(@"^\%\%(HiRes)?BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
            var bb = new BoundingBox();
            var hiresbb = new BoundingBox();
            bool bbread = false, hiresbbread = false;
            using(StreamReader sr = new StreamReader(Path.Combine(workingDir, inputEpsFileName), Encoding.GetEncoding("shift_jis"))) {
                string line;
                while((line = sr.ReadLine()) != null) {
                    Match match = regex.Match(line);
                    if(match.Success) {
                        var cb = new BoundingBox(
                            System.Convert.ToDecimal(match.Groups[2].Value),
                            System.Convert.ToDecimal(match.Groups[3].Value),
                            System.Convert.ToDecimal(match.Groups[4].Value),
                            System.Convert.ToDecimal(match.Groups[5].Value));
                        if(match.Groups[1].Value == "HiRes") {
                            hiresbb = cb; hiresbbread = true;
                        } else {
                            bb = cb; bbread = true;
                        }
                        if(bbread && hiresbbread) break;
                    }
                }
            }
            return new BoundingBoxPair(bb, hiresbb);
        }

        BoundingBoxPair readBBFromPDF(string inputPDFFileName,int page){
            BoundingBox bb,hiresbb;
            var gspath = setProcStartInfo(Properties.Settings.Default.gsPath);
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = gspath;
                proc.StartInfo.Arguments = "-dBATCH -dNOPAUSE -q -sDEVICE=bbox -dFirstPage=" + page.ToString() + " -dLastPage=" + page.ToString() + " \"" + inputPDFFileName + "\"";
                proc.OutputDataReceived += ((s, e) => { System.Diagnostics.Debug.WriteLine("Std: " + e.Data); });

                Regex regexBB = new Regex(@"^\%\%(HiRes)?BoundingBox\: ([-\d\.]+) ([-\d\.]+) ([-\d\.]+) ([-\d\.]+)$");
                bb = new BoundingBox();
                hiresbb = new BoundingBox();
                try {
                    printCommandLine(proc);
                    proc.Start();
                    proc.BeginOutputReadLine();
                    while(!proc.StandardError.EndOfStream) {
                        var line = proc.StandardError.ReadLine();
                        controller_.appendOutput(line + "\n");
                        var match = regexBB.Match(line);
                        if(match.Success) {
                            var currentbb = new BoundingBox(
                                System.Convert.ToDecimal(match.Groups[2].Value),
                                System.Convert.ToDecimal(match.Groups[3].Value),
                                System.Convert.ToDecimal(match.Groups[4].Value),
                                System.Convert.ToDecimal(match.Groups[5].Value));
                            if(match.Groups[1].Value == "HiRes") {
                                hiresbb = currentbb;
                            } else {
                                bb = currentbb;
                            }
                        }
                    }
                    proc.WaitForExit();
                    controller_.appendOutput("\n");
                    return new BoundingBoxPair(bb, hiresbb);
                }
                catch(Win32Exception) {
                    controller_.showPathError(gspath, "Ghostscript");
                    return null;
                }
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


        #region 変換用関数たち
        private bool tex2dvi(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
            generatedTeXFilesWithoutExtension.Add(Path.Combine(workingDir, baseName));
            ProcessStartInfo startinfo = GetProcessStartInfo();
            startinfo.FileName = setProcStartInfo(Properties.Settings.Default.platexPath, out arg);
            if(Properties.Settings.Default.platexPath == "") {
                controller_.showPathError("platex.exe", "TeX ディストリビューション（platex）");
                return false;
            }
            startinfo.Arguments = arg;
            //if(IspTeX(startinfo.FileName)) {
            if(Properties.Settings.Default.encode.Substring(0, 1) != "_") startinfo.Arguments += "-no-guess-input-enc -kanji=" + Properties.Settings.Default.encode + " ";
            //}
            startinfo.Arguments += "-interaction=nonstopmode " + baseName + ".tex";
            startinfo.StandardOutputEncoding = GetOutputEncoding();

            try {
                error_ignored = false;
                if(Properties.Settings.Default.guessLaTeXCompile) {
                    var analyzer = new AnalyzeLaTeXCompile(Path.Combine(workingDir, fileName));
                    analyzer.UseBibtex = analyzer.UseMakeIndex = false;
                    int i = 0;
                    while(analyzer.Check() != AnalyzeLaTeXCompile.Program.Done) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            ReadOutputs(proc, "TeX ソースのコンパイル");
                            if(proc.ExitCode != 0) {
                                if(!Properties.Settings.Default.ignoreErrorFlag) {
                                    controller_.showGenerateError();
                                    return false;
                                } else {
                                    error_ignored = true;
                                }
                            }
                            ++i;
                            if(i == Properties.Settings.Default.LaTeXCompileMaxNumber) break;
                        }
                    }
                } else {
                    for(int i = 0 ; i < Properties.Settings.Default.LaTeXCompileMaxNumber ; ++i) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            ReadOutputs(proc, "TeX ソースのコンパイル");
                            if(proc.ExitCode != 0) {
                                if(!Properties.Settings.Default.ignoreErrorFlag) {
                                    controller_.showGenerateError();
                                    return false;
                                } else {
                                    error_ignored = true;
                                }
                            }
                        }
                    }
                }
            }
            catch(Win32Exception) {
                controller_.showPathError(startinfo.FileName, "TeX ディストリビューション（platex）");
                return false;
            }
            catch(TimeoutException) {
                return false;
            }

            return true;
        }

        private bool dvi2pdf(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.dvipdfmxPath, out arg);
                if(Properties.Settings.Default.dvipdfmxPath == "") {
                    controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション（dvipdfmx）");
                    return false;
                }
                //proc.StartInfo.Arguments = arg + " -vv -o " + baseName + ".pdf " + baseName + ".dvi";
                proc.StartInfo.Arguments = arg + " " + baseName + ".dvi";

                try {
                    // 出力は何故か標準エラー出力から出てくる
                    ReadOutputs(proc, "DVI から PDF への変換");
                }
                catch(Win32Exception) {
                    controller_.showPathError(proc.StartInfo.FileName, "TeX ディストリビューション（dvipdfmx）");
                    return false;
                }
                catch(TimeoutException) {
                    return false;
                }
                if(proc.ExitCode != 0/* || !File.Exists(Path.Combine(workingDir, baseName + ".pdf"))*/) {
                    controller_.showGenerateError();
                    return false;
                }
            }
            return true;
        }

        bool ps2pdf(string filename) {
            var outputFileName = Path.ChangeExtension(filename, ".pdf");
            using(var proc = GetProcess()) {
                string arg;
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if(proc.StartInfo.FileName == "") {
                    controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                proc.StartInfo.Arguments = arg + "-dSAFER -dNOPAUSE -dBATCH -sDEVICE=pdfwrite -sOutputFile=\"" + outputFileName + "\" -c .setpdfwrite -f\"" + filename + "\"";
                try {
                    ReadOutputs(proc, "PS から PDF への変換");
                }
                catch(Win32Exception) {
                    controller_.showPathError(Properties.Settings.Default.gsPath, "Ghostscript ");
                    return false;
                }
                catch(TimeoutException) {
                    return false;
                }
                if(proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showGenerateError();
                    return false;
                }
                return true;
            }
        }

        // origbbには，GhostscriptのsDevice=bboxで得られた値を入れておく．（nullならばここで取得する．）
        private bool pdf2eps(string inputFileName, string outputFileName, int resolution, int page, BoundingBoxPair origbb = null) {
            string arg;
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if(proc.StartInfo.FileName == "") {
                    controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                proc.StartInfo.Arguments = arg + "-q -sDEVICE=" + Properties.Settings.Default.gsDevice + " -dFirstPage=" + page + " -dLastPage=" + page;
                if(Properties.Settings.Default.gsDevice == "eps2write") proc.StartInfo.Arguments += " -dNoOutputFonts";
                else proc.StartInfo.Arguments += " -dNOCACHE";
                proc.StartInfo.Arguments += " -sOutputFile=\"" + outputFileName + "\" -dNOPAUSE -dBATCH -r" + resolution + " \"" + inputFileName + "\"";

                try {
                    ReadOutputs(proc, "PDF から EPS への変換");
                }
                catch(Win32Exception) {
                    controller_.showPathError(Properties.Settings.Default.gsPath, "Ghostscript ");
                    return false;
                }
                catch(TimeoutException) {
                    return false;
                }
                if(proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showGenerateError();
                    return false;
                }
                // BoundingBoxをあらかじめ計測した物に取り替える．
                BoundingBoxPair bb;
                if(origbb == null) bb = readBBFromPDF(inputFileName, page);
                else bb = origbb;
                Func<BoundingBox, BoundingBox> bbfunc = (b) => bb.bb;
                Func<BoundingBox, BoundingBox> hiresbbfunc = (b) => bb.hiresbb;
                rewriteBB(outputFileName, bbfunc, hiresbbfunc);
            }
            return true;
        }

        int pdfpages(string file) {
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                proc.StartInfo.Arguments = "--output-page \"" + file + "\"";
                try {
                    proc.ErrorDataReceived += ((s, e) => { });
                    proc.Start();
                    string output = "";
                    while(!proc.StandardOutput.EndOfStream) {
                        output += proc.StandardOutput.ReadToEnd();
                    }
                    System.Diagnostics.Debug.WriteLine("pdfpages: " + output);
                    output = output.Replace("\r", "").Replace("\n", "");
                    output.Trim();
                    try {
                        return Int32.Parse(output);
                    }
                    catch(FormatException) {
                        return -1;
                    }
                }
                catch(Win32Exception) {
                    controller_.showToolError("pdfiumdraw.exe");
                    return -1;
                }

            }
        }

        bool png2img(string inputFileName, string outputFileName) {
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            System.Drawing.Imaging.ImageFormat format;
            var extension = Path.GetExtension(outputFileName).ToLower();
            switch(extension) {
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
            controller_.appendOutput("TeX2img: Convert " + inputFileName + " to " + outputFileName + "\n");
            try {
                using(var bitmap = new System.Drawing.Bitmap(Path.Combine(workingDir, inputFileName))) {
                    if(Properties.Settings.Default.transparentPngFlag && extension != ".gif") {
                        bitmap.MakeTransparent();
                    }
                    bitmap.Save(Path.Combine(workingDir, outputFileName), format);
                }
                return true;
            }
            catch(FileNotFoundException) {
                return false;
            }
            catch(UnauthorizedAccessException) {
                return false;
            }
        }

        bool pdf2img_mudraw(string inputFileName, string outputFileName,int page = 0) {
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "mudraw.exe");
                proc.StartInfo.Arguments = "-l -o \"" + outputFileName + "\" \"" + inputFileName + "\" " + page.ToString();
                try {
                    ReadOutputs(proc, "mudraw の実行");
                }
                catch(Win32Exception) {
                    controller_.showToolError("mudraw.exe");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showToolError("mudraw.exe");
                    return false;
                }
                return true;
            }
        }
        
        void DeleteHeightAndWidthFromSVGFile(string svgFile) {
            var fullpath = Path.Combine(workingDir, svgFile);
            var xml = new System.Xml.XmlDocument();
            xml.XmlResolver = null;
            xml.Load(fullpath);
            foreach(System.Xml.XmlNode node in xml.GetElementsByTagName("svg")) {
                var attr = node.Attributes["width"];
                if(attr != null) node.Attributes.Remove(attr);
                attr = node.Attributes["height"];
                if(attr != null) node.Attributes.Remove(attr);
            }
            xml.Save(fullpath);
        }


        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp, int page = 1, BoundingBoxPair origbb = null) {
            return pdfcrop(inputFileName, outputFileName, use_bp, new List<int>() { page }, new List<BoundingBoxPair>() { origbb });
        }

        // origbbには，GhostscriptのsDevice=bboxで得られた値を入れておく．（nullならばここで取得する．）
        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp, List<int> pages, List<BoundingBoxPair> origbb) {
            System.Diagnostics.Debug.Assert(pages.Count == origbb.Count);
            var tmpfile = GetTempFileName(".tex");
            if(tmpfile == null) return false;
            generatedTeXFilesWithoutExtension.Add(Path.Combine(workingDir, Path.GetFileNameWithoutExtension(tmpfile)));
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));

            var gspath = setProcStartInfo(Properties.Settings.Default.gsPath);
            var bbBox = new List<BoundingBox>();
            for(int i = 0 ; i < pages.Count ; ++i) {
                BoundingBoxPair bb;
                if(origbb[i] == null) {
                    bb = readBBFromPDF(inputFileName, pages[i]);
                } else {
                    bb = origbb[i];
                }
                var rect = AddMargineToBoundingBox(bb.hiresbb, use_bp);
                bbBox.Add(rect);
            }
            using(var fw = new StreamWriter(Path.Combine(workingDir,tmpfile))) {
                fw.WriteLine(@"\pdfoutput=1\relax");
                for(int i = 0 ; i < pages.Count ; ++i){
                    var box = bbBox[i];
                    var page = pages[i];
                    fw.WriteLine(@"\pdfhorigin=" + (-box.Left).ToString() + @"bp\relax");
                    fw.WriteLine(@"\pdfvorigin=" + box.Bottom.ToString() + @"bp\relax");
                    fw.WriteLine(@"\pdfpagewidth=" + (box.Right - box.Left).ToString() + @"bp\relax");
                    fw.WriteLine(@"\pdfpageheight=" + (box.Top - box.Bottom).ToString() + @"bp\relax");
                    fw.WriteLine(@"\setbox0=\hbox{\pdfximage page " + page.ToString() + " mediabox {" + inputFileName + @"}\pdfrefximage\pdflastximage}\relax");
                    fw.WriteLine(@"\ht0=\pdfpageheight\relax");
                    fw.WriteLine(@"\shipout\box0\relax");
                }
                fw.WriteLine(@"\bye");
            }
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(setProcStartInfo(Properties.Settings.Default.platexPath)), "pdftex.exe");
                proc.StartInfo.Arguments = "-no-shell-escape -interaction=batchmode \"" + tmpfile + "\"";
                try {
                    ReadOutputs(proc, "pdftex の実行 ");
                }
                catch(Win32Exception) {
                    controller_.showPathError("pdftex.exe", "TeX ディストリビューション");
                    return false;
                }
                catch(TimeoutException) { return false; }
            }
            File.Delete(Path.Combine(workingDir, outputFileName));
            File.Move(Path.Combine(workingDir, Path.GetFileNameWithoutExtension(tmpfile) + ".pdf"),Path.Combine(workingDir,outputFileName));
            return true;
        }

        // 余白の付加も行う．
        private bool eps2img(string inputFileName, string outputFileName, BoundingBoxPair origbb = null){
            string extension = Path.GetExtension(outputFileName).ToLower();
            string baseName = Path.GetFileNameWithoutExtension(inputFileName);
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            // ターゲットのepsを「含む」epsを作成．
            string trimEpsFileName = GetTempFileName(".eps");
            generatedImageFiles.Add(Path.Combine(workingDir, trimEpsFileName));
            if(origbb == null) origbb = readBB(inputFileName);
            decimal devicedevide = Properties.Settings.Default.yohakuUnitBP ? 1 : Properties.Settings.Default.resolutionScale;
            decimal translateleft = - origbb.bb.Left + Properties.Settings.Default.leftMargin / devicedevide;
            decimal translatebottom = - origbb.bb.Bottom + Properties.Settings.Default.bottomMargin / devicedevide;
            using(StreamWriter sw = new StreamWriter(Path.Combine(workingDir, trimEpsFileName), false, Encoding.GetEncoding("shift_jis"))) {
                sw.WriteLine("/NumbDict countdictstack def");
                sw.WriteLine("1 dict begin");
                sw.WriteLine("/showpage {} def");
                sw.WriteLine("userdict begin");
                if(!origbb.bb.IsEmpty) sw.WriteLine("{0} {1} translate", translateleft, translatebottom);
                sw.WriteLine("1.000000 1.000000 scale");
                sw.WriteLine("0.000000 0.000000 translate");
                if(!origbb.bb.IsEmpty) sw.WriteLine("({0}) run", inputFileName);
                sw.WriteLine("countdictstack NumbDict sub {end} repeat");
                sw.WriteLine("showpage");
            }
            // Ghostscript を使ったJPEG,PNG生成
            string device = "jpeg";

            switch(extension) {
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
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if(proc.StartInfo.FileName == "") {
                    controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                string antialias = Properties.Settings.Default.useMagickFlag ? "4" : "1";
                decimal marginmult = Properties.Settings.Default.yohakuUnitBP ? Properties.Settings.Default.resolutionScale : 1;
                int width = (int) ((origbb.bb.Right - origbb.bb.Left) * Properties.Settings.Default.resolutionScale + (Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin) * marginmult);
                int height = (int) ((origbb.bb.Top - origbb.bb.Bottom) * Properties.Settings.Default.resolutionScale + (Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin) * marginmult);
                proc.StartInfo.Arguments = arg;
                proc.StartInfo.Arguments += String.Format(
                    "-q -sDEVICE={0} -sOutputFile={1} -dNOPAUSE -dBATCH -dPDFFitPage -dTextAlphaBits={2} -dGraphicsAlphaBits={2} -r{3} -g{4}x{5} {6}",
                    device, outputFileName, antialias,
                    72 * Properties.Settings.Default.resolutionScale,
                    width, height, trimEpsFileName);
                try {
                    ReadOutputs(proc, "Ghostscript の実行");
                }
                catch(Win32Exception) {
                    controller_.showPathError(proc.StartInfo.FileName, "Ghostscript ");
                    return false;
                }
                catch(TimeoutException) {
                    return false;
                }
            }
            return true;
        }

        bool pdf2img_pdfium(string inputFilename, string outputFileName,int pages = 0) {
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            var type = Path.GetExtension(outputFileName).Substring(1).ToLower();
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                proc.StartInfo.Arguments = 
                    (type == "emf" ? "" : "--scale=" + Properties.Settings.Default.resolutionScale.ToString() + " ") +
                    "--" + type + " " + (Properties.Settings.Default.transparentPngFlag ? "--transparent " : "") +
                    (pages > 0 ? "--pages=" + pages.ToString() + " " : "") + 
                    "--output=\"" + outputFileName + "\" \"" + inputFilename + "\"";
                try {
                    ReadOutputs(proc, "pdfiumdraw の実行");
                }
                catch(Win32Exception) {
                    controller_.showToolError("pdfiumdraw.exe");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showToolError("pdfiumdraw.exe");
                    return false;
                } else {
                    return true;
                }
            }
        }

        bool img2img_pdfium(string inputFileName, string outputFileName) {
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            var inputtype = Path.GetExtension(inputFileName).Substring(1).ToLower();
            var type = Path.GetExtension(outputFileName).Substring(1).ToLower();
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = Path.Combine(GetToolsPath(), "pdfiumdraw.exe");
                proc.StartInfo.Arguments =
                    "--" + type + " --input-format=" + inputtype + 
                    " --output=\"" + outputFileName + "\" \"" + inputFileName + "\"";
                try {
                    ReadOutputs(proc, "pdfiumdraw の実行");
                }
                catch(Win32Exception) {
                    controller_.showToolError("pdfiumdraw.exe");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showToolError("pdfiumdraw.exe");
                    return false;
                } else {
                    return true;
                }
            }
        }

        bool eps2pdf(string inputFileName, string outputFileName) {
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if(proc.StartInfo.FileName == "") {
                    controller_.showPathError("gswin32c.exe", "Ghostscript");
                    return false;
                }
                proc.StartInfo.Arguments = arg + "-q -sDEVICE=pdfwrite -dNOPAUSE -dBATCH -dEPSCrop -sOutputFile=\"" + outputFileName + "\" \"" + inputFileName + "\"";
                try {
                    ReadOutputs(proc, "Ghostscript の実行");
                }
                catch(Win32Exception) {
                    controller_.showPathError(proc.StartInfo.FileName, "Ghostscript ");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showPathError(proc.StartInfo.FileName, "Ghostscript ");
                    return false;
                } else {
                    return true;
                }
            }
        }
        #endregion

        // 1 file1が生成，-1 file2が生成，0 生成に失敗

        static int IsGenerated(string file1, string file2) {
            if(!File.Exists(file1)) {
                if(!File.Exists(file2)) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if(File.Exists(file2) && System.IO.File.GetLastWriteTime(file2) > System.IO.File.GetLastWriteTime(file1)) {
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
            if(inputextension == ".tex") {
                if(!tex2dvi(tmpFileBaseName + ".tex")) return false;
                generated = IsGenerated(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), Path.Combine(workingDir, tmpFileBaseName + ".dvi"));
                if(generated == 0) {
                    controller_.showGenerateError();
                    return false;
                }
                if(generated == -1) {
                    if(!dvi2pdf(tmpFileBaseName + ".dvi")) return false;
                }
            }
            generated = IsGenerated(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), Path.Combine(workingDir, tmpFileBaseName + ".ps"));
            if(inputextension == ".ps" || generated == -1) {
                if(!ps2pdf(tmpFileBaseName + ".ps")) return false;
            }

            var bbs = new List<BoundingBoxPair>();

            // ページ数を取得
            int page = pdfpages(Path.Combine(workingDir, tmpFileBaseName + ".pdf"));

            // boundingBoxを取得
            for(int i = 1 ; i <= page ; ++i) {
                bbs.Add(readBBFromPDF(Path.Combine(workingDir, tmpFileBaseName + ".pdf"), i));
            }

            bool addMargin = ((Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin + Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin) > 0);

            for(int i = 1 ; i <= page ; ++i) {
                if(bbs[i - 1].bb.IsEmpty) {
                    if(Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin == 0 || Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin == 0) {
                        warnngs.Add(i.ToString() + " ページ目が空ページだったため画像生成をスキップしました．");
                        continue;
                    } else {
                        warnngs.Add(i.ToString() + " ページ目が空ページでした．");
                    }
                }
                // .svg，テキスト情報保持な pdf は PDF から作る
                if(
                    extension == ".svg" ||
                    (extension == ".pdf" && !Properties.Settings.Default.outlinedText) ||
                    (extension == ".gif" && Properties.Settings.Default.transparentPngFlag)
                    ) {
                    if(extension == ".svg") {
                        if(!pdfcrop(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".pdf", true, i, bbs[i - 1])) return false;
                        if(!pdf2img_mudraw(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + ".svg")) return false;
                        if(Properties.Settings.Default.deleteDisplaySize) {
                            DeleteHeightAndWidthFromSVGFile(tmpFileBaseName + "-" + i + extension);
                        }
                    } else {
                        if(!pdfcrop(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".pdf", extension == ".pdf" ? true : Properties.Settings.Default.yohakuUnitBP, i, bbs[i - 1])) return false;
                        if(extension != ".pdf") {
                            if(!pdf2img_pdfium(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + extension)) return false;
                        }
                    }
                } else {
                    // それ以外はEPSを経由する．
                    int resolution;
                    if(Properties.Settings.Default.useLowResolution) epsResolution_ = 72 * Properties.Settings.Default.resolutionScale;
                    else epsResolution_ = 20016;
                    if(vectorExtensions.Contains(extension)) resolution = epsResolution_;
                    else resolution = 72 * Properties.Settings.Default.resolutionScale;
                    if(!pdf2eps(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".eps", resolution, i, bbs[i - 1])) return false;
                    switch(extension) {
                    case ".pdf":
                        if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                        if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return false;
                        break;
                    case ".eps":
                        if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                        break;
                    case ".emf":
                        if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                        if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + ".pdf")) return false;
                        if(!pdf2img_pdfium(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + ".emf")) return false;
                        break;
                    case ".png":
                    case ".jpg":
                    case ".bmp":
                        if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension, bbs[i - 1])) return false;
                        break;
                    case ".gif":
                    case ".tiff":
                        if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + ".png", bbs[i - 1])) return false;
                        if(!img2img_pdfium(tmpFileBaseName + "-" + i + ".png", tmpFileBaseName + "-" + i + extension)) return false;
                        break;
                    }
                }
            }
            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if(outputDirectory != "" && !Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            // 出力ファイルをターゲットディレクトリにコピー
            try {
                if(page == 1) {
                    string generatedFile = Path.Combine(workingDir, tmpFileBaseName + "-1" + extension);
                    if(File.Exists(generatedFile)) {
                        File.Copy(generatedFile, outputFilePath, true);
                        outputFileNames.Add(outputFilePath);
                    }
                } else {
                    string outputFilePathBaseName = Path.Combine(Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath));
                    for(int i = 1 ; i <= page ; ++i) {
                        string generatedFile = Path.Combine(workingDir, tmpFileBaseName + "-" + i + extension);
                        if(File.Exists(generatedFile)) {
                            File.Copy(generatedFile, outputFilePathBaseName + "-" + i + extension, true);
                            outputFileNames.Add(outputFilePathBaseName + "-" + i + extension);
                        }
                    }
                }
                if(Properties.Settings.Default.previewFlag) {
                    if(outputFileNames.Count > 0) Process.Start(outputFileNames[0]);
                }
            }
            catch(UnauthorizedAccessException) {
                controller_.showUnauthorizedError(outputFilePath);
            }
            catch(IOException) {
                controller_.showIOError(outputFilePath);
            }

            if(Properties.Settings.Default.embedTeXSource) {
                // Alternative Data Streamにソースを書き込む
                try {
                    using(var source = new FileStream(inputTeXFilePath, FileMode.Open, FileAccess.Read)) {
                        var buf = new byte[source.Length];
                        source.Read(buf, 0, (int) source.Length);
                        // エンコードの決定
                        var enc = KanjiEncoding.CheckBOM(buf);
                        if(enc == null) enc = GetInputEncoding();
                        var srctext = enc.GetString(buf);
                        foreach(var f in outputFileNames) {
                            using(var fs = AlternativeDataStream.WriteAlternativeDataStream(f, ADSName))
                            using(var ws = new StreamWriter(fs, new UTF8Encoding(false))) {
                                ws.Write(srctext);
                            }
                        }
                    }
                }
                // 例外は無視
                catch(IOException) { }
                catch(NotImplementedException) { }
            }
            foreach(var w in warnngs) controller_.appendOutput("TeX2img: " + w + "\n");
            if(error_ignored) controller_.errorIgnoredWarning();
            return true;
        }

        #region ユーティリティー的な
        // Error -> 同期，Output -> 非同期
        // でとりあえずデッドロックしなくなったのでこれでよしとする．
        // 両方非同期で駄目な理由がわかりません……．
        //
        // 非同期だと全部読み込んだかわからない気がしたので，スレッドを作成することにした．
        //
        // 結局どっちもスレッドを回すことにしてみた……．
        public static string which(string basename) {
            string separator, fullPath;
            string[] extensions = { "", ".exe", ".bat", ".cmd", ".vbs", ".js", ".wsf" };

            foreach(string path in Environment.GetEnvironmentVariable("PATH").Split(';')) {
                if(path.Length > 0 && path[path.Length - 1] != '\\') {
                    separator = "\\";
                } else {
                    separator = "";
                }

                foreach(string extension in extensions) {
                    fullPath = path + separator + basename + extension;
                    if(File.Exists(fullPath)) {
                        return fullPath;
                    }
                }
            }
            return string.Empty;
        }
        public static string GetTempFileName(string ext = ".tex") {
            return GetTempFileName(ext, Path.GetTempPath());
        }

        public static string GetTempFileName(string ext, string dir) {
            for(int i = 0 ; i < 1000 ; ++i) {
                var random = Path.ChangeExtension(Path.GetRandomFileName(), ext);
                if(!File.Exists(Path.Combine(dir, random))) {
                    return random;
                }
            }
            return null;
        }

        ProcessStartInfo GetProcessStartInfo() {
            var rv = new ProcessStartInfo() {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDir,
            };
            foreach(var e in Environments) {
                try { rv.EnvironmentVariables.Add(e.Key, e.Value); }
                catch(ArgumentException) { }
            }
            return rv;
        }
        Process GetProcess() {
            return new Process() { StartInfo = GetProcessStartInfo() };
        }

        public bool CheckFormat() {
            string extension = Path.GetExtension(OutputFile).ToLower();
            if(!imageExtensions.Contains(extension)) {
                if(controller_ != null) controller_.showExtensionError(OutputFile);
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

        public static Encoding GetInputEncoding() {
            switch(Properties.Settings.Default.encode) {
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
            if(IspTeX(latex)) {
                if(arg.Contains("-sjis-terminal")) return Encoding.GetEncoding("shift_jis");
                switch(Properties.Settings.Default.encode) {
                case "sjis": return Encoding.GetEncoding("shift_jis");
                case "utf8": return Encoding.UTF8;
                case "jis": return Encoding.GetEncoding("iso-2022-jp");
                case "euc": return Encoding.GetEncoding("euc-jp");
                case "_utf8":
                    if(!IsupTeX(latex) && !arg.Contains("-kanji")) return Encoding.GetEncoding("shift_jis");
                    else return Encoding.UTF8;
                case "_sjis":
                default:
                    if(IsupTeX(latex) && arg.Contains("-kanji")) return Encoding.GetEncoding("shift_jis");
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
            // "がないならばFileName = path
            string FileName = path;
            Arguments = "";
            if(path.IndexOf("\"") != -1) {
                // そうでなければ
                // **"***"**(SPACE)という並びを探す 
                var m = Regex.Match(path, "^([^\" ]*(\"[^\"]*\")*[^\" ]*) (.*)$");
                if(m.Success) {
                    FileName = m.Groups[1].Value;
                    Arguments = m.Groups[3].Value;
                    if(Arguments != "") Arguments += " ";
                }
                FileName = FileName.Replace("\"", "");
            }
            return FileName;
        }

        volatile bool abort = false;
        public void Abort() {
            abort = true;
        }
        
        private void printCommandLine(Process proc) {
            controller_.appendOutput(proc.StartInfo.WorkingDirectory + ">\"" + proc.StartInfo.FileName + "\" " + proc.StartInfo.Arguments + "\r\n");
        }
        
        void ReadOutputs(Process proc, string freezemsg) {
            printCommandLine(proc);
            proc.Start();
            object syncObj = new object();
            var readThread = new Action<StreamReader>((sr) => {
                try {
                    while(!sr.EndOfStream) {
                        if(abort) return;
                        var str = sr.ReadLine();
                        if(str != null) {
                            lock(syncObj) {
                                controller_.appendOutput(str + "\n");
                            }
                        }
                    }
                }
                catch(System.Threading.ThreadAbortException) { return; }
            });
            var ReadStdOutThread = readThread.BeginInvoke(proc.StandardOutput, null, null);
            var ReadStdErrThread = readThread.BeginInvoke(proc.StandardError, null, null);
            while(true) {
                proc.WaitForExit(Properties.Settings.Default.timeOut <= 0 ? 100 : Properties.Settings.Default.timeOut);
                if(proc.HasExited) {
                    break;
                } else {
                    bool kill = false;
                    if(Properties.Settings.Default.timeOut > 0) {
                        if(Properties.Settings.Default.batchMode == Properties.Settings.BatchMode.Default) {
                            // プロセスからの読み取りを一時中断するためのlock．
                            // でないと特にCUI時にメッセージが混ざってわけがわからなくなる．
                            lock(syncObj) {
                                kill = !controller_.askYesorNo(
                                    freezemsg + "に時間がかかっているようです．\n" +
                                    "フリーズしている可能性もありますが，このまま実行を続けますか？\n" +
                                    "続けない場合は，現在実行中のプログラムを強制終了します．");
                            }
                        } else kill = (Properties.Settings.Default.batchMode == Properties.Settings.BatchMode.Stop);
                    }
                    if(kill || abort) {
                        //proc.Kill();
                        KillChildProcesses(proc);
                        if(!ReadStdOutThread.IsCompleted || !ReadStdErrThread.IsCompleted) {
                            System.Threading.Thread.Sleep(500);
                            abort = true;
                        }
                        controller_.appendOutput("処理を中断しました．\r\n");
                        readThread.EndInvoke(ReadStdOutThread);
                        readThread.EndInvoke(ReadStdErrThread);
                        throw new System.TimeoutException();
                    } else continue;
                }
            }
            // 残っているかもしれないのを読む．
            while(!ReadStdOutThread.IsCompleted || !ReadStdErrThread.IsCompleted) {
                System.Threading.Thread.Sleep(300);
            }
            readThread.EndInvoke(ReadStdOutThread);
            readThread.EndInvoke(ReadStdErrThread);
            controller_.appendOutput("\r\n");
            if(abort) throw new System.TimeoutException();
        }

        public static void KillChildProcesses(Process proc) {
            // taskkillを起動するのが早そう．
            using(var p = new Process()) {
                try {
                    p.StartInfo.FileName = "taskkill.exe";
                    p.StartInfo.Arguments = "/PID " + proc.Id.ToString() + " /T /F";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit(3000);
                    if(!p.HasExited) {
                        p.Kill();
                        proc.Kill();
                    }
                }
                catch(Win32Exception) { proc.Kill(); }
            }
        }
        #endregion

        public void AddInputPath(string path) {
            if(!Environments.ContainsKey("TEXINPUTS")) {
                string env;
                try {
                    env = Environment.GetEnvironmentVariable("TEXINPUTS");
                    if(env == null) env = "";
                }
                catch(System.Security.SecurityException) {
                    env = "";
                }
                if(!env.EndsWith(";")) env += ";";
                Environments["TEXINPUTS"] = env;
            }
            Environments["TEXINPUTS"] += path + ";";
        }

        public static string GetToolsPath() {
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location)),ShortToolPath);
        }
        public static readonly string ShortToolPath = "";
    }
}