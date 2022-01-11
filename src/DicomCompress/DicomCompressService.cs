using FellowOakDicom;
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
            var results = new List<DicomCompressResult>();

            foreach (var file in Options.Files)
            {
                if (!File.Exists(file))
                {
                    Logger.LogInformation("文件:{file} 不存在.", file);
                    return;
                }
                Logger.LogInformation("开始对图像{file} 进行压缩...", file);

                var dicomFile = await DicomFile.OpenAsync(file);
                dicomFile.Dataset.NotValidated();
                var sopInstanceUid = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "00");

                var sourceFile = new FileInfo(file);
                var result = new DicomCompressResult()
                {
                    FilePath = file,
                    FileSize = sourceFile.Length,
                    SOPInstanceUID = sopInstanceUid,
                    TransferSyntaxUID = dicomFile.FileMetaInfo.GetSingleValueOrDefault(DicomTag.TransferSyntaxUID, ""),
                    BitsStored = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsStored, ""),
                    BitsAllocated = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, ""),
                };
                results.Add(result);

                foreach (var (name, syntax) in GetTransferSyntaxes())
                {
                    try
                    {
                        Logger.LogInformation("对图像进行 {name}({UID}) 压缩...", name, syntax.UID.UID);
                        var newDicomFile = DicomFileTranscoder.Transcode(dicomFile, syntax);

                        var newBitsStored = newDicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsStored, "");
                        var newBitsAllocated = newDicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, "");


                        var fileName = $"{sopInstanceUid}-{name}.dcm";
                        var filePath = Path.Combine(Options.Output, fileName);
                        await newDicomFile.SaveAsync(filePath);
                        var newFileInfo = new FileInfo(filePath);

                        var item = new DicomCompressResult.DicomCompressResultItem()
                        {
                            Success = true,
                            FileName = fileName,
                            FilePath = filePath,
                            FileSize = newFileInfo.Length,
                            TransferSyntaxName = name,
                            TransferSyntaxUID = syntax.UID.UID,
                            BitsStored = newBitsStored,
                            BitsAllocated = newBitsAllocated
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

            foreach (var result in results)
            {
                Logger.LogInformation(result.ToString());
            }


        }

        protected virtual IEnumerable<(string, DicomTransferSyntax)> GetTransferSyntaxes()
        {
            yield return (nameof(DicomTransferSyntax.JPEG2000Lossless), DicomTransferSyntax.JPEG2000Lossless);
            yield return (nameof(DicomTransferSyntax.JPEG2000Lossy), DicomTransferSyntax.JPEG2000Lossy);
            yield return (nameof(DicomTransferSyntax.JPEGProcess1), DicomTransferSyntax.JPEGProcess1);
            yield return (nameof(DicomTransferSyntax.JPEGProcess2_4), DicomTransferSyntax.JPEGProcess2_4);
            yield return (nameof(DicomTransferSyntax.JPEGProcess14), DicomTransferSyntax.JPEGProcess14);
            yield return (nameof(DicomTransferSyntax.JPEGProcess14SV1), DicomTransferSyntax.JPEGProcess14SV1);
            yield return (nameof(DicomTransferSyntax.JPEGLSLossless), DicomTransferSyntax.JPEGLSLossless);
            yield return (nameof(DicomTransferSyntax.JPEGLSNearLossless), DicomTransferSyntax.JPEGLSNearLossless);
            yield return (nameof(DicomTransferSyntax.RLELossless), DicomTransferSyntax.RLELossless);
            yield break;
        }


    }
}
