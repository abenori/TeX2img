#include <Windows.h>
#include <tchar.h>
#include <stdio.h>
#include <string>
#include <vector>
#include <iostream>
#include <memory>
#include <exception>
#include <cstdlib>
#include <fpdfview.h>
#include "image_diff_png.h"

const int EMF = 0;
const int BMP = 1;
const int PNG = 2;

const int RENDER_FLAG = FPDF_PRINTING;

using namespace std;

struct Data {
	bool use_gdi = false;
	string input;
	string output;
	int target = EMF;
	int scale = 1;
	bool transparent = false;
};

void ShowUsage() {
	cout << "pdfiumdraw [option] input" << endl;
	cout << "option:" << endl;
	cout << "  --use-gdi: use gdi for drawing" << endl;
	cout << "  --emf: output emf file (default)" << endl;
	cout << "  --bmp: output bmp file" << endl;
	cout << "  --png: output png file" << endl;
	cout << "  --scale: specify resolution level" << endl;
	cout << "  --transparent: output transparent png file" << endl;
	cout << "  --output=<file>: specify output file name" << endl;
	cout << "  --help: show this message" << endl;
}

string GetCurrentDirectory() {
	static string currentdir = "";
	if(currentdir == "") {
		char buf[MAX_PATH]; buf[0] = '\0';
		::GetCurrentDirectory(MAX_PATH, buf);
		currentdir = buf;
		if(currentdir.length() > 0 && currentdir[currentdir.length() - 1] != '\\')currentdir += "\\";
	}
	return currentdir;
}

string GetFullName(const string &f) {
	if(f == "")return "";
	else if(f.length() > 1 && f[1] == ':')return f;
	else return GetCurrentDirectory() + f;
}

string GetExtension(string file) {
	auto dot = file.rfind(".");
	if(dot == string::npos)return "";
	auto yen = file.rfind("\\");
	auto slash = file.rfind("/");
	if(yen == string::npos || (slash != string::npos && slash > yen))yen = slash;
	if(yen != string::npos && yen > dot)return "";
	else return file.substr(dot);
}

string GetDirectory(string file) {
	auto yen = file.rfind("\\");
	auto slash = file.rfind("/");
	if(yen == string::npos || (slash != string::npos && slash > yen))yen = slash;
	if(yen == string::npos)return "";
	else return file.substr(0, yen);
}

string GetFileName(string file) {
	auto yen = file.rfind("\\");
	auto slash = file.rfind("/");
	if(yen == string::npos || (slash != string::npos && slash > yen))yen = slash;
	if(yen == string::npos)return file;
	else return file.substr(yen + 1);
}

string GetFileNameWithoutExtension(string file) {
	auto f = GetFileName(file);
	auto dot = f.rfind(".");
	if(dot == string::npos)return f;
	else return f.substr(0, dot);
}

class PDFPage;
class PDFDoc {
public:
	PDFDoc(const string &path) {
		doc = ::FPDF_LoadDocument(path.c_str(), NULL);
		if(doc == NULL)throw runtime_error("filed to open " + path);
	}
	~PDFDoc() {
		if(doc != NULL)::FPDF_CloseDocument(doc);
	}
	int GetPageCount() { return ::FPDF_GetPageCount(doc); }
private:
	friend class PDFPage;
	FPDF_DOCUMENT doc;
	PDFDoc();
	PDFDoc(const PDFDoc &);
	PDFDoc &operator=(const PDFDoc &);
};

class PDFPage {
public:
	PDFPage(const PDFDoc &doc, int pageNum) {
		auto p = ::FPDF_LoadPage(doc.doc, pageNum);
		if(p == NULL)throw runtime_error("failt to open page + " + to_string(pageNum));
		page = p;
	}
	~PDFPage() { ::FPDF_ClosePage(page); }
	void Render(HDC hdc, int width, int height) {
		::FPDF_RenderPage(hdc, page, 0, 0, width, height, 0, RENDER_FLAG);
	}
	void Render(FPDF_BITMAP bitmap, int width, int height) {
		::FPDF_RenderPageBitmap(bitmap, page, 0, 0, width, height, 0, RENDER_FLAG);
	}
	double GetWidth() {
		return ::FPDF_GetPageWidth(page);
	}
	double GetHeight() {
		return ::FPDF_GetPageHeight(page);
	}
private:
	FPDF_PAGE page;
	PDFPage();
	PDFPage(const PDFPage &);
	PDFPage &operator=(const PDFPage &);
};

