using System.IO;
using System.Text;
using Zero3UndubProcess.Constants;
using Zero3UndubProcess.GameFiles;

namespace Zero3UndubProcess.Iso
{
    public sealed class IsoReader
    {
        private readonly BinaryReader _reader;
        private readonly RegionInfo _regionInfo;
        private Fhd _fhd;

        public IsoReader(FileSystemInfo isoFile, RegionInfo regionInfo)
        {
            _reader = new BinaryReader(File.OpenRead(isoFile.FullName));
            _regionInfo = regionInfo;
            ReadFhdInfo();
        }

        public void Close()
        {
            _reader.Close();
        }

        public byte[] ExtractFileContent(ZeroFile zeroFile)
        {
            SeekFile(zeroFile);
            return _reader.ReadBytes(zeroFile.IsCompressed ? (int)zeroFile.CompressedSize : (int)zeroFile.DecompressedSize);
        }

        public ZeroFile ExtractFileInfo(int fileId)
        {
            var lba = GetZero3FileLba(fileId);
            var filePath = GetZero3FileName(fileId);
            var size = GetZero3FileSize(fileId);
            var isCompressed = (size & 1) == 1;
            size >>= 1;
                
            return new ZeroFile
            {
                Id = fileId,
                Offset = lba,
                IsCompressed = isCompressed,
                CompressedSize = size,
                DecompressedSize = GetZero3FileDecompressedSize(fileId),
                Folder = filePath[0],
                FileName = filePath[1]
            };
        }
        
        private void ReadFhdInfo()
        {
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var sig = _reader.ReadUInt32();
            
            //if (sig != 0x46484400)
            //{
            //    Console.WriteLine("This file is not FHD");
            //    return;
            //}
            
            _reader.BaseStream.Seek(8, SeekOrigin.Begin);

            _fhd = new Fhd
            {
                Size = _reader.ReadUInt32(),
                NumFiles = _reader.ReadInt32(),
                DecompressedBlockOffset = _reader.ReadUInt32(),
                TypeBlockOffset = _reader.ReadUInt32(),
                SizeBlockOffset = _reader.ReadUInt32(),
                NameBlockOffset = _reader.ReadUInt32(),
                LbaBlockOffset = _reader.ReadUInt32()
            };
        }
        
        private string[] GetZero3FileName(int fileId)
        {
            var fileNameOffset = _fhd.NameBlockOffset + fileId * 8;
            _reader.BaseStream.Seek(fileNameOffset, SeekOrigin.Begin);
            
            _reader.BaseStream.Seek(fileNameOffset, SeekOrigin.Begin);
            
            //Get folder name address
            long destFolderNameOffset = _reader.ReadUInt32();
            
            //Get the address of the subfile name
            long destFileNameOffset = _reader.ReadUInt32();
            
            _reader.BaseStream.Seek(destFolderNameOffset, SeekOrigin.Begin);
                
            var destFolderName = Encoding.ASCII.GetString(_reader.ReadBytes(64)).Split('\0')[0];
            _reader.BaseStream.Seek(destFileNameOffset, SeekOrigin.Begin);
            var destFileName = Encoding.ASCII.GetString(_reader.ReadBytes(64)).Split('\0')[0];

            return new string[]{destFolderName, destFileName};
        }
        
        private long GetZero3FileSize(int fileId)
        {
            var sizeOffset = _fhd.SizeBlockOffset + fileId * 4;
            _reader.BaseStream.Seek(sizeOffset, SeekOrigin.Begin);
            
            return _reader.ReadUInt32();
        }
        
        private long GetZero3FileDecompressedSize(int fileId)
        {
            var sizeDecompressedOffset = _fhd.DecompressedBlockOffset + fileId * 4;
            _reader.BaseStream.Seek(sizeDecompressedOffset, SeekOrigin.Begin);
            
            return _reader.ReadUInt32();
        }

        private long GetZero3FileLba(int fileId)
        {
            var lbaOffset = _fhd.LbaBlockOffset + fileId * 4;
            _reader.BaseStream.Seek(lbaOffset, SeekOrigin.Begin);
            
            return _reader.ReadUInt32() * 0x800;
        }

        private void SeekFile(ZeroFile zeroFile)
        {
            _reader.BaseStream.Seek(_regionInfo.FileArchiveStartAddress, SeekOrigin.Begin);
            _reader.BaseStream.Seek(zeroFile.Offset, SeekOrigin.Current);
        }
    }
}