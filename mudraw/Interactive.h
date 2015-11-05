#pragma once

#pragma warning(disable: 4611) // interaction between '_setjmp' and C++ object destruction is non-portable
extern "C" {
#include "mupdf/pdf.h"
}
#include <stdexcept>
#include <string>
#include <vector>
#include <algorithm>
#include <iostream>
#include <Windows.h>

namespace mudraw {
	using namespace std;
	string Convert(wstring str) {
		static ::std::vector<char> buf;
		DWORD size = ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), str.length(), NULL, 0, NULL, NULL);
		buf.resize(size + 1);
		size = ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), str.length(), &buf[0], buf.size(), NULL, NULL);
		buf[size] = '\0';
		return string(&buf[0]);
	}

	wstring Convert(const string &str) {
		static ::std::vector<wchar_t> buf;
		DWORD size = ::MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), NULL, 0);
		buf.resize(size + 1);
		size = ::MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), &buf[0], buf.size());
		buf[size] = L'\0';
		return wstring(&buf[0]);
	}

	fz_annot_type str_to_fz_annot_type(const string &type) {
		if (type == "Text")return FZ_ANNOT_TEXT;
		else if (type == "Link")return FZ_ANNOT_LINK;
		else if (type == "FreeText")return FZ_ANNOT_FREETEXT;
		else if (type == "Line")return FZ_ANNOT_LINE;
		else if (type == "Square")return FZ_ANNOT_SQUARE;
		else if (type == "Circle")return FZ_ANNOT_CIRCLE;
		else if (type == "Polygon")return FZ_ANNOT_POLYGON;
		else if (type == "PolyLine")return FZ_ANNOT_POLYLINE;
		else if (type == "Highlight")return FZ_ANNOT_HIGHLIGHT;
		else if (type == "Underline")return FZ_ANNOT_UNDERLINE;
		else if (type == "Squiggly")return FZ_ANNOT_SQUIGGLY;
		else if (type == "StrikeOut")return FZ_ANNOT_STRIKEOUT;
		else if (type == "Stamp")return FZ_ANNOT_STAMP;
		else if (type == "Caret")return FZ_ANNOT_CARET;
		else if (type == "Ink")return FZ_ANNOT_INK;
		else if (type == "Popup")return FZ_ANNOT_POPUP;
		else if (type == "FileAttachment")return FZ_ANNOT_FILEATTACHMENT;
		else if (type == "Sound")return FZ_ANNOT_SOUND;
		else if (type == "Movie")return FZ_ANNOT_MOVIE;
		else if (type == "Widget")return FZ_ANNOT_WIDGET;
		else if (type == "Screen")return FZ_ANNOT_SCREEN;
		else if (type == "PrinterMark")return FZ_ANNOT_PRINTERMARK;
		else if (type == "TrapNet")return FZ_ANNOT_TRAPNET;
		else if (type == "Watermark")return FZ_ANNOT_WATERMARK;
		else if (type == "3D")return FZ_ANNOT_3D;
		else return FZ_ANNOT_TEXT;
	}

	string fz_annot_type_to_str(fz_annot_type type) {
		switch (type) {
		case FZ_ANNOT_TEXT: return "Text";
		case FZ_ANNOT_LINK: return "Link";
		case FZ_ANNOT_FREETEXT: return "FreeText";
		case FZ_ANNOT_LINE: return "Line";
		case FZ_ANNOT_SQUARE: return "Square";
		case FZ_ANNOT_CIRCLE: return "Circle";
		case FZ_ANNOT_POLYGON: return "Polygon";
		case FZ_ANNOT_POLYLINE: return "PolyLine";
		case FZ_ANNOT_HIGHLIGHT: return "Highlight";
		case FZ_ANNOT_UNDERLINE: return "Underline";
		case FZ_ANNOT_SQUIGGLY: return "Squiggly";
		case FZ_ANNOT_STRIKEOUT: return "StrikeOut";
		case FZ_ANNOT_STAMP: return "Stamp";
		case FZ_ANNOT_CARET: return "Caret";
		case FZ_ANNOT_INK: return "Ink";
		case FZ_ANNOT_POPUP: return "Popup";
		case FZ_ANNOT_FILEATTACHMENT: return "FileAttachment";
		case FZ_ANNOT_SOUND: return "Sound";
		case FZ_ANNOT_MOVIE: return "Movie";
		case FZ_ANNOT_WIDGET: return "Widget";
		case FZ_ANNOT_SCREEN: return "Screen";
		case FZ_ANNOT_PRINTERMARK: return "PrinterMark";
		case FZ_ANNOT_TRAPNET: return "TrapNet";
		case FZ_ANNOT_WATERMARK: return "Watermark";
		case FZ_ANNOT_3D: return "3D";
		default: return "";
		}
	}

	class Interactive {
	private:
		fz_context *context = nullptr;
		::std::vector<pdf_document*> documents;
		struct Page {
		public:
			Page(pdf_page *p, pdf_document *doc) { page = p; document = doc; }
			pdf_page *page;
			pdf_document *document;
		};
		::std::vector<Page> pages;
		struct Annot {
			Annot(pdf_annot *a, pdf_document *doc) { annot = a; document = doc; }
			pdf_annot *annot;
			pdf_document *document;
		};
		::std::vector<Annot> annots;
		template<typename T> T Read();
		void Write(string str) {
			Write((int)str.length());
			::std::cout << str << ::std::endl;
		}
		void Write(int num) {
			::std::cout << std::to_string(num) << ::std::endl;;
		}
		void Write(float num){
			::std::cout << std::to_string(num) << ::std::endl;
		}
		void Write(fz_rect rect) {
			Write(rect.x0);
			Write(rect.y0);
			Write(rect.x1);
			Write(rect.y1);
		}
		void skip_line();

	public:
		int Main();
		Interactive() {
			context = fz_new_context(NULL, NULL, FZ_STORE_DEFAULT);
			if (context == nullptr)throw runtime_error("failed to get MuPDF context");
		}
		vector<string> args;
		void SetArgs(int argc, char** argv){
			for(int i = 0; i < argc; ++i)args.push_back(argv[i]);
		}
		~Interactive() {
			free_all();
			context->error = 0;
			::fz_free_context(context);
		}

		void free_all() {
			//for (auto a : annots)::pdf_free_annot(context, a.annot);
			annots.clear();
			for (auto p : pages)::pdf_free_page(p.document, p.page);
			pages.clear();
			for (auto d : documents)::pdf_close_document(d);
			documents.clear();
		}

		int open_document(const string &file) {
			auto d = ::pdf_open_document(context, file.c_str());
			documents.push_back(d);
			return documents.size();
		}
		int load_page(int document, int page) {
			auto docptr = documents[document - 1];
			auto p = ::pdf_load_page(docptr, page);
			pages.push_back(Page(p, docptr));
			return pages.size();
		}
		int count_pages(int document) {
			return ::pdf_count_pages(documents[document - 1]);
		}
		fz_rect bound_page(int page) {
			fz_rect rect;
			auto p = pages[page - 1];
			::pdf_bound_page(p.document, p.page, &rect);
			return rect;
		}
		int first_annot(int page) {
			auto p = pages[page - 1];
			auto a = ::pdf_first_annot(p.document, p.page);
			if (a == nullptr)return 0;
			annots.push_back(Annot(a, p.document));
			return annots.size();
		}
		int next_annot(int annot) {
			auto olda = annots[annot - 1];
			auto a = ::pdf_next_annot(olda.document, olda.annot);
			if (a == nullptr)return 0;
			annots.push_back(Annot(a, olda.document));
			return annots.size();
		}
		fz_annot_type annot_type(int annot) {
			return ::pdf_annot_type(annots[annot - 1].annot);
		}
		string annot_contents(int annot) {
			auto obj = ::pdf_dict_gets(annots[annot - 1].annot->obj, "Contents");
			int len = ::pdf_to_str_len(obj);
			std::vector<wchar_t> buf;
			buf.resize(len + 1);
			::pdf_to_ucs2_buf(reinterpret_cast<unsigned short*>(&buf[0]), obj);
			return Convert(std::wstring(&buf[0]));
		}
		fz_rect bound_annot(int annot) {
			fz_rect rect;
			auto a = annots[annot - 1];
			::pdf_bound_annot(a.document, a.annot, &rect);
			return rect;
		}
		int create_annot(int page, fz_annot_type type) {
			auto p = pages[page - 1];
			auto a = ::pdf_create_annot(p.document, p.page, type);
			annots.push_back(Annot(a, p.document));
			return annots.size();
		}
		void set_text_annot_position(int annot, int x, int y) {
			fz_point pt;
			pt.x = x; pt.y = y;
			auto a = annots[annot - 1];
			::pdf_set_text_annot_position(a.document, a.annot, pt);
		}
		void set_annot_contents(int annot, const string &str) {
			auto a = annots[annot - 1];
			auto s = Convert(str);
			std::vector<char> buf;
			buf.resize(s.length() * 2 + 3);
			buf[0] = static_cast<char>(0xFE); buf[1] = static_cast<char>(0xFF);
			buf[2 * s.length() + 2] = '\0';
			for (wstring::size_type i = 0; i < s.length(); ++i) {
				buf[2 * i + 3] = s[i] & 0x00FF;
				buf[2 * i + 2] = (s[i] & 0xFF00) >> 8;
			}
			pdf_dict_puts_drop(a.annot->obj, "Contents", pdf_new_string(a.document, &buf[0], 2 * s.length() + 2));
			return;
		}
		void set_annot_flag(int annot, int flag) {
			auto a = annots[annot - 1];
			pdf_dict_puts_drop(a.annot->obj, "F", pdf_new_int(a.document, flag));
		}

		void write_document(int doc, const string &file) {
			fz_write_options opt;
			::ZeroMemory(&opt, sizeof(fz_write_options));
			::pdf_write_document(documents[doc - 1], const_cast<char*>(file.c_str()),&opt);
		}
		void insert_page(int doc, int page, int at) {
			::pdf_insert_page(documents[doc - 1], pages[page - 1].page, at);
		}
		void delete_page(int doc, int number) {
			::pdf_delete_page(documents[doc - 1], number);
		}
		void delete_page_range(int doc, int start, int end) {
			::pdf_delete_page_range(documents[doc - 1], start, end);
		}
		int rotate_page(int page){
			auto p = pages[page - 1];
			return p.page->rotate;
		}
		fz_rect pdfbox_page(int page, const string &boxname){
			auto p = pages[page - 1];
			if(boxname == "media")return GetMediaBox(p);
			else if(boxname == "crop")return GetCropBox(p);
			else if(boxname == "bleed")return GetOtherBox(p, "BleedBox");
			else if(boxname == "trim")return GetOtherBox(p, "TrimBox");
			else if(boxname == "art")return GetOtherBox(p, "ArtBox");
			else fz_throw(context, 1, (string("unknown boxname: ") + boxname).c_str());
		}
		fz_rect GetMediaBox(Page &p){
			fz_rect rect;
			::pdf_to_rect(context, pdf_lookup_inherited_page_item(p.document, p.page->me, "MediaBox"), &rect);
			return rect;
		}
		fz_rect GetCropBox(Page &p){
			auto media = GetMediaBox(p);
			fz_rect rect;
			::pdf_to_rect(context, pdf_lookup_inherited_page_item(p.document, p.page->me, "CropBox"), &rect);
			if(fz_is_empty_rect(&rect))return media;
			else{
				::fz_intersect_rect(&rect, &media);
				return rect;
			}
		}
		fz_rect GetOtherBox(Page &p, const char *boxname){
			fz_rect rect;
			::pdf_to_rect(context, pdf_lookup_inherited_page_item(p.document, p.page->me, boxname), &rect);
			if(fz_is_empty_rect(&rect))return GetCropBox(p);
			else{
				auto media = GetMediaBox(p);
				::fz_intersect_rect(&rect, &media);
				return rect;
			}
		}
	};
}// namespace mudraw
