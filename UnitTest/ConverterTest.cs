using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeX2img;
using TeX2img.Properties;

namespace UnitTest {
    [TestClass]
    public class ConverterTest : IOutputController {
        void doEachTest() {
            tex2dvi_test(testfile + ".tex");
            dvi2pdf_test(testfile + ".dvi");
            pdfcrop_test(testfile + ".pdf");
            pdf2eps_test(testfile + ".pdf", Settings.Default.resolutionScale * 72);
            //eps2img_test(testfile + ".eps");
            //CallMethod(converter, "enlargeBB", testfile + ".eps", true);
            ps2pdf_test(testfile + ".eps");
            pdfpages_test(testfile + ".pdf");
            pdf2img_pdfium_test(testfile + ".pdf");
            Settings.Default.dvipdfmxPath = "dvips";
            dvi2ps_test(testfile + ".dvi");
            ps2pdf_test(testfile + ".ps");
        }
        [TestMethod]
        public void generateTest() {
            var vecExts = Converter.vectorExtensions;
            var bmpExts = new string[] { ".png", ".jpg" };
            var allExts = vecExts.Concat(bmpExts).ToArray();
            SetOutputDir("generate");
            PrepareTest();
            string[] sources = {
@"\documentclass{jsarticle}
\pagestyle{empty}
\begin{document}
あ$\frac{1}{2}$
\end{document}",
@"\documentclass{jsarticle}
\pagestyle{empty}
\begin{document}
あ$\frac{1}{2}$
\newpage
\mbox{}
\newpage
あ
\end{document}",
            };
            foreach (var source in sources) {
                Settings.Default.outlinedText = true;
                Settings.Default.transparentPngFlag = false;
                Settings.Default.useLowResolution = false;
                Settings.Default.useMagickFlag = true;
                Settings.Default.keepPageSize = false;
                doGenerateTest(source,"default", allExts);

                Settings.Default.outlinedText = false;
                doGenerateTest(source, "with-text", new string[] { ".pdf" });
                Settings.Default.outlinedText = true;

                Settings.Default.transparentPngFlag = true;
                doGenerateTest(source, "transparent", new string[] { ".png", ".tiff", ".emf" });
                Settings.Default.transparentPngFlag = false;

                Settings.Default.useLowResolution = true;
                doGenerateTest(source, "low-resolution", bmpExts);
                Settings.Default.useLowResolution = false;

                Settings.Default.useMagickFlag = false;
                doGenerateTest(source, "no-antialias",bmpExts);
                Settings.Default.useMagickFlag = true;

                Settings.Default.keepPageSize = true;
                doGenerateTest(source, "keep-pagesize");
                Settings.Default.keepPageSize = false;

                Settings.Default.keepPageSize = true;
                Settings.Default.transparentPngFlag = true;
                doGenerateTest(source, "keep-pagesize-transparent");
                Settings.Default.transparentPngFlag = false;
                Settings.Default.keepPageSize = false;

                Settings.Default.mergeOutputFiles = true;
                doGenerateTest(source, "merge", new string[] { ".svg", ".pdf", ".tiff", ".gif" });
                Settings.Default.mergeOutputFiles = false;
            }
        }


