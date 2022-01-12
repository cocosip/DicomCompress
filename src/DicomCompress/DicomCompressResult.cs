using System;
using System.Collections.Generic;
using System.Text;

namespace DicomCompress
{
    public class DicomCompressResultCollection : List<DicomCompressResult>
    {

        public override string ToString()
        {
            var sb = new StringBuilder();
            long totalSize = 0;
            foreach (var item in this)
            {
                totalSize += item.FileSize;
            }

            sb.AppendFormat("本次共处理文件 '{0}' 个,共计: {1} KB...\r\n", Count, Math.Round(totalSize * 1.0 / 1024, 1));
            foreach (var item in this)
            {
                sb.Append(item.ToString());
            }

            return sb.ToString();

        }
    }

    public class DicomCompressResult
    {

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

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
        /// 设备类型
        /// </summary>
        public string Modality { get; set; }

        public string BodyPart { get; set; }

        /// <summary>
        /// 传输语法
        /// </summary>
        /// <value></value>
        public string TransferSyntaxUID { get; set; }

        /// <summary>
        /// 传输语法名称
        /// </summary>
        public string TransferSyntaxName { get; set; }

        /// <summary>
        /// 是否为压缩类型
        /// </summary>
        public bool IsEncapsulated { get; set; }

        /// <summary>
        /// BitsStored
        /// </summary>
        /// <value></value>
        public string BitsStored { get; set; }

        /// <summary>
        /// BitsAllocated
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
            sb.AppendFormat("[压缩文件信息]\r\n");
            sb.AppendFormat("文件名:{0} \r\n", FileName);
            sb.AppendFormat("大小:{0} KB \r\n", Math.Round(FileSize * 1.0 / 1024, 1));
            sb.AppendFormat("是否压缩: {0} \r\n", IsEncapsulated);
            sb.AppendFormat("设备类型: {0} \r\n", Modality);
            sb.AppendFormat("部位: {0} \r\n", BodyPart);
            sb.AppendFormat("文件格式:{0}({1}) \r\n", TransferSyntaxName, TransferSyntaxUID);
            sb.AppendFormat("存储位: [{0}-{1}] \r\n", BitsStored, BitsAllocated);
            foreach (var item in Items)
            {
                sb.AppendFormat("{0}({1})", item.TransferSyntaxName, item.TransferSyntaxUID);
                if (!item.Success)
                {
                    sb.AppendFormat("| 失败 | - | - \r\n");
                }
                else
                {
                    var ratio = Math.Round(FileSize * 1.0 / item.FileSize, 1);

                    sb.AppendFormat("| 成功 | {0} | {1} KB \r\n", $"1:{ratio}", Math.Round(item.FileSize * 1.0 / 1024, 1));
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