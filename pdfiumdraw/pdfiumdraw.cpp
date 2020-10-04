#define NOMINMAX 
#include <windows.h>
#include <tchar.h>
#include <string>
#include <vector>
#include <set>
#include <tuple>
#include <iostream>
#include <fstream>
#include <exception>
#include <sstream>
#include <shlwapi.h>
#include <wincodec.h>
#include <fpdfview.h>
#include <fpdf_transformpage.h>
#include <fpdf_edit.h>
#include <fpdf_ppo.h>
#include <fpdf_save.h>
#include <fpdf_annot.h>
#include <algorithm>

const int PDF = 0;
const int EMF = 1;
const int BMP = 2;
const int PNG = 3;
const int JPG = 4;
const int GIF = 5;
const int TIFF = 6;
const int WMF = 7;

const int DEFAULT_PDF_VERSION = 15;

const int RENDER_FLAG = FPDF_PRINTING;

using namespace std;

struct Data {
	Data() : viewport({0, 0, 0, 0}) {
	}
	bool use_gdi = false;
	string input;
	string output;
	int target = EMF;
	int input_format = PDF;
	int scale = 1;
	vector<int> pages;
	bool transparent = false;
	float extent = 50;
	RECT viewport;
	COLORREF backcolor = RGB(255, 255, 255);
};

/** �y�[�W�ԍ��̈���
--pages=1,3 --output=out.pdf
�̎��C
* �o�͂�PDF�łȂ����out1.emf��out3.emf���o�͂���D
* �o�͂�PDF�Ȃ�ΑS2�y�[�W��out.pdf���o�͂���D
--output=out-%d.pdf�Ȃ��
* �o�͂�PDF�łȂ����out-1.emf��out-3.emf���o�͂���D
* �o�͂�PDF�Ȃ�ΑS2�y�[�W��out-%d.pdf���o�͂���D

��₱�����̂�--pdf�͉B���I�v�V�����D
*/
void ShowUsage() {
	cout << "pdfiumdraw [option] input" << endl;
	cout << "option:" << endl;
	cout << "  --use-gdi: use GDI for drawing" << endl;
	cout << "  --emf: output emf file (default)" << endl;
	cout << "  --bmp: output bmp file" << endl;
	cout << "  --png: output png file" << endl;
	cout << "  --jpg: output jpeg file" << endl;
	cout << "  --gif: output gif file" << endl;
	cout << "  --tiff: output tiff file" << endl;
	//cout << "  --pdf: output pdf file" << endl;
	//cout << "  --merge: merge output files (PDF -> PDF only)" << endl;
	cout << "  --scale: specify scale" << endl;
	cout << "  --transparent: output transparent file (if possible)" << endl;
	cout << "  --backcolor=<color>: specify background color (e.g. FFFFFF)" << endl;
	cout << "  --output=<file>: specify output file name (%d for the page number)" << endl;
	cout << "  --pages=<page>: specify output page (e.g. 1,5-9,10)" << endl;
//	cout << "  --viewport=<left>,<top>,<right>,<bottom>: specify viewport" << endl;
	cout << "  --help: show this message" << endl;
}

string GetCurrentDirectory() {
	static string currentdir = "";
	if (currentdir == "") {
		char buf[MAX_PATH]; buf[0] = '\0';
		::GetCurrentDirectory(MAX_PATH, buf);
		currentdir = buf;
		if (currentdir.length() > 0 && currentdir[currentdir.length() - 1] != '\\')currentdir += "\\";
	}
	return currentdir;
}

inline bool EndsWith(string s, char c) {
	return (s.length() > 0 && s[s.length() - 1] == c);
}

string GetFullName(const string &f) {
	if (f == "")return "";
	else if (f.length() > 1 && f[1] == ':')return f;
	else return GetCurrentDirectory() + f;
}

string GetExtension(string file) {
	auto r = ::PathFindExtension(file.c_str());
	if (r == nullptr)return "";
	else return string(r);
}

string GetDirectory(string file) {
	if (EndsWith(file, '\\'))return file;
	auto r = ::PathFindFileName(file.c_str());
	if (r == nullptr || r == file.c_str())return "";
	else return file.substr(0, r - file.c_str() - 1);
}

string GetFileName(string file) {
	if (EndsWith(file, '\\'))return "";
	auto r = ::PathFindFileName(file.c_str());
	if (r == nullptr)return file;
	else return string(r);
}

string GetFileNameWithoutExtension(string file) {
	auto f = GetFileName(file);
	auto r = ::PathFindExtension(f.c_str());
	if (r == nullptr)return f;
	else return f.substr(0, r - f.c_str());
}