        [TestMethod]
        public void sizeTest() {
            SetOutputDir("size");
            PrepareTest();
            using (converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                BeforeTest();
                tex2dvi_test(testfile + ".tex");
                dvi2pdf_test(testfile + ".dvi");
                int page = (int)CallMethod(converter, "pdfpages", Path.Combine(OutputDir, "dvi2pdf.pdf"));
                AfterTest();
                var orighiresbb = GetPDFBB("dvi2pdf.pdf", 1, page);
                var origbb = GetPDFBB("dvi2pdf.pdf", 1, page, false);
                var orighiresbox = GetPDFBox("dvi2pdf.pdf", Enumerable.Range(1, page).ToList());
                var origbox = GetPDFBox("dvi2pdf.pdf", Enumerable.Range(1, page).ToList(), false);
                Settings.Default.keepPageSize = false;
                var exts = new string[] { ".pdf", ".eps", ".jpg", ".png" };
                foreach (var ext in exts) {
                    doGenerateOneTest("dvi2pdf.pdf", testfile + "-not-keep" + ext);
                }
                Func<string, List<string>> get_output_files_func = (s) => {
                    if (page == 1) return new List<string>() { s };
                    else {
                        var basen = Path.GetFileNameWithoutExtension(s);
                        var ext = Path.GetExtension(s);
                        return Enumerable.Range(1, page).Select(i => basen + "-" + i.ToString() + ext).ToList();
                    }
                };
                Func<string, List<Size>> get_pdf_box = (s) => {
                    var rv = new List<Size>();
                    var files = get_output_files_func(s);
                    foreach (var file in files) {
                        if (File.Exists(Path.Combine(OutputDir, file))) {
                            rv.AddRange(GetPDFBox(file, new List<int>() { 1 }));
                        }
                    }
                    return rv;
                };
                Assert.IsTrue(CheckVerctorImageSize(orighiresbb, get_pdf_box(testfile + "-not-keep.pdf")));
                Assert.IsTrue(CheckVerctorImageSize(orighiresbb, GetEPSBB(get_output_files_func(testfile + "-not-keep.eps"))));
                Assert.IsTrue(CheckBitmapImageSize(orighiresbb, GetBitmapSize(get_output_files_func(testfile + "-not-keep.png"))));
                Assert.IsTrue(CheckBitmapImageSize(orighiresbb, GetBitmapSize(get_output_files_func(testfile + "-not-keep.jpg"))));

                Settings.Default.keepPageSize = true;
                foreach (var ext in exts) {
                    doGenerateOneTest("dvi2pdf.pdf", testfile + "-keep" + ext);
                }
                Assert.IsTrue(CheckVerctorImageSize(orighiresbox, get_pdf_box(testfile + "-keep.pdf")));
                Assert.IsTrue(CheckVerctorImageSize(orighiresbox, GetEPSBB(get_output_files_func(testfile + "-keep.eps"))));
                Assert.IsTrue(CheckBitmapImageSize(orighiresbox, GetBitmapSize(get_output_files_func(testfile + "-keep.png"))));
                Assert.IsTrue(CheckBitmapImageSize(orighiresbox, GetBitmapSize(get_output_files_func(testfile + "-keep.jpg"))));
            }
        }

        void PrepareTest() {
            Settings.Default.leftMargin = 0;
            Settings.Default.rightMargin = 0;
            Settings.Default.topMargin = 0;
            Settings.Default.bottomMargin = 0;
            Settings.Default.yohakuUnitBP = false;
            Settings.Default.deleteTmpFileFlag = true;
            Settings.Default.previewFlag = false;
            Settings.Default.setFileToClipBoard = false;
            Settings.Default.backgroundColor = System.Drawing.Color.Red;

            Settings.Default.platexPath = Settings.Default.GuessPlatexPath();
            Debug.WriteLine("platex = " + Settings.Default.platexPath);
            Settings.Default.dvipdfmxPath = Settings.Default.GuessDvipdfmxPath();
            Debug.WriteLine("dvipdfmx = " + Settings.Default.dvipdfmxPath);
            Settings.Default.gsPath = Settings.Default.GuessGsPath();
            Debug.WriteLine("gspath = " + Settings.Default.gsPath);
            Settings.Default.gsDevice = Settings.Default.GuessGsdevice();
        }

        private IOutputController controller;

        private string WorkDir, OutputDir;
        private string testfile = "test";
        Converter converter;
        void SetOutputDir(string d) {
            OutputDir = Path.Combine(WorkDir, d);
            Directory.CreateDirectory(OutputDir);
            foreach (var f in Directory.GetFiles(OutputDir)) File.Delete(f);
        }
        public ConverterTest() {
//            controller = new CUIOutput();
            controller = this;
            WorkDir = Path.Combine(System.Environment.CurrentDirectory, "test");
            Directory.CreateDirectory(WorkDir);
        }

        [TestMethod]
        public void eachTest() {
            SetOutputDir("each");
            PrepareTest();
            BeforeTest();
            using (converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                doEachTest();
            }
            AfterTest();
        }

        void doGenerateTest(string source, string output) {
            doGenerateTest(source, output, Converter.imageExtensions);
        }

