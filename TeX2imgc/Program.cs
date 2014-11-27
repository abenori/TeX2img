using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TeX2imgc {
    class Program {
        static void Main(string[] args) {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string tex2img = Path.Combine(dir,"tex2img.exe");
            if(!File.Exists(tex2img)) {
                Console.WriteLine("TeX2img.exe が見つかりませんでした．");
                Environment.Exit(-1);
            }
            using(Process proc = new Process()) {
                proc.StartInfo.FileName = tex2img;
                // TeX2imgの仕様から，/noguiは第一引数でなければならない．
                proc.StartInfo.Arguments = "/nogui ";
                // Environmet.CommandLine からTeX2imgc.exe... の部分を除去する．
                // Environment.GetCommandLineArgsを使うと"が完全に再現できないと思うので．
                var reg = new System.Text.RegularExpressions.Regex("^[^\" ]*(\"[^\"]*\")*[^\" ]* *");
                var m = reg.Match(Environment.CommandLine);
                if(m.Success) proc.StartInfo.Arguments += Environment.CommandLine.Substring(m.Length);
                else proc.StartInfo.Arguments += Environment.CommandLine;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.OutputDataReceived += ((s, e) => Console.WriteLine(e.Data));
                if(!proc.Start()) { 
                    Console.WriteLine("TeX2img.exe の実行に失敗しました．");
	                Environment.ExitCode = -1;
                    return;
                }
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                Environment.ExitCode = proc.ExitCode;
            }
            //Console.ReadKey();
        }
    }
}
