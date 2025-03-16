using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Logging;
using System.Xml;

namespace VisualNovelEngine.Model.Page
{
    /// <summary>
    /// 页面JSON信息类
    /// 用于存储和管理视觉小说页面的配置信息
    /// </summary>
    public class PageJsonInfo
    {
        #region 属性
        /// <summary>
        /// 文本文件路径（相对于项目目录）
        /// </summary>
        [JsonProperty("text_path")]
        public string TextPath { get; set; }

        /// <summary>
        /// 背景图片路径（相对于项目目录）
        /// </summary>
        [JsonProperty("image_path")]
        public string ImagePath { get; set; }

        /// <summary>
        /// 背景音乐路径（相对于项目目录）
        /// </summary>
        [JsonProperty("music_path")]
        public string MusicPath { get; set; }

        /// <summary>
        /// 上一页路径
        /// </summary>
        [JsonProperty("previous_page_path")]
        public string PreviousPagePath { get; set; }

        /// <summary>
        /// 下一页路径
        /// </summary>
        [JsonProperty("next_page_path")]
        public string NextPagePath { get; set; }

        /// <summary>
        /// 可跳转的页面路径列表
        /// </summary>
        [JsonProperty("jump_to_page_paths")]
        public string[] JumpToPagePaths { get; set; }

        /// <summary>
        /// 跳转点列表
        /// </summary>
        [JsonProperty("jump_points")]
        public List<JumpPoint> JumpPoints { get; set; } = new List<JumpPoint>();
        #endregion

        #region 路径转换方法
        /// <summary>
        /// 获取文本文件的绝对路径
        /// </summary>
        /// <returns>文本文件的绝对路径</returns>
        public string GetAbsoluteTextPath()
        {
            try
            {
                if (string.IsNullOrEmpty(TextPath))
                {
                    throw new InvalidOperationException("文本文件路径未设置");
                }
                return ProjectData.GetTextAbsolutePath(TextPath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取文本文件绝对路径时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取图片文件的绝对路径
        /// </summary>
        /// <returns>图片文件的绝对路径</returns>
        public string GetAbsoluteImagePath()
        {
            try
            {
                if (string.IsNullOrEmpty(ImagePath))
                {
                    throw new InvalidOperationException("图片文件路径未设置");
                }
                return ProjectData.GetImageAbsolutePath(ImagePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取图片文件绝对路径时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取音乐文件的绝对路径
        /// </summary>
        /// <returns>音乐文件的绝对路径</returns>
        public string GetAbsoluteMusicPath()
        {
            try
            {
                if (string.IsNullOrEmpty(MusicPath))
                {
                    throw new InvalidOperationException("音乐文件路径未设置");
                }
                return ProjectData.GetMusicAbsolutePath(MusicPath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取音乐文件绝对路径时出错: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 文件操作方法
        /// <summary>
        /// 从JSON文件加载页面信息
        /// </summary>
        /// <param name="jsonPath">JSON文件路径</param>
        /// <returns>页面信息对象</returns>
        public static PageJsonInfo LoadFromFile(string jsonPath)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPath))
                {
                    throw new ArgumentNullException(nameof(jsonPath));
                }

                if (!File.Exists(jsonPath))
                {
                    throw new FileNotFoundException("页面JSON文件不存在", jsonPath);
                }

                string jsonContent = File.ReadAllText(jsonPath);
                var pageInfo = JsonConvert.DeserializeObject<PageJsonInfo>(jsonContent);

                Logger.Log($"已从文件加载页面信息: {jsonPath}");
                return pageInfo;
            }
            catch (Exception ex)
            {
                Logger.LogError($"加载页面JSON文件时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 保存页面信息到JSON文件
        /// </summary>
        /// <param name="jsonPath">JSON文件路径</param>
        public void SaveToFile(string jsonPath)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPath))
                {
                    throw new ArgumentNullException(nameof(jsonPath));
                }

                string jsonContent = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonPath, jsonContent);

                Logger.Log($"已保存页面信息到文件: {jsonPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"保存页面JSON文件时出错: {ex.Message}");
                throw;
            }
        }
        #endregion
    }

    /// <summary>
    /// 跳转点信息类
    /// 用于存储页面内跳转点的目标信息
    /// </summary>
    public class JumpPoint
    {
        #region 属性
        /// <summary>
        /// 跳转点文本
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// 目标页面名称
        /// </summary>
        [JsonProperty("target_page")]
        public string TargetPage { get; set; }

        /// <summary>
        /// 目标组名称
        /// </summary>
        [JsonProperty("target_group")]
        public string TargetGroup { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public JumpPoint()
        {
        }

        /// <summary>
        /// 使用指定参数创建跳转点
        /// </summary>
        /// <param name="text">跳转点文本</param>
        /// <param name="targetPage">目标页面名称</param>
        /// <param name="targetGroup">目标组名称（可选）</param>
        public JumpPoint(string text, string targetPage, string targetGroup = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (string.IsNullOrEmpty(targetPage))
            {
                throw new ArgumentNullException(nameof(targetPage));
            }

            Text = text;
            TargetPage = targetPage;
            TargetGroup = targetGroup;
        }
        #endregion

        /// <summary>
        /// 返回跳转点的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"JumpPoint[Text='{Text}', Target={TargetGroup ?? "当前组"}/{TargetPage}]";
        }
    }
}
