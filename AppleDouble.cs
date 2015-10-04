using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TeX2img {
    // http://www.opensource.apple.com/source/xnu/xnu-792/bsd/vfs/vfs_xattr.c
    // ビッグエンディアンで入っている．
    // [(4) MAGIC:0x00 0x05 0x16 0x07]
    // [(4) Version:0x00 0x02 0x00 0x00]
    // [(16) filler "Mac OS X        "]
    // [(2) n = entryの数]n = 2で固定？
    // [(12×n) entry header達] 一つ目はFinderInfo，最後はリソースフォークで固定
    // [entry本体]
    //
    // (entry header)
    // [(4) type]
    // [(4) offset]
    // [(4) length]
    //
    // (FinderInfo)
    // [(32) 何かデータ]
    // [(2) 何かデータ]
    // [(?×n) 拡張属性ヘッダ達]
    // [(?×n) 拡張属性本体]
    //
    // (拡張属性ヘッダ)
    // [(4) "ATTR"]
    // [(4) debug_tag]
    // [(4) 拡張属性サイズ（ヘッダ含）]
    // [(4) データoffset]
    // [(4) データ長さ]
    // [(12) 予約済み]
    // [(2) flag]
    // [(2) n = 拡張属の数]
    // [(?×n) 各エントリーのヘッダ]
    //
    // (拡張属性エントリーヘッダ)
    // [(4) offset]
    // [(4) 長さ]
    // [(2) flag]
    // [(1) len = 名前長さ] 終端0を含む長さ
    // [(len) 名前] 最後は0で終わる．
    // [(?) ヘッダ長さが4の倍数になるように調整]

    class AppleDouble {
        public string BaseFileFormat { get; private set; }
        List<Entry> entries = null;
        public List<Entry> Entries { get { return entries; } }

        struct EntryHeader {
            public uint id, offset, length;
        }
        public AppleDouble(string path) {
            using (var fr = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                var entriesList = new List<Entry>();
                var br = new BinaryReader(fr);
                var magic = FromBE(br.ReadInt32());
                if (magic != 0x00051607) throw new FormatException();
                var version = FromBE(br.ReadInt32());
                if (version != 0x00020000) throw new FormatException();
                BaseFileFormat = Encoding.UTF8.GetString(br.ReadBytes(16));
                int entriesCount = FromBE(br.ReadUInt16());
                var entryHeaders = new List<EntryHeader>();
                for (uint i = 0; i < entriesCount; ++i) {
                    var header = new EntryHeader();
                    header.id = FromBE(br.ReadUInt32());
                    header.offset = FromBE(br.ReadUInt32());
                    header.length = FromBE(br.ReadUInt32());
                    entryHeaders.Add(header);
                }
                entries = new List<Entry>();
                foreach (var header in entryHeaders) {
                    switch (header.id) {
                    case 9:
                        fr.Seek(header.offset, SeekOrigin.Begin);
                        entries.Add(new FinderInfo(fr));
                        break;
                    default:
                        entries.Add(new NotImplementedEntry(header.id));
                        break;
                    }
                }
            }
        }
        public interface Entry {
            uint ID { get; }
        }
        struct AttrHeader {
            public uint offset, length;
            public string name;
        }
        public class FinderInfo : Entry {
            public uint ID { get { return 9; } }
            public uint Length { get; private set; }
            public struct Attr {
                public string Name { get; private set; }
                public byte[] Data { get; private set; }
                public Attr(string n, byte[] d) {
                    Name = n; Data = d;
                }
            }
            public List<Attr> Attrs = new List<Attr>();

            public FinderInfo(FileStream fr) {
                var br = new BinaryReader(fr);
                fr.Position += 34;// Finder Info ヘッダ本体？（読み飛ばし）
                if (!br.ReadBytes(4).SequenceEqual(new byte[] { (byte)'A', (byte)'T', (byte)'T', (byte)'R' })) throw new FormatException();
                br.ReadInt32();// debug_tag
                Length = FromBE(br.ReadUInt32());
                var data_start = FromBE(br.ReadUInt32());
                var data_length = FromBE(br.ReadUInt32());
                fr.Position += 12;
                var flag = FromBE(br.ReadUInt16());
                var attrCount = FromBE(br.ReadUInt16());
                var attrHeaders = new List<AttrHeader>();
                for (uint i = 0; i < attrCount; ++i) {
                    var header = new AttrHeader();
                    header.offset = FromBE(br.ReadUInt32());
                    header.length = FromBE(br.ReadUInt32());
                    br.ReadUInt16();//flags
                    var namelen = br.ReadByte();
                    header.name = Encoding.UTF8.GetString(br.ReadBytes(namelen - 1));// 0終端
                    if (br.ReadByte() != 0) throw new FormatException();
                    fr.Position += 3 - ((11 + namelen + 3) % 4);
                    attrHeaders.Add(header);
                }
                foreach(var header in attrHeaders) {
                    fr.Seek(header.offset, SeekOrigin.Begin);
                    Attrs.Add(new Attr(header.name, br.ReadBytes((int)header.length)));
                }
            }
        }
        public class NotImplementedEntry : Entry {
            public uint ID { get; private set; }
            public NotImplementedEntry(uint id) { ID = id; }
        }

        static UInt32 FromBE(UInt32 n) {
            if (BitConverter.IsLittleEndian) {
                return ((n & 0xFF000000) >> 24) | ((n & 0xFF0000) >> 8) | ((n & 0xFF00) << 8) | ((n & 0xFF) << 24);
            } else return n;
        }
        static Int32 FromBE(Int32 n) {
            return (Int32)FromBE((UInt32)n);
        }
        static UInt16 FromBE(UInt16 n) {
            if (BitConverter.IsLittleEndian) {
                return (UInt16)(((n & 0xFF00) >> 8) | ((n & 0xFF) << 8));
            } else return n;
        }
        static Int16 FromBE(Int16 n) {
            return (Int16)FromBE((UInt16)n);
        }

    }
}
