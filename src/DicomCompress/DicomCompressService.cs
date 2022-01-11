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
            if (!File.Exists(Options.FilePath))
            {
                Logger.LogInformation("文件:{FilePath} 不存在.", Options.FilePath);
                return;
            }
            Logger.LogInformation("开始进行图像的压缩...");

            var dicomFile = await DicomFile.OpenAsync(Options.FilePath);
            var sopInstanceUid = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "00");

            Logger.LogInformation("---------原始图像信息--------");
            Logger.LogInformation("TransferSyntaxUID {TransferSyntaxUID}", dicomFile.FileMetaInfo.GetSingleValueOrDefault(DicomTag.TransferSyntaxUID, ""));
            Logger.LogInformation("BitsStored {BitsStored}", dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsStored, ""));
            Logger.LogInformation("BitsAllocated {BitsAllocated}", dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, ""));
            Logger.LogInformation("---------原始图像信息--------");                                


            foreach (var (name, syntax) in GetTransferSyntaxes())
            {
                try
                {
                    Logger.LogInformation("对图像进行 {name}({UID}) 压缩...", name, syntax.UID.UID);
                    var newDicomFile = DicomFileTranscoder.Transcode(dicomFile, syntax);
                    var fileName = $"{sopInstanceUid}-{name}.dcm";
                    var filePath = Path.Combine(Options.Output, fileName);
                    await newDicomFile.SaveAsync(filePath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "图像格式: {name} 压缩失败,异常信息:{Message}.", name, ex.Message);
                }
            }
            Logger.LogInformation("完成图像的压缩");

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