string UTF16ToUTF8(const wstring& str) {
	static ::std::vector<char> buf;
	DWORD size = ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), str.length(), NULL, 0, NULL, NULL);
	buf.resize(size + 1);
	size = ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), str.length(), &buf[0], buf.size(), NULL, NULL);
	buf[size] = '\0';
	return string(&buf[0]);
}

class PDFPage;
class PDFDoc {
public:
	PDFDoc(const string &path) {
		doc = ::FPDF_LoadDocument(path.c_str(), nullptr);
		if (doc == nullptr)throw runtime_error("filed to open " + path);
	}
	PDFDoc(FPDF_DOCUMENT d) {
		doc = d;
	}
	~PDFDoc() {
		if (doc != nullptr)::FPDF_CloseDocument(doc);
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
		if (p == nullptr)throw runtime_error("faild to open page " + to_string(pageNum + 1));
		page = p;
	}
	~PDFPage() { ::FPDF_ClosePage(page); page = nullptr; }
	void Render(HDC hdc, int width, int height) {
		::FPDF_RenderPage(hdc, page, 0, 0, width, height, 0, RENDER_FLAG);
	}
	void Render(HDC hdc, int x, int y, int width, int height) {
		::FPDF_RenderPage(hdc, page, x, y, width, height, 0, RENDER_FLAG);
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
	if (output == "") {
		outputpre = GetDirectory(input) + "\\" + GetFileNameWithoutExtension(input) + "-";
		outputpost = extension;
	} else {
		auto r = output.rfind("%d");
		if (r == string::npos) {
			outputpre = GetDirectory(output) + "\\" + GetFileNameWithoutExtension(output);
			outputpost = GetExtension(output);
		} else {
			outputpre = output.substr(0, r);
			outputpost = output.substr(r + 2);
		}
	}
}

class hBitmap {
public:
	HBITMAP bitmap = nullptr;
	~hBitmap() {
		if (bitmap)::DeleteObject(bitmap);
		bitmap = nullptr;
	}
};
class hDC {
public:
	HDC dc = nullptr;
	~hDC() {
		if (dc)::DeleteObject(dc);
		dc = nullptr;
	}
};
class PDFiumBitmap {
public:
	FPDF_BITMAP bitmap = nullptr;
	~PDFiumBitmap() {
		if (bitmap)::FPDFBitmap_Destroy(bitmap);
		bitmap = nullptr;
	}
};

wstring ToUnicode(const string &str) {
	static vector<wchar_t> buf;
	DWORD size = ::MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, str.c_str(), str.length(), nullptr, 0);
	buf.resize(size + 1);
	size = ::MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, str.c_str(), str.length(), &buf[0], size);
	buf[size] = L'\0';
	return wstring(&buf[0]);
}

void *GetBitmapByGDI(PDFPage &page, HBITMAP &bitmap, const BITMAPINFOHEADER &infohead, COLORREF backcolor) {
	void *buf;
	int width = abs(infohead.biWidth);
	int height = abs(infohead.biHeight);
	BITMAPINFO info = {0};
	info.bmiHeader = infohead;
	HDC hdc = ::GetDC(nullptr);
	bitmap = ::CreateDIBSection(nullptr, &info, DIB_RGB_COLORS, (void**)&buf, nullptr, 0);
	hDC bitmapdc;
	bitmapdc.dc = ::CreateCompatibleDC(hdc);
	::ReleaseDC(nullptr, hdc);
	auto savebitmap = (HBITMAP)::SelectObject(bitmapdc.dc, bitmap);
	RECT rc;
	rc.left = 0; rc.top = 0; rc.right = width; rc.bottom = height;
	HBRUSH brush = ::CreateSolidBrush(backcolor);
	::FillRect(bitmapdc.dc, &rc, brush);
	::DeleteObject(brush);
	page.Render(bitmapdc.dc, width, height);
	bitmap = (HBITMAP)::SelectObject(bitmapdc.dc, savebitmap);
	return buf;
}

void *GetBitmapByPDFBITMAP(PDFPage &page, FPDF_BITMAP &bitmap, const BITMAPINFOHEADER &infohead, bool transparent, COLORREF backcolor) {
	int width = abs(infohead.biWidth);
	int height = abs(infohead.biHeight);
	bitmap = ::FPDFBitmap_Create(width, height, transparent ? TRUE : FALSE);
	::FPDFBitmap_FillRect(bitmap, 0, 0, width, height, backcolor);
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
	infohead.biSizeImage = width * height * 4;
	return infohead;
}

