using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using VisualNovelEngine.Model.Logging;
using System.Xml.Linq;
using System.Xml;

namespace VisualNovelEngine.Model.Page.PageEdit
{
    /// <summary>
    /// JSON文件写入工具类
    /// 提供页面JSON文件的读写、创建和修改功能
    /// </summary>
    public static class WritePageJson
    {
        #region 常量
        /// <summary>
        /// JSON文件扩展名
        /// </summary>
        private const string JsonExtension = ".json";

        /// <summary>
        /// 默认JSON缩进格式
        /// </summary>
        private static readonly Newtonsoft.Json.Formatting DefaultFormatting = Newtonsoft.Json.Formatting.Indented;
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置JSON文件中的键值对
        /// </summary>
        /// <param name="filePath">JSON文件路径</param>
        /// <param name="key">要设置的键</param>
        /// <param name="value">要设置的值</param>
        /// <exception cref="ArgumentNullException">文件路径、键或值为null时抛出</exception>
        /// <exception cref="FileNotFoundException">指定的文件不存在时抛出</exception>
        /// <exception cref="JsonException">JSON解析错误时抛出</exception>
        public static void SetValue(string filePath, string key, object value)
        {
            ValidateSetValueParameters(filePath, key, value);

            try
            {
                var jsonObject = ReadJsonFile(filePath);
                jsonObject[key] = JToken.FromObject(value);
                WriteJsonFile(filePath, jsonObject);

                Logger.Log($"已更新JSON文件: {Path.GetFileName(filePath)}, 键: {key}");
            }
            catch (JsonException ex)
            {
                Logger.LogError($"JSON解析错误: {filePath}", ex);
                throw;
            }
            catch (IOException ex)
            {
                Logger.LogError($"文件IO错误: {filePath}", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError($"设置JSON值时发生错误: {filePath}, 键: {key}", ex);
                throw;
            }
        }

        /// <summary>
        /// 从JSON文件中获取指定键的值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="filePath">JSON文件路径</param>
        /// <param name="key">要获取的键</param>
        /// <param name="defaultValue">如果键不存在时返回的默认值</param>
        /// <returns>键对应的值，如果键不存在则返回默认值</returns>
        public static T GetValue<T>(string filePath, string key, T defaultValue = default)
        {
            if (!ValidateGetValueParameters(filePath, key))
            {
                return defaultValue;
            }

            try
            {
                var jsonObject = ReadJsonFile(filePath);
                var token = jsonObject[key];

                return token != null ? token.ToObject<T>() : defaultValue;
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取JSON值时发生错误: {filePath}, 键: {key}", ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 创建新的JSON文件
        /// </summary>
        /// <param name="filePath">要创建的JSON文件路径</param>
        /// <param name="initialData">初始数据对象</param>
        /// <returns>是否成功创建文件</returns>
        public static bool CreateJsonFile(string filePath, object initialData)
        {
            if (!ValidateCreateFileParameters(filePath, initialData))
            {
                return false;
            }

            try
            {
                EnsureDirectoryExists(filePath);
                string jsonContent = JsonConvert.SerializeObject(initialData, DefaultFormatting);
                File.WriteAllText(filePath, jsonContent);

                Logger.Log($"已创建JSON文件: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"创建JSON文件失败: {filePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 删除JSON文件中的指定键
        /// </summary>
        /// <param name="filePath">JSON文件路径</param>
        /// <param name="key">要删除的键</param>
        /// <returns>是否成功删除键</returns>
        public static bool RemoveKey(string filePath, string key)
        {
            if (!ValidateRemoveKeyParameters(filePath, key))
            {
                return false;
            }

            try
            {
                var jsonObject = ReadJsonFile(filePath);
                if (jsonObject.Remove(key))
                {
                    WriteJsonFile(filePath, jsonObject);
                    Logger.Log($"已从JSON文件中删除键: {filePath}, 键: {key}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"删除JSON键时发生错误: {filePath}, 键: {key}", ex);
                return false;
            }
        }
        #endregion

        #region 私有辅助方法
        /// <summary>
        /// 验证SetValue方法的参数
        /// </summary>
        private static void ValidateSetValueParameters(string filePath, string key, object value)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "文件路径不能为空");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "键不能为空");

            if (value == null)
                throw new ArgumentNullException(nameof(value), "值不能为null");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("指定的JSON文件不存在", filePath);
        }

        /// <summary>
        /// 验证GetValue方法的参数
        /// </summary>
        private static bool ValidateGetValueParameters(string filePath, string key)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Logger.LogWarning($"JSON文件不存在: {filePath}");
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning("键不能为空");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证CreateJsonFile方法的参数
        /// </summary>
        private static bool ValidateCreateFileParameters(string filePath, object initialData)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.LogWarning("文件路径不能为空");
                return false;
            }

            if (initialData == null)
            {
                Logger.LogWarning("初始数据不能为null");
                return false;
            }

            if (!filePath.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning($"文件必须是JSON格式: {filePath}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证RemoveKey方法的参数
        /// </summary>
        private static bool ValidateRemoveKeyParameters(string filePath, string key)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Logger.LogWarning($"JSON文件不存在: {filePath}");
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning("键不能为空");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Logger.Log($"已创建目录: {directory}");
            }
        }

        /// <summary>
        /// 读取JSON文件内容
        /// </summary>
        private static JObject ReadJsonFile(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JObject.Parse(jsonString);
        }

        /// <summary>
        /// 写入JSON文件内容
        /// </summary>
        private static void WriteJsonFile(string filePath, JObject jsonObject)
        {
            File.WriteAllText(filePath, jsonObject.ToString(DefaultFormatting));
        }
        #endregion
    }
}