        void doGenerateTest(string source, string output, string[] exts) {
            Tuple<List<Size>, List<Size>> expected = null;
            var pdf = "dvi2pdf.pdf";
            using (var fs = new StreamWriter(Path.Combine(WorkDir, testfile + ".tex"))) {
                fs.Write(source);
            }
            using (converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + "-" + output + ".pdf"))) {
                tex2dvi_test(testfile + ".tex");
                dvi2pdf_test(testfile + ".dvi");
                var page = (int)CallMethod(converter, "pdfpages", Path.Combine(OutputDir, pdf));
                if (Settings.Default.keepPageSize) {
                    expected = new Tuple<List<Size>, List<Size>>(
                        GetPDFBox(pdf, Enumerable.Range(1, page).ToList(), false),
                        GetPDFBox(pdf, Enumerable.Range(1, page).ToList(), true));
                } else {
                    expected = new Tuple<List<Size>, List<Size>>(
                        GetPDFBB(pdf, 1, page, false),
                        GetPDFBB(pdf, 1, page, true));
                }
                File.Delete(Path.Combine(WorkDir, pdf));
                File.Move(Path.Combine(OutputDir,pdf),Path.Combine(WorkDir,pdf));
            }
            foreach (var ext in exts) {
                File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(WorkDir, testfile + ".pdf"), true);
                using (converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".pdf"), Path.Combine(OutputDir, testfile + "-" + output + ext))) {
                    converter.Convert();
                    if (expected.Item1.Count == 1 || Settings.Default.mergeOutputFiles) {
                        Assert.IsTrue(File.Exists(Path.Combine(OutputDir, testfile + "-" + output + ext)));
                    } else {
                        bool widthpos = (Settings.Default.leftMargin + Settings.Default.rightMargin > 0);
                        bool heightpos = (Settings.Default.topMargin + Settings.Default.bottomMargin > 0);
                        for (int i = 0; i < expected.Item1.Count; ++i) {
                            if ((expected.Item1[i].width != 0 || widthpos) && (expected.Item1[i].height != 0 || heightpos)) {
                                Assert.IsTrue(File.Exists(Path.Combine(OutputDir, testfile + "-" + output + "-" + (i + 1).ToString() + ext)));
                            } else {
                                Assert.IsFalse(File.Exists(Path.Combine(OutputDir, testfile + "-" + output + "-" + (i + 1).ToString() + ext)));
                            }
                        }
                    }

                }
            }
            File.Delete(Path.Combine(WorkDir,pdf));
        }

        void doGenerateOneTest(string inputfile, string outputfile) { 
            BeforeTest();
            if(!Path.IsPathRooted(inputfile))inputfile = Path.Combine(OutputDir,inputfile);
            string tempfile = Path.Combine(WorkDir, Path.GetFileName(inputfile));
            File.Copy(inputfile, tempfile, true);
            if(!Path.IsPathRooted(outputfile))outputfile = Path.Combine(OutputDir,outputfile);
            using(converter = new Converter(controller, tempfile, outputfile)) {
                converter.Convert();
                Assert.IsTrue(File.Exists(outputfile) || File.Exists(Path.Combine(Path.GetDirectoryName(outputfile),Path.GetFileNameWithoutExtension(outputfile) + "-1" + Path.GetExtension(outputfile))));
            }
            AfterTest();
        }

        bool sourceFileExisted = false;
        void BeforeTest() {
            sourceFileExisted = File.Exists(Path.Combine(WorkDir, testfile + ".tex"));
            if(!sourceFileExisted) {
                using(var fs = new StreamWriter(Path.Combine(WorkDir, testfile + ".tex"))) {
                    fs.WriteLine(@"\documentclass{jsarticle}");
                    fs.WriteLine(@"\pagestyle{empty}");
                    fs.WriteLine(@"\begin{document}");
                    fs.WriteLine(@"\[\frac{1}{2}\]");
                    fs.WriteLine(@"\end{document}");
                }
            } else {
                File.Copy(Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(WorkDir, testfile + "-backup.tex"), true);
            }
        }
        void AfterTest() {
            if(!sourceFileExisted) File.Delete(Path.Combine(WorkDir, testfile + ".tex"));
            else {
                File.Delete(Path.Combine(WorkDir, testfile + ".tex"));
                File.Move(Path.Combine(WorkDir, testfile + "-backup.tex"), Path.Combine(WorkDir, testfile + ".tex"));
            }
        }

        void tex2dvi_test(string file) {
            Debug.WriteLine("TEST: tex2dvi");
            Settings.Default.guessLaTeXCompile = false;
            string dvi = Path.ChangeExtension(file,".dvi");
            File.Delete(Path.Combine(WorkDir, dvi));
            CallMethod(converter, "tex2dvi", file);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, dvi)));
            File.Copy(Path.Combine(WorkDir, dvi), Path.Combine(OutputDir, "tex2dvi-noguess.dvi"), true);
            Settings.Default.guessLaTeXCompile = true;
            File.Delete(Path.Combine(WorkDir, dvi));
            CallMethod(converter, "tex2dvi", file);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, dvi)));
            File.Copy(Path.Combine(WorkDir, dvi), Path.Combine(OutputDir, "tex2dvi-guess.dvi"), true);
       }

        void dvi2pdf_test(string file) {
            Debug.WriteLine("TEST: dvi2pdf");
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "dvi2pdf", file);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "dvi2pdf.pdf"), true);
        }

        void dvi2ps_test(string file) {
            Debug.WriteLine("TEST: dvi2pdf");
            string ps = Path.ChangeExtension(file, ".ps");
            File.Delete(Path.Combine(WorkDir, ps));
            CallMethod(converter, "dvi2pdf", file);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, ps)));
            File.Copy(Path.Combine(WorkDir, ps), Path.Combine(OutputDir, "dvi2ps.ps"), true);
        }

        void ps2pdf_test(string file) {
            Debug.WriteLine("TEST: ps2pdf");
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "ps2pdf", file, pdf);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "ps2pdf.pdf"), true);
        }

        void pdf2eps_test(string file, int resolution) {
            Debug.WriteLine("TEST: pdf2eps");
            string eps = Path.ChangeExtension(file, ".eps");
            File.Delete(Path.Combine(WorkDir, eps));
            CallMethod(converter, "pdf2eps", file, Path.ChangeExtension(file, ".eps"), resolution, 1, null);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, eps)));
            File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, ".eps")), Path.Combine(OutputDir, "pdf2eps.eps"), true);
        }

        void pdfpages_test(string file) {
            Debug.WriteLine("TEST: pdfpages");
            int page = (int) CallMethod(converter, "pdfpages", file);
            System.Diagnostics.Debug.WriteLine("pdfpages: " + page.ToString());
            using(var fs = new StreamWriter(Path.Combine(OutputDir, "pdfpages.txt"))) {
                fs.WriteLine(page.ToString());
            }
        }

        void pdfcrop_test(string file) {
            Debug.WriteLine("TEST: pdfcrop");
            string cropped = Path.GetFileNameWithoutExtension(file) + "-crop.pdf";
            File.Delete(Path.Combine(WorkDir, cropped));
            CallMethod(converter, "pdfcrop", file, cropped, true, 1, new NullArgument(typeof(BoundingBoxPair)));
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, cropped)));
            File.Copy(Path.Combine(WorkDir, cropped), Path.Combine(OutputDir, "pdfcrop-usebp.pdf"), true);
            File.Delete(Path.Combine(WorkDir, cropped));
            CallMethod(converter, "pdfcrop", file, cropped, false, 1, new NullArgument(typeof(BoundingBoxPair)));
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, cropped)));
            File.Copy(Path.Combine(WorkDir, cropped), Path.Combine(OutputDir, "pdfcrop-nonusebp.pdf"), true);
        }

        void pdf2img_pdfium_test(string file) {
            Debug.WriteLine("TEST: pdf2img_pdfium");
            foreach(var extension in Converter.bmpExtensions) {
                Settings.Default.transparentPngFlag = true;
                string img = Path.ChangeExtension(file, extension);
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "pdf2img_pdfium", file, img, 0);
                Assert.IsTrue(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "pdf2img_pdfium-transparent" + extension), true);
                Settings.Default.transparentPngFlag = false;
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "pdf2img_pdfium", file, img, 0);
                Assert.IsTrue(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "pdf2img_pdfium-notransparent" + extension), true);
            }
        }

        void eps2img_test(string file) {
            Debug.WriteLine("TEST: eps2img");
            foreach(var extension in Converter.bmpExtensions) {
                Settings.Default.transparentPngFlag = true;
                string img = Path.ChangeExtension(file, extension);
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "eps2img", file, img, new NullArgument(typeof(BoundingBoxPair)));
                Assert.IsTrue(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "eps2img-transparent" + extension), true);
                Settings.Default.transparentPngFlag = false;
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "eps2img", file, img, new NullArgument(typeof(BoundingBoxPair)));
                Assert.IsTrue(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "eps2img-notransparent" + extension), true);
            }
        }

        void eps2pdf_test(string file) {
            Debug.WriteLine("TEST: eps2pdf");
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "eps2pdf", file, pdf);
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "eps2pdf.pdf"), true);
        }

        struct Size { 
            public decimal width, height;
            public bool IsEmpty { get { return width <= 0 || height <= 0; } }
        }
        Tuple<List<Size>,List<Size>> GetExpectedSize(string source) {
            tex2dvi_test(source);
            dvi2pdf_test(Path.ChangeExtension(source, ".dvi"));
            var pdf = "dvi2pdf.pdf";
            int page = (int) CallMethod(converter, "pdfpages", Path.Combine(OutputDir, pdf));
            if(Settings.Default.keepPageSize) {
                return new Tuple<List<Size>, List<Size>>(
                    GetPDFBox(pdf, Enumerable.Range(1, page).ToList(), false),
                    GetPDFBox(pdf, Enumerable.Range(1, page).ToList(), true));
            } else {
                return new Tuple<List<Size>, List<Size>>(
                    GetPDFBB(pdf, 1, page, false),
                    GetPDFBB(pdf, 1, page, true));
            }
        }

        List<Size> GetPDFBox(string file,List<int> pages,bool hires = true){
            var dir = Path.GetDirectoryName(file);
            if(dir == "") dir = OutputDir;
            else file = Path.GetFileName(file);
            using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                var bb = (List<BoundingBoxPair>)CallMethod(conv, "readPDFBox", Path.GetFileName(file), pages, "crop");
                var rv = new List<Size>();
                for(int i = 0 ; i < bb.Count ; ++i) rv.Add(BBToSize(bb[i], hires));
                return rv;
            }
        }

        List<Size> GetPDFBB(string file, int firstpage, int lastpage, bool hires = true) {
            var dir = Path.GetDirectoryName(file);
            if(dir == "") dir = OutputDir;
            else file = Path.GetFileName(file);
            var rv = new List<Size>();
            using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                var bb = (List<BoundingBoxPair>)CallMethod(conv, "readPDFBB", Path.GetFileName(file), firstpage, lastpage);
                for(int i = 0 ; i < bb.Count ; ++i )rv.Add(BBToSize(bb[i], hires));
                return rv;
            }
        }

        
        List<Size> GetEPSBB(List<string> files,bool hires = true){
            var rv = new List<Size>();
            foreach(var f in files) {
                var file = f;
                var dir = Path.GetDirectoryName(file);
                if(dir == "") dir = OutputDir;
                else file = Path.GetFileName(file);
                if(!File.Exists(Path.Combine(dir, file))) continue;
                using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                    var bb = (BoundingBoxPair)CallMethod(conv, "readBB", Path.GetFileName(file));
                    rv.Add(BBToSize(bb, hires));
                }
            }
            return rv;
        }

        static Size BBToSize(BoundingBoxPair bb, bool hires){
            var usebb = hires ? bb.hiresbb : bb.bb;
            var size = new Size();
            size.width = usebb.Width;
            size.height = usebb.Height;
            return size;
        }

        List<Size> GetBitmapSize(List<string> files) {
            var rv = new List<Size>();
            foreach(var f in files) {
                var file = f;
                if(!Path.IsPathRooted(file)) file = Path.Combine(OutputDir, file);
                if(!File.Exists(file)) continue;
                using(var bitmap = new System.Drawing.Bitmap(file)) {
                    var size = new Size();
                    size.width = bitmap.Size.Width;
                    size.height = bitmap.Size.Height;
                    rv.Add(size);
                }
            }
            return rv;
        }

        bool SameValue(decimal a, decimal b) {
            return Math.Abs(a - b) < 1;
        }

        bool CheckVerctorImageSize(List<Size> original, List<Size> gend) {
            int j = 0;
            for(int i = 0 ; i < original.Count ; ++i) {
                decimal width = original[i].width + Settings.Default.leftMargin + Settings.Default.rightMargin;
                decimal height = original[i].height + Settings.Default.topMargin + Settings.Default.bottomMargin;
                if(width == 0 || height == 0) continue;
                bool bw = SameValue(width, gend[j].width);
                bool bh = SameValue(height, gend[j].height);
                if (!bw || !bh) {
                    System.Diagnostics.Debug.WriteLine("CheckVectorImageSize is failed: page = " + i.ToString()
                        + ", (height,width) = (" + height.ToString() + ", " + width.ToString() + "), (" +
                        gend[j].height.ToString() + ", " + gend[j].width.ToString());
                    return false;
                }
                ++j;
            }
            if (j != gend.Count) System.Diagnostics.Debug.WriteLine("CheckVectorImageSize is failed: the pageCount is not the same");
            return (j == gend.Count);
        }

        bool CheckBitmapImageSize(List<Size> original, List<Size> gend) {
            int j = 0;
            for(int i = 0 ; i < original.Count ; ++i) {
                var width = original[i].width * Settings.Default.resolutionScale;
                var height = original[i].height * Settings.Default.resolutionScale;
                var addwidth = Settings.Default.leftMargin + Settings.Default.rightMargin;
                var addheight = Settings.Default.topMargin + Settings.Default.bottomMargin;
                if(Settings.Default.yohakuUnitBP) {
                    addwidth *= Settings.Default.resolutionScale;
                    addheight *= Settings.Default.resolutionScale;
                }
                width += addwidth;
                height += addheight;
                if(width == 0 && height == 0) continue;
                bool bw = SameValue(width, gend[j].width);
                bool bh = SameValue(height, gend[j].height);
                if (!bw || !bh) {
                    System.Diagnostics.Debug.WriteLine("CheckVectorImageSize is failed: page = " + i.ToString()
                        + ", (height,width) = (" + height.ToString() + ", " + width.ToString() + "), (" +
                        gend[j].height.ToString() + ", " + gend[j].width.ToString());
                    return false;
                }
                ++j;
            }
            return (j == gend.Count);
        }


        public void showExtensionError(string file) { Debug.WriteLine("showExtensionError: \nfile = " + file); }
        public void showPathError(string exeName, string necessary) { Debug.WriteLine("showPathError:\n exeName = " + exeName + "\nnecessary = " + necessary); }
