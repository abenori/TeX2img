using System;
using System.Collections.Generic;
using System.Text;

namespace TeX2img {
    class CUIOutput : IOutputController {
        bool quiet = false;
        public CUIOutput(bool q) { quiet = q; }
        public CUIOutput() { }

        public void showPathError(string exeName, string necessary) {
            Console.WriteLine(exeName + " を起動することができませんでした。\n" + necessary + "がインストールされているか，\n" + exeName + " のパスの設定が正しいかどうか，\n確認してください。");
        }

        public void showExtensionError(string file) {
            Console.WriteLine(file + ": ファイルの拡張子が不正です。");
        }

        public void appendOutput(string log) {
            if(!quiet) Console.Write(log);
        }

        public void showGenerateError() {
            Console.WriteLine("画像生成に失敗しました。\nソースコードにエラーがないか確認してください。");
        }

        public void showImageMagickError() {
            Console.WriteLine("ImageMagick がインストールされていないため，画像変換ができませんでした。\nImageMagick を別途インストールしてください。\nインストールされている場合は，パスが通っているかどうか確認してください。");
        }

        public void scrollOutputTextBoxToEnd() {
        }

        public void showUnauthorizedError(string filePath) {
            Console.WriteLine(filePath + "\nに上書き保存できませんでした。\n一度このファイルを削除してから再試行してください。");
        }

        public void showIOError(string filePath) {
            Console.WriteLine(filePath + "\nが他のアプリケーション開かれているため生成できませんでした。\nこのファイルを開いているアプリケーションを閉じて再試行してください。");
        }

        public bool askYesorNo(string msg) {
            Console.WriteLine(msg + "（y/n）");
            while(true) {
                var s = Console.ReadLine().ToLower();
                switch(s) {
                case "y": return true;
                case "n": return false;
                default:
                    Console.WriteLine("y または n を入力してください。");
                    break;
                }
            }
        }

        public void showToolError(string tool) {
            var path = System.IO.Path.Combine(Converter.ShortToolPath, tool);
            Console.WriteLine(path + @" を起動することができませんでした。" + "\n付属の " + path + " フォルダを消さないでください。");
        }

        public void errorIgnoredWarning() {
            Console.WriteLine("コンパイルエラーを無視して画像化を強行しました。\n結果は期待と異なる可能性があります。\nソースを修正しエラーを解決することを推奨します。");
        }
    }
}
