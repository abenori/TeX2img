#include "Interactive.h"
#include <map>
#undef max

extern "C" int Interactive() {
	::_setmode(::_fileno(stdout), _O_BINARY);
	::_setmode(::_fileno(stdin), _O_BINARY);
	::_setmode(::_fileno(stderr), _O_BINARY);
	mudraw::Interactive interactive;
	return interactive.Main();
}

namespace mudraw {
	using namespace std;
	template<> int Interactive::Read<int>() {
		int num;
		std::cin >> num;
		if (std::cin.fail())throw std::runtime_error("invalid type: number expected");
		skip_line();
		return num;
	}

	template<> std::string Interactive::Read<std::string>() {
		int len = Read<int>();
		std::vector<char> buf;
		buf.resize(len + 1);
		std::cin.read(&buf[0], len);
		std::string rv(&buf[0], &buf[0] + len);
		skip_line();
		return rv;
	}

	void Interactive::skip_line() {
		while (true) {
			if (cin.eof())return;
			auto b = cin.get();
			if (b == '\r' || b == '\n') {
				if (b == '\r') {
					b = cin.peek();
					if (b == '\n')cin.get();
				}
				return;
			}
		}
	}

	int Interactive::Main() {
		try {
			while (true) {
				std::string func;
				std::cin >> func;
				if (func == "quit")return 0;
				else if (func == "open_document") Write(open_document(Read<string>()));
				else if (func == "load_page") {
					int doc = Read<int>();
					int page = Read<int>();
					Write(load_page(doc, page));
				}
				else if (func == "annot_type") Write(fz_annot_type_to_str(annot_type(Read<int>())));
				else if (func == "first_annot") Write(first_annot(Read<int>()));
				else if (func == "next_annot") Write(next_annot(Read<int>()));
				else if (func == "annot_contents") Write(annot_contents(Read<int>()));
				else if (func == "bound_annot") {
					auto rect = bound_annot(Read<int>());
					Write(rect.x0);
					Write(rect.y0);
					Write(rect.x1);
					Write(rect.y1);
				}
				else if (func == "create_annot") {
					int page = Read<int>();
					auto type = Read<string>();
					Write(create_annot(page, str_to_fz_annot_type(type)));
				}
				else if (func == "set_text_annot_position") {
					int annot = Read<int>();
					int x = Read<int>();
					int y = Read<int>();
					set_text_annot_position(annot, x, y);
					Write("");
				}
				else if (func == "set_annot_contents") {
					int annot = Read<int>();
					auto contents = Read<string>();
					set_annot_contents(annot, contents);
					Write("");
				}
				else if (func == "set_annot_flag") {
					int annot = Read<int>();
					int flag = Read<int>();
					set_annot_flag(annot, flag);
					Write("");
				}
				else if (func == "write_document") {
					auto doc = Read<int>();
					auto file = Read<string>();
					write_document(doc, file);
					Write("");
				}
				else if (func == "free_all")free_all();
				else ::std::cout << "function \"" + func + "\" does not exist" << ::std::endl;
			}
		}
		catch (std::runtime_error e) {
			std::cerr << e.what() << std::endl;
			return 1;
		}
	}
}