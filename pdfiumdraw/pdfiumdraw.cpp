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
#include <algorithm>
#include <sstream>
#include <fpdfedit.h>
#include <GdiPlus.h>

const int EMF = 0;
const int BMP = 1;
const int PNG = 2;
const int JPG = 3;
const int GIF = 4;
const int TIFF = 5;

const int RENDER_FLAG = FPDF_PRINTING;

using namespace std;
using namespace Gdiplus;

struct Data {
	Data() : viewport({0, 0, 0, 0}){
	}
	bool use_gdi = false;
	string input;
	string output;
	int target = EMF;
	int scale = 1;
	vector<int> pages;
	bool transparent = false;
	RECT viewport;
};

void ShowUsage() {
	cout << "pdfiumdraw [option] input" << endl;
	cout << "option:" << endl;
	cout << "  --use-gdi: use gdi for drawing" << endl;
	cout << "  --emf: output emf file (default)" << endl;
	cout << "  --bmp: output bmp file" << endl;
	cout << "  --png: output png file" << endl;
	cout << "  --jpg: output jpeg file" << endl;
	cout << "  --gif: output gif file" << endl;
	cout << "  --tiff: output tiff file" << endl;
	cout << "  --scale: specify resolution level" << endl;
	cout << "  --transparent: output transparent png file" << endl;
	cout << "  --output=<file>: specify output file name" << endl;
	cout << "  --pages=<page>: specify output page" << endl;
//	cout << "  --viewport=<left>,<top>,<right>,<bottom>: specify viewport" << endl;
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
	PDFDoc(FPDF_DOCUMENT d) {
		doc = d;
	}
	~PDFDoc() {
		if(doc != NULL)::FPDF_CloseDocument(doc);
	}
	int GetPageCount() { return ::FPDF_GetPageCount(doc); }
	FPDF_DOCUMENT doc;
	PDFDoc() = delete;
	PDFDoc(const PDFDoc &) = delete;
	PDFDoc &operator=(const PDFDoc &) = delete;
private:
	friend class PDFPage;
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
	FPDF_PAGE page;
	PDFPage() = delete;
	PDFPage(const PDFPage &) = delete;
	PDFPage &operator=(const PDFPage &) = delete;
};

