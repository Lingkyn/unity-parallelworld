using System;
using System.IO;
using UnityEngine;

namespace ParallelWorld
{
    [Serializable]
    public class AgentLogExtra { public int frame; }
    [Serializable]
    public class AgentLogStrings { public string a; public string b; public string c; public bool d; }
    [Serializable]
    public class AgentLogCollision { public string otherName; public string otherTag; public string rootName; public string rootTag; public string otherLayer; public string rootLayer; public bool hasController; public bool tagCheckPassed; public bool rootTagCheckPassed; }

    /// <summary>Agent 调试日志：写入 NDJSON 到 debug-8f7fbf.log</summary>
    public static class DebugAgentLog
    {
        private const string LogFile = "debug-8f7fbf.log";

        public static void Write(string location, string message, object data, string hypothesisId) => Log(location, message, data, hypothesisId);

        public static void Log(string location, string message, object data, string hypothesisId)
        {
            try
            {
                var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", LogFile));
                var ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var frame = Time.frameCount;
                string dataStr = "{}";
                if (data is AgentLogExtra extra) dataStr = JsonUtility.ToJson(extra);
                else if (data is AgentLogStrings strs) dataStr = JsonUtility.ToJson(strs);
                else if (data is AgentLogCollision col) dataStr = JsonUtility.ToJson(col);
                var line = $"{{\"sessionId\":\"8f7fbf\",\"location\":\"{location}\",\"message\":\"{Escape(message)}\",\"data\":{{\"frame\":{frame},\"extra\":{dataStr}}},\"timestamp\":{ts},\"hypothesisId\":\"{hypothesisId}\"}}\n";
                File.AppendAllText(path, line);
            }
            catch { }
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