_GUID GetFormat(const string &imgtype) {
	if (imgtype == "bmp")return GUID_ContainerFormatBmp;
	else if (imgtype == "png")return GUID_ContainerFormatPng;
	else if (imgtype == "jpg")return GUID_ContainerFormatJpeg;
	else if (imgtype == "tiff")return GUID_ContainerFormatTiff;
	else if (imgtype == "gif")return GUID_ContainerFormatGif;
	else return GUID_ContainerFormatBmp;
}

// ComPtr�����t����Ȃ��̂Łc�c
template<class T> class ComPtr {
	T *buf = nullptr;
public:
	~ComPtr() {
		if (buf != nullptr) {
			buf->Release();
		}
		buf = nullptr;
	}
	const T*get() { return buf; }
	operator T*() { return buf; }
	T *operator->() { return buf; }
	T **operator&() { return &buf; }
	ComPtr(T*b) { buf = b; }
	ComPtr() {}
	ComPtr(const ComPtr &) = delete;
	ComPtr &operator=(const ComPtr &) = delete;
};

void SaveIMG(string outfile, _GUID format, IWICImagingFactory *factory, IWICBitmap *b) {
	ComPtr<IWICBitmapEncoder> encoder;
	if (!SUCCEEDED(factory->CreateEncoder(format, nullptr, &encoder)))throw runtime_error("failed to create encoder");
	ComPtr<IWICStream> stream;
	if (!SUCCEEDED(factory->CreateStream(&stream)))throw runtime_error("failed to create stream");
	if (!SUCCEEDED(stream->InitializeFromFilename(ToUnicode(outfile).c_str(), GENERIC_WRITE)))throw runtime_error("failed to create " + outfile);
	if (!SUCCEEDED(encoder->Initialize(stream, WICBitmapEncoderNoCache)))throw runtime_error("failed to initialize encoder");
	ComPtr<IWICBitmapFrameEncode> frame;
	if (!SUCCEEDED(encoder->CreateNewFrame(&frame, nullptr)))throw runtime_error("failed to create frame");
	if (!SUCCEEDED(frame->Initialize(nullptr)))throw runtime_error("failed to initialize frame");
	if (!SUCCEEDED(frame->WriteSource(b, nullptr)))throw runtime_error("failed to write bitmap");
	if (!SUCCEEDED(frame->Commit()))throw runtime_error("failed to commit frame");
	if (!SUCCEEDED(encoder->Commit()))throw runtime_error("failed to write " + outfile);
}

int WriteIMG(const Data &d, string imgtype) {
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, "." + imgtype, outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for (int i = 0; i < pages; ++i) {
		if (!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
		try {
			PDFPage page(doc, i);
			string outfile;
			if ((pages == 1 || d.pages.size() == 1) && d.output.find("%d") == string::npos)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + "." + imgtype);
			else outfile = outputpre + to_string(i + 1) + outputpost;
			cout << "output: " << outfile << endl;
			int width = (int)(page.GetWidth() *d.scale);
			int height = (int)(page.GetHeight() * d.scale);
			if (abs(width) > (1 << 15) || abs(height) > (1 << 15)) {
				throw runtime_error("image size is boo big (" + d.input + ", page=" + to_string(i + 1) + ")");
			}
			BITMAPINFOHEADER infohead = GetInfoHeader(width, height);
			void *buf;
			hBitmap bitmap;
			PDFiumBitmap pdfbitmap;
			int stride = width * 4;
			if (d.use_gdi) {
				buf = GetBitmapByGDI(page, bitmap.bitmap, infohead, d.backcolor);
			} else {
				buf = GetBitmapByPDFBITMAP(page, pdfbitmap.bitmap, infohead, d.transparent, d.backcolor);
				stride = ::FPDFBitmap_GetStride(pdfbitmap.bitmap);
			}
			ComPtr<IWICImagingFactory> factory;
			if (!SUCCEEDED(::CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&factory))))throw runtime_error("failed to initialize WIC");
			ComPtr<IWICBitmap> b;
			if (!SUCCEEDED(factory->CreateBitmapFromMemory(width, height, GUID_WICPixelFormat32bppBGRA, stride, stride*height, (BYTE*)buf, &b)))throw runtime_error("failed to create bitmap");
			SaveIMG(outfile, GetFormat(imgtype), factory, b);
		}
		catch (runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}

