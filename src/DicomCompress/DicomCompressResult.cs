using System;
using System.Collections.Generic;
using System.Text;

namespace DicomCompress
{
    public class DicomCompressResult
    {
        /// <summary>
        /// 图像存储地址
        /// </summary>
        /// <value></value>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        /// <value></value>
        public long FileSize { get; set; }

        /// <summary>
        /// SOP InstanceUID
        /// </summary>
        /// <value></value>
        public string SOPInstanceUID { get; set; }

        /// <summary>
        /// 传输语法
        /// </summary>
        /// <value></value>
        public string TransferSyntaxUID { get; set; }

        /// <summary>
        /// BitsStored
        /// </summary>
        /// <value></value>
        public string BitsStored { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string BitsAllocated { get; set; }

        public List<DicomCompressResultItem> Items { get; set; }
        public DicomCompressResult()
        {
            Items = new List<DicomCompressResultItem>();
        }

        public void AddItem(DicomCompressResultItem item)
        {
            Items.Add(item);
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("-------压缩图像 {0}------\r\n", FilePath);
            sb.AppendFormat("图像大小:{0} KB \r\n", Math.Round(FileSize * 1.0 / 1024, 1));
            sb.AppendFormat("SOPInstanceUID: {0} \r\n", SOPInstanceUID);
            sb.AppendFormat("原始文件格式:{0} \r\n", TransferSyntaxUID);
            sb.AppendFormat("BitsStored:{0} \r\n", BitsStored);
            sb.AppendFormat("BitsAllocated:{0} \r\n", BitsAllocated);
            sb.AppendLine($"--------------------------");

            foreach (var item in Items)
            {
                sb.AppendFormat("{0}({1})", item.TransferSyntaxName, item.TransferSyntaxUID);
                if (!item.Success)
                {
                    sb.AppendFormat("|失败|-|-|- \r\n");
                }
                else
                {
                    sb.AppendFormat("|成功|{0}|{1}|{2} KB \r\n", item.BitsStored, item.BitsAllocated, Math.Round(item.FileSize * 1.0 / 1024, 1));
                }
            }
            sb.AppendLine($"--------------------------");

            return sb.ToString();

        }

        public class DicomCompressResultItem
        {
            public bool Success { get; set; }

            /// <summary>
            /// 文件名
            /// </summary>
            /// <value></value>
            public string FileName { get; set; }

            /// <summary>
            /// 文件地址
            /// </summary>
            /// <value></value>
            public string FilePath { get; set; }

            /// <summary>
            /// 传输语法名称
            /// </summary>
            /// <value></value>
            public string TransferSyntaxName { get; set; }

            /// <summary>
            /// 传输语法
            /// </summary>
            /// <value></value>
            public string TransferSyntaxUID { get; set; }

            /// <summary>
            /// BitsStored
            /// </summary>
            /// <value></value>
            public string BitsStored { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <value></value>
            public string BitsAllocated { get; set; }

            /// <summary>
            /// 文件大小
            /// </summary>
            /// <value></value>
            public long FileSize { get; set; }




        }

    }
}