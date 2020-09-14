# TeX2img

(C) Yusuke Terada and Noriyuki Abe http://www.ms.u-tokyo.ac.jp/~abenori/

（日本語のドキュメントは [TeX2img.txt](./TeX2img.txt) をご覧ください．）

## What is TeX2img?
TeX2img generates an image in various file formats from a TeX source code.
You can choose the following file formats:

* EPS (outlined font)
* PDF (outlined font or keep the text)
* SVG(Z) (outlined font + keep the text)
* JPEG
* TIFF
* PNG
* BMP
* EMF

TeX2img was originally developed by Yusuke Terada (until Version 1.2).
Recently, Windows version (here) and [macOS version](https://github.com/doraTeX/TeX2img) has been developed independently.

## Requirements
* Windows Vista or later.
* .NET Framework 4.5.2 or later.
* TeX distribution (W32TeX or TeX Live is recommended), especially pdftex.
* Ghostscript (Version 9 or later is recommended). 

## How to install
Download a zip file, like `TeX2img_x.y.z.zip`, from http://www.ms.u-tokyo.ac.jp/~abenori/soft/index.html#TEX2IMG

Just extract the zip file and double click "TeX2img.exe" in the extracted directory. At the first time, TeX2img will try to get paths of pdflatex.exe, etc. If the paths are not correct, please set them from [Tools(T)] -> [Options...(O)].

TeX2imgc.exe is a wrapper program to execute TeX2img in a command-line.

	> TeX2imgc.exe [Options] Input Output [Input Output...]

All properties are inherited from those of TeX2img except preview after compiling (always false).

## Tips
* You can also specify the options for each program, for example, `"C:\w32tex\bin\platex.exe" --guess-input-enc`

* Since the internal character code is Unicode, you can use the characters outside of Shift_JIS encoding through "uplatex", "lualatex", etc. To achieve it, please choose "UTF-8" or "no (input UTF-8)" for the character encoding.

* If TeX2img guesses that the LaTeX program generates PDF file directly, TeX2img skips DVI driver.
* You can specify not only "dvipdfmx" but also "dvips" for `/dvidriver` because TeX2img converts PostScript to PDF via Ghostscript, if TeX2img guesses that the DVI driver generates a PostScript file.

* If you drag and drop the generated file to TeX2img (or [File(F)] -> [Import(O)]), the source file is restored.

## Options
The following are the options for TeX2img.exe or TeX2imgc.exe.

	/latex=<VAL>             Set path for latex
	/dvidriver=<VAL>         Set path for dvi driver
	/gs=<VAL>                Set path for Ghostscript
	/oldgs[-]                Ghostscript is before version 9.14
	/kanji=<VAL>             Character code (utf8/sjis/jis/euc/no)
	/guess-compile[-]        Guess compile
	/num=<NUM>               (maximum) number of times for LaTeX
	/resolution=<NUM>        Resolution level
	/left-margin=<NUM>       Left margin
	/top-margin=<NUM>        Top margin
	/right-margin=<NUM>      Right margin
	/bottom-margin=<NUM>     Bottom margin
	/margins=<VAL>           Margins (all / leftright topbottom / left top right bottom)
	/unit=<VAL>              Unit of margins ( bp/px )
	/keep-page-size[-]       Keep the original page size
	/merge-output-files[-]   Make a single file (PDF / TIFF /SVG(Z))
	/animation-delay=<VAL>   Delay of animation (sec)
	/animation-loop=<VAL>    Loop count of animation (0 =infinity)
	/background-color=<VAL>  Background color (ex: FF0000, red, "255 0 0")
	/transparent[-]          Transparent
	/with-text[-]            Keep original text information (PDF / SVG(Z))
	/delete-display-size[-]  Remove <width> and <height> (SVG(Z))
	/antialias[-]            Anti-aliasing
	/low-resolution[-]       Generate images with low resolution
	/ignore-errors[-]        Force conversion by ignoring nonfatal errors
	/delete-tmpfiles[-]      Delete temporary files after compiling
	/preview[-]              Preview after compiling
	/embed-source[-]         Embed the source in output image files
	/copy-to-clipboard[-]    Copy generated files to the clipboard
	/workingdir=<VAL>        Working folder (tmp/file/current)
	/savesettings[-]         Save settings
	/quiet[-]                Quiet mode
	/timeout=<NUM>           Time out period (seconds)
	/batch=<VAL>             Batch mode (stop/nonstop)
	/exit                    Save settings and exit
	/load-defaults           Load default settings
	/help                    Show this message
	/version                 Show version information
	/language=<VAL>          Language (system/ja/en)

[-] means negation. For example, `/guess-compile-` means that TeX2img does not guess the compiling.

If you specify several options, TeX2img takes the last one. For example,

	tex2imgc /transparent- /transparent a.tex a.png

will generate transparent png file. In particular, because `/load-defaults` makes all settings default ones, the options before /load-defaults are ignored. The option `/load-defaults` should be placed at the first.

## Reference
* Official webpage of TeX2img: https://tex2img.tech/

## Known bugs
Conversions to EMF file has some problems. See Issues in GitHub: [https://github.com/abenori/TeX2img/issues/3](https://github.com/abenori/TeX2img/issues/3).

## Acknowledgments
* Originally, TeX2img was developed by Yusuke Terada.
* Azuki text editor engine is used.
http://sgry.b.sourceforge.jp/
* To parse the command line, NDesk.Options is used.
http://www.ndesk.org/Options
* To guess Japanese character code, TeX2img uses a (C# version of) routine in Gauch.
http://practical-scheme.net/gauche/index.html
* TeX2img uses mudraw (with modifications).
http://www.mupdf.com/
* pdfiumdraw uses PDFium.
https://pdfium.googlesource.com/pdfium/

## License
For TeX2img itself, see [license.txt](./license.txt). For the libraries:

* Azuki text editor engine: zlib license.
* NDesk.Options: MIT license.
* Gauche: modified BSD license.
* mudraw: GPL v3 license.
* PDFium: modified BSD license.
* pdfiumdraw: modified BSD license.

For further details, see the website of each library.

## History (after version 2.0)
* 2.0.0 (2016/07/16)
    - Add English resources.
    - Add option /language
    - Options /platex and /dvipdfmx are now hidden.

* 2.0.1 (2016/10/06)
    - Fix typos in Japanese resources.
    - Update PDFium

* 2.0.2 (2016/12/19)
    - Fix bug: the conversion between strings and numbers are not correct with a certain system language.

* 2.0.3 (2018/01/22)
    - Require .NET Framework >= 4.5.2.
    - Update PDFium

* 2.1.0 (2018/05/16)
    - Use {strokepath fill} for preprocessing for EMF file. (The old method can't apply for new Ghostscript.)
    - Changed icons.

* 2.1.1 (2020/05/01)
    - Fixed a bug: the size of the image was incorrect if white objects are drawn.
    - Added a drop-down list to specify the extension of the generated file.

* 2.1.2 (2020/08/11)
    - Supported high DPI.
    - TeX2img requires .NET Framework version >= 4.7.2.

* 2.1.3 (2020/09/14)
    - Added the path setting of pdfTeX.

