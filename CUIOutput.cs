using System;
using System.Collections.Generic;
using System.Text;

namespace TeX2img {
    class CUIOutput : IOutputController {
        public void showPathError(string exeName, string necessary) {
            Console.WriteLine(exeName + " を起動することができませんでした。\n" + necessary + "がインストールされているか，\n" + exeName + " のパスの設定が正しいかどうか，\n確認してください。");
        }

        public void showExtensionError(string file) {
            Console.WriteLine(file + ": 出力ファイルの拡張子は eps/png/jpg/pdf のいずれかにしてください。");
        }

        public void appendOutput(string log) {
            Console.Write(log);
        }

        public void showGenerateError() {
            Console.WriteLine("画像生成に失敗しました。\nソースコードにエラーがないか確認してください。");
        }

        public void showPstoeditError() {
            Console.WriteLine(@".\pstoedit\pstoedit.exe を起動することができませんでした。" + "\n付属の pstoedit フォルダを消さないでください。");
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

    }
}
