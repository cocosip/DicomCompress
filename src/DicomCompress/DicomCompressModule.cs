using Kayisoft.Abp.Dicom.Transcoder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.IO;
using Volo.Abp.Modularity;

namespace DicomCompress
{

    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpDicomTranscoderModule)
    )]
    public class DicomCompressModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();

            context.Services.AddHostedService<DicomCompressHostedService>();

            Configure<DicomCompressOptions>(configuration.GetSection("DicomCompressOptions"));
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var options = context.ServiceProvider.GetService<IOptions<DicomCompressOptions>>().Value;
            //创建输出目录
            DirectoryHelper.CreateIfNotExists(options.Output);
        }
    }
}
