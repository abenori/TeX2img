using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TeX2imgc {
    class Program {
        static void Main(string[] args) {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string tex2img = Path.Combine(dir, "tex2img.exe");

#if DEBUG
            Console.WriteLine("TeX2imgc.exe，ビルド時刻：" + (new System.IO.FileInfo(Path.Combine(dir, "TeX2imgc.exe"))).CreationTime);
            Console.WriteLine("TeX2img.exe，ビルド時刻：" + (new System.IO.FileInfo(tex2img).CreationTime));
#endif

            if(!File.Exists(tex2img)) {
                Console.WriteLine("TeX2img.exe が見つかりませんでした．");
                Environment.Exit(-1);
            }

            using(Process proc = new Process()) {
                proc.StartInfo.FileName = tex2img;
                // "/nogui"を第一引数にする．
                proc.StartInfo.Arguments = "/nogui ";
                // Environmet.CommandLine からTeX2imgc.exe... の部分を除去する．
                // Environment.GetCommandLineArgsを使うと"が完全に再現できないと思うので．
                var reg = new System.Text.RegularExpressions.Regex("^[^\" ]*(\"[^\"]*\")*[^\" ]* *");
                var m = reg.Match(Environment.CommandLine);
                if(m.Success) proc.StartInfo.Arguments += Environment.CommandLine.Substring(m.Length);
                else proc.StartInfo.Arguments += Environment.CommandLine;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.OutputDataReceived += ((s, e) => Console.WriteLine(e.Data));
                if(!proc.Start()) {
                    Console.WriteLine("TeX2img.exe の実行に失敗しました．");
                    Environment.ExitCode = -1;
                    return;
                }
                int id = proc.Id;
                Console.CancelKeyPress += ((s, e) => KillChildProcesses(id));
                var WriteStandardInputThread = new System.Threading.Thread((o) => {
                    StreamWriter sw = (StreamWriter) o;
                    while(true){
                        try { sw.WriteLine(Console.ReadLine()); }
                        catch { return; }
                    }
                });
                // これを加えるとConsole.ReadLineの入力待ちでおわらないということはないことに気がついた……
                WriteStandardInputThread.IsBackground = true;
                WriteStandardInputThread.Start(proc.StandardInput);
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                Environment.ExitCode = proc.ExitCode;
                WriteStandardInputThread.Abort();
            }
        }

        static void KillChildProcesses(int id) {
            using(var proc = new Process()) {
                try {
                    proc.StartInfo.FileName = "taskkill.exe";
                    proc.StartInfo.Arguments = "/PID " + id.ToString() + " /T /F";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit(3000);
                    if(!proc.HasExited) proc.Kill();
                }
                catch(System.ComponentModel.Win32Exception) { }
            }
        }
    }
}