void GetOutputFileName(const string &input, const string &output, string extension, string &outputpre, string &outputpost) {
	if(output == "") {
		outputpre = GetDirectory(input) + "\\" + GetFileNameWithoutExtension(input) + "-";
		outputpost = extension;
	} else {
		auto r = output.find("%d");
		if(r == string::npos) {
			outputpre = GetDirectory(output) + "\\" + GetFileNameWithoutExtension(output);
			outputpost = GetFileNameWithoutExtension(output);
		} else {
			outputpre = output.substr(0, r);
			outputpost = output.substr(r + 1);
		}
	}
}

class hBitmap{
public:
	HBITMAP bitmap = NULL;
	~hBitmap() {
		if(bitmap)::DeleteObject(bitmap);
		bitmap = NULL;
	}
};
class hDC {
public:
	HDC dc = NULL;
	~hDC() {
		if(dc)::DeleteObject(dc);
		dc = NULL;
	}
};
class PDFiumBitmap {
public:
	FPDF_BITMAP bitmap = NULL;
	~PDFiumBitmap() {
		if(bitmap)::FPDFBitmap_Destroy(bitmap);
		bitmap = NULL;
	}
};

const int SCALE_MULTIPLE = 5;

void *GetBitmapByGDI(PDFPage &page, HBITMAP &bitmap, const BITMAPINFOHEADER &infohead){
	void *buf;
	int width = abs(infohead.biWidth);
	int height = abs(infohead.biHeight);
	BITMAPINFO info = {0};
	info.bmiHeader = infohead;
	HDC hdc = ::GetDC(NULL);
	bitmap = ::CreateDIBSection(NULL, &info, DIB_RGB_COLORS, (void**) &buf, NULL, 0);
	hDC bitmapdc;
	bitmapdc.dc = ::CreateCompatibleDC(hdc);
	::ReleaseDC(NULL, hdc);
	auto savebitmap = (HBITMAP)::SelectObject(bitmapdc.dc, bitmap);
	RECT rc;
	rc.left = 0; rc.top = 0; rc.right = width; rc.bottom = height;
	::FillRect(bitmapdc.dc, &rc, (HBRUSH)::GetStockObject(WHITE_BRUSH));
	page.Render(bitmapdc.dc, width, height);
	bitmap = (HBITMAP)::SelectObject(bitmapdc.dc, savebitmap);
	return buf;
}

void *GetBitmapByPDFBITMAP(PDFPage &page, FPDF_BITMAP &bitmap, const BITMAPINFOHEADER &infohead){
	int width = abs(infohead.biWidth);
	int height = abs(infohead.biHeight);
	bitmap = ::FPDFBitmap_Create(width, height, 0);
	::FPDFBitmap_FillRect(bitmap, 0, 0, width, height, 0xFFFFFF);
	page.Render(bitmap, width, height);
	return ::FPDFBitmap_GetBuffer(bitmap);
}

int WriteBMP(const Data &d){
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, ".bmp", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		try {
			PDFPage page(doc, i);
			string outfile;
			if(pages == 1)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".bmp");
			else outfile = outputpre + to_string(i) + outputpost;
			int width = (int) (page.GetWidth() *d.scale);
			int height = (int) (page.GetHeight() * d.scale);
			BITMAPINFOHEADER infohead = {0};
			void *buf;
			hBitmap bitmap;
			PDFiumBitmap pdfbitmap;
			infohead.biSize = sizeof(BITMAPINFOHEADER);
			infohead.biWidth = width;
			infohead.biHeight = -height;
			infohead.biPlanes = 1;
			infohead.biBitCount = 32;
			infohead.biCompression = BI_RGB;
			infohead.biSizeImage = width*height * 4;
			if(d.use_gdi) {
				buf = GetBitmapByGDI(page, bitmap.bitmap, infohead);
			} else {
				buf = GetBitmapByPDFBITMAP(page, pdfbitmap.bitmap, infohead);
			}
			BITMAPFILEHEADER header = {0};
			header.bfType = 0x4d42;
			header.bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
			header.bfSize = header.bfOffBits + infohead.biSizeImage;

			FILE *fp;
			if(auto fopen_rv = ::fopen_s(&fp, outfile.c_str(), "wb") != 0) {
				throw runtime_error("failed to open (" + to_string(fopen_rv) + ") " + outfile);
			}
			::fwrite(&header, sizeof(BITMAPFILEHEADER), 1, fp);
			::fwrite(&infohead, sizeof(BITMAPINFOHEADER), 1, fp);
			::fwrite(buf, infohead.biSizeImage, 1, fp);
			::fclose(fp);
		} catch(runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}