int WriteBMP(const Data &d) {
	Data dd = d;
	dd.transparent = false;
	return WriteIMG(dd, "bmp");
}
int WritePNG(const Data &d) {
	return WriteIMG(d, "png");
}
int WriteJPG(const Data &d) {
	Data dd = d;
	dd.transparent = false;
	return WriteIMG(dd, "jpg");
}
int WriteGIF(const Data &d) {
	return WriteIMG(d, "gif");
}
int WriteTIFF(const Data &d) {
	return WriteIMG(d, "tiff");
}

int CALLBACK EnhMetaFileProc(HDC hdc, HANDLETABLE FAR *lpHTable, ENHMETARECORD FAR *lpEMFR, int nObj, LPARAM lpData) {
	::PlayEnhMetaFileRecord(hdc, lpHTable, lpEMFR, nObj);
	return TRUE;
}

void DrawEMF(HDC dc, PDFPage &page, int extent, int scale, bool transparent, COLORREF background) {
//	cout << "PDF: width = " << page.GetWidth() << ", height = " << page.GetHeight() << endl;
//	cout << "PDF: width = " << (96*page.GetWidth()/72) << ", height = " << (96*page.GetHeight()/72) << endl;
//	cout << "PDF: width = " << (page.GetWidth() * scale * extent) << ", height = " << (page.GetHeight() * scale * extent) << endl;
	int width = static_cast<int>(page.GetWidth() * scale * extent) + 1;
	int height = static_cast<int>(page.GetHeight() * scale * extent) + 1;
	//width += 4 * scale*extent; height += 4 * scale*extent;
//	cout << "ClipRgn: width = " << width << ", height = " << height << endl;
	//int width = (int) (page.GetWidth() * 1000 * d.scale);
	//int height = (int) (page.GetHeight() * 1000 * d.scale);
	if(extent != 1) {
		::SetMapMode(dc, MM_ANISOTROPIC);
		::SetWindowExtEx(dc, extent, extent, nullptr);
	}
	HRGN rgn = CreateRectRgn(0, 0, width, height);
	::SelectClipRgn(dc, rgn);
	RECT rc;
	::GetClipBox(dc, &rc);
	int tmpscale = std::max((int)(width / rc.right) + 1, (int)(height / rc.bottom) + 1);
//	cout << "ClipRgn: left = " << rc.left << ", right = " << rc.right << ", top = " << rc.top << ", bottom = " << rc.bottom << endl;
// �����L���h��i����Ȃ��Ɛ؂��j2�ɗ��R�͂Ȃ��D
	rc.left = 0; rc.top = 0; rc.bottom = height + static_cast<int>(4 * extent); rc.right = width + static_cast<int>(4 * extent);
	if(transparent) {
		::SetBkMode(dc, TRANSPARENT);
		::FillRect(dc, &rc, (HBRUSH)::GetStockObject(NULL_BRUSH));
	} else {
		::SetBkMode(dc, OPAQUE);
		auto brush = ::CreateSolidBrush(background);
		::FillRect(dc, &rc, brush);
		::DeleteObject(brush);
	}
	HDC tmpdc = ::CreateEnhMetaFile(nullptr, nullptr, nullptr, nullptr);
	::SetMapMode(tmpdc, MM_ANISOTROPIC);
	::SetWindowExtEx(tmpdc, extent*tmpscale, extent*tmpscale, nullptr);
	//rc.left = 0; rc.top = 0; rc.bottom = height - 2 * extent*tmpscale; rc.right = width - 2 * extent*tmpscale;
	rc.left = 0; rc.top = 0; rc.bottom = height; rc.right = width;
	::SetBkMode(tmpdc, TRANSPARENT);
	::FillRect(tmpdc, &rc, (HBRUSH)::GetStockObject(NULL_BRUSH));
	page.Render(tmpdc, 0, 0, width, height);
	HENHMETAFILE  meta = ::CloseEnhMetaFile(tmpdc);
	//	rc.left = 0; rc.top = 0; rc.bottom = height - extent*tmpscale; rc.right = width - extent*tmpscale;
	rc.left = 0; rc.top = 0; rc.bottom = height; rc.right = width;
	::EnumEnhMetaFile(dc, meta, (ENHMFENUMPROC)EnhMetaFileProc, nullptr, &rc);
	::DeleteEnhMetaFile(meta);
	::DeleteObject(rgn);
}

