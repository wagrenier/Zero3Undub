namespace Zero3UndubProcess.GameFiles
{
    public sealed class Fhd
    {
        public long Size { get; set; }
        public int NumFiles { get; set; }
        public long DecompressedBlockOffset { get; set; }
        public long TypeBlockOffset { get; set; }
        public long SizeBlockOffset { get; set; }
        public long NameBlockOffset { get; set; }
        public long LbaBlockOffset { get; set; }
    }
}