int WritePNG(const Data &d) {
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, ".png", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		try {
			PDFPage page(doc, i);
			string outfile;
			if(pages == 1)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".png");
			else outfile = outputpre + to_string(i) + outputpost;
			int width = (int) (page.GetWidth() *d.scale);
			int height = (int) (page.GetHeight() * d.scale);
			BITMAPINFOHEADER infohead = {0};
			void *buf;
			hBitmap bitmap;
			PDFiumBitmap pdfbitmap;
			infohead.biSize = sizeof(BITMAPINFOHEADER);
			infohead.biWidth = width;
			infohead.biHeight = -height;
			infohead.biPlanes = 1;
			infohead.biBitCount = 32;
			infohead.biCompression = BI_RGB;
			infohead.biSizeImage = width*height * 4;
			int stride = width * 4;
			if(d.use_gdi) {
				buf = GetBitmapByGDI(page, bitmap.bitmap, infohead);
			} else {
				buf = GetBitmapByPDFBITMAP(page, pdfbitmap.bitmap, infohead);
				stride = ::FPDFBitmap_GetStride(pdfbitmap.bitmap);
			}
			vector<BYTE> pngbuf;
			if(!::image_diff_png::EncodeBGRAPNG((BYTE*) buf, width, height, stride, !d.transparent, &pngbuf)) {
				throw runtime_error("Failed to decode bitmap to png");
			}
			FILE *fp;
			if(auto fopen_rv = ::fopen_s(&fp, outfile.c_str(), "wb") != 0) {
				throw runtime_error("failed to open (" + to_string(fopen_rv) + ") " + outfile);
			}
			::fwrite(&pngbuf[0], 1, pngbuf.size(), fp);
			::fclose(fp);
		} catch(runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}


int WriteEMF(const Data &d){
	string outputpre,outputpost;
	GetOutputFileName(d.input, d.output, ".emf", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		try {
			PDFPage page(doc, i);
			string outfile;
			if(pages == 1)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".emf");
			else outfile = outputpre + to_string(i) + outputpost;
			HDC dc = ::CreateEnhMetaFile(NULL, outfile.c_str(), NULL, NULL);
			::SetMapMode(dc, MM_ANISOTROPIC);
			::SetWindowExtEx(dc, 1000, 1000, NULL);
			int width = (int) (page.GetWidth() * 1000*d.scale);
			int height = (int) (page.GetHeight() * 1000*d.scale);
			HRGN rgn = CreateRectRgn(0, 0, width, height);
			::SelectClipRgn(dc, rgn);
			::DeleteObject(rgn);
			page.Render(dc, width, height);
			::DeleteEnhMetaFile(::CloseEnhMetaFile(dc));
		}
		catch(runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}


int main(int argc, char *argv[]) {
	::FPDF_InitLibrary();
	vector<Data> files;
	{
		bool current_use_gdi = false;
		int current_target = EMF;
		string current_output;
		bool current_transparent = false;
		int current_scale = 1;
		for(int i = 1; i < argc; ++i) {
			std::string arg = argv[i];
			if(arg == "--use-gdi")current_use_gdi = true;
			else if(arg == "--bmp")current_target = BMP;
			else if(arg == "--emf")current_target = EMF;
			else if(arg == "--png")current_target = PNG;
			else if(arg == "--transparent")current_transparent = true;
			else if(arg.find("--scale=") == 0)current_scale = std::atoi(arg.substr(string("--scale=").length()).c_str());
			else if(arg.find("--output=") == 0) current_output = arg.substr(string("--output=").length());
			else if(arg == "--help") {
				ShowUsage();
				return 0;
			} else {
				Data d;
				d.input = GetFullName(arg);
				d.output = GetFullName(current_output);
				d.target = current_target;
				d.use_gdi = current_use_gdi;
				d.scale = current_scale;
				d.transparent = current_transparent;
				files.push_back(d);
				current_output = "";
			}
		}
	}
	/*
	Data d;
	d.input = "C:\\Users\\Abe_Noriyuki\\Desktop\\equation.pdf";
	d.scale = 5;
	d.target = PNG;
	d.use_gdi = true;
	d.transparent = true;
	files.push_back(d);
	*/
	for(auto d : files) {
		try {
			switch(d.target) {
			case EMF:
				WriteEMF(d);
				break;
			case BMP:
				WriteBMP(d);
				break;
			case PNG:
				WritePNG(d);
				break;
			default:
				break;
			}
		}
		catch(runtime_error e) {
			cout << e.what() << endl;
		}
	}

	::FPDF_DestroyLibrary();
	return 0;
}




#ifdef __cplusplus
extern "C"{
#endif
BOOL APIENTRY DllMain(HINSTANCE hInstance, DWORD ul_reason_for_call, LPVOID pParam) {
	return TRUE;
}
__declspec(dllexport) int GetPDFPage(char *path) {
	try {
		PDFDoc doc(path);
		return doc.GetPageCount();
	}
	catch(runtime_error) {
		return -1;
	}
}
#ifdef __cplusplus
}
#endif