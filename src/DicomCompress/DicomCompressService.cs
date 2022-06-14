using FellowOakDicom;
using FellowOakDicom.Imaging.Codec;
using Kayisoft.Abp.Dicom.Transcoder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DicomCompress
{
    public class DicomCompressService : ITransientDependency
    {
        protected ILogger Logger { get; }
        protected DicomCompressOptions Options { get; }
        protected IDicomFileTranscoder DicomFileTranscoder { get; }

        public DicomCompressService(
            ILogger<DicomCompressService> logger,
            IOptions<DicomCompressOptions> options,
            IDicomFileTranscoder dicomFileTranscoder)
        {
            Logger = logger;
            Options = options.Value;
            DicomFileTranscoder = dicomFileTranscoder;
        }

        public virtual async Task CompressAsync()
        {
            var results = new DicomCompressResultCollection();
            if (!Directory.Exists(Options.Input))
            {
                Logger.LogInformation("输入目录:{Input} 不存在", Options.Input);
                return;
            }


            var files = Directory.GetFiles(Options.Input);
            if (files.Length == 0)
            {
                Logger.LogInformation("文件下不存在任何DICOM文件");
                return;
            }

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                Logger.LogInformation("开始对图像 {Name} 进行压缩...", fileInfo.Name);

                var dicomFile = await DicomFile.OpenAsync(file);
                var sopInstanceUid = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "00");

                var result = new DicomCompressResult()
                {
                    FileName = fileInfo.Name,
                    FilePath = file,
                    FileSize = fileInfo.Length,
                    SOPInstanceUID = sopInstanceUid,
                    IsEncapsulated = dicomFile.FileMetaInfo.TransferSyntax.IsEncapsulated,
                    Modality = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.Modality, ""),
                    BodyPart = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BodyPartThickness, ""),
                    TransferSyntaxUID = dicomFile.FileMetaInfo.TransferSyntax.UID.UID,
                    TransferSyntaxName = dicomFile.FileMetaInfo.TransferSyntax.UID.Name,
                    BitsStored = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsStored, ""),
                    BitsAllocated = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, ""),
                };
                results.Add(result);
                var inputSyntax = dicomFile.FileMetaInfo.TransferSyntax;
                var prefixName = GetPrefixName(fileInfo.Name);

                foreach (var (name, syntax) in GetTransferSyntaxes())
                {
                    try
                    {
                        Logger.LogInformation("对图像进行 {name}({UID}) 压缩...", name, syntax.UID.UID);
                        var dicomTranscoder = new DicomTranscoder(inputSyntax, syntax);
                        var newDicomFile = dicomTranscoder.Transcode(dicomFile);

                        var newBitsStored = newDicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsStored, "");
                        var newBitsAllocated = newDicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, "");

                        var fileName = $"{prefixName}-{name}.dcm";
                        var filePath = Path.Combine(Options.Output, fileName);
                        await newDicomFile.SaveAsync(filePath);
                        var newFileInfo = new FileInfo(filePath);

                        var isReduction = IsReduction(syntax);

                        var item = new DicomCompressResult.DicomCompressResultItem()
                        {
                            Success = true,
                            FileName = fileName,
                            FilePath = filePath,
                            FileSize = newFileInfo.Length,
                            TransferSyntaxName = name,
                            TransferSyntaxUID = syntax.UID.UID,
                            BitsStored = newBitsStored,
                            BitsAllocated = newBitsAllocated,
                            IsReduction = isReduction
                        };
                        result.AddItem(item);

                    }
                    catch (Exception ex)
                    {
                        result.AddItem(new DicomCompressResult.DicomCompressResultItem()
                        {
                            Success = false,
                            TransferSyntaxName = name,
                            TransferSyntaxUID = syntax.UID.UID
                        });
                        Logger.LogError(ex, "图像格式: {name} 压缩失败,异常信息:{Message}.", name, ex.Message);
                    }
                }
                Logger.LogInformation("完成图像的压缩");
            }


            var logFile = Path.Combine(Options.Output, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");

            File.WriteAllText(logFile, results.ToString());
        }

        protected virtual string GetPrefixName(string fileName)
        {
            if (fileName.Contains("."))
            {
                return fileName.Substring(0, fileName.LastIndexOf("."));

            }
            return fileName;
        }

        protected virtual IEnumerable<(string, DicomTransferSyntax)> GetTransferSyntaxes()
        {
            //yield return (nameof(DicomTransferSyntax.JPEG2000Lossless), DicomTransferSyntax.JPEG2000Lossless);
            // yield return (nameof(DicomTransferSyntax.JPEGProcess14), DicomTransferSyntax.JPEGProcess14);
            yield return (nameof(DicomTransferSyntax.JPEGProcess14SV1), DicomTransferSyntax.JPEGProcess14SV1);
            // yield return (nameof(DicomTransferSyntax.JPEGLSLossless), DicomTransferSyntax.JPEGLSLossless);
            // yield return (nameof(DicomTransferSyntax.RLELossless), DicomTransferSyntax.RLELossless);

            //yield return (nameof(DicomTransferSyntax.JPEG2000Lossy), DicomTransferSyntax.JPEG2000Lossy);
            //yield return (nameof(DicomTransferSyntax.JPEGProcess1), DicomTransferSyntax.JPEGProcess1);
            //yield return (nameof(DicomTransferSyntax.JPEGProcess2_4), DicomTransferSyntax.JPEGProcess2_4);
            //yield return (nameof(DicomTransferSyntax.JPEGLSNearLossless), DicomTransferSyntax.JPEGLSNearLossless);

            //还原的格式
            //yield return (nameof(DicomTransferSyntax.ExplicitVRLittleEndian), DicomTransferSyntax.ExplicitVRLittleEndian);
            //yield return (nameof(DicomTransferSyntax.ExplicitVRBigEndian), DicomTransferSyntax.ExplicitVRBigEndian);
            yield break;
        }


        private bool IsReduction(DicomTransferSyntax transferSyntax)
        {
            if (transferSyntax == DicomTransferSyntax.ExplicitVRBigEndian || transferSyntax == DicomTransferSyntax.ExplicitVRLittleEndian || transferSyntax == DicomTransferSyntax.ImplicitVRBigEndian || transferSyntax == DicomTransferSyntax.ImplicitVRLittleEndian || transferSyntax == DicomTransferSyntax.DeflatedExplicitVRLittleEndian)
            {
                return true;
            }
            return false;
        }




    }
}
