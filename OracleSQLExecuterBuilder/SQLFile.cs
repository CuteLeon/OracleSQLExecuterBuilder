using System;
using System.ComponentModel;
using System.IO;

namespace OracleSQLExecuterBuilder
{
    /// <summary>
    /// SQL脚本文件
    /// </summary>
    public class SQLFile
    {
        /// <summary>
        /// 数据库
        /// </summary>
        public enum Databases
        {
            /// <summary>
            /// None
            /// </summary>
            [AmbientValue("None")]
            None = 0,

            /// <summary>
            /// XIR_APP
            /// </summary>
            [AmbientValue("XIR_APP")]
            APP = 1,

            /// <summary>
            /// XIR_MD
            /// </summary>
            [AmbientValue("XIR_MD")]
            MD = 2,

            /// <summary>
            /// XIR_TRD
            /// </summary>
            [AmbientValue("XIR_TRD")]
            TRD = 3,

            /// <summary>
            /// XIR_TRD_EXH
            /// </summary>
            [AmbientValue("XIR_TRD_EXH")]
            TRDEXH = 4,

            /// <summary>
            /// XIR_TRD_ACC
            /// </summary>
            [AmbientValue("XIR_TRD_ACC")]
            TRDACC = 5,

            /// <summary>
            /// XIR_TRD_D
            /// </summary>
            [AmbientValue("XIR_TRD_D")]
            TRDD = 6,

            /// <summary>
            /// XIR_TRD_DFZQ
            /// </summary>
            [AmbientValue("XIR_TRD_DFZQ")]
            TRDDFZQ = 7,
        }

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
                this.DataBase = Enum.TryParse(elements[1], out Databases database) ? database : Databases.None;
            }
            else
            {
                this.Index = 0;
                this.DataBase = Databases.None;
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
        public Databases DataBase { get; protected set; }

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
