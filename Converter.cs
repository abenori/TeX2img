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

        string platexPath_, dvipdfmxPath_, gsPath_,encode_;
        int resolutionScale_;
        decimal leftMargin_, rightMargin_, topMargin_, bottomMargin_;
        bool useMagickFlag_, transparentPngFlag_, showOutputWindowFlag_, previewFlag_, deleteTmpFileFlag_, ignoreErrorFlag_,yohakuUnitBP_;
        IOutputController controller_;
        Process proc_;
        const int epsResolution_ = 20016;

        public Converter(string platexPath, string dvipdfmxPath, string gsPath,string encode,
                  int resolutionScale, decimal leftMargin, decimal rightMargin, decimal topMargin, decimal bottomMargin,bool yohakuUnitBP,
                        bool useMagickFlag, bool transparentPngFlag, bool showOutputWindowFlag, bool previewFlag, bool deleteTmpFileFlag, bool ignoreErrorFlag,
                            IOutputController controller) {
            platexPath_ = platexPath;
            dvipdfmxPath_ = dvipdfmxPath;
            gsPath_ = gsPath;
            encode_ = encode;
            resolutionScale_ = resolutionScale;
            leftMargin_ = leftMargin;
            rightMargin_ = rightMargin;
            topMargin_ = topMargin;
            bottomMargin_ = bottomMargin;
            useMagickFlag_ = useMagickFlag;
            transparentPngFlag_ = transparentPngFlag;
            showOutputWindowFlag_ = showOutputWindowFlag;
            previewFlag_ = previewFlag;
            deleteTmpFileFlag_ = deleteTmpFileFlag;
            ignoreErrorFlag_ = ignoreErrorFlag;
            yohakuUnitBP_ = yohakuUnitBP;
            controller_ = controller;
        }

        public bool CheckFormat(string outputFilePath) {
            string extension = Path.GetExtension(outputFilePath).ToLower();
            if(extension != ".eps" && extension != ".png" && extension != ".jpg" && extension != ".pdf") {
                controller_.showExtensionError();
                return false;
            }
            return true;
        }

        public void Convert(string inputTeXFilePath, string outputFilePath) {
            generate(inputTeXFilePath, outputFilePath);

            if(deleteTmpFileFlag_) {
                try {
                    string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);
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
                catch(UnauthorizedAccessException) { }
            }
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

        private void printCommandLine() {
            controller_.appendOutput(proc_.StartInfo.WorkingDirectory + ">\"" + proc_.StartInfo.FileName + "\" " + proc_.StartInfo.Arguments + "\r\n");
        }

        private bool tex2dvi(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            if(platexPath_ == "" || !File.Exists(platexPath_)) {
                controller_.showPathError("platex.exe", "TeX ディストリビューション");
                return false;
            }

            string arg;
            proc_.StartInfo.FileName = setProcStartInfo(platexPath_, out arg);
            proc_.StartInfo.Arguments = arg;
            if(encode_.Substring(0, 1) != "_") proc_.StartInfo.Arguments += "-no-guess-input-enc -kanji=" + encode_ + " ";
            proc_.StartInfo.Arguments += "-interaction=nonstopmode " + baseName + ".tex";

            try {
                printCommandLine();
                proc_.Start();
                controller_.appendOutput(proc_.StandardOutput.ReadToEnd());
                proc_.WaitForExit();
            }
            catch(Win32Exception) {
                controller_.showPathError("platex.exe", "TeX ディストリビューション");
                return false;
            }

            controller_.appendOutput("\r\n\r\n");

            if((!ignoreErrorFlag_ && proc_.ExitCode != 0) || !File.Exists(baseName + ".dvi")) {
                controller_.appendOutput(proc_.StandardError.ReadToEnd());
                controller_.showGenerateError();
                return false;
            }

            return true;
        }

        private bool dvi2pdf(string fileName) {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            if(dvipdfmxPath_ == "" || !File.Exists(dvipdfmxPath_)) {
                controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション");
                return false;
            }

            string arg;
            proc_.StartInfo.FileName = setProcStartInfo(dvipdfmxPath_, out arg);
            proc_.StartInfo.Arguments = arg + "-vv -o " + baseName + ".pdf " + baseName + ".dvi";

            try {
                printCommandLine();
                proc_.Start();
                controller_.appendOutput(proc_.StandardOutput.ReadToEnd());
                proc_.WaitForExit();
            }
            catch(Win32Exception) {
                controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション");
                return false;
            }
            if(proc_.ExitCode != 0 || !File.Exists(baseName + ".pdf")) {
                controller_.appendOutput(proc_.StandardError.ReadToEnd());
                controller_.showPathError("dvipdfmx.exe", "TeX ディストリビューション");
                return false;
            }
            return true;
        }

        private bool pdf2eps(string inputFileName, string outputFileName,int resolution) {
            string arg;
            proc_.StartInfo.FileName = setProcStartInfo(gsPath_, out arg);
            proc_.StartInfo.Arguments = arg + "-q -sDEVICE=epswrite -sOutputFile=\"" + outputFileName + "\" -dNOPAUSE -dBATCH -r" + resolution + " \"" + inputFileName + "\"";

            try {
                printCommandLine();
                proc_.Start();
                proc_.WaitForExit();
            }
            catch(Win32Exception) {
                controller_.showPathError(Path.GetFileName(gsPath_), "Ghostscript ");
                return false;
            }
            if(proc_.ExitCode != 0) {
                controller_.appendOutput(proc_.StandardOutput.ReadToEnd());
                controller_.appendOutput(proc_.StandardError.ReadToEnd());
                controller_.showGenerateError();
                return false;
            }

            return true;
        }


        private void enlargeBB(string inputEpsFileName) {
            Regex regexBB = new Regex(@"^\%\%(HiRes|)BoundingBox\: ([\d\.]+) ([\d\.]+) ([\d\.]+) ([\d\.]+)$");
            
            decimal leftbottom_x, leftbottom_y, righttop_x, righttop_y;
            List<String> lines = new List<string>();

            using(StreamReader sr = new StreamReader(inputEpsFileName, Encoding.GetEncoding("shift_jis"))) {
                try {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        Match match = regexBB.Match(line);
                        if(match.Success) {
                            leftbottom_x = System.Convert.ToDecimal(match.Groups[2].ToString());
                            leftbottom_y = System.Convert.ToDecimal(match.Groups[3].ToString());
                            righttop_x = System.Convert.ToDecimal(match.Groups[4].ToString());
                            righttop_y = System.Convert.ToDecimal(match.Groups[5].ToString());
                            string HiRes = match.Groups[1].Value;
                            if(HiRes == "") lines.Add(String.Format("%%BoundingBox: {0} {1} {2} {3}", (int) (leftbottom_x - leftMargin_), (int) (leftbottom_y - bottomMargin_), (int) (righttop_x + rightMargin_), (int) (righttop_y + topMargin_)));
                            else lines.Add(String.Format("%%HiResBoundingBox: {0} {1} {2} {3}", leftbottom_x - leftMargin_, leftbottom_y - bottomMargin_, righttop_x + rightMargin_, righttop_y + topMargin_));
                            continue;
                        }

                        lines.Add(line.TrimEnd('\r', '\n'));
                    }
                }
                finally {
                    if(sr != null) sr.Close();
                }
            }

            using(StreamWriter sw = new StreamWriter(inputEpsFileName, false, Encoding.GetEncoding("shift_jis"))) {
                try {
                    foreach(string line in lines) {
                        sw.WriteLine(line);
                    }
                }
                finally {
                    if(sw != null) sw.Close();
                }
            }
        }

        private void readBB(string inputEpsFileName, out decimal leftbottom_x, out decimal leftbottom_y, out decimal righttop_x, out decimal righttop_y) {
            Regex regex = new Regex(@"^\%\%BoundingBox\: (\d+) (\d+) (\d+) (\d+)$");

            leftbottom_x = 0;
            leftbottom_y = 0;
            righttop_x = 0;
            righttop_y = 0;

            using(StreamReader sr = new StreamReader(inputEpsFileName, Encoding.GetEncoding("shift_jis"))) {
                try {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        Match match = regex.Match(line);
                        if(match.Success) {
                            leftbottom_x = System.Convert.ToDecimal(match.Groups[1].ToString());
                            leftbottom_y = System.Convert.ToDecimal(match.Groups[2].ToString());
                            righttop_x = System.Convert.ToDecimal(match.Groups[3].ToString());
                            righttop_y = System.Convert.ToDecimal(match.Groups[4].ToString());
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


        private bool pdfcrop(string inputFileName, string outputFileName, bool addMargine) {
            proc_.StartInfo.FileName = Converter.which("pdfcrop.exe");
            if(proc_.StartInfo.FileName == "") return false;
            if(addMargine) proc_.StartInfo.Arguments = String.Format("--margins \"{0} {1} {2} {3}\" ", leftMargin_, topMargin_, rightMargin_, bottomMargin_);
            else proc_.StartInfo.Arguments = "";
            proc_.StartInfo.Arguments += inputFileName + " " + outputFileName;
            try {
                printCommandLine();
                proc_.Start();
                proc_.WaitForExit();
                if(proc_.ExitCode == 0) return true;
                else {
                    controller_.appendOutput(proc_.StandardOutput.ReadToEnd());
                    controller_.appendOutput(proc_.StandardError.ReadToEnd());
                    controller_.showGenerateError();
                    return false;
                }
            }
            catch(Win32Exception) {
                controller_.showPathError("pdfcrop.exe", "pdfcrop.exe");
            }
            return false;
        }

        private bool eps2img(string inputFileName, string outputFileName) {
            string extension = Path.GetExtension(outputFileName).ToLower();
            string baseName = Path.GetFileNameWithoutExtension(inputFileName);
            string inputEpsFileName = baseName + ".eps";

            if(useMagickFlag_) {
                // ImageMagick を利用した画像変換
                #region ImageMagick を利用して JPEG/PNG を生成

                proc_.StartInfo.FileName = getImageMagickPath();
                if(proc_.StartInfo.FileName == "") return false;
                try {
                    proc_.StartInfo.Arguments = "-density " + (72 * resolutionScale_) + "x" + (72 * resolutionScale_) + "% " + inputEpsFileName + " " + outputFileName;
                    printCommandLine();
                    proc_.Start();
                    proc_.WaitForExit();
                    int margintimes = yohakuUnitBP_ ? resolutionScale_ : 1;

                    if(topMargin_ > 0) {
//                        proc_.StartInfo.Arguments = "-splice 0x" + topMargin_ + " -gravity north " + outputFileName + " " + outputFileName;
                        proc_.StartInfo.Arguments = "-splice 0x" + topMargin_ * margintimes + " -gravity north " + outputFileName + " " + outputFileName;
                        printCommandLine();
                        proc_.Start();
                        proc_.WaitForExit();
                    }
                    if(bottomMargin_ > 0) {
//                        proc_.StartInfo.Arguments = "-splice 0x" + bottomMargin_ + " -gravity south " + outputFileName + " " + outputFileName;
                        proc_.StartInfo.Arguments = "-splice 0x" + bottomMargin_ * margintimes + " -gravity south " + outputFileName + " " + outputFileName;
                        printCommandLine();
                        proc_.Start();
                        proc_.WaitForExit();
                    }
                    if(rightMargin_ > 0) {
//                        proc_.StartInfo.Arguments = "-splice " + rightMargin_ + "x0 -gravity east " + outputFileName + " " + outputFileName;
                        proc_.StartInfo.Arguments = "-splice " + rightMargin_ * margintimes + "x0 -gravity east " + outputFileName + " " + outputFileName;
                        printCommandLine();
                        proc_.Start();
                        proc_.WaitForExit();
                    }
                    if(leftMargin_ > 0) {
//                        proc_.StartInfo.Arguments = "-splice " + leftMargin_ + "x0 -gravity west " + outputFileName + " " + outputFileName;
                        proc_.StartInfo.Arguments = "-splice " + leftMargin_ * margintimes + "x0 -gravity west " + outputFileName + " " + outputFileName;
                        printCommandLine();
                        proc_.Start();
                        proc_.WaitForExit();
                    }
                    if(extension == ".png") {
                        if(transparentPngFlag_) proc_.StartInfo.Arguments = "-transparent white " + outputFileName + " " + outputFileName;
                        else proc_.StartInfo.Arguments = "-background white -alpha off " + outputFileName + " " + outputFileName;
                        printCommandLine();
                        proc_.Start();
                        proc_.WaitForExit();
                    }
                }
                catch(Win32Exception) {
                    controller_.showImageMagickError();
                    return false;
                }
                if(!File.Exists(outputFileName)) {
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
                int margindevide = yohakuUnitBP_ ? 1 : resolutionScale_;
                leftbottom_x -= leftMargin_ / margindevide;
                leftbottom_y -= bottomMargin_ / margindevide;
                righttop_x += rightMargin_ / margindevide;
                righttop_y += topMargin_ / margindevide;
                #endregion

                #region 次にトリミングするためのEPSファイルを作成
                string trimEpsFileName = baseName + "-trim.eps";
                using(StreamWriter sw = new StreamWriter(trimEpsFileName, false, Encoding.GetEncoding("shift_jis"))) {
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
                    device = transparentPngFlag_ ? "pngalpha" : "png256";
                }

                string arg;
                proc_.StartInfo.FileName = setProcStartInfo(gsPath_, out arg);
                proc_.StartInfo.Arguments = arg + String.Format("-q -sDEVICE={0} -sOutputFile={1} -dNOPAUSE -dBATCH -dPDFFitPage -r{2} -g{3}x{4} {5}", device, outputFileName, 72 * resolutionScale_, (int) ((righttop_x - leftbottom_x) * resolutionScale_), (int) ((righttop_y - leftbottom_y) * resolutionScale_), trimEpsFileName);
                try {
                    printCommandLine();
                    proc_.Start();
                    proc_.WaitForExit();
                }
                catch(Win32Exception) {
                    controller_.showPathError(Path.GetFileName(proc_.StartInfo.FileName), "Ghostscript ");
                    return false;
                }
                #endregion
                #endregion
            }
            return true;
        }

        public bool eps2pdf(string inputFileName, string outputFileName) {
            /*
            if(useMagickFlag_) {
                proc_.StartInfo.FileName = getImageMagickPath();
                if(proc_.StartInfo.FileName == "") return false;
                proc_.StartInfo.Arguments = "\"" + inputFileName + "\" \"" + outputFileName + "\"";
                try {
                    printCommandLine();
                    proc_.Start();
                    proc_.WaitForExit();
                    if(proc_.ExitCode == 0) return true;
                    else {
                        controller_.appendOutput(proc_.StandardOutput.ReadToEnd());
                        controller_.appendOutput(proc_.StandardError.ReadToEnd());
                        controller_.showGenerateError();
                        return false;
                    }
                }
                catch(Win32Exception) {
                    controller_.showImageMagickError();
                    return false;
                }
            } else {
                */
            // Ghostscript を使った PDF 生成
            string arg;
            proc_.StartInfo.FileName = setProcStartInfo(gsPath_, out arg);
            proc_.StartInfo.Arguments = arg + "-q -sDEVICE=pdfwrite -dNOPAUSE -dBATCH -dEPSCrop -sOutputFile=\"" + outputFileName + "\" \"" + inputFileName + "\"";
            try {
                printCommandLine();
                proc_.Start();
                proc_.WaitForExit();
            }
            catch(Win32Exception) {
                controller_.showPathError(Path.GetFileName(proc_.StartInfo.FileName), "Ghostscript ");
                return false;
            }
            if(!File.Exists(outputFileName)) {
                controller_.showPathError(Path.GetFileName(proc_.StartInfo.FileName), "Ghostscript ");
                return false;
            } else return true;
            //            }
        }

  

        public void generate(string inputTeXFilePath, string outputFilePath) {
            string extension = Path.GetExtension(outputFilePath).ToLower();
            string tmpDir = Path.GetDirectoryName(inputTeXFilePath);
            Directory.SetCurrentDirectory(tmpDir);

            string tmpFileBaseName = Path.GetFileNameWithoutExtension(inputTeXFilePath);

            proc_ = new Process();
            proc_.StartInfo.WorkingDirectory = tmpDir;
            proc_.StartInfo.UseShellExecute = false;
            proc_.StartInfo.CreateNoWindow = true;
            proc_.StartInfo.RedirectStandardOutput = true;
            proc_.StartInfo.RedirectStandardError = true;

            // ImageMagickのための準備
            if(gsPath_ != "") {
                try {
                    string dummy;
                    string path = Converter.setProcStartInfo(gsPath_, out dummy);
                    if(Path.GetFileName(path).ToLower() == "rungs.exe") {
                        if(Environment.GetEnvironmentVariable("MAGICK_GHOSTSCRIPT_PATH") == null
                            && !proc_.StartInfo.EnvironmentVariables.ContainsKey("MAGICK_GHOSTSCRIPT_PATH")
                            ) {
                            string gswincPath = Path.GetDirectoryName(path);//...\win32
                            gswincPath = Path.GetDirectoryName(gswincPath);//...bin
                            gswincPath = Path.GetDirectoryName(gswincPath);
                            gswincPath += "\\tlpkg\\tlgs\\bin";
                            if(Directory.Exists(gswincPath)) {
                                proc_.StartInfo.EnvironmentVariables.Add("MAGICK_GHOSTSCRIPT_PATH", gswincPath);
                            }
                        }
                    }
                }
                catch { }
            }


            // とりあえずPDFを作る
            if(!tex2dvi(tmpFileBaseName + ".tex") || !dvi2pdf(tmpFileBaseName + ".dvi")) return;

            string tmpEpsFile;
//            if(extension == ".pdf") tmpEpsFile = tmpFileBaseName + ".eps";
//            else tmpEpsFile = tmpFileBaseName + "-%d.eps";
            tmpEpsFile = tmpFileBaseName + "-%d.eps";

            // epsに変換する
            int resolution;
            if(useMagickFlag_ || extension == ".eps" || extension == ".pdf") resolution = epsResolution_;
            else resolution = 72 * resolutionScale_;
            if(!pdf2eps(tmpFileBaseName + ".pdf", tmpEpsFile,resolution)) return;
            // 何枚できたか数える
            int page = 1;
            for( ; ; ++page) {
                if(!File.Exists(tmpFileBaseName + "-" + (page + 1) + ".eps")) break;
            }
            // 何故か一枚多くできるのよね
            File.Delete(tmpFileBaseName + "-" + page + ".eps");
            --page;
            // そして変換
            bool addMargin = ((leftMargin_ + rightMargin_ + topMargin_ + bottomMargin_) > 0);
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
            if(!Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            // 出力ファイルをターゲットディレクトリにコピー
            try {
                if(page == 1) {
                    File.Copy(tmpFileBaseName + "-1" + extension, outputFilePath,true);
                    if(previewFlag_) Process.Start(outputFilePath);
                } else {
                    string outputFilePathBaseName = Path.GetDirectoryName(outputFilePath) + "\\" + Path.GetFileNameWithoutExtension(outputFilePath);
                    for(int i = 1 ; i <= page ; ++i) {
                        File.Copy(tmpFileBaseName + "-" + i + extension, outputFilePathBaseName + "-" + i + extension,true);
                    }
                    outputFilePath = outputFilePathBaseName + "-1" + extension;
                    if(previewFlag_) Process.Start(outputFilePath);
                }

            }
            catch(UnauthorizedAccessException) {
                controller_.showUnauthorizedError(outputFilePath);
            }
            catch(IOException) {
                controller_.showIOError(outputFilePath);
            }

        }

        public static string guessgsPath() {
            string gs = Converter.which("gswin32c.exe");
            if(gs != "") return gs;
            gs = Converter.which("gswin64c.exe");
            if(gs != "") return gs;
            string platex = which("platex.exe");
            if(platex == "") return "";
            gs = System.IO.Path.GetDirectoryName(platex) + "\\rungs.exe";
            if(System.IO.File.Exists(gs)) return gs;
            else return "";
        }

        private string getImageMagickPath() {
            string convertPath = Converter.which("convert");
            if(convertPath == "" || convertPath.ToLower() == Path.Combine(Environment.SystemDirectory, "convert.exe").ToLower()) {
                controller_.showImageMagickError();
                return "";
            } else return convertPath;

        }

    }
}