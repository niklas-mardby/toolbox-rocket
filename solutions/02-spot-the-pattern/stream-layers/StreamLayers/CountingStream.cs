/// <summary>A write-only decorator that counts the bytes passing through it.</summary>
public class CountingStream(Stream inner, bool leaveOpen = false) : Stream
{
    public long BytesWritten { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        BytesWritten += count;
        inner.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        BytesWritten += buffer.Length;
        inner.Write(buffer);
    }

    public override void Flush() => inner.Flush();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen) inner.Dispose();
        base.Dispose(disposing);
    }
}
