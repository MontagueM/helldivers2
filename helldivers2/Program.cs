using System.Runtime.InteropServices;
using helldivers2;





partial class Program
{
    public static void Main(string[] args)
    {
        // ParseDataFiles();
        ParseTypeLib();
    }
    
    [StructLayout(LayoutKind.Sequential, Size = 0x24)]
    struct TLHeader
    {
        public UInt32 Magic;  // DLTL
        public Int32 UnkCount04;
        public Int32 UnkFirstEntriesCount08;
        public Int32 UnkSecondEntriesCount0C;
        public Int32 UnkThirdEntriesCount10;
        public Int32 UnkFourthEntriesCount14;
        public Int32 UnkFifthEntriesCount18;
        public Int32 RelativeOffsetEndOfEntriesToStrings1C;
        public Int32 StringsDataSize20;
    }
    
    [StructLayout(LayoutKind.Sequential, Size=0x24)]
    struct TLUnk1
    {
        public Int32 UnkOffset00;
        public Int32 Unk04;
        public Int32 Unk08;
        public Int32 Unk0C;
        public Int32 Unk10;
        public Int32 Unk14;
        public Int32 UnkSize18;
        public Int32 UnkIndex1C;
        public Int32 UnkOffset20; // usually -1
    }
    
    [StructLayout(LayoutKind.Sequential, Size=0x20)]
    struct TLStringGroupDefinition
    {
        public Int64 StringGroupNameOffset00;
        public Int32 Unk08;
        public Int32 StringGroupSize0C;
        public Int32 StringGroupOffset10;
        public Int32 StringGroupSize14; // always same as 0C i think?
        public Int32 StringGroupOffset18; // always same as 10 i think?
        public Int32 StringGroupExportTypeOffset1C; // sometimes -1
    }
    
    [StructLayout(LayoutKind.Sequential, Size=0x34)]
    struct TLUnk3
    {
        public Int32 UnkOffset00;
        public Int32 UnkOffset04; // usually -1
        public Int32 Unk08;
        public Int32 Unk0C;
        public Int32 Unk10;
        public Int32 Unk14;
        public Int32 Unk18;
        public Int32 Unk1C;
        public Int32 UnkSize20;
        public Int32 UnkSize24;
        public Int32 UnkOffset28;
        public Int32 Unk2C;
        public Int32 Unk30;  // usually 3
    }
    
    [StructLayout(LayoutKind.Sequential, Size=0x10)]
    struct TLStringUnk1
    {
        public Int32 UnkIndex00;
        public Int32 Unk04; // usually -1
        public Int64 Unk08;
    }
    
    [StructLayout(LayoutKind.Sequential, Size=0x8)]
    struct TLStringGroupStringDefinition // it excludes some strings
    {
        public Int32 StringDataOffset00;
        public Int32 StringIndex04;
    }

    struct TLStringGroup
    {
        public List<string> Strings;
        public string Name;
        public string ExportType;
    }

    private static void ParseTypeLib()
    {
        string file = @"D:\SteamLibrary\steamapps\common\Helldivers 2\data\game\dl_library.dl_typelib";
        var reader = new HDReader(file);
        var header = reader.ReadType<TLHeader>();
        reader.Seek(0x1330, SeekOrigin.Current);
        var unk1s = new List<TLUnk1>();
        for (int i = 0; i < header.UnkFirstEntriesCount08; i++)
        {
            unk1s.Add(reader.ReadType<TLUnk1>());
        }
        var unk2s = new List<TLStringGroupDefinition>();
        for (int i = 0; i < header.UnkSecondEntriesCount0C; i++)
        {
            unk2s.Add(reader.ReadType<TLStringGroupDefinition>());
        }
        var unk3s = new List<TLUnk3>();
        for (int i = 0; i < header.UnkThirdEntriesCount10; i++)
        {
            unk3s.Add(reader.ReadType<TLUnk3>());
        }
        // todo theres like 15k strings but only 5261 strings used in the unk2 groups
        var unk4s = new List<TLStringUnk1>();
        for (int i = 0; i < header.UnkFourthEntriesCount14; i++)
        {
            unk4s.Add(reader.ReadType<TLStringUnk1>());
        }
        var unk5s = new List<TLStringGroupStringDefinition>();
        for (int i = 0; i < header.UnkFifthEntriesCount18; i++)
        {
            unk5s.Add(reader.ReadType<TLStringGroupStringDefinition>());
        }
        long stringOffset = reader.Seek(header.RelativeOffsetEndOfEntriesToStrings1C, SeekOrigin.Current);
        var strings = new List<string>();
        while (reader.Position < reader.BaseStream.Length) // can also use StringsDataSize20 but this also works
        {
            strings.Add(reader.ReadNullTerminatedString());
        }
        var stringGroups = new List<TLStringGroup>();
        foreach (var unk2 in unk2s)
        {
            TLStringGroup stringGroup = new();
            var relevantunk5s = unk5s.GetRange(unk2.StringGroupOffset10, unk2.StringGroupSize0C);
            stringGroup.Strings = new();
            foreach (var unk5 in relevantunk5s)
            {
                reader.BSeek(stringOffset+unk5.StringDataOffset00);
                stringGroup.Strings.Add(reader.ReadNullTerminatedString());
            }
            reader.BSeek(stringOffset+unk2.StringGroupNameOffset00);
            stringGroup.Name = reader.ReadNullTerminatedString();
            if (unk2.StringGroupExportTypeOffset1C != -1)
            {
                reader.BSeek(stringOffset+unk2.StringGroupExportTypeOffset1C);
                stringGroup.ExportType = reader.ReadNullTerminatedString();
            }
            stringGroups.Add(stringGroup);
        }
        var a = 0;
    }
    
