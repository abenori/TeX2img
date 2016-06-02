/*
コンパイル：SumatraPDFのソースを使っている．

mudrawにパッチを当てる．

SumatraPDFソースツリー内，pdf-write.cのwritexref内の
fprintf(opts->out, "\n");
を消す．

Interactive.cppとInteractive.hを入れる．この二つのファイルの言語をC++にして，例外を有効にする．

適当に警告をエラー扱いするのをやめる．
*/

#include "Interactive.h"
#include <map>
#undef max

extern "C" int Interactive(int argc, char **argv) {
	::_setmode(::_fileno(stdout), _O_BINARY);
	::_setmode(::_fileno(stdin), _O_BINARY);
	::_setmode(::_fileno(stderr), _O_BINARY);
	try {
		mudraw::Interactive interactive(argc, argv);
		return interactive.Main();
	}
	catch (std::runtime_error) {
		std::cerr << "fatal error: failed to get MuPDF context" << std::endl;
		return 1;
	}
}

namespace mudraw {
	using namespace std;
	template<> int Interactive::Read<int>() {
		int num;
		std::cin >> num;
		if(std::cin.fail()) {
			cin.clear();
			skip_line();
			fz_throw(context, 1, "invalid type: number expected");
		}
		skip_line();
		return num;
	}

	template<> std::string Interactive::Read<std::string>() {
		string fline;
		std::cin >> fline;
		skip_line();
		if(fline[0] == '%')return args[std::atoi(fline.substr(1).c_str())];
		int len = std::atoi(fline.c_str());
		std::vector<char> buf;
		buf.resize(len + 1);
		std::cin.read(&buf[0], len);
		std::string rv(&buf[0], &buf[0] + len);
		skip_line();
		return rv;
	}

	void Interactive::skip_line() {
		while(true) {
			if(cin.eof())return;
			auto b = cin.get();
			if(b == '\r' || b == '\n') {
				if(b == '\r') {
					b = cin.peek();
					if(b == '\n')cin.get();
				}
				return;
			}
		}
	}

	int Interactive::Main() {
		while(true) {
			fz_try(context) {
				std::string func;
				std::cin >> func;
				if(func == "quit")return 0;
				else if(func == "open_document") Write(open_document(Read<string>()));
				else if(func == "load_page") {
					int doc = Read<int>();
					int page = Read<int>();
					Write(load_page(doc, page));
				} else if(func == "count_pages")Write(count_pages(Read<int>()));
				else if(func == "bound_page")Write(bound_page(Read<int>()));
				else if(func == "annot_type") Write(fz_annot_type_to_str(annot_type(Read<int>())));
				else if(func == "first_annot") Write(first_annot(Read<int>()));
				else if(func == "next_annot") Write(next_annot(Read<int>()));
				else if(func == "annot_contents") Write(annot_contents(Read<int>()));
				else if(func == "bound_annot") Write(bound_annot(Read<int>()));
				else if(func == "create_annot") {
					int page = Read<int>();
					auto type = Read<string>();
					Write(create_annot(page, str_to_fz_annot_type(type)));
				} else if(func == "set_text_annot_position") {
					int annot = Read<int>();
					int x = Read<int>();
					int y = Read<int>();
					set_text_annot_position(annot, x, y);
					cout << endl;
				} else if(func == "set_annot_contents") {
					int annot = Read<int>();
					auto contents = Read<string>();
					set_annot_contents(annot, contents);
					cout << endl;
				} else if(func == "set_annot_flag") {
					int annot = Read<int>();
					int flag = Read<int>();
					set_annot_flag(annot, flag);
					cout << endl;
				} else if(func == "create_document"){
                    Write(create_document());
				} else if(func == "write_document") {
					auto doc = Read<int>();
					auto file = Read<string>();
					write_document(doc, file);
					cout << endl;
				} else if(func == "insert_page") {
					int doc = Read<int>();
					int page = Read<int>();
					int at = Read<int>();
					insert_page(doc, page, at);
					cout << endl;
				} else if(func == "delete_page") {
					int doc = Read<int>();
					int number = Read<int>();
					delete_page(doc, number);
					cout << endl;
				} else if(func == "delete_page_range") {
					int doc = Read<int>();
					int start = Read<int>();
					int end = Read<int>();
					delete_page_range(doc, start, end);
					cout << endl;
				} else if(func == "free_all"){
					free_all();
					cout << endl;
				} else if(func == "pdfbox_page"){
					int page = Read<int>();
					string boxname = Read<string>();
					Write(pdfbox_page(page, boxname));
				} else if(func == "rotate_page")Write(rotate_page(Read<int>()));
				else fz_throw(context, 1, ("function " + func + " is not defined").c_str());
			}
			fz_catch(context) {
				cerr << context->error->message << endl;
			}
		}
	}
}