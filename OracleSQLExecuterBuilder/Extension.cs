using System.ComponentModel;
using System.Reflection;

namespace OracleSQLExecuterBuilder
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 获取环境值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetAmbientValue(this object value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            AmbientValueAttribute[] attributes = (AmbientValueAttribute[])fieldInfo.GetCustomAttributes(typeof(AmbientValueAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Value.ToString() : string.Empty;
        }
    }
}
