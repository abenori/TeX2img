#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TeX2img {
    class ConverterTEST : IOutputController{
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
            //Properties.Settings.Default.deleteTmpFileFlag = false;
            Properties.Settings.Default.platexPath = Properties.Settings.Default.GuessPlatexPath();
            Debug.WriteLine("platex = " + Properties.Settings.Default.platexPath);
            Properties.Settings.Default.dvipdfmxPath = Properties.Settings.Default.GuessDvipdfmxPath();
            Debug.WriteLine("dvipdfmx = " + Properties.Settings.Default.dvipdfmxPath);
            Properties.Settings.Default.gsPath = Properties.Settings.Default.GuessGsPath();
            Debug.WriteLine("gspath = " + Properties.Settings.Default.gsPath);
            Properties.Settings.Default.gsDevice = Properties.Settings.Default.GuessGsdevice();
            bool existed = File.Exists(Path.Combine(WorkDir, testfile + ".tex"));
            if(!existed) {
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
            using(converter = new Converter(new CUIOutput(), Path.Combine(WorkDir, testfile + ".tex"), Path.Combine(OutputDir, testfile + ".pdf"))) {
                tex2dvitest(testfile + ".tex");
                dvi2pdftest(testfile + ".dvi");
                pdfcroptest(testfile + ".pdf");
                pdf2epstest(testfile + ".pdf", Properties.Settings.Default.resolutionScale * 72);
                eps2pdftest(testfile + ".eps");
                pdfpagestest(testfile + ".pdf");
                pdf2img_pdfiumtest(testfile + ".pdf");
            }
            if(!existed) File.Delete(Path.Combine(WorkDir, testfile + ".tex"));
            else File.Move(Path.Combine(WorkDir, testfile + "-backup.tex"), Path.Combine(WorkDir, testfile + ".tex"));
        }

        void tex2dvitest(string file) {
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

        void dvi2pdftest(string file) {
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "dvi2pdf", file);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, pdf)));
            File.Copy(Path.Combine(WorkDir, pdf), Path.Combine(OutputDir, "dvi2pdf.pdf"), true);
        }

        void pdf2epstest(string file, int resolution) {
            string eps = Path.ChangeExtension(file, ".eps");
            File.Delete(Path.Combine(WorkDir, eps));
            CallMethod(converter, "pdf2eps", file, Path.ChangeExtension(file, ".eps"), resolution, 1, null);
            Debug.Assert(File.Exists(Path.Combine(WorkDir, eps)));
            File.Copy(Path.Combine(WorkDir, Path.ChangeExtension(file, ".eps")), Path.Combine(OutputDir, "pdf2eps.eps"), true);
        }

        void pdfpagestest(string file) {
            int page = (int) CallMethod(converter, "pdfpages", file);
            System.Diagnostics.Debug.WriteLine("pdfpages: " + page.ToString());
            using(var fs = new StreamWriter(Path.Combine(OutputDir, "pdfpages.txt"))) {
                fs.WriteLine(page.ToString());
            }
        }

        void pdfcroptest(string file) {
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

        void pdf2img_pdfiumtest(string file) {
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

        void eps2pdftest(string file) {
            string pdf = Path.ChangeExtension(file, ".pdf");
            File.Delete(Path.Combine(WorkDir, pdf));
            CallMethod(converter, "eps2pdf", file, pdf);
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
        
    }
}
#endif