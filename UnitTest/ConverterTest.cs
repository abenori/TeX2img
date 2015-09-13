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
    public class ConverterTest : IOutputController{
        void doEachTest() {
            tex2dvi_test(testfile + ".tex");
            dvi2pdf_test(testfile + ".dvi");
            pdfcrop_test(testfile + ".pdf");
            pdf2eps_test(testfile + ".pdf", Settings.Default.resolutionScale * 72);
            eps2img_test(testfile + ".eps");
            CallMethod(converter, "enlargeBB", testfile + ".eps", true);
            eps2pdf_test(testfile + ".eps");
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
            Settings.Default.outlinedText = true;
            Settings.Default.transparentPngFlag = false;
            Settings.Default.useLowResolution = false;
            Settings.Default.useMagickFlag = true;
            doGenerateTest("default", allExts);
            Settings.Default.outlinedText = false;
            Settings.Default.transparentPngFlag = false;
            Settings.Default.useLowResolution = false;
            Settings.Default.useMagickFlag = true;
            doGenerateTest("with-text", new string[] { ".pdf" });
            Settings.Default.transparentPngFlag = true;
            Settings.Default.useLowResolution = false;
            Settings.Default.useMagickFlag = true;
            doGenerateTest("transparent", new string[] { ".png", ".tiff", ".emf" });
            Settings.Default.transparentPngFlag = false;
            Settings.Default.useLowResolution = true;
            Settings.Default.useMagickFlag = true;
            doGenerateTest("low-resolution", bmpExts);
            Settings.Default.transparentPngFlag = false;
            Settings.Default.useLowResolution = false;
            Settings.Default.useMagickFlag = false;
            doGenerateTest("no-antialias");
            Settings.Default.transparentPngFlag = false;
            Settings.Default.useLowResolution = false;
            Settings.Default.useMagickFlag = true;
            Settings.Default.keepPageSize = true;
            doGenerateTest("keep-pagesize");
        }

        [TestMethod]
        public void sizeTest() {
            SetOutputDir("size");
            PrepareTest();
            using(converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                BeforeTest();
                tex2dvi_test(testfile + ".tex");
                dvi2pdf_test(testfile + ".dvi");
                AfterTest();
                var orighiresbb = GetPDFBB("dvi2pdf.pdf", 1);
                var origbb = GetPDFBB("dvi2pdf.pdf", 1, false);
                var orighiresbox = GetPDFBox("dvi2pdf.pdf", 1);
                var origbox = GetPDFBox("dvi2pdf.pdf", 1, false);
                Settings.Default.keepPageSize = false;
                var exts = new string[] { ".pdf", ".eps", ".jpg", ".png" };
                foreach(var ext in exts){
                    doGenerateTest("dvi2pdf.pdf", testfile + "-not-keep" + ext);
                }
                Assert.IsTrue(CheckVerctorImageSize(orighiresbb, GetPDFBox(testfile + "-not-keep.pdf", 1)));
                Assert.IsTrue(CheckVerctorImageSize(orighiresbb, GetEPSBB(testfile + "-not-keep.eps")));
                Assert.IsTrue(CheckBitmapImageSize(origbb, GetBitmapSize(testfile + "-not-keep.png")));
                Assert.IsTrue(CheckBitmapImageSize(origbb, GetBitmapSize(testfile + "-not-keep.jpg")));

                Settings.Default.keepPageSize = true;
                foreach(var ext in exts) {
                    doGenerateTest("dvi2pdf.pdf", testfile + "-keep" + ext);
                }
                Assert.IsTrue(CheckVerctorImageSize(orighiresbox, GetPDFBox(testfile + "-keep.pdf", 1)));
                Assert.IsTrue(CheckVerctorImageSize(orighiresbox, GetEPSBB(testfile + "-keep.eps")));
                Assert.IsTrue(CheckBitmapImageSize(origbox, GetBitmapSize(testfile + "-keep.png")));
                Assert.IsTrue(CheckBitmapImageSize(origbox, GetBitmapSize(testfile + "-keep.jpg")));
            }
        }

        void PrepareTest() {
            Settings.Default.leftMargin = 5;
            Settings.Default.rightMargin = 5;
            Settings.Default.topMargin = 5;
            Settings.Default.bottomMargin = 5;
            Settings.Default.yohakuUnitBP = false;
            Settings.Default.deleteTmpFileFlag = true;
            Settings.Default.previewFlag = false;
            Settings.Default.setFileToClipBoard = false;

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
            foreach(var f in Directory.GetFiles(OutputDir)) File.Delete(f);
        }
        public ConverterTest() {
            controller = new CUIOutput();
            controller = this;
            WorkDir = Path.Combine(System.Environment.CurrentDirectory, "test");
            Directory.CreateDirectory(WorkDir);
        }

        [TestMethod]
        public void eachTest() {
            SetOutputDir("each");
            PrepareTest();
            BeforeTest();
            using(converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                doEachTest();
            }
            AfterTest();
        }

        void doGenerateTest(string output) {
            doGenerateTest(output, Converter.imageExtensions);
        }

        void doGenerateTest(string output, string[] exts) {
            foreach(var ext in exts) {
                BeforeTest();
                using(converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + "-" + output + ext))) {
                    converter.Convert();
                    Assert.IsTrue(File.Exists(Path.Combine(OutputDir, testfile + "-" + output + ext)));
                }
                AfterTest();
            }
        }

        void doGenerateTest(string inputfile, string outputfile){
            BeforeTest();
            if(!Path.IsPathRooted(inputfile))inputfile = Path.Combine(OutputDir,inputfile);
            string tempfile = Path.Combine(WorkDir, Path.GetFileName(inputfile));
            File.Copy(inputfile, tempfile, true);
            if(!Path.IsPathRooted(outputfile))outputfile = Path.Combine(OutputDir,outputfile);
            using(converter = new Converter(controller, tempfile, outputfile)) {
                converter.Convert();
                Assert.IsTrue(File.Exists(outputfile));
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
                if(!File.Exists(Path.Combine(WorkDir, testfile + ".tex"))) File.Move(Path.Combine(WorkDir, testfile + "-backup.tex"), Path.Combine(WorkDir, testfile + ".tex"));
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
            CallMethod(converter, "ps2pdf", file);
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
            CallMethod(converter, "pdfcrop", file, cropped, true, 1, new NullArgument(BoundingBoxPairTypeName));
            Assert.IsTrue(File.Exists(Path.Combine(WorkDir, cropped)));
            File.Copy(Path.Combine(WorkDir, cropped), Path.Combine(OutputDir, "pdfcrop-usebp.pdf"), true);
            File.Delete(Path.Combine(WorkDir, cropped));
            CallMethod(converter, "pdfcrop", file, cropped, false, 1, new NullArgument(BoundingBoxPairTypeName));
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
                CallMethod(converter, "eps2img", file, img, new NullArgument(BoundingBoxPairTypeName));
                Assert.IsTrue(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "eps2img-transparent" + extension), true);
                Settings.Default.transparentPngFlag = false;
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "eps2img", file, img, new NullArgument(BoundingBoxPairTypeName));
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

        struct Size { public decimal width, height;}
        Size GetPDFBox(string file,int page,bool hires = true){
            var dir = Path.GetDirectoryName(file);
            if(dir == "") dir = OutputDir;
            else file = Path.GetFileName(file);
            using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                var bb = CallMethod(conv, "readPDFBox", Path.GetFileName(file), new List<int>() { page }, 0);
                return BBToSize(GetIndexer(bb, 0), hires);
            }
        }

        Size GetPDFBB(string file, int page, bool hires = true) {
            var dir = Path.GetDirectoryName(file);
            if(dir == "") dir = OutputDir;
            else file = Path.GetFileName(file);
            using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                var bb = CallMethod(conv, "readPDFBB", Path.GetFileName(file), page);
                return BBToSize(bb, hires);
            }
        }

        Size GetEPSBB(string file,bool hires = true){
            var dir = Path.GetDirectoryName(file);
            if(dir == "") dir = OutputDir;
            else file = Path.GetFileName(file);
            using(var conv = new Converter(null, Path.Combine(dir, "dummy.tex"), file)) {
                var bb = CallMethod(conv, "readBB", Path.GetFileName(file));
                return BBToSize(bb, hires);
            }
        }

        static Size BBToSize(object bb, bool hires){
            var hiresbb = GetField(bb, hires ? "hiresbb" : "bb");
            var size = new Size();
            size.width = (decimal) GetProperty(hiresbb, "Right") - (decimal) GetProperty(hiresbb, "Left");
            size.height = (decimal) GetProperty(hiresbb, "Top") - (decimal) GetProperty(hiresbb, "Bottom");
            return size;
        }

        Size GetBitmapSize(string file) {
            if(!Path.IsPathRooted(file)) file = Path.Combine(OutputDir, file);
            using(var bitmap = new System.Drawing.Bitmap(file)) {
                var size = new Size();
                size.width = bitmap.Size.Width;
                size.height = bitmap.Size.Height;
                return size;
            }
        }

        bool SameValue(decimal a, decimal b) {
            return Math.Abs(a - b) < 1;
        }

        bool CheckVerctorImageSize(Size original, Size gend) {
            bool bw = SameValue(original.width + Settings.Default.leftMargin + Settings.Default.rightMargin,gend.width);
            bool bh = SameValue(original.height + Settings.Default.topMargin + Settings.Default.bottomMargin, gend.height);
            return bw && bh;
        }

        bool CheckBitmapImageSize(Size original, Size gend) {
            original.width *= Settings.Default.resolutionScale;
            original.height *= Settings.Default.resolutionScale;
            var addwidth = Settings.Default.leftMargin + Settings.Default.rightMargin;
            var addheight = Settings.Default.topMargin + Settings.Default.bottomMargin;
            if(Settings.Default.yohakuUnitBP){
                addwidth *= Settings.Default.resolutionScale;
                addheight *= Settings.Default.resolutionScale;
            }
            bool bw = SameValue(original.width + addwidth, gend.width);
            bool bh = SameValue(original.height + addheight, gend.height);
            return bw && bh;
        }




        public void showExtensionError(string file) { Debug.WriteLine("showExtensionError: \nfile = " + file); }
        public void showPathError(string exeName, string necessary) { Debug.WriteLine("showPathError:\n exeName = " + exeName + "\nnecessary = " + necessary); }
        public void appendOutput(string log) { Debug.WriteLine("appendOutput: log = \n" + log); }
        public void showGenerateError() { Debug.WriteLine("showGenerateError"); }
        public void scrollOutputTextBoxToEnd() { Debug.WriteLine("scrollOutputTextBoxToEnd"); }
        public void showUnauthorizedError(string filePath) { Debug.WriteLine("showUnauthorizedError: filePath = " + filePath); }
        public void showIOError(string filePath) { Debug.WriteLine("showIOError: filePath = " + filePath); }
        public void showToolError(string tool) { Debug.WriteLine("showToolError: tool = " + tool); }
        public void errorIgnoredWarning() { Debug.WriteLine("errorIgnoredWarning"); }
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
        string BoundingBoxPairTypeName = "TeX2img.Converter+BoundingBoxPair, TeX2img";
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
