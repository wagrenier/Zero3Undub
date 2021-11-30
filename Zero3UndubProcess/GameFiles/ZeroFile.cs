namespace Zero3UndubProcess.GameFiles
{
    public class ZeroFile
    {
        public int Id { get; set; }
        public string Folder { get; set; }
        public string FileName { get; set; }
        public long Offset { get; init; }
        public bool IsCompressed { get; set; }
        public long CompressedSize { get; set; }
        public long DecompressedSize { get; set; }
    }
}