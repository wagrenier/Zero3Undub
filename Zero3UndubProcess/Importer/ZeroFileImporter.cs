using System;
using Zero3UndubProcess.GameFiles;
using Zero3UndubProcess.Iso;
using Zero3UndubProcess.Reporter;

namespace Zero3UndubProcess.Importer
{
    public sealed class ZeroFileImporter
    {
        public InfoReporter InfoReporterUi { get; private set; }
        private readonly IsoHandler _isoHandler;

        public ZeroFileImporter(string originFile, string targetFile)
        {
            _isoHandler = new IsoHandler(originFile, targetFile);
            
            InfoReporterUi = new InfoReporter
            {
                 IsCompleted = false,
                 IsSuccess = false,
                 TotalFiles = _isoHandler.IsoRegionHandler.TargetRegionInfo.NumberFiles,
                 FilesCompleted = 0
            };
        }

        public void RestoreGame()
        {
            try
            {
                for (var i = 0; i < _isoHandler.IsoRegionHandler.TargetRegionInfo.NumberFiles; i++)
                {
                    InfoReporterUi.FilesCompleted += 1;
                    var targetFile = _isoHandler.TargetGetFile(i);
                    var originFile = _isoHandler.OriginGetFile(i);

                    // Check for splash screen logo
                    if (targetFile.Id == 2)
                    {
                        _isoHandler.OverwriteSplashScreen(originFile, targetFile);
                    }

                    if (!targetFile.FileName.EndsWith("PSS") || targetFile.FileName.EndsWith("str"))
                    {
                        continue;
                    }

                    if (originFile.CompressedSize <= targetFile.CompressedSize)
                    {
                        _isoHandler.WriteNewFile(originFile, targetFile);

                        if (originFile.Type != FileType.AUDIO)
                        {
                            continue;
                        }
                        
                        HandleAudioFile(originFile, targetFile);
                    } 
                    else if (targetFile.Type == FileType.AUDIO)
                    {
                        HandleAudioFile(originFile, targetFile);
                        _isoHandler.AppendFile(originFile, targetFile);
                    }
                    else if (targetFile.Type == FileType.VIDEO)
                    {
                        _isoHandler.VideoAudioSwitch(originFile, targetFile);
                    }
                }

                InfoReporterUi.IsSuccess = true;
            }
            catch (Exception e)
            {
                InfoReporterUi.ErrorMessage = e.Message;
                InfoReporterUi.IsSuccess = false;
            }

            InfoReporterUi.IsCompleted = true;
            CloseFiles();
        }

        private void HandleAudioFile(ZeroFile origin, ZeroFile target)
        {
            var originHeaderFile = _isoHandler.OriginGetFile(origin.Id - 1);
            var targetHeaderFile = _isoHandler.TargetGetFile(target.Id - 1);
            _isoHandler.WriteNewFile(originHeaderFile, targetHeaderFile);
        }
        
        private void CloseFiles()
        {
            _isoHandler.Close();
        }
    }
}