using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;


namespace TeX2img {
    namespace pdfium{
        public class PDFDocument : IDisposable {
            IntPtr documentPtr;
            public string FileName { get; private set; }
            //static int documentloadednum = 0;
            public PDFDocument(string path) {
                documentPtr = PInvoke.FPDF_LoadDocument(path, null); 
                if(documentPtr == IntPtr.Zero) throw new System.IO.FileNotFoundException();
                //{ ++documentloadednum; System.Diagnostics.Debug.WriteLine("PDFDocumentLoaded: " + documentloadednum.ToString()); }
                FileName = path;
            }
            //static int pageloadednum = 0;
            public PDFPage GetPage(int pageNum) {
                IntPtr p = PInvoke.FPDF_LoadPage(documentPtr, pageNum);
                //{++pageloadednum;System.Diagnostics.Debug.WriteLine("PDFPageLoadPage: " + pageloadednum.ToString());}
                if(p == IntPtr.Zero) throw new Exception();//後で直す
                return new PDFPage(p);
            }
            public int GetPageCount() {
                return PInvoke.FPDF_GetPageCount(documentPtr);
            }
            //static int documentunloadednum = 0;
            public void Dispose() {
                if(documentPtr != IntPtr.Zero) {
                    PInvoke.FPDF_CloseDocument(documentPtr);
                    //{ ++documentunloadednum; System.Diagnostics.Debug.WriteLine("PDFDocumentUnLoaded: " + documentunloadednum.ToString()); }
                    documentPtr = IntPtr.Zero;
                }
            }
            static PDFDocument() {
                if(pdfiumInitializerHolder.initializer == null) pdfiumInitializerHolder.initializer = new pdfiumInitializer();
            }
            ~PDFDocument() { Dispose(); }
        }
        public class PDFPage : IDisposable {
            public PDFPage(IntPtr p) {
                pagePtr = p;
            }
            IntPtr pagePtr;
            public double Width { get{return PInvoke.FPDF_GetPageWidth(pagePtr);}}
            public double Height { get { return PInvoke.FPDF_GetPageHeight(pagePtr); } }
            public void Draw(IntPtr hdc,int width,int height) {
                PInvoke.FPDF_RenderPage(hdc, pagePtr, 0, 0, width, height, 0, 0x800);
            }

            //static int pageunloadednum = 0;
            public void Dispose() {
                if(pagePtr != IntPtr.Zero) {
                    //{ ++pageunloadednum; System.Diagnostics.Debug.WriteLine("PdfPageUnloaded: " + pageunloadednum.ToString());}
                    PInvoke.FPDF_ClosePage(pagePtr);
                    pagePtr = IntPtr.Zero;
                }
            }
            ~PDFPage() { Dispose(); }
            static PDFPage() {
                if(pdfiumInitializerHolder.initializer == null) pdfiumInitializerHolder.initializer = new pdfiumInitializer();
            }
        }
        class PInvoke {
            [DllImport("gdi32.dll")]
            internal static extern bool Rectangle(
               IntPtr hdc,
               int ulCornerX, int ulCornerY,
               int lrCornerX, int lrCornerY);
            [DllImport("pdfium.dll")]
            public static extern void FPDF_InitLibrary();
            [DllImport("pdfium.dll")]
            public static extern void FPDF_DestroyLibrary();
            [DllImport("pdfium.dll")]
            public static extern IntPtr FPDF_LoadDocument(string file_path, string password);
            [DllImport("pdfium.dll")]
            public static extern IntPtr FPDF_LoadPage(IntPtr document, int page);
            [DllImport("pdfium.dll")]
            public static extern double FPDF_GetPageWidth(IntPtr page);
            [DllImport("pdfium.dll")]
            public static extern double FPDF_GetPageHeight(IntPtr page);
            [DllImport("pdfium.dll")]
            public static extern void FPDF_ClosePage(IntPtr page);
            [DllImport("pdfium.dll")]
            public static extern void FPDF_CloseDocument(IntPtr document);
            [DllImport("pdfium.dll")]
            public static extern int FPDF_GetPageCount(IntPtr document);
            [DllImport("pdfium.dll")]
            public static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);
            [DllImport("pdfium.dll")]
            public static extern void FPDF_RenderPage(IntPtr dc, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [DllImport("pdfium.dll")]
            public static extern IntPtr FPDFBitmap_Create(int width, int height, int alpha);
            [DllImport("pdfium.dll")]
            public static extern void FPDFBitmap_FillRect(IntPtr bitmap, int left, int top, int width, int height, uint color);
            [DllImport("pdfium.dll")]
            public static extern IntPtr FPDFBitmap_GetBuffer(IntPtr bitmap);
            [DllImport("pdfium.dll")]
            public static extern void FPDFBitmap_Destroy(IntPtr bitmap);
            [DllImport("pdfium.dll")]
            public static extern int FPDFBitmap_GetStride(IntPtr bitmap);


            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("gdi32.dll")]
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
        class pdfiumInitializer {
            public pdfiumInitializer() {
                PInvoke.FPDF_InitLibrary();
            }
            ~pdfiumInitializer() {
                PInvoke.FPDF_DestroyLibrary();
            }
        }
        static class pdfiumInitializerHolder {
            public static pdfiumInitializer initializer = null;
        }
    }
}
