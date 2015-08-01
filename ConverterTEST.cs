#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TeX2img {
    class ConverterTEST : IOutputController{
        void doEachTest() {
            tex2dvi_test(testfile + ".tex");
            dvi2pdf_test(testfile + ".dvi");
            pdfcrop_test(testfile + ".pdf");
            pdf2eps_test(testfile + ".pdf", Properties.Settings.Default.resolutionScale * 72);
            CallMethod(converter, "enlargeBB", testfile + ".eps");
            eps2img_test(testfile + ".eps");
            eps2pdf_test(testfile + ".eps");
            pdfpages_test(testfile + ".pdf");
            pdf2img_pdfium_test(testfile + ".pdf");
        }
        public void generateTest() {
            PrepareTest();
            Properties.Settings.Default.outlinedText = true;
            Properties.Settings.Default.transparentPngFlag = false;
            Properties.Settings.Default.useLowResolution = false;
            Properties.Settings.Default.useMagickFlag = true;
            doGenerateTest("default");
            Properties.Settings.Default.outlinedText = false;
            Properties.Settings.Default.transparentPngFlag = false;
            Properties.Settings.Default.useLowResolution = false;
            Properties.Settings.Default.useMagickFlag = true;
            doGenerateTest("with-text");
            Properties.Settings.Default.transparentPngFlag = true;
            Properties.Settings.Default.useLowResolution = false;
            Properties.Settings.Default.useMagickFlag = true;
            doGenerateTest("transparent");
            Properties.Settings.Default.transparentPngFlag = false;
            Properties.Settings.Default.useLowResolution = true;
            Properties.Settings.Default.useMagickFlag = true;
            doGenerateTest("low-resolution");
            Properties.Settings.Default.transparentPngFlag = false;
            Properties.Settings.Default.useLowResolution = false;
            Properties.Settings.Default.useMagickFlag = false;
            doGenerateTest("no-antialias");
        }

        void PrepareTest() {
            Properties.Settings.Default.leftMargin = 10;
            Properties.Settings.Default.rightMargin = 0;
            Properties.Settings.Default.topMargin = 10;
            Properties.Settings.Default.bottomMargin = 0;
            Properties.Settings.Default.yohakuUnitBP = false;
            Properties.Settings.Default.deleteTmpFileFlag = true;
            Properties.Settings.Default.previewFlag = false;
            Properties.Settings.Default.setFileToClipBoard = false;

            //Properties.Settings.Default.platexPath = Properties.Settings.Default.GuessPlatexPath();
            Debug.WriteLine("platex = " + Properties.Settings.Default.platexPath);
            //Properties.Settings.Default.dvipdfmxPath = Properties.Settings.Default.GuessDvipdfmxPath();
            Debug.WriteLine("dvipdfmx = " + Properties.Settings.Default.dvipdfmxPath);
            //Properties.Settings.Default.gsPath = Properties.Settings.Default.GuessGsPath();
            Debug.WriteLine("gspath = " + Properties.Settings.Default.gsPath);
            //Properties.Settings.Default.gsDevice = Properties.Settings.Default.GuessGsdevice();
        }

        private IOutputController controller = new CUIOutput();

        private string WorkDir,OutputDir;
        private string testfile = "test";
        Converter converter;
        public ConverterTEST() {
            WorkDir = Path.Combine(System.Environment.CurrentDirectory, "test");
            Directory.CreateDirectory(WorkDir);
            OutputDir = Path.Combine(WorkDir,"output");
            Directory.CreateDirectory(OutputDir);
            foreach(var f in Directory.GetFiles(OutputDir)) File.Delete(f);
        }

        public void eachTest() {
            PrepareTest();
            BeforeTest();
            using(converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                doEachTest();
            }
            AfterTest();
        }

        void doGenerateTest(string output) {
            foreach(var ext in Converter.imageExtensions) {
                BeforeTest();
                using(converter = new Converter(controller, Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + "-" + output + ext))) {
                    converter.Convert();
                }
                AfterTest();
            }
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
            Properties.Settings.Default.guessLaTeXCompile = false;
            string dvi = Path.ChangeExtension(file,".dvi");
            File.Delete(Path.Combine(WorkDir, dvi));
            CallMethod(converter, "tex2dvi", file);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, dvi)));
            File.Copy(Path.Combine(WorkDir, dvi), Path.Combine(OutputDir, "tex2dvi-noguess.dvi"), true);
            Properties.Settings.Default.guessLaTeXCompile = true;
            File.Delete(Path.Combine(WorkDir, dvi));
            CallMethod(converter, "tex2dvi", file);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, dvi)));
            File.Copy(Path.Combine(WorkDir, dvi), Path.Combine(OutputDir, "tex2dvi-guess.dvi"), true);
       }

        void dvi2pdf_test(string file) {
            Debug.WriteLine("TEST: dvi2pdf");
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "dvi2pdf", file);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "dvi2pdf.pdf"), true);
        }

        void pdf2eps_test(string file, int resolution) {
            Debug.WriteLine("TEST: pdf2eps");
            string eps = Path.ChangeExtension(file, ".eps");
            File.Delete(Path.Combine(WorkDir, eps));
            CallMethod(converter, "pdf2eps", file, Path.ChangeExtension(file, ".eps"), resolution, 1, null);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, eps)));
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
            Debug.Assert(File.Exists(Path.Combine(WorkDir, cropped)));
            File.Copy(Path.Combine(WorkDir, cropped), Path.Combine(OutputDir, "pdfcrop-usebp.pdf"), true);
            File.Delete(Path.Combine(WorkDir, cropped));
            CallMethod(converter, "pdfcrop", file, cropped, false, 1, new NullArgument(BoundingBoxPairTypeName));
            Debug.Assert(File.Exists(Path.Combine(WorkDir, cropped)));
            File.Copy(Path.Combine(WorkDir, cropped), Path.Combine(OutputDir, "pdfcrop-nonusebp.pdf"), true);
        }

        void pdf2img_pdfium_test(string file) {
            Debug.WriteLine("TEST: pdf2img_pdfium");
            foreach(var extension in Converter.bmpExtensions) {
                Properties.Settings.Default.transparentPngFlag = true;
                string img = Path.ChangeExtension(file, extension);
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "pdf2img_pdfium", file, img, 0);
                Debug.Assert(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "pdf2img_pdfium-transparent" + extension), true);
                Properties.Settings.Default.transparentPngFlag = false;
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "pdf2img_pdfium", file, img, 0);
                Debug.Assert(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "pdf2img_pdfium-notransparent" + extension), true);
            }
        }

        void eps2img_test(string file) {
            Debug.WriteLine("TEST: eps2img");
            foreach(var extension in Converter.bmpExtensions) {
                Properties.Settings.Default.transparentPngFlag = true;
                string img = Path.ChangeExtension(file, extension);
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "eps2img", file, img);
                Debug.Assert(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "eps2img-transparent" + extension), true);
                Properties.Settings.Default.transparentPngFlag = false;
                File.Delete(Path.Combine(WorkDir, img));
                CallMethod(converter, "eps2img", file, img);
                Debug.Assert(File.Exists(Path.Combine(WorkDir, img)));
                File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, extension)), Path.Combine(OutputDir, "eps2img-notransparent" + extension), true);
            }
        }

        void eps2pdf_test(string file) {
            Debug.WriteLine("TEST: eps2pdf");
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "eps2pdf", file, pdf, new NullArgument(BoundingBoxPairTypeName));
            Debug.Assert(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "eps2pdf.pdf"), true);
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
        string BoundingBoxPairTypeName = "TeX2img.Converter+BoundingBoxPair";
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
        static object GetMember(object obj, string name) {
            return obj.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(obj);
        }

    }
}
#endif