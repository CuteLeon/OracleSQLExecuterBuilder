using System.Collections.Generic;
using System.IO;

namespace OracleSQLExecuterBuilder
{
    /// <summary>
    /// SQL脚本文件
    /// </summary>
    public class SQLFile
    {
        /// <summary>
        /// 数据库字典
        /// </summary>
        public static Dictionary<string, string> DataBases = new Dictionary<string, string>()
        {
            { "APP", "XIR_APP" },
            { "MD", "XIR_MD" },
            { "TRD", "XIR_TRD" },
            { "TRDEXH", "XIR_TRD_EXH" },
            { "TRDACC", "XIR_TRD_ACC" },
            { "TRDD", "XIR_TRD_D" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLFile"/> class.
        /// </summary>
        /// <param name="sqlRelationalPath"></param>
        public SQLFile(string sqlRelationalPath)
        {
            this.FileName = Path.GetFileName(sqlRelationalPath);
            this.Extention = Path.GetExtension(sqlRelationalPath).ToUpper();
            this.RelationalPath = sqlRelationalPath;
            this.RelationalDirectory = Path.GetDirectoryName(sqlRelationalPath);
            string[] elements = Path.GetFileName(sqlRelationalPath).Split(new[] { '_' }, 3);
            if (elements.Length == 3)
            {
                this.Index = int.TryParse(elements[0].ToUpper(), out int index) ? index : 0;
                this.DataBase = DataBases.TryGetValue(elements[1], out string database) ? database : "NONE";
            }
            else
            {
                this.Index = 0;
                this.DataBase = "NONE";
            }
        }

        /// <summary>
        /// Gets or sets 相对路径
        /// </summary>
        public string RelationalPath { get; protected set; }

        /// <summary>
        /// Gets or sets 相对目录
        /// </summary>
        public string RelationalDirectory { get; protected set; }

        /// <summary>
        /// Gets or sets 索引
        /// </summary>
        public int Index { get; protected set; }

        /// <summary>
        /// Gets or sets 数据库
        /// </summary>
        public string DataBase { get; protected set; }

        /// <summary>
        /// Gets or sets 文件名称
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// Gets or sets 扩展名
        /// </summary>
        public string Extention { get; protected set; }

        public override string ToString()
            => $"{this.RelationalDirectory} > {this.DataBase} > {this.Index}";
    }
}