    [StructLayout(LayoutKind.Sequential, Size = 0x50)]
    struct Header
    {
        public UInt32 Magic;
        public UInt32 UnkCount1;  // different types?
        public UInt64 UnkCount2;  // total data count
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    struct Unk1  // one for each data type
    {
        public UInt64 UnkId;
        public UInt64 DataCount;
        public UInt32 UnkSize1; // specifies ID size of data header?
        public UInt32 UnkSize2;  // specifies data size of data header?
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x50)]
    struct UnkDataHeader
    {
        public UInt64 UnkId00;
        public UInt64 UnkId08;
        public Int64 DataOffset10;
        public UInt16 Unk18;
        public UInt16 Unk1A;
        public UInt32 Zeros1C;
        public Int64 GPUDataOffset20;
        public UInt64 Unk28;
        public UInt64 Unk30;
        public UInt32 DataSize38;
        public UInt32 StreamDataSize3C;
        public UInt32 GPUDataSize40;
        public UInt32 UnkSize44;
        public UInt32 UnkSize48;
        public UInt32 UnkIndex4C;
    }

    class Texture
    {
        public byte[] DDSHeaderBytes;
        public byte[] DDSData;
    }

    private static void ParseDataFiles()
    {
        string dataDir = @"D:\SteamLibrary\steamapps\common\Helldivers 2\data\";
        string saveDir = @"C:\Users\monta\OneDrive\helldivers2\saved\";
        string file = "05b7f582b44bac01";
        // string file = "01710ddcfcdc9e8f";
        var topfile = new HDFile(Path.Combine(dataDir, file));
        var gpuFile = new HDFile(Path.Combine(dataDir, file + ".gpu_resources"));
        var reader = topfile.GetReader();
        var gpuReader = gpuFile.GetReader();
        var header = reader.ReadType<Header>();
        List<Unk1> unk1s = new();
        for (int i = 0; i < header.UnkCount1; i++)
        {
            unk1s.Add(reader.ReadType<Unk1>());
        }
        Dictionary<int, List<UnkDataHeader>> unkDataHeaders = new();
        for (int i = 0; i < header.UnkCount1; i++)
        {
            var unkDataHeader = new List<UnkDataHeader>();
            for (ulong j = 0; j < unk1s[i].DataCount; j++)
            {
                unkDataHeader.Add(reader.ReadType<UnkDataHeader>());
            }
            unkDataHeaders.Add(i, unkDataHeader);
        }
        
        List<Texture> textures = new();
        foreach (var unkDataHeader in unkDataHeaders[0])
        {
            reader.BSeek(unkDataHeader.DataOffset10 + 0xC0); // unknown 0xC0
            var ddsHeaderBytes = reader.ReadBytes(0x94);  // assumes DX10 extra
            gpuReader.BSeek(unkDataHeader.GPUDataOffset20);
            var ddsData = gpuReader.ReadBytes((int)unkDataHeader.GPUDataSize40);
            textures.Add(new Texture { DDSHeaderBytes = ddsHeaderBytes, DDSData = ddsData });
        }
        
        for (int i = 0; i < textures.Count; i++)
        {
            var texture = textures[i];
            File.WriteAllBytes(Path.Combine(saveDir, $"{file}_{i}.dds"), texture.DDSHeaderBytes.Concat(texture.DDSData).ToArray());
        }

        var a = 0;
    }
}
