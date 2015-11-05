using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TeX2img {
    class MuPDF : IDisposable {
        Process process;
        List<byte> StdInputBuf = new List<byte>(), StdOutputBuf = new List<byte>();
        Action ReadStdOutputAction; 
        IAsyncResult ReadStdOutputThread;
        string error_str;
        volatile bool error_occured = false;
        public MuPDF(string path) {
            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = "--interactive";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            ReadStdOutputAction = new Action(ReadFromStdOutput);
            ReadStdOutputThread = ReadStdOutputAction.BeginInvoke(null, null);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            error_str += e.Data + "\n";
            error_occured = true;
        }

        public void ClearError() {
            error_occured = false;
            error_str = "";
        }

        public void Dispose() {
            WriteLine("quit");
            process.WaitForExit(1000);
            if (!process.HasExited) process.Kill();
            for(int i = 0; i < 10; ++i) {
                if (ReadStdOutputThread.IsCompleted) break;
                System.Threading.Thread.Sleep(i < 5 ? 1 : 10);
            }
            if (!ReadStdOutputThread.IsCompleted) ReadStdOutputAction.EndInvoke(ReadStdOutputThread);
            process.Dispose();
        }

        void ReadFromStdOutput() {
            while (true) {
                var b = process.StandardOutput.BaseStream.ReadByte();
                if(b == -1) {
                    if (process.HasExited) return;
                    process.WaitForExit(100);
                    b = process.StandardOutput.BaseStream.ReadByte();
                }
                if (b == -1) return;
                StdOutputBuf.Add((byte)b);
            }
        }

        string ReadLineSub() {
            List<byte> rv;
            for (int i = 0; i < StdOutputBuf.Count; ++i) {
                var b = StdOutputBuf[i];
                if (b == '\r' || b == '\n') {
                    var j = i;
                    if (b == '\r') {
                        if (i + 1 < StdOutputBuf.Count && StdOutputBuf[i + 1] == '\n') ++i;
                    }
                    rv = StdOutputBuf.GetRange(0, j);
                    StdOutputBuf.RemoveRange(0, i + 1);
                    string buf = Encoding.UTF8.GetString(rv.ToArray());
                    return buf;
                    //return Encoding.UTF8.GetString(rv.ToArray());
                }
            }
            return null;
        }

        string ReadLine() {
            for(int i = 0; i < 25; ++i) {
                if (error_occured) throw new Exception(error_str);
                var s = ReadLineSub();
                if (s != null) return s;
                System.Threading.Thread.Sleep(i < 10 ? 1 : (i < 15 ? 10 : 100));
            }
            return null;
        }

        string ReadString() {
            int size = Int32.Parse(ReadLine());
            for (int i = 0; i < 15; ++i) {
                if (StdOutputBuf.Count >= size) {
                    System.Diagnostics.Debug.WriteLine(i);
                    break;
                }
                System.Threading.Thread.Sleep(i < 5 ? 1 : (i < 10 ? 10 : 100));
            }
            if (StdOutputBuf.Count < size) System.Threading.Thread.Sleep(100);
            if (StdOutputBuf.Count < size) throw new TimeoutException(); ;
            var buf = StdOutputBuf.GetRange(0, size);
            StdOutputBuf.RemoveRange(0, size);
            ReadLine();
            return Encoding.UTF8.GetString(buf.ToArray());
        }

        int ReadInt() {
            var str = ReadLine();
            if (str == null) throw new TimeoutException();
            return Int32.Parse(str);
        }

        decimal ReadDecimal() {
            var str = ReadLine();
            if (str == null) throw new TimeoutException();
            return Decimal.Parse(str);
        }

        BoundingBox ReadBoundingBox() {
            // 左下
            var x0 = ReadDecimal();
            var y0 = ReadDecimal();
            // 右上
            var x1 = ReadDecimal();
            var y1 = ReadDecimal();
            return new BoundingBox(x0, y0, x1, y1);
        }

        void Write(string str) {
            var buf = Encoding.UTF8.GetBytes(str);
            Write(buf.Length);
            process.StandardInput.BaseStream.Write(buf, 0, buf.Length);
            process.StandardInput.BaseStream.WriteByte((byte)'\n');
            process.StandardInput.BaseStream.Flush();
        }

        void Write(int n) {
            var buf = Encoding.UTF8.GetBytes(n.ToString());
            process.StandardInput.BaseStream.Write(buf, 0, buf.Length);
            process.StandardInput.BaseStream.WriteByte((byte)'\n');
            process.StandardInput.BaseStream.Flush();
        }

        void WriteLine(string str) {
            var buf = Encoding.UTF8.GetBytes(str);
            process.StandardInput.BaseStream.Write(buf, 0, buf.Length);
            process.StandardInput.BaseStream.WriteByte((byte)'\n');
            process.StandardInput.BaseStream.Flush();
        }

        public void Execute(string func, params object[] inputs) {
            Execute(func, new Type[] { }, inputs);
            var s = ReadLine();
        }

        public object Execute(string func,Type type,params object[] inputs) {
            return Execute(func, new Type[] { type }, inputs)[0];
        }

        public object[] Execute(string func,Type[] types, params object[] inputs) {
            WriteLine(func);
            foreach(var o in inputs) {
                if (o.GetType() == typeof(string)) Write((string)o);
                else if (o.GetType() == typeof(int)) Write((int)o);
                else throw new System.NotImplementedException();
            }
            var rv = new object[types.Length];
            for(int i = 0; i < 5; ++i) {
                if (StdOutputBuf.Count != 0) break;
                System.Threading.Thread.Sleep(1);
            }
            for(int i = 0; i < types.Length; ++i) {
                if (types[i] == typeof(string)) rv[i] = ReadString();
                else if (types[i] == typeof(int)) rv[i] = ReadInt();
                else if (types[i] == typeof(BoundingBox)) rv[i] = ReadBoundingBox();
                else throw new System.NotImplementedException();
            }
            return rv;
        }

    }
}