int WriteWMF(const Data &d) {
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, ".wmf", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for (int i = 0; i < pages; ++i) {
		if (!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
		try {
			string outfile;
			if ((pages == 1 || d.pages.size() == 1) && d.output.find("%d") == string::npos)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".wmf");
			else outfile = outputpre + to_string(i + 1) + outputpost;
			cout << "output: " << outfile << endl;
			HDC dc = ::CreateEnhMetaFile(nullptr, nullptr, nullptr, nullptr);
			PDFPage page(doc, i);
			DrawEMF(dc, page, static_cast<int>(d.extent), d.scale, d.transparent, d.backcolor);
			auto enhmeta = ::CloseEnhMetaFile(dc);
			auto size = ::GetWinMetaFileBits(enhmeta, 0, nullptr, MM_ANISOTROPIC, dc);
			vector<BYTE> buf;
			buf.resize(size + 1);
			::GetWinMetaFileBits(enhmeta, size, &buf[0], MM_ANISOTROPIC, dc);
			ofstream out(outfile, ios_base::out | ios_base::binary);
			if (out)out.write((char*)&buf[0], size);
			::DeleteEnhMetaFile(enhmeta);
		}
		catch (runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}

int WriteEMF(const Data &d) {
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, ".emf", outputpre, outputpost);
	PDFDoc doc(d.input);
	auto pages = doc.GetPageCount();
	int errpages = 0;
	for (int i = 0; i < pages; ++i) {
		if (!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
		try {
			string outfile;
			if ((pages == 1 || d.pages.size() == 1) && d.output.find("%d") == string::npos)outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + ".emf");
			else outfile = outputpre + to_string(i + 1) + outputpost;
			cout << "output: " << outfile << endl;
			HDC dc = ::CreateEnhMetaFile(nullptr, outfile.c_str(), nullptr, nullptr);
			PDFPage page(doc, i);
			DrawEMF(dc, page, static_cast<int>(d.extent), d.scale, d.transparent, d.backcolor);
			::DeleteEnhMetaFile(::CloseEnhMetaFile(dc));
		}
		catch (runtime_error e) {
			cout << e.what() << endl;
			++errpages;
		}
	}
	return errpages;
}

int ConvertIMG(Data &d) {
	string outputimgtype;
	switch (d.target) {
	case PNG: outputimgtype = "png"; break;
	case GIF:outputimgtype = "gif"; break;
	case TIFF:outputimgtype = "tiff"; break;
	case BMP:outputimgtype = "bmp"; break;
	case JPG:outputimgtype = "jpg"; break;
	default: return 1;
	}
	string outputpre, outputpost;
	GetOutputFileName(d.input, d.output, "." + outputimgtype, outputpre, outputpost);
	string outfile = (d.output != "" ? d.output : GetDirectory(d.input) + "\\" + GetFileNameWithoutExtension(d.input) + "." + outputimgtype);
	HRESULT hr;
	ComPtr<IWICImagingFactory> factory;
	if (!SUCCEEDED(hr = ::CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&factory))))throw runtime_error("failed to initialize WIC (" + to_string(hr) + ")");
	ComPtr<IStream> instream;
	if (!SUCCEEDED(hr = SHCreateStreamOnFile(d.input.c_str(), STGM_READ, &instream)))throw runtime_error("failed to create stream (" + to_string(hr) + ")");
	ComPtr<IWICBitmapDecoder> decoder;
	if (!SUCCEEDED(hr = factory->CreateDecoderFromStream(instream, nullptr, WICDecodeMetadataCacheOnDemand, &decoder)))throw runtime_error("failed to create decoder (" + to_string(hr) + ")");
	ComPtr<IWICBitmapFrameDecode> inframe;
	if (!SUCCEEDED(hr = decoder->GetFrame(0, &inframe)))throw runtime_error("failed to get frame (" + to_string(hr) + ")");
	ComPtr<IWICBitmapEncoder> encoder;
	if (!SUCCEEDED(hr = factory->CreateEncoder(GetFormat(outputimgtype), nullptr, &encoder)))throw runtime_error("failed to create encoder (" + to_string(hr) + ")");
	ComPtr<IWICStream> outstream;
	if (!SUCCEEDED(factory->CreateStream(&outstream)))throw runtime_error("failed to create stream");
	if (!SUCCEEDED(outstream->InitializeFromFilename(ToUnicode(outfile).c_str(), GENERIC_WRITE)))throw runtime_error("failed to create " + outfile);
	if (!SUCCEEDED(encoder->Initialize(outstream, WICBitmapEncoderNoCache)))throw runtime_error("failed to initialize encoder");
	ComPtr<IWICBitmapFrameEncode> outframe;
	if (!SUCCEEDED(hr = encoder->CreateNewFrame(&outframe, nullptr)))throw runtime_error("failed to create frame for output(" + to_string(hr) + ")");
	if (!SUCCEEDED(outframe->Initialize(nullptr)))throw runtime_error("failed to initialize frame");
	if (!SUCCEEDED(hr = outframe->WriteSource(inframe, nullptr)))throw runtime_error("failed to write bitmap (" + to_string(hr) + ")");
	if (!SUCCEEDED(hr = outframe->Commit()))throw runtime_error("failed to commit frame (" + to_string(hr) + ")");
	if (!SUCCEEDED(hr = encoder->Commit()))throw runtime_error("failed to write " + outfile + " (" + to_string(hr) + ")");
	return 0;
}