void GetOutputFileName(const string &input, const string &output, string extension, string &outputpre, string &outputpost) {
	if(output == "") {
		outputpre = GetDirectory(input) + "\\" + GetFileNameWithoutExtension(input) + "-";
		outputpost = extension;
	} else {
		auto r = output.rfind("%d");
		if(r == string::npos) {
			outputpre = GetDirectory(output) + "\\" + GetFileNameWithoutExtension(output);
			outputpost = GetExtension(output);
		} else {
			outputpre = output.substr(0, r);
			outputpost = output.substr(r + 2);
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

wstring ToUnicode(const string &str) {
	static vector<wchar_t> buf;
	DWORD size = ::MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, str.c_str(), str.length(), NULL, 0);
	buf.resize(size + 1);
	size = ::MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, str.c_str(), str.length(), &buf[0], size);
	buf[size] = L'\0';
	return wstring(&buf[0]);
}

// from https://msdn.microsoft.com/en-us/library/ms533843.aspx
int GetEncoderClsid(const WCHAR* format, CLSID* pClsid) {
	UINT  num = 0;          // number of image encoders
	UINT  size = 0;         // size of the image encoder array in bytes

	ImageCodecInfo* pImageCodecInfo = NULL;

	GetImageEncodersSize(&num, &size);
	if(size == 0)
		return -1;  // Failure

	pImageCodecInfo = (ImageCodecInfo*) (malloc(size));
	if(pImageCodecInfo == NULL)
		return -1;  // Failure

	GetImageEncoders(num, size, pImageCodecInfo);

	for(UINT j = 0; j < num; ++j) {
		if(wcscmp(pImageCodecInfo[j].MimeType, format) == 0) {
			*pClsid = pImageCodecInfo[j].Clsid;
			free(pImageCodecInfo);
			return j;  // Success
		}
	}

	free(pImageCodecInfo);
	return -1;  // Failure
}

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

void *GetBitmapByPDFBITMAP(PDFPage &page, FPDF_BITMAP &bitmap, const BITMAPINFOHEADER &infohead,bool transparent){
	int width = abs(infohead.biWidth);
	int height = abs(infohead.biHeight);
	bitmap = ::FPDFBitmap_Create(width, height, transparent ? TRUE : FALSE);
	::FPDFBitmap_FillRect(bitmap, 0, 0, width, height, 0x00FFFFFF);
	page.Render(bitmap, width, height);
	return ::FPDFBitmap_GetBuffer(bitmap);
}
BITMAPINFOHEADER GetInfoHeader(int width, int height) {
	BITMAPINFOHEADER infohead = {0};
	infohead.biSize = sizeof(BITMAPINFOHEADER);
	infohead.biWidth = width;
	infohead.biHeight = -height;
	infohead.biPlanes = 1;
	infohead.biBitCount = 32;
	infohead.biCompression = BI_RGB;
	infohead.biSizeImage = width*height * 4;
	return infohead;
}


int WriteBMP(const Data &d){
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, ".bmp", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		if(!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
		try {
			PDFPage page(doc, i);
			string outfile;
			if(pages == 1)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".bmp");
			else outfile = outputpre + to_string(i) + outputpost;
			int width = (int) (page.GetWidth() *d.scale);
			int height = (int) (page.GetHeight() * d.scale);
			BITMAPINFOHEADER infohead = GetInfoHeader(width, height);
			void *buf;
			hBitmap bitmap;
			PDFiumBitmap pdfbitmap;
			if(d.use_gdi) {
				buf = GetBitmapByGDI(page, bitmap.bitmap, infohead);
			} else {
				buf = GetBitmapByPDFBITMAP(page, pdfbitmap.bitmap, infohead, /*d.transparent*/false);
			}
			BITMAPFILEHEADER header = {0};
			header.bfType = 0x4d42;
			header.bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
			header.bfSize = header.bfOffBits + infohead.biSizeImage;

			FILE *fp;
			if(auto fopen_rv = ::fopen_s(&fp, outfile.c_str(), "wb") != 0) {
				throw runtime_error("failed to open (error code = " + to_string(fopen_rv) + ") " + outfile);
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

const wchar_t *GetEncodeName(string imgtype) {
	static wstring rv;
	if(imgtype == "jpg")rv = L"image/jpeg";
	else rv = L"image/" + ToUnicode(imgtype);
	return rv.c_str();
}

int WriteIMG(const Data &d,string imgtype) {
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, "." + imgtype, outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		if(!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
		try {
			PDFPage page(doc, i);
			string outfile;
			if(pages == 1)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + "." + imgtype);
			else outfile = outputpre + to_string(i) + outputpost;
			int width = (int) (page.GetWidth() *d.scale);
			int height = (int) (page.GetHeight() * d.scale);
			BITMAPINFOHEADER infohead = GetInfoHeader(width, height);
			void *buf;
			hBitmap bitmap;
			PDFiumBitmap pdfbitmap;
			int stride = width * 4;
			if(d.use_gdi) {
				buf = GetBitmapByGDI(page, bitmap.bitmap, infohead);
			} else {
				buf = GetBitmapByPDFBITMAP(page, pdfbitmap.bitmap, infohead,d.transparent);
				stride = ::FPDFBitmap_GetStride(pdfbitmap.bitmap);
			}
			Gdiplus::Bitmap b(width, height, stride, PixelFormat32bppARGB, (BYTE*) buf);
			CLSID clsid;
			if(GetEncoderClsid(GetEncodeName(imgtype), &clsid) >= 0) {
				if(b.Save(ToUnicode(outfile).c_str(), &clsid, NULL) != Ok) {
					throw runtime_error("failed to open " + outfile);
				}
			} else throw runtime_error("failed to encode bitmap to " + imgtype); 
		} catch(runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}

int WritePNG(const Data &d) {
	return WriteIMG(d, "png");
}
int WriteJPG( Data &d) {
	Data dd = d;
	dd.transparent = false;
	return WriteIMG(d, "jpg");
}
int WriteGIF(const Data &d) {
	Data dd = d;
	dd.transparent = false;
	return WriteIMG(d, "gif");
}
int WriteTIFF(const Data &d) {
	Data dd = d;
	dd.transparent = false;
	return WriteIMG(d, "tiff");
}

int WriteEMF(const Data &d){
	string outputpre,outputpost;
	GetOutputFileName(d.input, d.output, ".emf", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for(int i = 0; i < pages; ++i) {
		if(!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
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
			RECT rc;
			rc.left = 0; rc.top = 0; rc.right = width; rc.bottom = height;
			::FillRect(dc, &rc, (HBRUSH)::GetStockObject(WHITE_BRUSH));
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

std::vector<std::string> split(const std::string &str, char sep) {
	std::vector<std::string> v;
	std::stringstream ss(str);
	std::string buffer;
	while(std::getline(ss, buffer, sep)) {
		v.push_back(buffer);
	}
	return v;
}

int main(int argc, char *argv[]) {
	::FPDF_InitLibrary();
	vector<Data> files;
	{
		Data current_data;
		bool output_page = false;
		for(int i = 1; i < argc; ++i) {
			std::string arg = argv[i];
			if(arg == "--use-gdi")current_data.use_gdi = true;
			else if(arg == "--use-gdi-")current_data.use_gdi = false;
			else if(arg == "--transparent")current_data.transparent = true;
			else if(arg == "--transparent-")current_data.transparent = false;
			else if(arg == "--bmp")current_data.target = BMP;
			else if(arg == "--emf")current_data.target = EMF;
			else if(arg == "--png")current_data.target = PNG;
			else if(arg == "--jpg")current_data.target = JPG;
			else if(arg == "--gif")current_data.target = GIF;
			else if(arg == "--tiff")current_data.target = TIFF;
			else if(arg == "--output-page")output_page = true;
			else if(arg.find("--pages=") == 0)current_data.pages.push_back(std::atoi(arg.substr(string("--pages=").length()).c_str()) - 1);
			else if(arg.find("--scale=") == 0)current_data.scale = std::atoi(arg.substr(string("--scale=").length()).c_str());
			else if(arg.find("--output=") == 0) current_data.output = arg.substr(string("--output=").length());
			else if(arg.find("--viewport=") == 0) {
				auto viewport = split(arg.substr(string("--viewport=").length()),',');
				if(viewport.size() == 4) {
					current_data.viewport.left = atoi(viewport[0].c_str());
					current_data.viewport.top = atoi(viewport[1].c_str());
					current_data.viewport.right = atoi(viewport[2].c_str());
					current_data.viewport.bottom = atoi(viewport[3].c_str());
				}
			}else if(arg == "--help") {
				ShowUsage();
				return 0;
			} else {
				if(output_page) {
					try {
						cout << to_string(PDFDoc(GetFullName(arg)).GetPageCount()) << endl;
					} catch(runtime_error e) {
						cout << e.what() << endl;
					}
					output_page = false;
				} else {
					Data d = current_data;
					d.input = GetFullName(arg);
					d.output = GetFullName(current_data.output);
					files.push_back(d);
					current_data.output = "";
					current_data.pages.clear();
					current_data.viewport = RECT{0, 0, 0, 0};
				}
			}
		}
	}
	int errpages = 0;
	for(auto d : files) {
		try {
			switch(d.target) {
			case EMF:
				errpages += WriteEMF(d);
				break;
			case BMP:
				errpages += WriteBMP(d);
				break;
			case PNG:
				errpages += WritePNG(d);
				break;
			case JPG:
				errpages += WriteJPG(d);
				break;
			case GIF:
				errpages += WriteGIF(d);
				break;
			case TIFF:
				errpages += WriteTIFF(d);
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
	return errpages;
}



