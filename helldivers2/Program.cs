using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using helldivers2;





partial class Program
{
    public static void Main(string[] args)
    {
        string dataDir = @"D:\SteamLibrary\steamapps\common\Helldivers 2\data\";
        string saveDir = @"C:\Users\monta\OneDrive\helldivers2\saved\";
        // string file = "05b7f582b44bac01"; // 4 textures
        // string file = "01710ddcfcdc9e8f"; // 1 texture
        // string file = "fd26c7b93257d7a6";  // no stream file
        // string file = "009da023c64d178d"; // huge file with textures, models, etc.
        // file = "c6d14774e5c77651";
        // iterate over all files in data dir with no extension
        foreach (var file in Directory.EnumerateFiles(dataDir, "*", SearchOption.TopDirectoryOnly))
        {
            if (file.Contains("."))
            {
                continue;
            }

            ParseDataFiles(dataDir, saveDir, Path.GetFileNameWithoutExtension(file));
        }
        // ParseTypeLib();
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
        public UInt64 TypeId;
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
        public UInt32 StreamDataOffset18;
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
    
    [StructLayout(LayoutKind.Sequential, Size = 0xC0)]
    struct TextureHeader
    {
        public UInt32 UnkId00;
        public Int32 Unk04;
        public Int32 Unk08;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0F)]
        public List<TextureDetail> TextureDetails;
    }
    
    [StructLayout(LayoutKind.Sequential, Size = 0x0C)]
    struct TextureDetail
    {
        public Int32 DataOffset;
        public Int32 DataSize;
        public Int16 Width;
        public Int16 Height;
    }

    class Texture
    {
        public byte[] DDSHeaderBytes;
        public byte[] DDSData;
    }

    struct Vertex
    {
        public Vector3 Position;
        public Vector2 Texcoord;
        public Vector3 Normal;
    }

    enum ResourceType : ulong
    {
        Texture = 0xCD4238C6_A0C69E32,
        Model = 0xE0A48D0B_E9A7453F,
        Havok = 0x5F7203C8_F280DAb8
    }

    private static void ParseDataFiles(string dataDir, string saveDir, string file)
    {
        var topfile = new HDFile(Path.Combine(dataDir, file));
        HDFile? gpuFile = null;
        HDReader? gpuReader = null;
        HDFile? streamFile = null;
        var reader = topfile.GetReader();
        HDReader? streamReader = null;
        var header = reader.ReadType<Header>();
        List<Unk1> unk1s = new();
        for (int i = 0; i < header.UnkCount1; i++)
        {
            unk1s.Add(reader.ReadType<Unk1>());
            if (i < header.UnkCount1-1)
            {
                // padded to 0x10
                reader.Seek(0x8, SeekOrigin.Current);
            }
        }
        Dictionary<int, List<UnkDataHeader>> unkDataHeaders = new();
        for (int i = 0; i < header.UnkCount1; i++)
        {
            var unkDataHeaderss = new List<UnkDataHeader>();
            for (ulong j = 0; j < unk1s[i].DataCount; j++)
            {
                var unkDataHeader = reader.ReadType<UnkDataHeader>();
                unkDataHeaderss.Add(unkDataHeader);
            }
            unkDataHeaders.Add(i, unkDataHeaderss);
        }
        
        if (unkDataHeaders.Count == 0)
        {
            return;
        }
        
        
        List<UnkDataHeader> unkDataHeaderModels = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Model).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderModels == null)
        {
            unkDataHeaderModels = new List<UnkDataHeader>();
        }
        unkDataHeaderModels.Clear();
        for (int i = 0; i < unkDataHeaderModels.Count; i++)
        {
            // if (i != 77)
            // {
            //     continue;
            // }
            var unkDataHeader = unkDataHeaderModels[i];
            reader.BSeek(unkDataHeader.DataOffset10+0x5C);
            long offset = unkDataHeader.DataOffset10 + reader.ReadUInt32();
            reader.BSeek(offset);
            int unkCount = reader.ReadInt32();
            // these are lods
            List<int> offsets = new();
            for (int j = 0; j < unkCount; j++)
            {
                offsets.Add(reader.ReadInt32());
                // their ids are also here i think
            }
            
            for (int j = 0; j < offsets.Count; j++)
            {
                int off = offsets[j];
                reader.BSeek(offset+off+0x160);
                uint vertexCount = reader.ReadUInt32();
                int stride = reader.ReadInt32();
                reader.BSeek(offset+off+0x1A0);
                uint vertexStartOffset = reader.ReadUInt32();
                uint vertexDataSize = reader.ReadUInt32();
                uint indexStartOffset = reader.ReadUInt32();
                uint indexDataSize = reader.ReadUInt32();
                if (gpuReader == null)
                {
                    gpuFile = new HDFile(Path.Combine(dataDir, file + ".gpu_resources"));
                    gpuReader = gpuFile.GetReader();
                }
                gpuReader.BSeek(unkDataHeader.GPUDataOffset20+vertexStartOffset);
                var vertexData = gpuReader.ReadBytes((int)(vertexCount*stride));
                List<Vertex> vertices = new();
                // vertexCount = 5_000;
                if (stride == 36)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 24)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 28)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride), BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x10) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x10+2) / 65_535.0f);
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 16)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 20)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride), BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x10) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x10+2) / 65_535.0f);
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 32)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x14) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x14+2) / 65_535.0f);
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 40)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x14) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x14+2) / 65_535.0f);
                        // i think its pos -> tex -> norm, tex and norm as uint16
                        vertices.Add(vertex);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                gpuReader.BSeek(unkDataHeader.GPUDataOffset20+indexStartOffset);
                var indexData = gpuReader.ReadBytes((int)indexDataSize);
                var indexCount = indexData.Length/6;
                List<List<int>> indices = new();
                // int indexStart = 329;
                // indexCount = 5000;
                for (int k = 0; k < indexCount; k++)
                {
                    indices.Add(new List<int>
                    {
                        BitConverter.ToUInt16(indexData, k*6),
                        BitConverter.ToUInt16(indexData, k*6+2),
                        BitConverter.ToUInt16(indexData, k*6+4)
                    });
                }
                // write to obj
                Directory.CreateDirectory(Path.Combine(saveDir, $"{file}/models"));
                using (StreamWriter sw = new(Path.Combine(saveDir, $"{file}/models/{i}_{j}_{stride}_{indexCount}.obj")))
                {
                    int t = 0;
                    foreach (var vertex in vertices)
                    {
                        sw.WriteLine($"i {t}");
                        sw.WriteLine($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z}");
                        sw.WriteLine($"vt {vertex.Texcoord.X} {vertex.Texcoord.Y}");
                        t++;
                    }
                    foreach (var vertex in vertices)
                    {
                    }
                    foreach (var index in indices)
                    {
                        sw.WriteLine($"f {index[0]+1}/{index[0]+1} {index[1]+1}/{index[1]+1} {index[2]+1}/{index[2]+1}");
                    }
                }

                break;
            }
        }
        
        List<UnkDataHeader> unkDataHeaderTextures = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Texture).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderTextures == null)
        {
            unkDataHeaderTextures = new List<UnkDataHeader>();
        }
        // Directory.CreateDirectory(Path.Combine(saveDir, $"{file}/textures"));
        Directory.CreateDirectory(Path.Combine(saveDir, $"textures"));
        for (int i = 0; i < unkDataHeaderTextures.Count; i++)
        {
            string filename = Path.Combine(saveDir, $"textures/{file}_{i}.dds");
            if (File.Exists(filename))
            {
                continue;
            }
            var unkDataHeader = unkDataHeaderTextures[i];
            reader.BSeek(unkDataHeader.DataOffset10 + 0xC0); // unknown 0xC0
            var ddsHeaderBytes = reader.ReadBytes(0x94);  // assumes DX10 extra
            if (unkDataHeader.StreamDataSize3C > 0)
            {
                if (streamFile == null)
                {
                    streamFile = new HDFile(Path.Combine(dataDir, file + ".stream"));
                    streamReader = streamFile.GetReader();
                }
                streamReader.BSeek(unkDataHeader.StreamDataOffset18);
                var streamData = streamReader.ReadBytes((int)unkDataHeader.StreamDataSize3C);
                File.WriteAllBytes(filename, ddsHeaderBytes.Concat(streamData).ToArray());
                // File.WriteAllBytes(Path.Combine(saveDir, $"{file}/textures/{i}_{Endian.U64ToString(unkDataHeader.UnkId00)}.dds"), ddsHeaderBytes.Concat(streamData).ToArray());
            }
            else
            {
                if (gpuReader == null)
                {
                    gpuFile = new HDFile(Path.Combine(dataDir, file + ".gpu_resources"));
                    gpuReader = gpuFile.GetReader();
                }
                gpuReader.BSeek(unkDataHeader.GPUDataOffset20);
                var gpuData = gpuReader.ReadBytes((int)unkDataHeader.GPUDataSize40);
                File.WriteAllBytes(filename, ddsHeaderBytes.Concat(gpuData).ToArray());
            }
        }


        var a = 0;
    }
}
