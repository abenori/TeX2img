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
    class Converter {
        // ADS名
        public const string ADSName = "TeX2img.source";
        public static readonly string[] bmpExtensions = new string[] { ".jpg", ".png", ".bmp", ".gif", ".tiff" };
        public static readonly string[] vectorExtensions = new string[] { ".eps", ".pdf", ".emf", ".svg" };
        public static string[] imageExtensions {
            get { return bmpExtensions.Concat(vectorExtensions).ToArray(); }
        }

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

        IOutputController controller_;
        int epsResolution_ = 20016;
        string workingDir;
        string InputFile, OutputFile;
        public Converter(IOutputController controller, string inputTeXFilePath, string outputFilePath) {
            InputFile = inputTeXFilePath;
            OutputFile = outputFilePath;
            controller_ = controller;
            workingDir = Path.GetDirectoryName(inputTeXFilePath);
        }
        Dictionary<string, string> Environments = new Dictionary<string, string>();

        ProcessStartInfo GetProcessStartInfo() {
            var rv = new ProcessStartInfo();
            rv.UseShellExecute = false;
            rv.CreateNoWindow = true;
            rv.RedirectStandardOutput = true;
            rv.RedirectStandardError = true;
            rv.WorkingDirectory = workingDir;
            foreach(var e in Environments) {
                try { rv.EnvironmentVariables.Add(e.Key, e.Value); }
                catch(ArgumentException) { }
            }
            return rv;
        }
        Process GetProcess() {
            var proc = new Process();
            proc.StartInfo = GetProcessStartInfo();
            return proc;
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
            var l = Path.GetFileNameWithoutExtension(latex);
            return (l == "platex" || l == "uplatex" || l == "ptex" || l == "uptex");
        }
        static bool IsupTeX(string latex) {
            var l = Path.GetFileNameWithoutExtension(latex);
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

        List<string> generatedImageFiles = new List<string>();
        List<string> generatedTeXFilesWithoutExtension = new List<string>();

        public bool Convert() {
            if(GetInputEncoding().CodePage == Encoding.UTF8.CodePage) {
                Environments["command_line_encoding"] = "utf8";
            }

            bool rv = generate(InputFile, OutputFile);

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
            return rv;
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

        private void printCommandLine(Process proc) {
            controller_.appendOutput(proc.StartInfo.WorkingDirectory + ">\"" + proc.StartInfo.FileName + "\" " + proc.StartInfo.Arguments + "\r\n");
        }

        private bool tex2dvi(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
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
                if(Properties.Settings.Default.guessLaTeXCompile) {
                    var analyzer = new AnalyzeLaTeXCompile(Path.Combine(workingDir, fileName));
                    analyzer.UseBibtex = analyzer.UseMakeIndex = false;
                    int i = 0;
                    while(analyzer.Check() != AnalyzeLaTeXCompile.Program.Done) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            ReadOutputs(proc, "TeX ソースのコンパイル");
                            if(!Properties.Settings.Default.ignoreErrorFlag && proc.ExitCode != 0) {
                                controller_.showGenerateError();
                                return false;
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
                            if(!Properties.Settings.Default.ignoreErrorFlag && proc.ExitCode != 0) {
                                controller_.showGenerateError();
                                return false;
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
            generatedTeXFilesWithoutExtension.Add(Path.Combine(workingDir,baseName));

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
                proc.StartInfo.Arguments = arg + "-vv -o " + baseName + ".pdf " + baseName + ".dvi";

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
                if(proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, baseName + ".pdf"))) {
                    controller_.showGenerateError();
                    return false;
                }
            }
            return true;
        }

        private bool pdf2eps(string inputFileName, string outputFileName, int resolution, int page) {
            string arg;
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
                generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            }
            return true;
        }


        private void enlargeBB(string inputEpsFileName) {
            Regex regexBB = new Regex(@"^\%\%(HiRes|)BoundingBox\: ([\d\.]+) ([\d\.]+) ([\d\.]+) ([\d\.]+)$");
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
                        decimal leftbottom_x = System.Convert.ToDecimal(match.Groups[2].Value);
                        decimal leftbottom_y = System.Convert.ToDecimal(match.Groups[3].Value);
                        decimal righttop_x = System.Convert.ToDecimal(match.Groups[4].Value);
                        decimal righttop_y = System.Convert.ToDecimal(match.Groups[5].Value);
                        string HiRes = match.Groups[1].Value;
                        if(HiRes == "") {
                            bbfound = true;
                            line = String.Format("%%BoundingBox: {0} {1} {2} {3}", (int) (leftbottom_x - Properties.Settings.Default.leftMargin), (int) (leftbottom_y - Properties.Settings.Default.bottomMargin), (int) (righttop_x + Properties.Settings.Default.rightMargin), (int) (righttop_y + Properties.Settings.Default.topMargin));
                        } else {
                            hiresbbfound = true;
                            line = String.Format("%%HiResBoundingBox: {0} {1} {2} {3}", leftbottom_x - Properties.Settings.Default.leftMargin, leftbottom_y - Properties.Settings.Default.bottomMargin, righttop_x + Properties.Settings.Default.rightMargin, righttop_y + Properties.Settings.Default.topMargin);
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

        private void readBB(string inputEpsFileName, out decimal leftbottom_x, out decimal leftbottom_y, out decimal righttop_x, out decimal righttop_y) {
            Regex regex = new Regex(@"^\%\%BoundingBox\: (\d+) (\d+) (\d+) (\d+)$");

            leftbottom_x = 0;
            leftbottom_y = 0;
            righttop_x = 0;
            righttop_y = 0;

            using(StreamReader sr = new StreamReader(Path.Combine(workingDir, inputEpsFileName), Encoding.GetEncoding("shift_jis"))) {
                string line;
                while((line = sr.ReadLine()) != null) {
                    Match match = regex.Match(line);
                    if(match.Success) {
                        leftbottom_x = System.Convert.ToDecimal(match.Groups[1].Value);
                        leftbottom_y = System.Convert.ToDecimal(match.Groups[2].Value);
                        righttop_x = System.Convert.ToDecimal(match.Groups[3].Value);
                        righttop_y = System.Convert.ToDecimal(match.Groups[4].Value);
                        break;
                    }
                }
            }
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
                generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
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
                generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
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

        struct RectangleDecimal {
            public Decimal Left, Right, Bottom, Top;
        }
        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp,int page = 1) {
            return pdfcrop(inputFileName,outputFileName,use_bp,new List<int>(){page});
        }

        bool pdfcrop(string inputFileName, string outputFileName, bool use_bp, List<int> pages) {
            var tmpfile = Path.GetTempFileName();
            File.Delete(tmpfile);
            tmpfile = Path.GetFileNameWithoutExtension(tmpfile) + ".tex";

            var gspath = setProcStartInfo(Properties.Settings.Default.gsPath);
            var bbBox = new List<RectangleDecimal>();
            int margindevide = use_bp ? 1 : Properties.Settings.Default.resolutionScale;
            foreach(var page in pages) {
                var rect = new RectangleDecimal();
                using(var proc = GetProcess()) {
                    proc.StartInfo.FileName = gspath;
                    proc.StartInfo.Arguments = "-dBATCH -dNOPAUSE -q -sDEVICE=bbox -dFirstPage=" + page.ToString() + " -dLastPage=" + page.ToString() + " \"" + inputFileName + "\"";
                    proc.OutputDataReceived += ((s, e) => { System.Diagnostics.Debug.WriteLine("Std: " + e.Data); });
                    Regex regexBB = new Regex(@"^\%\%BoundingBox\: ([\d\.]+) ([\d\.]+) ([\d\.]+) ([\d\.]+)$");
                    try {
                        proc.Start();
                        proc.BeginOutputReadLine();
                        while(!proc.StandardError.EndOfStream) {
                            var line = proc.StandardError.ReadLine();
                            System.Diagnostics.Debug.WriteLine(line);
                            var match = regexBB.Match(line);
                            if(match.Success) {
                                rect.Left = System.Convert.ToDecimal(match.Groups[1].Value);
                                rect.Bottom =  System.Convert.ToDecimal(match.Groups[2].Value);
                                rect.Right = System.Convert.ToDecimal(match.Groups[3].Value);
                                rect.Top = System.Convert.ToDecimal(match.Groups[4].Value);
                                break;
                            }
                        }
                        proc.WaitForExit();
                    }
                    catch(Win32Exception) {
                        controller_.showPathError(gspath, "Ghostscript");
                        return false;
                    }
                }
                rect.Left -= Properties.Settings.Default.leftMargin / margindevide;
                rect.Bottom -= Properties.Settings.Default.bottomMargin / margindevide;
                rect.Right += Properties.Settings.Default.rightMargin / margindevide;
                rect.Top += Properties.Settings.Default.topMargin / margindevide;
                bbBox.Add(rect);
            }
            generatedTeXFilesWithoutExtension.Add(Path.Combine(workingDir,Path.GetFileNameWithoutExtension(tmpfile)));
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
                proc.StartInfo.Arguments = "-no-shell-escape  -interaction=batchmode \"" + tmpfile + "\"";
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
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            return true;
        }

        private bool eps2img(string inputFileName, string outputFileName) {
            string extension = Path.GetExtension(outputFileName).ToLower();
            string baseName = Path.GetFileNameWithoutExtension(inputFileName);
            string inputEpsFileName = baseName + ".eps";

            // Ghostscript を使ったJPEG,PNG生成
            #region Ghostscript を利用して JPEG/PNG を生成
            #region まずは生成したEPSファイルのバウンディングボックスを取得
            decimal leftbottom_x, leftbottom_y, righttop_x, righttop_y;
            readBB(inputEpsFileName, out leftbottom_x, out leftbottom_y, out righttop_x, out righttop_y);
            int margindevide = Properties.Settings.Default.yohakuUnitBP ? 1 : Properties.Settings.Default.resolutionScale;
            leftbottom_x -= Properties.Settings.Default.leftMargin / margindevide;
            leftbottom_y -= Properties.Settings.Default.bottomMargin / margindevide;
            righttop_x += Properties.Settings.Default.rightMargin / margindevide;
            righttop_y += Properties.Settings.Default.topMargin / margindevide;
            #endregion

            #region 次にトリミングするためのEPSファイルを作成
            string trimEpsFileName = baseName + "-trim.eps";
            using(StreamWriter sw = new StreamWriter(Path.Combine(workingDir, trimEpsFileName), false, Encoding.GetEncoding("shift_jis"))) {
                try {
                    sw.WriteLine("/NumbDict countdictstack def");
                    sw.WriteLine("1 dict begin");
                    sw.WriteLine("/showpage {} def");
                    sw.WriteLine("userdict begin");
                    sw.WriteLine("-{0}.000000 -{1}.000000 translate", (int) leftbottom_x, (int) leftbottom_y);
                    sw.WriteLine("1.000000 1.000000 scale");
                    sw.WriteLine("0.000000 0.000000 translate");
                    sw.WriteLine("({0}) run", inputEpsFileName);
                    sw.WriteLine("countdictstack NumbDict sub {end} repeat");
                    sw.WriteLine("showpage");
                }
                finally {
                    if(sw != null) sw.Close();
                }
            }
            generatedImageFiles.Add(Path.Combine(workingDir, trimEpsFileName));
            #endregion

            #region 最後に目的の画像形式に変換
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
                proc.StartInfo.Arguments = arg;
                proc.StartInfo.Arguments += String.Format(
                    "-q -sDEVICE={0} -sOutputFile={1} -dNOPAUSE -dBATCH -dPDFFitPage -dTextAlphaBits={2} -dGraphicsAlphaBits={2} -r{3} -g{4}x{5} {6}",
                    device, outputFileName, antialias, 
                    72 * Properties.Settings.Default.resolutionScale,
                    (int) ((righttop_x - leftbottom_x) * Properties.Settings.Default.resolutionScale),
                    (int) ((righttop_y - leftbottom_y) * Properties.Settings.Default.resolutionScale),
                    trimEpsFileName);
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
            #endregion
            #endregion
            generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
            return true;
        }

        bool pdf2img_pdfium(string inputFilename, string outputFileName,int pages = 0) {
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
                    generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
                    return true;
                }
            }
        }

        bool img2img_pdfium(string inputFileName, string outputFileName) {
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
                    generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
                    return true;
                }
            }
        }

        bool eps2pdf(string inputFileName, string outputFileName) {
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
                    generatedImageFiles.Add(Path.Combine(workingDir, outputFileName));
                    return true;
                }
            }
        }

        bool generate(string inputTeXFilePath, string outputFilePath) {
            string extension = Path.GetExtension(outputFilePath).ToLower();
            string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);

            // とりあえずPDFを作る
            if(!tex2dvi(tmpFileBaseName + ".tex")) return false;
            string outdvi = Path.Combine(workingDir, tmpFileBaseName + ".dvi");
            string outpdf = Path.Combine(workingDir, tmpFileBaseName + ".pdf");
            if(!File.Exists(outpdf)) {
                if(!File.Exists(outdvi)) {
                    controller_.showGenerateError();
                    return false;
                } else {
                    if(!dvi2pdf(tmpFileBaseName + ".dvi")) return false;
                }
            } else {
                if(File.Exists(outdvi) && System.IO.File.GetLastWriteTime(outdvi) > System.IO.File.GetLastWriteTime(outpdf)) {
                    if(!dvi2pdf(tmpFileBaseName + ".dvi")) return false;
                }
            }

            // ページ数を取得
            int page = pdfpages(Path.Combine(workingDir, tmpFileBaseName + ".pdf"));

            // .svg，テキスト情報保持な pdf は PDF から作る
            if(
                extension == ".svg" || 
                (extension == ".pdf" && !Properties.Settings.Default.outlinedText) ||
                extension == ".gif" && Properties.Settings.Default.transparentPngFlag
                ) {
                for(int i = 1 ; i <= page ; ++i) {
                    if(extension == ".svg") {
                        if(!pdfcrop(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".pdf", true, i)) return false;
                        if(!pdf2img_mudraw(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + ".svg")) return false;
                        if(Properties.Settings.Default.deleteDisplaySize) {
                            DeleteHeightAndWidthFromSVGFile(tmpFileBaseName + "-" + i + extension);
                        }
                    } else {
                        if(!pdfcrop(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".pdf", Properties.Settings.Default.yohakuUnitBP, i)) return false;
                        if(extension != ".pdf") {
                            if(!pdf2img_pdfium(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + extension)) return false;
                        }
                    }
                }
            } else {
                // それ以外はEPSを経由する．
                // epsに変換する
                int resolution;
                if(Properties.Settings.Default.useLowResolution) epsResolution_ = 72 * Properties.Settings.Default.resolutionScale;
                else epsResolution_ = 20016;
                if(vectorExtensions.Contains(extension)) resolution = epsResolution_;
                else resolution = 72 * Properties.Settings.Default.resolutionScale;
                for(int i = 1 ; i <= page ; ++i) {
                    if(!pdf2eps(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".eps", resolution, i)) return false;
                    bool addMargin = ((Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin + Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin) > 0);
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
                        if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return false;
                        break;
                    case ".gif":
                    case ".tiff":
                        if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + ".png")) return false;
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
            var outputFileNames = new List<string>();
            try {
                if(page == 1) {
                    File.Copy(Path.Combine(workingDir, tmpFileBaseName + "-1" + extension), outputFilePath, true);
                    outputFileNames.Add(outputFilePath);
                    if(Properties.Settings.Default.previewFlag) Process.Start(outputFilePath);
                } else {
                    string outputFilePathBaseName = Path.Combine(Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath));
                    for(int i = 1 ; i <= page ; ++i) {
                        File.Copy(Path.Combine(workingDir, tmpFileBaseName + "-" + i + extension), outputFilePathBaseName + "-" + i + extension, true);
                        outputFileNames.Add(outputFilePathBaseName + "-" + i + extension);
                    }
                    outputFilePath = outputFilePathBaseName + "-1" + extension;
                    if(Properties.Settings.Default.previewFlag) Process.Start(outputFilePath);
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
            return true;
        }

        // Error -> 同期，Output -> 非同期
        // でとりあえずデッドロックしなくなったのでこれでよしとする．
        // 両方非同期で駄目な理由がわかりません……．
        //
        // 非同期だと全部読み込んだかわからない気がしたので，スレッドを作成することにした．
        //
        // 結局どっちもスレッドを回すことにしてみた……．
        void ReadOutputs(Process proc, string freezemsg) {
            printCommandLine(proc);
            proc.Start();
            bool abort = false;
            object syncObj = new object();
            var thStart = new System.Threading.ParameterizedThreadStart((o) => {
                try {
                    StreamReader sr = (StreamReader) o;
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
            var ReadStdOutThread = new System.Threading.Thread(thStart);
            ReadStdOutThread.IsBackground = true;
            var ReadStdErrThread = new System.Threading.Thread(thStart);
            ReadStdErrThread.IsBackground = true;
            ReadStdOutThread.Start(proc.StandardOutput);
            ReadStdErrThread.Start(proc.StandardError);
            while(true) {
                // 10秒待つ
                proc.WaitForExit(10000);
                //proc.WaitForExit(1000);
                if(proc.HasExited) {
                    break;
                } else {
                    bool kill;
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
                    if(kill) {
                        //proc.Kill();
                        KillChildProcesses(proc);
                        if(ReadStdOutThread.IsAlive || ReadStdErrThread.IsAlive) {
                            System.Threading.Thread.Sleep(500);
                            abort = true;
                        }
                        controller_.appendOutput("処理を中断しました．\r\n");
                        throw new System.TimeoutException();
                    } else continue;
                }
            }
            // 残っているかもしれないのを読む．
            while(ReadStdOutThread.IsAlive || ReadStdErrThread.IsAlive) {
                System.Threading.Thread.Sleep(300);
            }
            controller_.appendOutput("\r\n");
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

        public void AddInputPath(string path) {
            if(!Environments.ContainsKey("TEXINUTS")) Environments["TEXINPUTS"] = "";
            Environments["TEXINPUTS"] += path + ";";
        }

        public static string GetToolsPath() {
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location)),ShortToolPath);
        }
        public static readonly string ShortToolPath = "";
    }
}