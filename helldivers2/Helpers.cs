using System.Runtime.InteropServices;
using System.Text;

namespace helldivers2;

public class HDReader : BinaryReader
{
    public HDReader(Stream stream) : base(stream) { }

    public HDReader(byte[] data) : base(new MemoryStream(data)) { }

    public HDReader(string filePath) : base(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public long Position => BaseStream.Position;

    public long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
    
    public long BSeek(long offset) => BaseStream.Seek(offset, SeekOrigin.Begin);

    public string ReadNullTerminatedString()
    {
        StringBuilder sb = new();
        char c;
        while ((c = ReadChar()) != 0)
        {
            sb.Append(c);
        }
        return sb.ToString();
    }

    public void BSeek(ulong offset) => BSeek((long)offset);
}

public class HDFile
{
    private byte[]? _data = null;
    private string _path = "";

    public HDFile(string path)
    {
        _path = path;
    }

    public HDReader GetReader()
    {
        return new HDReader(GetStream());
    }

    public MemoryStream GetStream()
    {
        return new MemoryStream(GetData());
    }

    public byte[] GetData(bool shouldCache = true)
    {
        if (shouldCache)
        {
            if (_data == null)
            {
                _data = File.ReadAllBytes(_path);
            }

            return _data;
        }
        else
        {
            return File.ReadAllBytes(_path);
        }
    }
}

public static class StructConverter
{
    public static T ToType<T>(this byte[] bytes)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T)); }
        finally { handle.Free(); }
    }

    public static dynamic ToType(this byte[] bytes, Type type)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
        finally { handle.Free(); }
    }

    public static T ReadType<T>(this BinaryReader stream)
    {
        return (T)ReadType(stream, typeof(T));
    }

    public static dynamic ReadType(this BinaryReader stream, Type type)
    {
        var buffer = new byte[Marshal.SizeOf(type)];
        stream.Read(buffer, 0, buffer.Length);
        return buffer.ToType(type);
    }

    public static void WriteStruct<T>(this BinaryWriter stream, T value) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf(typeof(T))];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            stream.Write(buffer, 0, buffer.Length);
        }
        finally { handle.Free(); }
    }

    public static byte[] FromType<T>(T value) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf(typeof(T))];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
        }
        finally { handle.Free(); }

        return buffer;
    }
}
