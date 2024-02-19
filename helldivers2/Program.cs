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
        string file = "009da023c64d178d"; // huge file with textures, models, etc.
        file = "09985dc611a3a8b6";
        // iterate over all files in data dir with no extension
        // foreach (var file in Directory.EnumerateFiles(dataDir, "*", SearchOption.TopDirectoryOnly))
        // {
        //     if (file.Contains("."))
        //     {
        //         continue;
        //     }
        //
        //     ParseDataFiles(dataDir, saveDir, Path.GetFileNameWithoutExtension(file));
        // }
        ParseDataFiles(dataDir, saveDir, file);
        // SearchBytesInAllFiles(dataDir, "4D762B7CF63061C8");
        // ParseTypeLib();
    }
    
    private static byte[] StringToByteArray(string hex)
    {
        // Convert the string to a byte array.
        hex = hex.Replace(" ", "");
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }
    
    private static void SearchBytesInAllFiles(string dataDir, string bytesStr)
    {
        byte[] bytes = StringToByteArray(bytesStr);
        Parallel.ForEach(Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories), file =>
        {
            ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(file);
            // search padded to every 8 bytes
            for (int i = 0; i < fileBytes.Length; i += 8)
            {
                if (i + 8 >= fileBytes.Length)
                {
                    break;
                }
                if (fileBytes.Slice(i, 8).SequenceEqual(bytes))
                {
                    Console.WriteLine($"{file} at 0x{i:X}/{i} {bytesStr} found");
                }
            }
        });
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
        Havok = 0x5F7203C8_F280DAB8,
        Material = 0xEAC0B497_876ADEDF,
        Map = 0x2A690FD3_48FE9AC5
    }
    
        
    [StructLayout(LayoutKind.Explicit, Size = 0x68)]
    struct UnkPartHeader
    {
        [FieldOffset(0x08)] public float Unk08;
        [FieldOffset(0x0C)] public float Unk0C;
        [FieldOffset(0x10)] public float Unk10;
        [FieldOffset(0x14)] public float Unk14;
        [FieldOffset(0x18)] public float Unk18;
        [FieldOffset(0x1C)] public float Unk1C;
        [FieldOffset(0x20)] public float Unk20;
        [FieldOffset(0x24)] public int UnkSize24;
        [FieldOffset(0x28)] public uint Unk28;
        [FieldOffset(0x2C)] public int UnkIndex2C;
        [FieldOffset(0x30)] public int UnkIndex30;
        [FieldOffset(0x38)] public int Unk38; // always -1
        [FieldOffset(0x3C)] public int MeshIndex3C; // probably
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    struct PartDefinition
    {
        public int Index00;
        public int VertexOffset04;
        public int VertexCount08;
        public int IndexOffset0C;
        public int IndexCount10;
        public uint Zeros14;
    }

    struct Part
    {
        public uint Id;
        public PartDefinition Definition;
        public long MaterialId;
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
        
        List<UnkDataHeader> unkDataHeaderHavoks = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Havok).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderHavoks == null)
        {
            unkDataHeaderHavoks = new List<UnkDataHeader>();
        }
        Dictionary<ulong, UnkDataHeader> havokDataHeaders = new();
        for (int i = 0; i < unkDataHeaderHavoks.Count; i++)
        {
            var unkDataHeader = unkDataHeaderHavoks[i];
            havokDataHeaders.Add(unkDataHeader.UnkId00, unkDataHeader);
        }
        
        List<UnkDataHeader> unkDataHeaderMaterials = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Material).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderMaterials == null)
        {
            unkDataHeaderMaterials = new List<UnkDataHeader>();
        }
        Dictionary<ulong, Tuple<long, long>> materialTextures = new();
        for (int i = 0; i < unkDataHeaderMaterials.Count; i++)
        {
            var unkDataHeader = unkDataHeaderMaterials[i];
            reader.BSeek(unkDataHeader.DataOffset10+0x90);
            long textureId1 = reader.ReadInt64();
            long textureId2 = reader.ReadInt64();
            materialTextures.Add(unkDataHeader.UnkId00, new Tuple<long, long>(textureId1, textureId2));
        }
        
                    
        List<UnkDataHeader> unkDataHeaderTextures = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Texture).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderTextures == null)
        {
            unkDataHeaderTextures = new List<UnkDataHeader>();
        }
        Dictionary<ulong, UnkDataHeader> textureDataHeaders = new();
        for (int j = 0; j < unkDataHeaderTextures.Count; j++)
        {
            var unkDataHeaderTex = unkDataHeaderTextures[j];
            textureDataHeaders.Add(unkDataHeaderTex.UnkId00, unkDataHeaderTex);
        }
        
        List<UnkDataHeader> unkDataHeaderModels = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Model).Select(kvp => kvp.Value).FirstOrDefault();
        if (unkDataHeaderModels == null)
        {
            unkDataHeaderModels = new List<UnkDataHeader>();
        }
        for (int i = 0; i < unkDataHeaderModels.Count; i++)
        {
            // if (i != 230)
            // {
            //     continue;
            // }
            var unkDataHeader = unkDataHeaderModels[i];
            
            reader.BSeek(unkDataHeader.DataOffset10+0x4C);
            long nameOffset = unkDataHeader.DataOffset10 + reader.ReadUInt32();
            reader.BSeek(nameOffset + 0x14);
            uint testVal = reader.ReadUInt32();
            string name = "";
            if (nameOffset != unkDataHeader.DataOffset10 && testVal == 3)
            {
                reader.BSeek(nameOffset+0x1C);
                name = reader.ReadNullTerminatedString();
                Console.WriteLine($"{i}: Found name {name}");
            }
            
            // get alt name from havok
            if (havokDataHeaders.ContainsKey(unkDataHeader.UnkId00))
            {
                var havokDataHeader = havokDataHeaders[unkDataHeader.UnkId00];
                reader.BSeek(havokDataHeader.DataOffset10);
                ReadOnlySpan<byte> data = reader.ReadBytes((int)havokDataHeader.DataSize38);
                // find double occurence of the id in hex
                byte[] search = BitConverter.GetBytes(unkDataHeader.UnkId00);
                // search = search.Concat(search).ToArray();
                int index = data.IndexOf(search);
                if (index != -1)
                {
                    reader.BSeek(havokDataHeader.DataOffset10+index+0x18);
                    name = reader.ReadNullTerminatedString();
                    Console.WriteLine($"{i}: Found havok name {name}");
                }
            }


            reader.BSeek(unkDataHeader.DataOffset10+0x5C);
            long offset = unkDataHeader.DataOffset10 + reader.ReadUInt32();
            reader.BSeek(offset);
            int lodCount = reader.ReadInt32();
            // these are lods
            List<int> offsets = new();
            for (int j = 0; j < lodCount; j++)
            {
                offsets.Add(reader.ReadInt32());
                // their ids are also here i think
            }

            // materials
            reader.BSeek(unkDataHeader.DataOffset10+0x70);
            long materialOffset = unkDataHeader.DataOffset10 + reader.ReadUInt32();
            reader.BSeek(materialOffset);
            int materialCount = reader.ReadInt32();
            Dictionary<uint, long> materials = new();
            for (int j = 0; j < materialCount; j++)
            {
                reader.BSeek(materialOffset+4+j*4);
                uint id = reader.ReadUInt32();
                reader.BSeek(materialOffset+4+j*8+materialCount*4);
                long unkId = reader.ReadInt64();
                materials.Add(id, unkId);
            }
                        
            // parts
            reader.BSeek(unkDataHeader.DataOffset10+0x64);
            long unkOffset = unkDataHeader.DataOffset10 + reader.ReadUInt32();
            reader.BSeek(unkOffset);
            int unkCount = reader.ReadInt32();
            Dictionary<int, List<Part>> parts = new();
            for (int j = 0; j < unkCount; j++)
            {
                Dictionary<uint, PartDefinition> subparts = new();
                reader.BSeek(unkOffset + 4 + j * 4);
                int reloffset = reader.ReadInt32();
                reader.BSeek(unkOffset + 4 + unkCount * 4 + j * 4);
                uint id = reader.ReadUInt32();
                reader.BSeek(unkOffset + reloffset);
                UnkPartHeader unkPartHeader = reader.ReadType<UnkPartHeader>();
                reader.Seek(0x10, SeekOrigin.Current);
                uint unkCount2 = reader.ReadUInt32();
                uint unkSize = reader.ReadUInt32();
                List<uint> ids = new();
                for (int k = 0; k < unkCount2; k++)
                {
                    uint unkId = reader.ReadUInt32();
                    ids.Add(unkId);
                }

                for (int k = 0; k < unkCount2; k++)
                {
                    PartDefinition partDefinition = reader.ReadType<PartDefinition>();
                    subparts.Add(ids[partDefinition.Index00], partDefinition);
                }
                if (!parts.ContainsKey(unkPartHeader.MeshIndex3C))
                {
                    parts.Add(unkPartHeader.MeshIndex3C, []);
                }
                

                foreach (var subpart in subparts)
                {
                    Part part = new();
                    part.Id = subpart.Key;
                    part.Definition = subpart.Value;
                    part.MaterialId = materials.GetValueOrDefault(part.Id, -1);
                    parts[unkPartHeader.MeshIndex3C].Add(part);
                }
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
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride), BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x10) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x10+2) / 65_535.0f);
                        vertices.Add(vertex);
                    }
                }
                else if (stride == 24)
                {
                    for (int k = 0; k < vertexCount; k++)
                    {
                        var vertex = new Vertex();
                        vertex.Position = new Vector3(BitConverter.ToSingle(vertexData, k*stride+4), BitConverter.ToSingle(vertexData, k*stride+4+4), BitConverter.ToSingle(vertexData, k*stride+4+8));
                        vertex.Texcoord = new Vector2(BitConverter.ToUInt16(vertexData, k*stride+0x14) / 65_535.0f, BitConverter.ToUInt16(vertexData, k*stride+0x14+2) / 65_535.0f);
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
                else if (stride == 44)
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
                else if (stride == 52)
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
                else if (stride == 56)
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
                // indexCount = 30000;
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
                string fileName = $"{i}_{j}_{stride}_{indexCount}";
                if (name != "")
                {
                    fileName += $"_{name.Replace('/', '-')}";
                    // make filename safe
                    var invalidChars = Path.GetInvalidFileNameChars();
                    fileName = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
                }
                Directory.CreateDirectory(Path.Combine(saveDir, $"{file}/models/{fileName}"));

                List<Part> partDefinitions = parts[j];
                
                using (StreamWriter sw = new(Path.Combine(saveDir, $"{file}/models/{fileName}/model_{Endian.U64ToString(unkDataHeader.UnkId00)}.obj")))
                {
                    // for (int k = part.VertexOffset04; k < part.VertexOffset04+part.VertexCount08; k++)
                    // {
                    //     sw.WriteLine($"v {vertices[k].Position.X} {vertices[k].Position.Y} {vertices[k].Position.Z}");
                    //     sw.WriteLine($"vt {vertices[k].Texcoord.X} {vertices[k].Texcoord.Y}");
                    // }
                    int q = 0;
                    foreach (var vertex in vertices)
                    {
                        sw.WriteLine($"i {q++}");
                        sw.WriteLine($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z}");
                        sw.WriteLine($"vt {vertex.Texcoord.X} {vertex.Texcoord.Y}");
                    }
                    foreach (var part in partDefinitions)
                    {
                        sw.WriteLine($"o {fileName}_{part.Definition.Index00}_part{part.Id}");

                        List<List<int>> localindices = new();
                        for (int k = part.Definition.IndexOffset0C; k < part.Definition.IndexOffset0C+part.Definition.IndexCount10; k+=3)
                        {
                            localindices.Add([
                                BitConverter.ToUInt16(indexData, k * 2)+part.Definition.VertexOffset04,
                                BitConverter.ToUInt16(indexData, k * 2 + 2)+part.Definition.VertexOffset04,
                                BitConverter.ToUInt16(indexData, k * 2 + 4)+part.Definition.VertexOffset04
                            ]);
                        }
                        
                        foreach (var index in localindices)
                        {
                            sw.WriteLine($"f {index[0]+1}/{index[0]+1} {index[1]+1}/{index[1]+1} {index[2]+1}/{index[2]+1}");
                        }
                    }
                }
                
                // write materials
                foreach (var part in partDefinitions)
                {
                    if (part.MaterialId == -1)
                    {
                        continue;
                    }
                    if (!materialTextures.ContainsKey((ulong)part.MaterialId))
                    {
                        Console.WriteLine($"Material {part.MaterialId} not found");
                        continue;
                    }
                    var textureIds = materialTextures[(ulong)part.MaterialId];
                    var texlist = new List<long> { textureIds.Item1, textureIds.Item2 };
                    foreach (var textureId in texlist)
                    {
                        if (!textureDataHeaders.ContainsKey((ulong)textureId))
                        {
                            Console.WriteLine($"Texture {textureId} not found");
                            continue;
                        }
                        var unkDataHeaderTex = textureDataHeaders[(ulong)textureId];
                        string filename = Path.Combine(saveDir, $"{file}/models/{fileName}/part{part.Id}_{Endian.U64ToString(unkDataHeaderTex.UnkId00)}.dds");
                        if (File.Exists(filename))
                        {
                            continue;
                        }
                        reader.BSeek(unkDataHeaderTex.DataOffset10 + 0xC0); // unknown 0xC0
                        var ddsHeaderBytes = reader.ReadBytes(0x94);  // assumes DX10 extra
                        if (unkDataHeaderTex.StreamDataSize3C > 0)
                        {
                            if (streamFile == null)
                            {
                                streamFile = new HDFile(Path.Combine(dataDir, file + ".stream"));
                                streamReader = streamFile.GetReader();
                            }
                            streamReader.BSeek(unkDataHeaderTex.StreamDataOffset18);
                            var streamData = streamReader.ReadBytes((int)unkDataHeaderTex.StreamDataSize3C);
                            File.WriteAllBytes(filename, ddsHeaderBytes.Concat(streamData).ToArray());
                        }
                        else
                        {
                            if (gpuReader == null)
                            {
                                gpuFile = new HDFile(Path.Combine(dataDir, file + ".gpu_resources"));
                                gpuReader = gpuFile.GetReader();
                            }
                            gpuReader.BSeek(unkDataHeaderTex.GPUDataOffset20);
                            var gpuData = gpuReader.ReadBytes((int)unkDataHeaderTex.GPUDataSize40);
                            File.WriteAllBytes(filename, ddsHeaderBytes.Concat(gpuData).ToArray());
                        }
                    }
                }

                // break;
            }
        }
        

        Directory.CreateDirectory(Path.Combine(saveDir, $"{file}/textures"));
        // Directory.CreateDirectory(Path.Combine(saveDir, $"textures"));
        for (int i = 0; i < unkDataHeaderTextures.Count; i++)
        {
            var unkDataHeader = unkDataHeaderTextures[i];
            string filename = Path.Combine(saveDir, $"{file}/textures/{i}_{Endian.U64ToString(unkDataHeader.UnkId00)}.dds");
            if (File.Exists(filename))
            {
                continue;
            }
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
        
        List<UnkDataHeader> mapHeaders = unkDataHeaders.Where(kvp => kvp.Value.Count > 0 && kvp.Value[0].UnkId08 == (ulong)ResourceType.Map).Select(kvp => kvp.Value).FirstOrDefault();
        if (mapHeaders.Count == 0)
        {
            return;
        }
        Debug.Assert(mapHeaders.Count == 1);
        UnkDataHeader mapHeader = mapHeaders[0];

        var a = 0;
    }
}
