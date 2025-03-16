using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace VisualNovelEngine.Model.Logging
{
    /// <summary>
    /// 日志记录器，提供全局日志记录功能
    /// </summary>
    /// <remarks>
    /// 该类提供了统一的日志记录接口，支持不同级别的日志记录、错误跟踪和日志文件管理
    /// </remarks>
    public static class Logger
    {
        // 日志文件路径
        private static string _logFilePath = "log.txt";

        // 备份日志文件路径
        private static string _backupLogFilePath;

        // 线程同步锁
        private static readonly object _lock = new object();

        // 是否启用控制台输出
        private static bool _enableConsoleOutput = true;

        // 是否启用文件日志
        private static bool _enableFileLogging = true;

        // 日志文件大小限制（字节）
        private static long _maxLogFileSize = 5 * 1024 * 1024; // 默认 5MB

        // 内存中保存的最近日志（用于性能优化）
        private static readonly List<string> _recentLogs = new List<string>(100);

        // 最大内存日志条数
        private static int _maxMemoryLogCount = 100;

        /// <summary>
        /// 初始化日志记录器
        /// </summary>
        /// <param name="projectPath">项目路径，用于确定日志文件位置</param>
        /// <param name="enableConsole">是否启用控制台输出</param>
        /// <param name="enableFileLogging">是否启用文件日志</param>
        public static void Initialize(string projectPath, bool enableConsole = true, bool enableFileLogging = true)
        {
            try
            {
                // 设置日志文件路径
                _logFilePath = Path.Combine(projectPath, "logs", "log.txt");
                _backupLogFilePath = Path.Combine(projectPath, "logs", "log_backup.txt");

                // 确保日志目录存在
                string logDirectory = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 设置日志选项
                _enableConsoleOutput = enableConsole;
                _enableFileLogging = enableFileLogging;

                // 记录初始化信息
                Log($"日志记录器已初始化 - 项目路径: {projectPath}", LogLevel.Info);
                Log($"系统信息: {Environment.OSVersion}, .NET: {Environment.Version}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                // 初始化失败时，尝试输出到控制台
                Console.WriteLine($"初始化日志记录器失败: {ex.Message}");

                // 设置默认值
                _logFilePath = "log.txt";
                _enableFileLogging = false;
                _enableConsoleOutput = true;
            }
        }

        /// <summary>
        /// 记录日志信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="level">日志级别</param>
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // 格式化日志消息
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            // 添加到内存日志
            lock (_lock)
            {
                _recentLogs.Add(formattedMessage);
                if (_recentLogs.Count > _maxMemoryLogCount)
                {
                    _recentLogs.RemoveAt(0);
                }
            }

            // 输出到控制台
            if (_enableConsoleOutput)
            {
                // 根据日志级别设置控制台颜色
                ConsoleColor originalColor = Console.ForegroundColor;
                switch (level)
                {
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.WriteLine(formattedMessage);
                Console.ForegroundColor = originalColor;
            }

            // 写入日志文件
            if (_enableFileLogging)
            {
                lock (_lock)
                {
                    try
                    {
                        // 检查日志文件大小
                        CheckLogFileSize();

                        // 写入日志
                        File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        // 如果写入日志文件失败，输出到控制台
                        Console.WriteLine($"写入日志文件失败: {ex.Message}");
                        Console.WriteLine(formattedMessage);
                    }
                }
            }
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="ex">异常对象（可选）</param>
        public static void LogError(string message, Exception ex = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}");

            if (ex != null)
            {
                sb.Append($" - {ex.Message}");

                // 添加内部异常信息
                if (ex.InnerException != null)
                {
                    sb.Append($" - 内部异常: {ex.InnerException.Message}");
                }

                // 添加堆栈跟踪
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    sb.Append($" - 堆栈跟踪: {ex.StackTrace}");
                }
            }

            // 获取调用堆栈信息
            StackTrace stackTrace = new StackTrace(1, true);
            if (stackTrace.FrameCount > 0)
            {
                StackFrame frame = stackTrace.GetFrame(0);
                string caller = $"{frame.GetMethod().DeclaringType}.{frame.GetMethod().Name}";
                sb.Append($" - 调用者: {caller}");
            }

            Log(sb.ToString(), LogLevel.Error);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">调试消息</param>
        public static void LogDebug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        /// 获取最近的日志记录
        /// </summary>
        /// <param name="count">要获取的日志条数</param>
        /// <returns>日志记录数组</returns>
        public static string[] GetRecentLogs(int count = 100)
        {
            lock (_lock)
            {
                // 首先尝试从内存中获取
                if (_recentLogs.Count > 0)
                {
                    int startIndex = Math.Max(0, _recentLogs.Count - count);
                    int length = Math.Min(count, _recentLogs.Count - startIndex);
                    string[] result = new string[length];
                    _recentLogs.CopyTo(startIndex, result, 0, length);
                    return result;
                }

                // 如果内存中没有，则从文件读取
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        var lines = File.ReadAllLines(_logFilePath);
                        int startIndex = Math.Max(0, lines.Length - count);
                        int length = Math.Min(count, lines.Length - startIndex);
                        string[] result = new string[length];
                        Array.Copy(lines, startIndex, result, 0, length);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取日志失败: {ex.Message}");
                }
            }

            return new string[0];
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        /// <param name="createBackup">是否创建备份</param>
        public static void ClearLogs(bool createBackup = true)
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        // 创建备份
                        if (createBackup)
                        {
                            try
                            {
                                File.Copy(_logFilePath, _backupLogFilePath, true);
                                Log("已创建日志备份", LogLevel.Debug);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"创建日志备份失败: {ex.Message}");
                            }
                        }

                        // 清空日志文件
                        File.WriteAllText(_logFilePath, string.Empty);

                        // 清空内存日志
                        _recentLogs.Clear();

                        Log("日志已清除", LogLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清除日志失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 设置日志选项
        /// </summary>
        /// <param name="enableConsole">是否启用控制台输出</param>
        /// <param name="enableFileLogging">是否启用文件日志</param>
        /// <param name="maxLogFileSize">最大日志文件大小（字节）</param>
        public static void SetLogOptions(bool? enableConsole = null, bool? enableFileLogging = null, long? maxLogFileSize = null)
        {
            lock (_lock)
            {
                if (enableConsole.HasValue)
                {
                    _enableConsoleOutput = enableConsole.Value;
                }

                if (enableFileLogging.HasValue)
                {
                    _enableFileLogging = enableFileLogging.Value;
                }

                if (maxLogFileSize.HasValue && maxLogFileSize.Value > 0)
                {
                    _maxLogFileSize = maxLogFileSize.Value;
                }

                Log($"日志选项已更新 - 控制台: {_enableConsoleOutput}, 文件: {_enableFileLogging}, 大小限制: {_maxLogFileSize / 1024 / 1024}MB", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 检查日志文件大小，如果超过限制则创建备份并清空
        /// </summary>
        private static void CheckLogFileSize()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    FileInfo fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length > _maxLogFileSize)
                    {
                        // 创建备份文件名（包含时间戳）
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string backupPath = Path.Combine(
                            Path.GetDirectoryName(_backupLogFilePath),
                            Path.GetFileNameWithoutExtension(_backupLogFilePath) + "_" + timestamp + Path.GetExtension(_backupLogFilePath)
                        );

                        // 创建备份
                        File.Copy(_logFilePath, backupPath, true);

                        // 清空原文件
                        File.WriteAllText(_logFilePath, string.Empty);

                        // 记录日志轮转信息
                        string message = $"日志文件已轮转 - 大小: {fileInfo.Length / 1024}KB, 备份: {backupPath}";
                        File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Info] {message}{Environment.NewLine}");

                        if (_enableConsoleOutput)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Info] {message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查日志文件大小失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>调试信息，仅在开发和调试时使用</summary>
        Debug,

        /// <summary>一般信息，记录应用程序的正常操作</summary>
        Info,

        /// <summary>警告信息，表示可能的问题</summary>
        Warning,

        /// <summary>错误信息，表示发生了错误</summary>
        Error
    }
}