class PDFWriter : public FPDF_FILEWRITE {
public:
	PDFWriter(const string &path) : ofs(path, ios_base::out | ios_base::binary) {
		version = 1;
		WriteBlock = [](struct FPDF_FILEWRITE_* pThis, const void* pData, unsigned long size) {
			((PDFWriter*)pThis)->ofs.write((char*)pData, size);
			return TRUE;
		};
	}
private:
	ofstream ofs;
};

int WritePDF(const vector<tuple<string, vector<int>>> &files, string output) {
	PDFDoc newdoc(::FPDF_CreateNewDocument());
	int errpage = 0;
	int pos = 0;
	int pdfversion = 0;
	for (auto &&f : files) {
		PDFDoc doc(get<0>(f));
		int tmppdfversion;
		if (::FPDF_GetFileVersion(doc.doc, &tmppdfversion)) {
			pdfversion = std::max(pdfversion, tmppdfversion);
		}
		if (get<1>(f).empty()) {
			int pagecount = doc.GetPageCount();
			for (int i = 0; i < pagecount; ++i) {
				::FPDF_ImportPages(newdoc.doc, doc.doc, to_string(i + 1).c_str(), pos);
				++pos;
			}
		} else {
			for (auto p : get<1>(f)) {
				::FPDF_ImportPages(newdoc.doc, doc.doc, to_string(p + 1).c_str(), pos);
				++pos;
			}
		}
	}
	PDFWriter writer(output);
	if (pdfversion == 0)pdfversion = DEFAULT_PDF_VERSION;
	if (::FPDF_SaveWithVersion(newdoc.doc, &writer, 0, pdfversion) == FALSE)throw new runtime_error("fail to save " + output);
	else cout << "output: " << output << endl;
	return errpage;
}

int WritePDF(const Data &d) {
	PDFDoc doc(d.input);
	int pdfversion = 0;
	if (!::FPDF_GetFileVersion(doc.doc, &pdfversion))pdfversion = 0;
	if (pdfversion == 0)pdfversion = DEFAULT_PDF_VERSION;
	int errpage = 0;
	PDFDoc newdoc(::FPDF_CreateNewDocument());
	int pos = 0;
	if (d.pages.empty()) {
		for (int i = 0; i < doc.GetPageCount(); ++i) {
			::FPDF_ImportPages(newdoc.doc, doc.doc, to_string(i + 1).c_str(), pos);
			++pos;
		}
	} else {
		for (auto p : d.pages) {
			::FPDF_ImportPages(newdoc.doc, doc.doc, to_string(p + 1).c_str(), pos);
			++pos;
		}
	}
	PDFWriter writer(d.output);
	if (::FPDF_SaveWithVersion(newdoc.doc, &writer, 0, pdfversion) == FALSE)throw new runtime_error("fail to save " + d.output);
	else cout << "output: " << d.output << endl;
	return errpage;
}

