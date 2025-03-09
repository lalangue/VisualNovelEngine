using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;

namespace VisualNovelEngine.Model
{
    public partial class Page
    {
        // 生成带时间戳的页面名称
        public void GeneratePageJsonNameWithTimestamp()
        {
            PageName = $"Page_{DateTime.Now:yyyyMMddHHmmssfff}";
        }

        // 建立一个空的 JSON 文件，在 group path 所对应的文件夹下面建立并且它的名字是 PageName
        public void CreateEmptyJsonFile()
        {
            // 假设 Group 类单例可通过 Instance 访问，并且有 GroupPath 属性
            string groupPath = Group.Instance.GroupPath;
            if (!Directory.Exists(groupPath))
            {
                Directory.CreateDirectory(groupPath);
            }

            string jsonFilePath = Path.Combine(groupPath, $"{PageName}.json");
            File.WriteAllText(jsonFilePath, string.Empty);
            PagePath = jsonFilePath;
        }

        // 规定一个方法将 JSON 文件模板写入一个空的 JSON 文件
        public void WriteJsonTemplateToFile()
        {
            if (string.IsNullOrEmpty(PagePath))
            {
                throw new InvalidOperationException("PagePath is not set.");
            }

            var pageTemplate = new
            {
                PageName = PageName,
                PagePath = PagePath,
                TextFilePath = string.Empty,
                ImageFilePath = string.Empty,
                MusicFilePath = string.Empty,
                PreviousPagePath = string.Empty,
                NextPagePath = string.Empty,
                PageJumpPaths = Array.Empty<string>()
            };

            string json = JsonSerializer.Serialize(pageTemplate, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PagePath, json);
        }

        // 读取页面信息
        public void ReadPageInfo()
        {
            if (string.IsNullOrEmpty(PagePath))
            {
                throw new InvalidOperationException("PagePath is not set.");
            }

            if (!File.Exists(PagePath))
            {
                throw new FileNotFoundException($"The JSON file at {PagePath} was not found.");
            }

            string jsonContent = File.ReadAllText(PagePath);
            var pageData = JsonSerializer.Deserialize<dynamic>(jsonContent);

            if (pageData != null)
            {
                TextFilePath = pageData.TextFilePath;
                ImageFilePath = pageData.ImageFilePath;
                MusicFilePath = pageData.MusicFilePath;
                PreviousPagePath = pageData.PreviousPagePath;
                NextPagePath = pageData.NextPagePath;
                PageJumpPaths = pageData.PageJumpPaths.ToObject<string[]>();
            }
        }

        // 初始化页面 JSON 文件
        public void InitializePageJson()
        {
            GeneratePageJsonNameWithTimestamp();
            CreateEmptyJsonFile();
            WriteJsonTemplateToFile();
        }

        // 通过当前 PagePath 属性读取 JSON 文件并更新其他属性
        public void UpdatePagePropertiesFromJson()
        {
            if (string.IsNullOrEmpty(PagePath))
            {
                throw new InvalidOperationException("PagePath is not set.");
            }

            if (!File.Exists(PagePath))
            {
                throw new FileNotFoundException($"The JSON file at {PagePath} was not found.");
            }

            string jsonContent = File.ReadAllText(PagePath);
            var pageData = JsonSerializer.Deserialize<dynamic>(jsonContent);

            if (pageData != null)
            {
                PageName = pageData.PageName;
                TextFilePath = pageData.TextFilePath;
                ImageFilePath = pageData.ImageFilePath;
                MusicFilePath = pageData.MusicFilePath;
                PreviousPagePath = pageData.PreviousPagePath;
                NextPagePath = pageData.NextPagePath;
                PageJumpPaths = pageData.PageJumpPaths.ToObject<string[]>();
            }
        }

        // 选择页面 JSON 文件并更新属性
        public void SelectPageJson()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json";
            openFileDialog.Title = "Select a Page JSON File";

            if (openFileDialog.ShowDialog() == true)
            {
                PagePath = openFileDialog.FileName;
                UpdatePagePropertiesFromJson();
            }
        }

        // 获取前一个页面
        public void GoToPreviousPage()
        {
            if (string.IsNullOrEmpty(PreviousPagePath))
            {
                throw new InvalidOperationException("PreviousPagePath is not set.");
            }

            PagePath = PreviousPagePath;
            UpdatePagePropertiesFromJson();
        }

        // 获取后一个页面
        public void GoToNextPage()
        {
            if (string.IsNullOrEmpty(NextPagePath))
            {
                throw new InvalidOperationException("NextPagePath is not set.");
            }

            PagePath = NextPagePath;
            UpdatePagePropertiesFromJson();
        }

        // 选择并保存图片路径
        public void SelectAndSaveImagePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.Title = "Select an Image File";

            if (openFileDialog.ShowDialog() == true)
            {
                ImageFilePath = openFileDialog.FileName;
                SaveSinglePropertyToJson(nameof(ImageFilePath), ImageFilePath);
            }
        }

        // 选择并保存音乐路径
        public void SelectAndSaveMusicPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Music Files (*.mp3, *.wav, *.ogg)|*.mp3;*.wav;*.ogg";
            openFileDialog.Title = "Select a Music File";

            if (openFileDialog.ShowDialog() == true)
            {
                MusicFilePath = openFileDialog.FileName;
                SaveSinglePropertyToJson(nameof(MusicFilePath), MusicFilePath);
            }
        }

        // 保存单个属性到 JSON 文件
        private void SaveSinglePropertyToJson(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(PagePath))
            {
                throw new InvalidOperationException("PagePath is not set.");
            }

            try
            {
                string jsonContent = File.ReadAllText(PagePath);
                var pageData = JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(jsonContent);

                if (pageData != null)
                {
                    pageData[propertyName] = propertyValue;
                    string updatedJson = JsonSerializer.Serialize(pageData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(PagePath, updatedJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {propertyName} to JSON: {ex.Message}");
            }
        }
    }
}