//        public void appendOutput(string log) { Debug.WriteLine("appendOutput: log = \n" + log); }
        public void appendOutput(string log) { Debug.Write(log); }
        public void showGenerateError() { Debug.WriteLine("showGenerateError"); }
        public void scrollOutputTextBoxToEnd() { Debug.WriteLine("scrollOutputTextBoxToEnd"); }
        public void showUnauthorizedError(string filePath) { Debug.WriteLine("showUnauthorizedError: filePath = " + filePath); }
        public void showIOError(string filePath) { Debug.WriteLine("showIOError: filePath = " + filePath); }
        public void showToolError(string tool) { Debug.WriteLine("showToolError: tool = " + tool); }
        public void errorIgnoredWarning() { Debug.WriteLine("errorIgnoredWarning"); }
        public void showError(string msg) { Debug.WriteLine("showError: msg = " + msg); }
        public bool askYesorNo(string msg) {
            Debug.WriteLine("askYesorNo: msg = \n" + msg);
            return true;
        }

        class NullArgument {
            public Type type;
            public NullArgument(Type t) { type = t; }
            public NullArgument(string name) {
                type = Type.GetType(name);
            }
        }

        //string BoundingBoxPairTypeName = "TeX2img.Converter+BoundingBoxPair, TeX2img, Version=1.5.5.0, Culture=neutral, PublicKeyToken=null";
        //string BoundingBoxPairTypeName = "TeX2img.Converter+BoundingBoxPair, TeX2img";
        static object CallMethod(object obj, string func, params object[] args) {
            if(args.Contains(null)) {
                return obj.GetType().GetMethod(func, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(obj, args);
            } else {
                var types = new Type[args.Length];
                var modifilers = new System.Reflection.ParameterModifier[args.Length];
                for(int i = 0 ; i < args.Length ; ++i) {
                    var arg = args[i] as NullArgument;
                    if(arg != null) {
                        types[i] = arg.type;
                        args[i] = null;
                    } else types[i] = args[i].GetType();
                    modifilers[i] = new System.Reflection.ParameterModifier();
                }
                return obj.GetType().GetMethod(func, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, types, modifilers).Invoke(obj, args);
            }
        }

        static object GetField(object obj, string name) {
            var t = obj.GetType();
            var f = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return f.GetValue(obj);
        }

        static object GetProperty(object obj, string name) {
            var t = obj.GetType();
            var p = t.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return p.GetValue(obj);
            //return obj.GetType().GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(obj);
        }

        static object GetIndexer(object obj, object index) {
            var t = obj.GetType();
            return t.InvokeMember("Item", System.Reflection.BindingFlags.GetProperty, null, obj, new object[] { index });
        }
    }
}
