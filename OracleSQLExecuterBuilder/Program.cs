using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OracleSQLExecuterBuilder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            int currentDirLength = currentDir.Length;
            Console.WriteLine($"扫描目录：{currentDir}");

            var files = Directory.GetFiles(currentDir, "*", SearchOption.AllDirectories)
                .Select(sqlfile => new SQLFile(sqlfile.Substring(currentDirLength)))
                .Where(sqlfile => !string.IsNullOrEmpty(sqlfile.RelationalDirectory))
                .ToList();

            var sqlFiles = files
                .Where(sqlFile =>
                        sqlFile.Extention == ".SQL" ||
                        sqlFile.Extention == ".PCK" ||
                        sqlFile.Extention == ".PRC" ||
                        sqlFile.Extention == ".SPC" ||
                        sqlFile.Extention == ".FNC" ||
                        sqlFile.Extention == ".TRG")
                .OrderBy(sqlfile => sqlfile, new SQLFileComparer())
                .ToList();

            Console.WriteLine($"SQL文件：{sqlFiles.Count} 个");

            string executer = BuildSQLExecuter(sqlFiles);

            string executerPath = Path.Combine(currentDir, $"SQLExecuter_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.sql");
            Console.WriteLine($"正在导出到文件：{executerPath}");
            File.WriteAllText(executerPath, executer, Encoding.Default);

            Console.WriteLine("导出完成，输入任意键结束程序...");

            var predicate = new Func<SQLFile, bool>(sqlfile =>
                sqlfile.DataBase == SQLFile.Databases.None ||
                sqlfile.FileName.Contains(" ") ||
                sqlfile.FileName.ToUpper().EndsWith(".TXT")
            );
            var noneSQLFile = files.Where(sqlfile => predicate(sqlfile)).ToList();
            if (noneSQLFile.Count > 0)
            {
                Console.Write($"\n小提示：\n发现 {noneSQLFile.Count} 个名称不符合规范的SQL文件：\n\t{string.Join("\n\t", noneSQLFile.Select(sql => sql.RelationalPath))}");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 创建SQL执行器
        /// </summary>
        /// <param name="sqlFiles"></param>
        /// <returns></returns>
        public static string BuildSQLExecuter(IEnumerable<SQLFile> sqlFiles)
        {
            StringBuilder executerBuilder = new StringBuilder();
            executerBuilder.AppendLine($"spool ./_Update_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log");
            executerBuilder.AppendLine();
            executerBuilder.AppendLine("--登录密码");
            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.APP))
            {
                executerBuilder.AppendLine("define xir_app_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.MD))
            {
                executerBuilder.AppendLine("define xir_md_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.TRD))
            {
                executerBuilder.AppendLine("define xir_trd_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.TRDEXH))
            {
                executerBuilder.AppendLine("define xir_trdexh_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.TRDACC))
            {
                executerBuilder.AppendLine("define xir_trdacc_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.TRDD))
            {
                executerBuilder.AppendLine("define xir_trdd_pwd = ''");
            }

            if (sqlFiles.Any(sqlfile => sqlfile.DataBase == SQLFile.Databases.TRDDFZQ))
            {
                executerBuilder.AppendLine("define xir_trddfzq_pwd = ''");
            }

            executerBuilder.AppendLine();
            executerBuilder.AppendLine("set feedback off");
            executerBuilder.AppendLine("set define off");
            executerBuilder.AppendLine("SET SQLBLANKLINES ON");
            executerBuilder.AppendLine("WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;");
            executerBuilder.AppendLine();

            List<SQLFile> sqlFilesOfDatabase = null;

            foreach (Tuple<SQLFile.Databases, string> database in new[]
                {
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.APP, "xir_app_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.MD, "xir_md_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.TRD, "xir_trd_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.TRDEXH, "xir_trdexh_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.TRDACC, "xir_trdacc_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.TRDD, "xir_trdd_pwd"),
                    new Tuple<SQLFile.Databases,string> (SQLFile.Databases.TRDDFZQ, "xir_trddfzq_pwd"),
            })
            {
                sqlFilesOfDatabase = sqlFiles.Where(sqlFile => sqlFile.DataBase == database.Item1).ToList();
                if (sqlFilesOfDatabase.Count > 0)
                {
                    string dbName = database.Item1.GetAmbientValue();
                    Console.WriteLine($"正在写入 {dbName} 数据库的 {sqlFilesOfDatabase.Count} 个脚本文件...");
                    AppendPrompt($"登录数据库： {dbName}");
                    AppendConnectCommand(dbName, database.Item2);
                    executerBuilder.AppendLine($"-- database: {dbName}");
                    AppendPrompt($"执行数据库脚本： {dbName}");
                    sqlFilesOfDatabase.ForEach(sql => AppendSQLFileCommand(sql));
                    AppendGrantCommand();
                    AppendRebuildIndex();

                    if (database.Item1 == SQLFile.Databases.APP)
                    {
                        AppendUpgradeFinishTime();
                    }

                    executerBuilder.AppendLine();
                }
            }

            executerBuilder.AppendLine("set feedback on");
            executerBuilder.AppendLine("set define on");
            executerBuilder.AppendLine("SET SQLBLANKLINES OFF");
            executerBuilder.AppendLine("spool off ");

            void AppendPrompt(string prompt)
            {
                executerBuilder.AppendLine($"prompt {prompt}");
            }

            void AppendConnectCommand(string database, string password)
            {
                executerBuilder.AppendLine("set define on");
                executerBuilder.AppendLine($"connect {database}/&{password}@&_CONNECT_IDENTIFIER");
                executerBuilder.AppendLine("set define off");
                executerBuilder.AppendLine();
            }

            void AppendSQLFileCommand(SQLFile sqlFile)
            {
                executerBuilder.AppendLine("prompt");
                executerBuilder.AppendLine($"prompt {sqlFile.DataBase} => {sqlFile.RelationalPath}");
                executerBuilder.AppendLine("prompt ==========================================");
                executerBuilder.AppendLine("prompt");
                executerBuilder.AppendLine($"@@{sqlFile.RelationalPath}");
                executerBuilder.AppendLine();
            }

            void AppendGrantCommand()
            {
                executerBuilder.Append(@"prompt
prompt 数据表授权
prompt ====================================
prompt
DECLARE
  V_SQL VARCHAR2(1000);
BEGIN
  FOR OBJ_INFO IN (SELECT T1.IS_TABLE, T1.OWNER, T1.OBJ_NAME, T2.USER_NAME
                   FROM   (SELECT 1 AS IS_TABLE, USER AS OWNER,
                                   TABLE_NAME AS OBJ_NAME
                            FROM   USER_TABLES
                            UNION ALL
                            SELECT 0 AS IS_TABLE, USER AS OWNER,
                                   VIEW_NAME AS OBJ_NAME
                            FROM   USER_VIEWS) T1,
                          (SELECT 'XIR_APP' AS USER_NAME
                            FROM   DUAL
                            UNION ALL
                            SELECT 'XIR_TRD' AS USER_NAME
                            FROM   DUAL
                            UNION ALL
                            SELECT 'XIR_MD' AS USER_NAME
                            FROM   DUAL
                            UNION ALL
                            SELECT 'XIR_TRD_EXH' AS USER_NAME
                            FROM   DUAL
                            UNION ALL
                            SELECT 'XIR_TRD_D' AS USER_NAME
                            FROM   DUAL) T2
                   WHERE  T1.OWNER <> T2.USER_NAME AND
                          (T1.OBJ_NAME, T2.USER_NAME, T1.OWNER) NOT IN
                          (SELECT TABLE_NAME, GRANTEE, OWNER
                           FROM   USER_TAB_PRIVS)
                   ORDER  BY T1.IS_TABLE DESC, T1.OBJ_NAME, T2.USER_NAME) LOOP
  
    SELECT 'GRANT SELECT' ||
            DECODE(OBJ_INFO.IS_TABLE, 1,
                   ', INSERT, UPDATE, DELETE, INDEX, ALTER ', ' ') || ' ON ' ||
            OBJ_INFO.OWNER || '.' || OBJ_INFO.OBJ_NAME || ' TO ' ||
            OBJ_INFO.USER_NAME || ' WITH GRANT OPTION'
    INTO   V_SQL
    FROM   DUAL;
  
    BEGIN
      EXECUTE IMMEDIATE V_SQL;
    EXCEPTION
      WHEN OTHERS THEN
        ---用户或角色不存在，不打印错误信息
        IF SQLCODE NOT IN (-1917, -1918, -1919) THEN
          DBMS_OUTPUT.PUT_LINE(V_SQL || ';');
        END IF;
    END;
  END LOOP;
END;
/");
                executerBuilder.AppendLine();
            }

            void AppendRebuildIndex()
            {
                executerBuilder.Append(@"prompt
prompt 重建索引
prompt ====================================
prompt
declare
  cursor v_index_list is
    SELECT 'alter Index ' || INDEX_NAME || ' REBUILD PARTITION ' ||
           PARTITION_NAME as rebuild_sql
      FROM USER_IND_PARTITIONS
     WHERE status = 'UNUSABLE'
    union all
    select 'alter index ' || Index_name || ' rebuild online' as rebuild_sql
      from User_indexes
     where status = 'UNUSABLE'
       and partitioned = 'NO';
  v_index v_index_list%rowtype;
begin
  open v_index_list;
  Loop
    fetch v_index_list
      into v_index;
    exit when v_index_list%notfound;
    execute immediate v_index.rebuild_sql;
  end loop;
  close v_index_list;
end;
/");
                executerBuilder.AppendLine();
            }

            void AppendUpgradeFinishTime()
            {
                executerBuilder.Append(@"prompt
prompt 表TSYS_PRODUCT_INFO更新升级完成时间
prompt ==================================================
prompt
DECLARE
    V_COUNT INTEGER;
BEGIN
    -- 如果存在 FINISH_TIME 列，则更新升级完成时间
    SELECT COUNT(1)
    INTO V_COUNT
    FROM USER_TAB_COLUMNS
    WHERE TABLE_NAME = 'TSYS_PRODUCT_INFO' AND
        COLUMN_NAME = 'FINISH_TIME';
    IF V_COUNT = 1 THEN
        EXECUTE IMMEDIATE 'update TSYS_PRODUCT_INFO
set finish_Time = sysdate
where upgrade_ID >=
      (select upgrade_ID
       from (select row_number() over(order by pversion, upgrade_id desc) as rn,
                     upgrade_id
              from TSYS_PRODUCT_INFO
              where upgrade_ID > (select max(upgrade_ID) as prior_upgrade_ID
                                  from TSYS_PRODUCT_INFO
                                  where finish_time is not null))
       where rn = 1)';
        COMMIT;
    END IF;
END;
/");
                executerBuilder.AppendLine();
            }

            Console.WriteLine("脚本执行器生成完成。");
            return executerBuilder.ToString();
        }
    }
}
