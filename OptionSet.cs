using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TeX2img {
    public class OptionSet : NDesk.Options.OptionSet {
        Dictionary<string, Func<string>> default_values = new Dictionary<string, Func<string>>();
        public OptionSet Add<T>(string prototype, string description, Action<T> action, T defval) {
            return Add(prototype, description, action, new Func<T>(() => defval));
        }
        public OptionSet Add<T, S>(string prototype, string description, Action<T> action, Func<S> defval) {
            default_values[prototype] = new Func<string>(() => defval().ToString());
            return (OptionSet) base.Add(prototype, description, action);
        }
        public OptionSet Add<S>(string prototype, string description, Action<string> action, Func<S> defval) {
            default_values[prototype] = new Func<string>(() => defval().ToString());
            return (OptionSet) base.Add(prototype, description, action);
        }

        // 次でエラーを出すようにする
        // "option"の指定の時に--option=abc
        // "option="の指定の時に--option-
        protected override bool Parse(string argument, NDesk.Options.OptionContext c) {
            if(c.Option == null) {
                string f, n, s, v;
                if(!GetOptionParts(argument, out f, out n, out s, out v)) return false;
                if(Contains(n)) {
                    var p = this[n];
                    if(v != null && p.OptionValueType == NDesk.Options.OptionValueType.None) {
                        // メッセージはさぼり
                        throw new NDesk.Options.OptionException(c.OptionSet.MessageLocalizer(""), f + n);
                    }
                } else {
                    string rn;
                    if(n.Length >= 1 && (n[n.Length - 1] == '-' || n[n.Length - 1] == '+') && Contains((rn = n.Substring(0, n.Length - 1)))) {
                        var p = this[rn];
                        if(p.OptionValueType == NDesk.Options.OptionValueType.Required) {
                            throw new NDesk.Options.OptionException(c.OptionSet.MessageLocalizer("An argument is required for the option '" + f + rn + "'"), f + rn);
                        }
                    }
                }
            }
            return base.Parse(argument, c);
        }

        // OptionSet.WriteOptionDescriptionsがちょっと気にくわないので独自に
        // GetNames().Count == 1と仮定してある．
        public new void WriteOptionDescriptions(TextWriter output) {
            int maxlength = 0;
            bool minus_exist = false;
            foreach(var oh in this) {
                if(oh.Description != null) {
                    int length = oh.GetNames()[0].Length;
                    if(oh.Description.EndsWith("[-]")) {
                        minus_exist = true;
                        length += 3;
                    } else if(oh.OptionValueType == NDesk.Options.OptionValueType.Optional) length += 7;
                    else if(oh.OptionValueType == NDesk.Options.OptionValueType.Required) length += 5;
                    maxlength = Math.Max(maxlength, length);
                }
            }
            if(minus_exist) maxlength += 3;
            foreach(var oh in this) {
                if(oh.Description != null) {
                    string valtype = "VAL";
                    if(oh.GetType().ToString().EndsWith("[System.Int32]")) valtype = "NUM";
                    string opstr = "/" + oh.GetNames()[0];
                    string desc = oh.Description.Replace("\n", "\n" + new string(' ', maxlength + 1));
                    if(oh.OptionValueType == NDesk.Options.OptionValueType.Optional) opstr += "[=<" + valtype + ">]";
                    else if(oh.OptionValueType == NDesk.Options.OptionValueType.Required) opstr += "=<" + valtype + ">";
                    else if(desc.EndsWith("[-]")) {
                        // 説明文の最後が[-]なものは，否定が可能なオプション．/helpではオプション名の最後に表示する．
                        opstr += "[-]";
                        desc = desc.Substring(0, desc.Length - 3);
                    }
                    if(default_values.ContainsKey(oh.Prototype)) {
                        desc += "（現在：" + default_values[oh.Prototype]().ToString() + "）";
                    }
                    output.WriteLine("  " + opstr + new string(' ', maxlength - opstr.Length) + desc);
                }
            }
        }
    }
}
