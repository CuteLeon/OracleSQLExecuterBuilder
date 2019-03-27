using System;
using System.Collections.Generic;

namespace OracleSQLExecuterBuilder
{
    /// <summary>
    /// SQLFile 比较器
    /// </summary>
    public class SQLFileComparer : IComparer<SQLFile>
    {
        /// <summary>
        /// 当前对象更大
        /// </summary>
        private const int CurrentIsBigger = 1;

        /// <summary>
        /// 相等
        /// </summary>
        private const int AreEqual = 0;

        /// <summary>
        /// 目标对象更大
        /// </summary>
        private const int TargetIsBigger = -1;

        /// <summary>
        /// 比较 SQLFile
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int Compare(SQLFile current, SQLFile target)
        {
            /* SQLFile 排序优先级：APP>MD>TRD>TRD_???>子目录[.子目录]*>根目录>脚本索引序号
             */
            if (current == null)
            {
                return target == null ? AreEqual : TargetIsBigger;
            }

            if (target == null)
            {
                return CurrentIsBigger;
            }

            int result = AreEqual;

            // 按数据库比较
            result = current.DataBase.CompareTo(target.DataBase);
            if (result == AreEqual)
            {
                // 判断是否相同目录
                if (current.RelationalDirectory.Equals(target.RelationalDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    // 按文件索引比较
                    result = current.Index.CompareTo(target.Index);
                }
                else
                {
                    // 按所在目录比较
                    if (string.IsNullOrEmpty(current.RelationalDirectory))
                    {
                        result = CurrentIsBigger;
                    }
                    else if (current.RelationalDirectory.StartsWith(target.RelationalDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        result = TargetIsBigger;
                    }
                    else if (target.RelationalDirectory.StartsWith(current.RelationalDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        result = CurrentIsBigger;
                    }
                    else
                    {
                        result = string.Compare(current.RelationalDirectory, target.RelationalDirectory, true);
                    }
                }
            }

            return result;
        }
    }
}
