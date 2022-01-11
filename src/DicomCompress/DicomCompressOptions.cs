using System.Collections.Generic;

namespace DicomCompress
{
    public class DicomCompressOptions
    {
        /// <summary>
        /// 压缩文件的地址
        /// </summary>
        public List<string> Files { get; set; }

        /// <summary>
        /// 输出的目录
        /// </summary>
        public string Output { get; set; }

        public DicomCompressOptions()
        {
            Files = new List<string>();
        }

    }
}