void OutputBox(string boxname, Data &d) {
	try {
		PDFDoc doc(d.input);
		int pagecount = doc.GetPageCount();
		for (int i = 0; i < pagecount; ++i) {
			if (!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
			PDFPage page(doc, i);
			float left, bottom, right, top;
			FPDF_BOOL result;
			if (boxname == "cropbox") {
				result = ::FPDFPage_GetCropBox(page.page, &left, &bottom, &right, &top);
				if (!result) boxname = "mediabox";
			}
			if (boxname == "mediabox") result = ::FPDFPage_GetMediaBox(page.page, &left, &bottom, &right, &top);
			if (result) {
				int ileft = (int)left;
				int itop = (int)top;
				int ibottom = (int)bottom; if ((float)ibottom != bottom)++ibottom;
				int iright = (int)right; if ((float)iright != right)++iright;
				cout << "%%Page: " << i << endl;
				cout << "%%BoundingBox: " << ileft << " " << ibottom << " " << iright << " " << itop << endl;
				cout << "%%HiResBoundingBox: " << left << " " << bottom << " " << right << " " << top << endl;
			} else {
				cout << "Failed to get box size, file = " << d.input << ", page = " << i << endl;
			}
		}
	}
	catch (runtime_error e) { cout << e.what() << endl; }
}

void OutputRotate(Data &d) {
	try {
		PDFDoc doc(d.input);
		int pagecount = doc.GetPageCount();
		for (int i = 0; i < pagecount; ++i) {
			if (!d.pages.empty() && find(d.pages.begin(), d.pages.end(), i) == d.pages.end())continue;
			PDFPage page(doc, i);
			int rotate = ::FPDFPage_GetRotation(page.page) * 90;
			cout << "%%Page: " << (i + 1) << endl;
			cout << "%%Rotate: " << rotate << endl;
		}
	}
	catch (runtime_error e) { cout << e.what() << endl; }
}

void OutputTextAnnots(Data& d) {
	try {
		PDFDoc doc(d.input);
		int page_cnt = doc.GetPageCount();
		string outputpre, outputpost;
		GetOutputFileName(d.input, d.output, ".txt", outputpre, outputpost);
		int total_annot_number = 1;
		for (int i = 0; i < page_cnt; ++i) {
			PDFPage page(doc, i);
			auto annot_cnt = ::FPDFPage_GetAnnotCount(page.page);
			for (int j = 0; j < annot_cnt; ++j) {
				auto annot = ::FPDFPage_GetAnnot(page.page, j);
				if (annot == nullptr)continue;
				auto t = ::FPDFAnnot_GetSubtype(annot);
				if (t == FPDF_ANNOT_TEXT) {
					auto len = ::FPDFAnnot_GetStringValue(annot, "Contents", nullptr, 0);
					std::vector<char> buf(len);
					::FPDFAnnot_GetStringValue(annot, "Contents", &buf[0], len);
					auto str = UTF16ToUTF8(wstring((wchar_t*)&buf[0]));
					ofstream ofs(outputpre + to_string(total_annot_number) + outputpost,ios_base::out | ios_base::binary);
					ofs.write(str.c_str(), str.length());
					ofs.close();
					total_annot_number++;
				}
			}
		}
	}
	catch (runtime_error e) { cout << e.what() << endl; }
}

std::vector<std::string> split(const std::string &str, char sep) {
	std::vector<std::string> v;
	std::stringstream ss(str);
	std::string buffer;
	while (std::getline(ss, buffer, sep)) {
		v.push_back(buffer);
	}
	return v;
}

vector<int> AnalyzePageFormat(string &str) {
	auto pformats = split(str, ',');
	vector<int> rv;
	for (auto &&format : pformats) {
		auto r = format.find("-");
		if (r == string::npos) {
			auto x = std::stoi(format);
			rv.push_back(x);
		} else {
			int start = atoi(format.substr(0, r).c_str());
			int end = atoi(format.substr(r + 1).c_str());
			for (int i = start; i <= end; ++i) rv.push_back(i);
		}
	}
	return rv;
}

class Initializer {
public:
	Initializer() {
		::FPDF_InitLibrary();
		::CoInitialize(nullptr);
	}
	~Initializer() {
		::FPDF_DestroyLibrary();
		::CoUninitialize();
	}
};


int main(int argc, char *argv[]) {
	if (argc <= 1) {
		cout << "No input file" << endl;
		ShowUsage();
		return 0;
	}
	Initializer init;
	vector<Data> files;
	bool merge = false;
	{
		Data current_data;
		bool output_page = false;
		string box = "";
		bool output_rotate = false;
		bool output_text_annots = false;
		for (int i = 1; i < argc; ++i) {
			std::string arg = argv[i];
			if (arg == "--use-gdi")current_data.use_gdi = true;
			else if (arg == "--use-gdi-")current_data.use_gdi = false;
			else if (arg == "--transparent")current_data.transparent = true;
			else if (arg == "--transparent-")current_data.transparent = false;
			else if (arg == "--bmp")current_data.target = BMP;
			else if (arg == "--emf")current_data.target = EMF;
			else if (arg == "--png")current_data.target = PNG;
			else if (arg == "--jpg")current_data.target = JPG;
			else if (arg == "--gif")current_data.target = GIF;
			else if (arg == "--tiff")current_data.target = TIFF;
			else if (arg == "--pdf")current_data.target = PDF;
			else if (arg == "--wmf")current_data.target = WMF;
			else if (arg == "--input-format=png")current_data.input_format = PNG;
			else if (arg == "--input-format=bmp")current_data.input_format = BMP;
			else if (arg == "--input-format=jpg")current_data.input_format = JPG;
			else if (arg == "--input-format=gif")current_data.input_format = GIF;
			else if (arg == "--input-format=tiff")current_data.input_format = TIFF;
			else if (arg == "--input-format=pdf")current_data.input_format = PDF;
			else if (arg == "--output-page")output_page = true;// �y�[�W�����o�͂���
			else if (arg == "--output-rotate")output_rotate = true;
			else if (arg == "--output-text-annots")output_text_annots = true;
			else if (arg == "--merge")merge = true;
			else if (arg.find("--pages=") == 0) {
				try {
					auto pages = AnalyzePageFormat(arg.substr(string("--pages=").length()));
					for (auto p : pages)current_data.pages.push_back(p - 1);
				}
				catch (exception e) { cout << "failed to analyze page format: " << e.what() << endl; return -1; }
			} else if (arg.find("--scale=") == 0)current_data.scale = std::atoi(arg.substr(string("--scale=").length()).c_str());
			else if (arg.find("--extent=") == 0)current_data.extent = static_cast<float>(std::atoi(arg.substr(string("--extent=").length()).c_str()));
			else if (arg.find("--output=") == 0) current_data.output = arg.substr(string("--output=").length());
			else if (arg.find("--box=") == 0) box = arg.substr(string("--box=").length());
			else if (arg.find("--backcolor=") == 0) {
				try {
					int len = ::lstrlen("--backcolor=");
					int r = std::stoi(arg.substr(len, 2), nullptr, 16);
					int g = std::stoi(arg.substr(len + 2, 2), nullptr, 16);
					int b = std::stoi(arg.substr(len + 4, 2), nullptr, 16);
					current_data.backcolor = RGB(r, g, b);
				}
				catch (exception e) { cout << "failed to analyze color format: " << e.what() << endl; return -1; }
			} else if (arg.find("--viewport=") == 0) {
				auto viewport = split(arg.substr(string("--viewport=").length()), ',');
				if (viewport.size() == 4) {
					current_data.viewport.left = atoi(viewport[0].c_str());
					current_data.viewport.top = atoi(viewport[1].c_str());
					current_data.viewport.right = atoi(viewport[2].c_str());
					current_data.viewport.bottom = atoi(viewport[3].c_str());
				}
			} else if (arg == "--help") {
				ShowUsage();
				return 0;
			} else {
				if (output_page) {
					try {
						cout << to_string(PDFDoc(GetFullName(arg)).GetPageCount()) << endl;
					}
					catch (runtime_error e) {
						cout << e.what() << endl;
					}
					output_page = false;
				} else if (output_rotate) {
					cout << "output_rotate" << endl;
					Data d = current_data;
					d.input = GetFullName(arg);
					d.output = GetFullName(current_data.output);
					OutputRotate(d);
					output_rotate = false;
				} else if (output_text_annots) {
					Data d = current_data;
					d.input = GetFullName(arg);
					d.output = GetFullName(current_data.output);
					OutputTextAnnots(d);
					output_text_annots = false;
				} else {
					Data d = current_data;
					d.input = GetFullName(arg);
					d.output = GetFullName(current_data.output);
					if (box != "") {
						OutputBox(box, d);
						box = "";
					} else {
						files.push_back(d);
					}
					current_data.output = "";
					current_data.pages.clear();
					current_data.viewport = RECT{ 0, 0, 0, 0 };
					current_data.backcolor = RGB(255, 255, 255);
				}
			}
		}
	}
	int errpages = 0;
	try {
		if (merge) {
			vector<tuple<string, vector<int>>> fds;
			string out;
			for (auto &&d : files) {
				if (d.target != PDF || d.input_format != PDF)throw runtime_error("merge is only supported PDF -> PDF");
				if (d.output != "")out = d.output;
				fds.push_back(tuple<string, vector<int>>(d.input, d.pages));
			}
			if (out == "")throw runtime_error("Outut file name should be specified for --merge");
			return WritePDF(fds, out);
		} else {
			for (auto &&d : files) {
				if (d.input_format == PDF) {
					switch (d.target) {
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
					case PDF:
						errpages += WritePDF(d);
						break;
					case WMF:
						errpages += WriteWMF(d);
						break;
					default:
						break;
					}
				} else {
					ConvertIMG(d);
				}
			}
		}
	}
	catch (runtime_error e) {
		cout << e.what() << endl;
	}
	return errpages;
}
