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
            Console.WriteLine("TeX2imgc.exe，ビルド時刻：" + GetBuildDateTime(Path.Combine(dir, "TeX2imgc.exe")));
            Console.WriteLine("TeX2img.exe，ビルド時刻：" + GetBuildDateTime(tex2img));
            Console.WriteLine("pdfiumdraw.exe，ビルド時刻：" + GetBuildDateTime(Path.Combine(dir,"pdfiumdraw.exe")));
            Console.WriteLine("mudraw.exe，ビルド時刻：" + GetBuildDateTime(Path.Combine(dir,"mudraw.exe")));
#endif

            if (!File.Exists(tex2img)) {
                Console.WriteLine("TeX2img.exe が見つかりませんでした．");
                Environment.Exit(-1);
            }

            using(Process proc = new Process()) {
                proc.StartInfo.FileName = tex2img;
                // "/nogui"を第一引数にする．
                proc.StartInfo.Arguments = "/nogui ";
                // Environmet.CommandLine からTeX2imgc.exe... の部分を除去する．
                // Environment.GetCommandLineArgsを使うと"が完全に再現できないと思うので．
                var reg = new System.Text.RegularExpressions.Regex("^[^\" ]*(\"[^\"]*\")*[^\" ]* +");
                var m = reg.Match(Environment.CommandLine);
                if(m.Success) proc.StartInfo.Arguments += Environment.CommandLine.Substring(m.Length);
                //else proc.StartInfo.Arguments += Environment.CommandLine;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.OutputDataReceived += ((s, e) => Console.WriteLine(e.Data));
                proc.ErrorDataReceived += ((s, e) => Console.Error.WriteLine(e.Data));
                if(!proc.Start()) {
                    Console.WriteLine("TeX2img.exe の実行に失敗しました．");
                    Environment.ExitCode = -1;
                    return;
                }
                int id = proc.Id;
                Console.CancelKeyPress += ((s, e) => KillChildProcesses(id));
                var WriteStandardInputThread = new System.Threading.Thread((o) => {
                    StreamWriter sw = (StreamWriter) o;
                    while(true) {
                        try { sw.WriteLine(Console.ReadLine()); }
                        catch { return; }
                    }
                });
                // これを加えるとConsole.ReadLineの入力待ちでおわらないということはないことに気がついた……
                WriteStandardInputThread.IsBackground = true;
                WriteStandardInputThread.Start(proc.StandardInput);
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
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
#if DEBUG
        // http://sumikko8note.blog.fc2.com/blog-entry-30.html
        static DateTime GetBuildDateTime(string asmPath) {
            // ファイルオープン
            using(FileStream fs = new FileStream(asmPath, FileMode.Open, FileAccess.Read))
            using(BinaryReader br = new BinaryReader(fs)) {
                // まずはシグネチャを探す
                byte[] signature = { 0x50, 0x45, 0x00, 0x00 };// "PE\0\0"
                List<byte> bytes = new List<byte>();
                while(true) {
                    bytes.Add(br.ReadByte());
                    if(bytes.Count < signature.Length) continue;

                    while(signature.Length < bytes.Count) bytes.RemoveAt(0);

                    bool isMatch = true;
                    for(int i = 0 ; i < signature.Length ; i++) {
                        if(signature[i] != bytes[i]) {
                            isMatch = false;
                            break;
                        }
                    }
                    if(isMatch) break;
                }

                // COFFファイルヘッダを読み取る
                var coff = new {
                    Machine = br.ReadBytes(2),
                    NumberOfSections = br.ReadBytes(2),
                    TimeDateStamp = br.ReadBytes(4),
                    PointerToSymbolTable = br.ReadBytes(4),
                    NumberOfSymbols = br.ReadBytes(4),
                    SizeOfOptionalHeader = br.ReadBytes(2),
                    Characteristics = br.ReadBytes(2),
                };

                // タイムスタンプをDateTimeに変換
                int timestamp = BitConverter.ToInt32(coff.TimeDateStamp, 0);
                DateTime baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime buildDateTimeUtc = baseDateTime.AddSeconds(timestamp);
                DateTime buildDateTimeLocal = buildDateTimeUtc.ToLocalTime();
                return buildDateTimeLocal;
            }
        }
#endif
    }
}
