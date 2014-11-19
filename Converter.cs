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
        public Converter(IOutputController controller) {
            controller_ = controller;
        }
        Dictionary<string, string> Environments = new Dictionary<string,string>();

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
            proc.OutputDataReceived += proc_OutputDataReceived;
            return proc;
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            controller_.appendOutput(e.Data + "\r\n");
            controller_.scrollOutputTextBoxToEnd();
        }

        public bool CheckFormat(string outputFilePath) {
            return CheckFormat(outputFilePath, controller_);
        }

        public static bool CheckFormat(string outputFilePath,IOutputController controller) {
            string extension = Path.GetExtension(outputFilePath).ToLower();
            if(extension != ".eps" && extension != ".png" && extension != ".jpg" && extension != ".pdf") {
                if(controller != null)controller.showExtensionError(outputFilePath);
                return false;
            }
            return true;
        }

        public void Convert(string inputTeXFilePath, string outputFilePath) {
            workingDir = Path.GetDirectoryName(inputTeXFilePath);
            SetImageMagickEnvironment();
            generate(inputTeXFilePath, outputFilePath);

            if(Properties.Settings.Default.deleteTmpFileFlag) {
                try {
                    string tmpFileBaseName = Path.Combine(workingDir,Path.GetFileNameWithoutExtension(inputTeXFilePath));
                    File.Delete(tmpFileBaseName + ".tex");
                    File.Delete(tmpFileBaseName + ".dvi");
                    File.Delete(tmpFileBaseName + ".log");
                    File.Delete(tmpFileBaseName + ".aux");
                    File.Delete(tmpFileBaseName + ".pdf");
                    File.Delete(tmpFileBaseName + ".eps");
                    File.Delete(tmpFileBaseName + ".emf");
                    File.Delete(tmpFileBaseName + ".png");
                    File.Delete(tmpFileBaseName + ".jpg");
                    File.Delete(tmpFileBaseName + ".tmp");
                    for(int i = 1 ;  ; ++i) {
                        if(
                            File.Exists(tmpFileBaseName + "-" + i + ".jpg") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".png") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".eps") ||
                            File.Exists(tmpFileBaseName + "-" + i + "-trim.eps") ||
                            File.Exists(tmpFileBaseName + "-" + i + ".pdf")
                        ) {
                            File.Delete(tmpFileBaseName + "-" + i + ".jpg");
                            File.Delete(tmpFileBaseName + "-" + i + ".png");
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
        }

        public static string setProcStartInfo(String path) {
			string dummy;
			return setProcStartInfo(path,out dummy);
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
            controller_.scrollOutputTextBoxToEnd();
        }

        private bool tex2dvi(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string arg;
            ProcessStartInfo startinfo = GetProcessStartInfo();
            startinfo.FileName = setProcStartInfo(Properties.Settings.Default.platexPath, out arg);
            if(Properties.Settings.Default.platexPath == "") {
                controller_.showPathError("platex.exe", "TeX ディストリビューション");
                return false;
            }
            
            startinfo.Arguments = arg;
            if(Properties.Settings.Default.encode.Substring(0, 1) != "_") startinfo.Arguments += "-no-guess-input-enc -kanji=" + Properties.Settings.Default.encode + " ";
            startinfo.Arguments += "-interaction=nonstopmode " + baseName + ".tex";

            try {
                if(Properties.Settings.Default.guessLaTeXCompile) {
                    var analyzer = new AnalyzeLaTeXCompile(Path.Combine(workingDir,fileName));
                    analyzer.UseBibtex = analyzer.UseMakeIndex = false;
                    int i = 0;
                    while(analyzer.Check() != AnalyzeLaTeXCompile.Program.Done){
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc, "TeX ソースのコンパイル");
                            if(!Properties.Settings.Default.ignoreErrorFlag && proc.ExitCode != 0){
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
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"TeX ソースのコンパイル");
                            if(!Properties.Settings.Default.ignoreErrorFlag && proc.ExitCode != 0) {
                                controller_.showGenerateError();
                                return false;
                            }
                        }
                    }
                }
            }
            catch(Win32Exception) {
                controller_.showPathError("platex.exe", "TeX ディストリビューション");
                return false;
            }

            if(!File.Exists(Path.Combine(workingDir, baseName + ".dvi"))) {
                controller_.showGenerateError();
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
                    controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション");
                    return false;
                }
                proc.StartInfo.Arguments = arg + "-vv -o " + baseName + ".pdf " + baseName + ".dvi";

                try {
                    printCommandLine(proc);
                    proc.Start();
                    // 出力は何故か標準エラー出力から出てくる
                    ReadOutputs(proc,"DVI から PDF への変換");
                }
                catch(Win32Exception) {
                    controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション");
                    return false;
                }
                if(proc.ExitCode != 0 || !File.Exists(Path.Combine(workingDir, baseName + ".pdf"))) {
                    controller_.showGenerateError();
                    return false;
                }
            }
            return true;
        }

        int pdfpages(string pdfFile) {
            using(var proc = GetProcess()){
                proc.ErrorDataReceived += proc_OutputDataReceived;
                proc.StartInfo.FileName = Path.GetDirectoryName(setProcStartInfo(Properties.Settings.Default.platexPath)) + "\\pdfinfo.exe";
                if(!File.Exists(proc.StartInfo.FileName)) proc.StartInfo.FileName = which("pdfinfo.exe");
                if(!File.Exists(proc.StartInfo.FileName)) { 
                    controller_.showPathError("pdfinfo.exe", "TeX ディストリビューション");
                    return -1;
                }
                proc.StartInfo.Arguments = "\"" + pdfFile + "\"";
                try {
                    proc.Start();
                    proc.BeginErrorReadLine();
                    Regex reg = new Regex("^Pages:[ \t]*([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    int page = -1;
                    while(proc.StandardOutput.Peek() != -1) {
                        string line = proc.StandardOutput.ReadLine();
                        var m = reg.Match(line);
                        if(m.Success) {
                            page = Int32.Parse(m.Groups[1].Value);
                            proc.StandardOutput.ReadToEnd();
                            break;
                        }
                    }
                    proc.WaitForExit();
                    proc.CancelErrorRead();
                    return page;
                }
                catch(Win32Exception) {
                    controller_.showPathError("pdfinfo.exe", "TeX ディストリビューション");
                    return -1;
                }
            }
        }

        private bool pdf2eps(string inputFileName, string outputFileName,int resolution,int page){
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                proc.StartInfo.Arguments = arg + "-q -sDEVICE=" + Properties.Settings.Default.gsDevice + " -dFirstPage=" + page + " -dLastPage=" + page;
                if(Properties.Settings.Default.gsDevice == "eps2write") proc.StartInfo.Arguments += " -dNoOutputFonts";
                proc.StartInfo.Arguments += " -sOutputFile=\"" + outputFileName + "\" -dNOPAUSE -dBATCH -r" + resolution + " \"" + inputFileName + "\"";

                try {
                    printCommandLine(proc);
                    proc.Start();
                    ReadOutputs(proc,"PDF から EPS への変換");
                }
                catch(Win32Exception) {
                    controller_.showPathError(Path.GetFileName(Properties.Settings.Default.gsPath), "Ghostscript ");
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
            
            FileStream fs = new FileStream(Path.Combine(workingDir,inputEpsFileName), FileMode.Open, FileAccess.Read);
            if(fs.CanRead){
                byte[] inbuf = new byte[fs.Length];
                byte[] outbuf = new byte[fs.Length + 200];
                byte[] tmpbuf;
                fs.Read(inbuf, 0, (int)fs.Length);

                // 現在読んでいるinufの「行」の先頭
                int inp = 0;
                // inbufの現在読んでいる場所
                int q = 0;
                // outbufに書き込んだ量
                int outp = 0;
                bool bbfound = false, hiresbbfound = false;
                while(q < inbuf.Length) {
                    if(q == inbuf.Length - 1 || inbuf[q] == '\r' || inbuf[q] == '\n') {
                        string line = System.Text.Encoding.ASCII.GetString(inbuf,inp,q - inp);
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
                fs.Dispose();
                using(FileStream wfs = new System.IO.FileStream(Path.Combine(workingDir ,inputEpsFileName), FileMode.Open, FileAccess.Write)) {
                    wfs.Write(outbuf, 0, outp);
                }
            }
        }

        private void readBB(string inputEpsFileName, out decimal leftbottom_x, out decimal leftbottom_y, out decimal righttop_x, out decimal righttop_y) {
            Regex regex = new Regex(@"^\%\%BoundingBox\: (\d+) (\d+) (\d+) (\d+)$");

            leftbottom_x = 0;
            leftbottom_y = 0;
            righttop_x = 0;
            righttop_y = 0;

            using(StreamReader sr = new StreamReader(Path.Combine(workingDir,inputEpsFileName), Encoding.GetEncoding("shift_jis"))) {
                try {
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
                finally {
                    if(sr != null) {
                        sr.Close();
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
                        printCommandLine(proc);
                        proc.Start();
                        ReadOutputs(proc,"Imagemagick の実行");
                        startinfo = proc.StartInfo;
                    }
                    int margintimes = Properties.Settings.Default.yohakuUnitBP ? Properties.Settings.Default.resolutionScale : 1;

                    if(Properties.Settings.Default.topMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            //                        proc_.StartInfo.Arguments = "-splice 0x" + topMargin_ + " -gravity north " + outputFileName + " " + outputFileName;
                            proc.StartInfo.Arguments = "-splice 0x" + Properties.Settings.Default.topMargin * margintimes + " -gravity north " + outputFileName + " " + outputFileName;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.bottomMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice 0x" + Properties.Settings.Default.bottomMargin * margintimes + " -gravity south " + outputFileName + " " + outputFileName;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.rightMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice " + Properties.Settings.Default.rightMargin * margintimes + "x0 -gravity east " + outputFileName + " " + outputFileName;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"Imagemagick の実行");
                        }
                    }
                    if(Properties.Settings.Default.leftMargin > 0) {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            proc.StartInfo.Arguments = "-splice " + Properties.Settings.Default.leftMargin * margintimes + "x0 -gravity west " + outputFileName + " " + outputFileName;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"Imagemagick の実行");
                        }
                    }
                    if(extension == ".png") {
                        using(var proc = GetProcess()) {
                            proc.StartInfo = startinfo;
                            if(Properties.Settings.Default.transparentPngFlag) proc.StartInfo.Arguments = "-transparent white " + outputFileName + " " + outputFileName;
                            else proc.StartInfo.Arguments = "-background white -alpha off " + outputFileName + " " + outputFileName;
                            printCommandLine(proc);
                            proc.Start();
                            ReadOutputs(proc,"Imagemagick の実行");
                        }
                    }
                }
                catch(Win32Exception) {
                    controller_.showImageMagickError();
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showImageMagickError();
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
                    proc.StartInfo.Arguments = arg + String.Format("-q -sDEVICE={0} -sOutputFile={1} -dNOPAUSE -dBATCH -dPDFFitPage -r{2} -g{3}x{4} {5}", device, outputFileName, 72 * Properties.Settings.Default.resolutionScale, (int) ((righttop_x - leftbottom_x) * Properties.Settings.Default.resolutionScale), (int) ((righttop_y - leftbottom_y) * Properties.Settings.Default.resolutionScale), trimEpsFileName);
                    try {
                        printCommandLine(proc);
                        proc.Start();
                        ReadOutputs(proc, "Ghostscript の実行");
                    }
                    catch(Win32Exception) {
                        controller_.showPathError(Path.GetFileName(proc.StartInfo.FileName), "Ghostscript ");
                        return false;
                    }
                }
                #endregion
                #endregion
            }
            return true;
        }

        public bool eps2pdf(string inputFileName, string outputFileName) {
            string arg;
            using(var proc = GetProcess()) {
                proc.StartInfo.FileName = setProcStartInfo(Properties.Settings.Default.gsPath, out arg);
                proc.StartInfo.Arguments = arg + "-q -sDEVICE=pdfwrite -dNOPAUSE -dBATCH -dEPSCrop -sOutputFile=\"" + outputFileName + "\" \"" + inputFileName + "\"";
                try {
                    printCommandLine(proc);
                    proc.Start();
                    ReadOutputs(proc,"Ghostscript の実行");

                }
                catch(Win32Exception) {
                    controller_.showPathError(Path.GetFileName(proc.StartInfo.FileName), "Ghostscript ");
                    return false;
                }
                if(!File.Exists(Path.Combine(workingDir, outputFileName))) {
                    controller_.showPathError(Path.GetFileName(proc.StartInfo.FileName), "Ghostscript ");
                    return false;
                } else return true;
            }
        }

  

        public void generate(string inputTeXFilePath, string outputFilePath) {
            string extension = Path.GetExtension(outputFilePath).ToLower();

            string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);

            // とりあえずPDFを作る
            if(!tex2dvi(tmpFileBaseName + ".tex") || !dvi2pdf(tmpFileBaseName + ".dvi")) return;

            
            // ページ数を取得
            int page = pdfpages(tmpFileBaseName + ".pdf");
            if(page == -1) {
                controller_.showGenerateError();
                return;
            }
            // epsに変換する
            int resolution;
            if(Properties.Settings.Default.useLowResolution) epsResolution_ = 72 * Properties.Settings.Default.resolutionScale;
            else epsResolution_ = 20016;
            if(Properties.Settings.Default.useMagickFlag || extension == ".eps" || extension == ".pdf") resolution = epsResolution_;
            else resolution = 72 * Properties.Settings.Default.resolutionScale;
            for(int i = 1 ; i <= page ; ++i) {
                if(!pdf2eps(tmpFileBaseName + ".pdf",tmpFileBaseName + "-" + i + ".eps",resolution,i))return;
            }
            // そして最終的な変換
            bool addMargin = ((Properties.Settings.Default.leftMargin + Properties.Settings.Default.rightMargin + Properties.Settings.Default.topMargin + Properties.Settings.Default.bottomMargin) > 0);
            for(int i = 1 ; i <= page ; ++i) {
                if(extension == ".pdf") {
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                    if(!eps2pdf(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return;
                } else if(extension == ".png" || extension == ".jpg") {
                    if(!eps2img(tmpFileBaseName + "-" + i + ".eps", tmpFileBaseName + "-" + i + extension)) return;
                } else {
                    if(addMargin) enlargeBB(tmpFileBaseName + "-" + i + ".eps");
                }
            }

            // 出力テキストボックスを最後の行までスクロール
            controller_.scrollOutputTextBoxToEnd();

            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if(outputDirectory != "" && !Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            // 出力ファイルをターゲットディレクトリにコピー
            try {
                if(page == 1) {
                    File.Copy(Path.Combine(workingDir,tmpFileBaseName + "-1" + extension), outputFilePath,true);
                    if(Properties.Settings.Default.previewFlag) Process.Start(outputFilePath);
                } else {
                    string outputFilePathBaseName = Path.Combine(Path.GetDirectoryName(outputFilePath),Path.GetFileNameWithoutExtension(outputFilePath));
                    for(int i = 1 ; i <= page ; ++i) {
                        File.Copy(Path.Combine(workingDir,tmpFileBaseName + "-" + i + extension), outputFilePathBaseName + "-" + i + extension,true);
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
        }

        private void SetImageMagickEnvironment() {
            if(Environments.ContainsKey("MAGICK_GHOSTSCRIPT_PATH")) return;
            // ImageMagickのための準備
            if(Properties.Settings.Default.gsPath != "") {
                try {
                    string path = Converter.setProcStartInfo(Properties.Settings.Default.gsPath);
                    if(Path.GetFileName(path).ToLower() == "rungs.exe") {
                        if(Environment.GetEnvironmentVariable("MAGICK_GHOSTSCRIPT_PATH") == null){
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
                catch { }
            }
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
        void ReadOutputs(Process proc,string freezemsg) {
            // procのままアクセスしたら例外でた．
            var StandardOutputStream = proc.StandardOutput;
            var ReadStdOutThread = new System.Threading.Thread(() => {
                while(!StandardOutputStream.EndOfStream) {
                    controller_.appendOutput(StandardOutputStream.ReadToEnd());
                    controller_.scrollOutputTextBoxToEnd();
                }
            });
            ReadStdOutThread.Start();
            // 標準出力→標準エラー出力の順番で表示させるために，エラーの方は一時変数に保持．
            string errmsg = "";
            while(true) {
                errmsg += proc.StandardError.ReadToEnd();
                // 10秒待つ
                proc.WaitForExit(10000);
                if(proc.HasExited) {
                    break;
                } else {
                    if(MessageBox.Show(
                        freezemsg + "に時間がかかっているようです．\n" +
                        "フリーズしている可能性もありますが，このまま実行を続けますか？",
                        "TeX2img",
                        MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        continue;
                    } else {
                        proc.Kill();
                        if(ReadStdOutThread.IsAlive) ReadStdOutThread.Abort();
                        controller_.appendOutput(errmsg);
                        controller_.scrollOutputTextBoxToEnd();
                        return;
                    }
                }
            }
            // 残っているかもしれないのを読む．
            while(ReadStdOutThread.IsAlive) {
                System.Threading.Thread.Sleep(300);
            }
            while(!proc.StandardError.EndOfStream) {
                errmsg += proc.StandardError.ReadToEnd();
            }
            controller_.appendOutput(errmsg);
            controller_.scrollOutputTextBoxToEnd();
            controller_.appendOutput("\r\n");
        }
    }
}
