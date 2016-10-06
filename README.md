# TeX2img

(C) Yusuke Terada and Noriyuki Abe http://www.math.sci.hokudai.ac.jp/~abenori/

（日本語のドキュメントは [TeX2img.txt](./TeX2img.txt) をご覧ください．）

## What is TeX2img?
This generates the images with the following formats from the TeX source code:

* EPS (outlined font)
* PDF (outlined font or keep the text)
* SVG(Z) (outlined font + keep the text)
* JPEG
* TIFF
* PNG
* BMP
* EMF

This was originally made by Yusuke Terada (until Version 1.2).

## Requirements
* Windows Vista or later.
* .NET Framework 3.5 or later.
* TeX distribution (W32TeX or TeX Live is recommended), especially pdftex.
* Ghostscript (Version 9 or later is recommended). 

## How to install
Download from http://www.math.sci.hokudai.ac.jp/~abenori/soft/index.html#TEX2IMG

Just extract and double click "TeX2img.exe". At the first time, TeX2img try to get paths of pdflatex.exe etc. If it is not correct, please set them from [Tools(T)] -> [Options...(O)]

TeX2imgc.exe is a wrapper program to execute TeX2img with the console.

	> TeX2imgc.exe [Options] Input Output [Input Output...]

Each property are inherited from those of TeX2img except preview after compiling (always false).


## Tips
* You can also specify the options to each programs.
Example："C:\w32tex\bin\platex.exe" --guess-input-enc

* Since the internal character code is Unicode, you can also use characters which is outside of Shift_JIS with "uplatex" etc. For that, please choose "UTF-8" or "no (input UTF-8)" for the character code.

* If TeX2img think that the LaTeX program generated pdf file, TeX2img skips DVI driver. If TeX2img thinks that the DVI driver generates ps, then TeX2img converts the ps file to pdf file via Ghostscript. Consequently, you can specify "dvips"

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

[-] means negation. For example. /guess-compile- means that TeX2img does not guess the compiling.

If you specify several options, TeX2img takes the last one. For example
	tex2imgc /transparent- /transparent a.tex a.png
will generate transparent png file. In particular, because /load-defaults makes all settings default ones, the options before /load-defaults are ignored. The option /load-defaults should be the first.

## Reference
* Webpage of Yusuke Terada (original developer of TeX2img)
http://island.geocities.jp/loveinequality/


## Acknowledgments
* Originally it is made by Yusuke Terada.
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


## license
See [license.txt](./license.txt). For the libraries:

* Azuki text editor engine: zlib license.
* NDesk.Options: MIT license.
* Gauche: modified BSD license.
* mudraw: GPL v3 license.
* PDFium: modified BSD license.
* pdfiumdraw: modified BSD license.

For the detail, see the site of each library.


## History (after 2.0)
* 2.0.0
    - Add English resources.
    - Add option /language
    - Options /platex and /dvipdfmx are now hidden.

* 2.0.1
    - Fix typos in Japanese resources.
    - Update PDFium
