using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TeX2img {
    class Converter {
        // ADS名
        public const string ADSName = "TeX2img.source";

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
            var allowedextension = new List<string> { ".eps", ".png", ".jpg", ".pdf", ".svg", ".emf" };
            if(!allowedextension.Contains(extension)) { 
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
        public static Encoding GetOutputEncoding(string latex,string arg) {
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
        
        public bool Convert() {
            SetImageMagickEnvironment();
            bool rv = generate(InputFile, OutputFile);

            if(Properties.Settings.Default.deleteTmpFileFlag) {
                try {
                    string tmpFileBaseName = Path.Combine(workingDir, Path.GetFileNameWithoutExtension(InputFile));
                    File.Delete(tmpFileBaseName + ".tex");
                    File.Delete(tmpFileBaseName + ".dvi");
                    File.Delete(tmpFileBaseName + ".log");
                    File.Delete(tmpFileBaseName + ".aux");
                    File.Delete(tmpFileBaseName + ".pdf");
                    File.Delete(tmpFileBaseName + ".eps");
                    File.Delete(tmpFileBaseName + ".emf");
                    File.Delete(tmpFileBaseName + ".svg");
                    File.Delete(tmpFileBaseName + ".png");
                    File.Delete(tmpFileBaseName + ".jpg");
                    File.Delete(tmpFileBaseName + ".tmp");
                    File.Delete(tmpFileBaseName + ".out");
                    for(int i = 1 ; ; ++i) {
                        if(
                            File.Exists(tmpFileBaseName + "-" + i + ".jpg") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".png") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".svg") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".emf") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".eps") ||
                            File.Exists(tmpFileBaseName + "-" + i + "-trim.eps") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".pdf")
                        ) {
                            File.Delete(tmpFileBaseName + "-" + i + ".jpg");
                            File.Delete(tmpFileBaseName + "-" + i + ".png");
                            File.Delete(tmpFileBaseName + "-" + i + ".svg");
                            File.Delete(tmpFileBaseName + "-" + i + ".emf"); 
                            File.Delete(tmpFileBaseName + "-" + i + ".eps");
                            File.Delete(tmpFileBaseName + "-" + i + "-trim.eps");
                            File.Delete(tmpFileBaseName + "-" + i + ".pdf");
                        } else break;
                    }
                }
                catch(UnauthorizedAccessException) {
                    controller_.appendOutput("一部の一時ファイルの削除に失敗しました．\r\n");
                }
            }
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

        /*
        int pdfpages(string pdfFile) {
            using(var proc = GetProcess()) {
                proc.ErrorDataReceived += ((s, e) => {
                    controller_.appendOutput(e.Data + "\r\n");
                });
                proc.StartInfo.FileName = Path.GetDirectoryName(setProcStartInfo(Properties.Settings.Default.platexPath)) + "\\pdfinfo.exe";
                if(!File.Exists(proc.StartInfo.FileName)) proc.StartInfo.FileName = which("pdfinfo.exe");
                if(!File.Exists(proc.StartInfo.FileName)) {
                    controller_.showPathError("pdfinfo.exe", "TeX ディストリビューション（pdfinfo）");
                    return -1;
                }
                proc.StartInfo.Arguments = "\"" + pdfFile + "\"";
                try {
                    proc.Start();
                    proc.BeginErrorReadLine();
                    Regex reg = new Regex("^Pages:[ \t]*([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    int page = -1;
                    while(!proc.StandardOutput.EndOfStream){
                        string line = proc.StandardOutput.ReadLine();
                        var m = reg.Match(line);
                        if(m.Success) {
                            // このParseは成功することが確定している．
                            page = Int32.Parse(m.Groups[1].Value);
                            proc.StandardOutput.ReadToEnd();
                            //break;
                        }
                    }
                    proc.WaitForExit();
                    proc.CancelErrorRead();
                    return page;
                }
                catch(Win32Exception) {
                    controller_.showPathError("pdfinfo.exe", "TeX ディストリビューション（pdfinfo）");
                    return -1;
                }
            }
        }*/

        private bool pdf2eps(string inputFileName, string outputFileName, int resolution, int page) {
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                if(proc.StartInfo.FileName == ""){
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

        private bool eps2img(string inputFileName, string outputFileName) {
            string extension = Path.GetExtension(outputFileName).ToLower();
            string baseName = Path.GetFileNameWithoutExtension(inputFileName);
            string inputEpsFileName = baseName + ".eps";

            if(Properties.Settings.Default.useMagickFlag) {
                // ImageMagick を利用した画像変換
                #region ImageMagick を利用して JPEG/PNG を生成

                try {
                    ProcessStartInfo startinfo = new ProcessStartInfo();
                    using(var proc = GetProcess()) {
                        proc.StartInfo.FileName = getImageMagickPath();
                        if(proc.StartInfo.FileName == "") return false;
                        proc.StartInfo.Arguments = "-density " + (72 * Properties.Settings.Default.resolutionScale) + "x" + (72 * Properties.Settings.Default.resolutionScale) + "% " + inputEpsFileName + " " + outputFileName;
                        ReadOutputs(proc, "Imagemagick の実行");
                        startinfo = proc.StartInfo;
                    }
                    int margintimes = Properties.Settings.Default.yohakuUnitBP ? Properties.Settings.Default.resolutionScale : 1;

                    if(Properties.Settings.Default.topMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            //                        proc_.StartInfo.Arguments = "-splice 0x" + topMargin_ + " -gravity north " + outputFileName + " " + outputFileName;
                            proc.StartInfo.Arguments = "-splice 0x" + Properties.Settings.Default.topMargin * margintimes + " -gravity north " + outputFileName + " " + outputFileName;
                            ReadOutputs(proc, "Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.bottomMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice 0x" + Properties.Settings.Default.bottomMargin * margintimes + " -gravity south " + outputFileName + " " + outputFileName;
                            ReadOutputs(proc, "Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.rightMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice " + Properties.Settings.Default.rightMargin * margintimes + "x0 -gravity east " + outputFileName + " " + outputFileName;
                            ReadOutputs(proc, "Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.leftMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice " + Properties.Settings.Default.leftMargin * margintimes + "x0 -gravity west " + outputFileName + " " + outputFileName;
                            ReadOutputs(proc, "Imagemagick の実行");
                        }
                    }
                    if(extension == ".png") {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            if(Properties.Settings.Default.transparentPngFlag) proc.StartInfo.Arguments = "-transparent white " + outputFileName + " " + outputFileName;
                            else proc.StartInfo.Arguments = "-background white -alpha off " + outputFileName + " " + outputFileName;
                            ReadOutputs(proc, "Imagemagick の実行");
                        }
                    }
                }
                catch(Win32Exception) {
                    controller_.showImageMagickError();
                    return false;
                }
                catch(TimeoutException) {
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showGenerateError();
                    return false;
                }

                #endregion
            } else {
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
                #endregion

                #region 最後に目的の画像形式に変換
                string device = "jpeg";

                if(extension == ".png") {
                    device = Properties.Settings.Default.transparentPngFlag ? "pngalpha" : "png256";
                }

                string arg;
                using(var proc = GetProcess()) {
                    proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                    if(proc.StartInfo.FileName == "") {
                        controller_.showPathError("gswin32c.exe", "Ghostscript");
                        return false;
                    }
                    proc.StartInfo.Arguments = arg + String.Format("-q -sDEVICE={0} -sOutputFile={1} -dNOPAUSE -dBATCH -dPDFFitPage -r{2} -g{3}x{4} {5}", device, outputFileName, 72 * Properties.Settings.Default.resolutionScale, (int) ((righttop_x - leftbottom_x) * Properties.Settings.Default.resolutionScale), (int) ((righttop_y - leftbottom_y) * Properties.Settings.Default.resolutionScale), trimEpsFileName);
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
            }
            return true;
        }
        bool pdf2img(string inputFilename, string outputFileName) {
            var extension = Path.GetExtension(outputFileName).ToLower();
            controller_.appendOutput("TeX2img: Convert PDF to " + extension.Substring(1).ToUpper() + "\n");
            using(var doc = new pdfium.PDFDocument(Path.Combine(workingDir, inputFilename)))
            using(var page = doc.GetPage(0)) {
                int width = (int)(page.Width*Properties.Settings.Default.resolutionScale);
                int height = (int)(page.Height*Properties.Settings.Default.resolutionScale);
                using(var bitmap = new System.Drawing.Bitmap(width, height))
                using(var g = System.Drawing.Graphics.FromImage(bitmap)){
                    System.Drawing.Brush brush;
                    if(extension == ".png" && Properties.Settings.Default.transparentPngFlag) brush = System.Drawing.Brushes.Transparent;
                    else brush = System.Drawing.Brushes.White;
                    g.FillRectangle(brush, new System.Drawing.Rectangle(0, 0, width, height));
                    var hdc = g.GetHdc();
                    try{
                        page.Draw(hdc,width,height);
                    }
                    finally{
                        g.ReleaseHdc(hdc);
                    }
                    bitmap.Save(Path.Combine(workingDir,outputFileName), 
                        (extension == ".png" ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg));
                    
                }
            }
            return true;
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
                } else return true;
            }
        }

        bool eps2svg(string inputFileName, string outputFileName) {
            var dvisvgm = which("dvisvgm");
            if(dvisvgm == "") {
                controller_.showPathError("dvisvgm.exe", "dvisvgm.exe");
                return false;
            }
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = dvisvgm;
                proc.StartInfo.Arguments = "-E --output=\"" + outputFileName + "\" \"" + inputFileName + "\"";
                try {
                    ReadOutputs(proc, "dvisvgm の実行");
                }
                catch(Win32Exception) {
                    controller_.showPathError("dvidvgm.exe", "TeX ディストリビューション（dvidvgm）");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showPathError("dvidvgm.exe", "TeX ディストリビューション（dvidvgm）");
                    return false;
                } else return true;
            }
        }

        bool pdf2emf(string inputFileName, string outputFileName, int resolution) {
            controller_.appendOutput("TeX2img: Convert PDF to EMF\n");
            using(var pdfdoc = new pdfium.PDFDocument(Path.Combine(workingDir, inputFileName)))
            using(var pdfpage = pdfdoc.GetPage(0))
            using(var dummy = new System.Drawing.Bitmap(1, 1)) {
                //dummy.SetResolution(72,72);
                using(var dummyg = System.Drawing.Graphics.FromImage(dummy)) {
                    var dummyhdc = dummyg.GetHdc();
                    //System.Diagnostics.Debug.WriteLine((pdfpage.Width / pdfpage.Height).ToString());
                    double scale = dummy.HorizontalResolution / 72;
                    int width = (int) (pdfpage.Width * scale);
                    int height = (int) (pdfpage.Height * scale);
                    try {
                        using(var meta = new System.Drawing.Imaging.Metafile(Path.Combine(workingDir, outputFileName), dummyhdc, new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.MetafileFrameUnit.Pixel))
                        using(var g = System.Drawing.Graphics.FromImage(meta)) {
                            var head = meta.GetMetafileHeader();
                            var hdc = g.GetHdc();
                            try {
                                PInvoke.SIZE size = new PInvoke.SIZE();
                                var dpiX = PInvoke.GetDeviceCaps(hdc, PInvoke.DeviceCap.HORZSIZE);
                                var dpiY = PInvoke.GetDeviceCaps(hdc, PInvoke.DeviceCap.VERTSIZE);
                                PInvoke.SetMapMode(hdc, PInvoke.MM_ANISOTROPIC);
                                PInvoke.SetWindowExtEx(hdc, 1000, 1000, ref size);
                                // 指定した縦横の1.5倍になるっぽい
                                pdfpage.Draw(hdc, (int) (pdfpage.Width * scale * 1500), (int) (pdfpage.Height * 1500));
                            }
                            finally {
                                g.ReleaseHdc(hdc);
                            }
                        }
                    }
                    finally {
                        dummyg.ReleaseHdc(dummyhdc);
                    }
                }
            }
            return true;
        }

        bool generate(string inputTeXFilePath, string outputFilePath) {
            string extension = Path.GetExtension(outputFilePath).ToLower();
            string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);

            // とりあえずPDFを作る
            if(!tex2dvi(tmpFileBaseName + ".tex"))return false;
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
                if(File.Exists(outdvi) && System.IO.File.GetLastWriteTime(outdvi) > System.IO.File.GetLastWriteTime(outpdf)){
                    if(!dvi2pdf(tmpFileBaseName + ".dvi")) return false;
                }
            }
            
            // ページ数を取得
            int page;
            using(var doc = new pdfium.PDFDocument(Path.Combine(workingDir, tmpFileBaseName + ".pdf"))) {
                page = doc.GetPageCount();
            }
            // epsに変換する
            int resolution;
            if(Properties.Settings.Default.useLowResolution) epsResolution_ = 72 * Properties.Settings.Default.resolutionScale;
            else epsResolution_ = 20016;
            if(Properties.Settings.Default.useMagickFlag || extension == ".eps" || extension == ".pdf" || extension == ".svg" || extension == ".emf") resolution = epsResolution_;
            else resolution = 72 * Properties.Settings.Default.resolutionScale;
            for(int i = 1 ; i <= page ; ++i) {
                if(!pdf2eps(tmpFileBaseName + ".pdf", tmpFileBaseName + "-" + i + ".eps", resolution, i)) return false;
            }
            // そして最終的な変換
            bool addMargin = ((Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin + Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin) > 0);
            for(int i = 1 ; i <= page ; ++i) {
                switch(extension) {
                case ".png":
                case ".jpg":
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + ".pdf")) return false;
                    if(!pdf2img(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + extension)) return false;
                    //if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return false;
                    break;
                case ".pdf":
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return false;
                    break;
                case ".eps":
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    break;
                case ".svg":
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    if(!eps2svg(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return false;
                    break;
                case ".emf":
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + ".pdf")) return false;
                    if(!pdf2emf(tmpFileBaseName + "-" + i + ".pdf", tmpFileBaseName + "-" + i + ".emf", resolution)) return false;
                    break;
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
                            try {
                                using(var fs = AlternativeDataStream.WriteAlternativeDataStream(f, ADSName))
                                using(var ws = new StreamWriter(fs, new UTF8Encoding(false))) {
                                    ws.Write(srctext);
                                }
                            }
                            // 例外は無視
                            catch(IOException) { }
                            catch(NotImplementedException) { }
                        }
                    }
                }
                // 例外は無視
                catch(IOException) { }
            }
            return true;
        }

        private void SetImageMagickEnvironment() {
            try {
                if(Environments.ContainsKey("MAGICK_GHOSTSCRIPT_PATH")) return;
                if(Environment.GetEnvironmentVariable("MAGICK_GHOSTSCRIPT_PATH") != null) return;
                // ImageMagickのための準備
                if(Properties.Settings.Default.gsPath != "") {
                    string path = Converter.setProcStartInfo(Properties.Settings.Default.gsPath);
                    if(Path.GetFileName(path).ToLower() == "rungs.exe") {
                        string gswincPath = Path.GetDirectoryName(path);//...\win32
                        gswincPath = Path.GetDirectoryName(gswincPath);//...bin
                        gswincPath = Path.GetDirectoryName(gswincPath);
                        gswincPath += "\\tlpkg\\tlgs\\bin";
                        if(Directory.Exists(gswincPath)) {
                            Environments.Add("MAGICK_GHOSTSCRIPT_PATH", gswincPath);
                        }
                    }
                }
            }
            catch { }// 例外は無視
        }

        private string getImageMagickPath() {
            string convertPath = Converter.which("convert");
            if(convertPath == "" || convertPath.ToLower() == Path.Combine(Environment.SystemDirectory, "convert.exe").ToLower()) {
                controller_.showImageMagickError();
                return "";
            } else return convertPath;

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

        static class PInvoke {
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            public struct SIZE {
                public int cx;
                public int cy;
            }
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern int SetMapMode(IntPtr hdc, int fnMapMode);
            public static int MM_TEXT = 1;
            public static int MM_LOMETRIC = 2;
            public static int MM_HIMETRIC = 3;
            public static int MM_LOENGLISH = 4;
            public static int MM_HIENGLISH = 5;
            public static int MM_TWIPS = 6;
            public static int MM_ISOTROPIC = 7;
            public static int MM_ANISOTROPIC = 8;
            public static int MM_MIN = MM_TEXT;
            public static int MM_MAX = MM_ANISOTROPIC;
            public static int MM_MAX_FIXEDSCALE = MM_TWIPS;
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool SetWindowExtEx(IntPtr hdc, int nXExtent, int nYExtent, ref SIZE lpSize);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool SetViewportExtEx(IntPtr hdc, int nXExtent, int nYExtent, ref SIZE lpSize);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);
            public enum DeviceCap {
                DRIVERVERSION = 0,
                TECHNOLOGY = 2,
                HORZSIZE = 4,
                VERTSIZE = 6,
                HORZRES = 8,
                VERTRES = 10,
                BITSPIXEL = 12,
                PLANES = 14,
                NUMBRUSHES = 16,
                NUMPENS = 18,
                NUMMARKERS = 20,
                NUMFONTS = 22,
                NUMCOLORS = 24,
                PDEVICESIZE = 26,
                CURVECAPS = 28,
                LINECAPS = 30,
                POLYGONALCAPS = 32,
                TEXTCAPS = 34,
                CLIPCAPS = 36,
                RASTERCAPS = 38,
                ASPECTX = 40,
                ASPECTY = 42,
                ASPECTXY = 44,
                SHADEBLENDCAPS = 45,
                LOGPIXELSX = 88,
                LOGPIXELSY = 90,
                SIZEPALETTE = 104,
                NUMRESERVED = 106,
                COLORRES = 108,
                PHYSICALWIDTH = 110,
                PHYSICALHEIGHT = 111,
                PHYSICALOFFSETX = 112,
                PHYSICALOFFSETY = 113,
                SCALINGFACTORX = 114,
                SCALINGFACTORY = 115,
                VREFRESH = 116,
                DESKTOPVERTRES = 117,
                DESKTOPHORZRES = 118,
                BLTALIGNMENT = 119
            }
        }
    }
}
