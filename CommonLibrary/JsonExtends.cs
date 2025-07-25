using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommonLibrary
{
    /// <summary>
    /// Json扩展方法
    /// </summary>
    public static class JsonExtends
    {
        public static T ToEntity<T>(this string val)
        {
            try
            {
                // 检查输入是否为空或null
                if (string.IsNullOrWhiteSpace(val))
                {
                    LogUtil.Log($"JSON解析警告: 输入字符串为空，尝试返回默认值");
                    return default(T);
                }

                // 检查是否是HTML内容（通常以<开头）
                if (val.TrimStart().StartsWith("<"))
                {
                    LogUtil.Log($"JSON解析错误: 接收到HTML内容而不是JSON数据。内容开头: {val.Substring(0, Math.Min(200, val.Length))}...");
                    throw new InvalidOperationException("服务器返回了HTML内容而不是JSON数据，请检查服务器状态或网络连接");
                }

                return JsonConvert.DeserializeObject<T>(val);
            }
            catch (JsonException ex)
            {
                LogUtil.Log($"JSON解析失败: {ex.Message}");
                LogUtil.Log($"原始内容: {val}");
                throw new InvalidOperationException($"JSON解析失败: {ex.Message}。原始内容: {val.Substring(0, Math.Min(500, val.Length))}", ex);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"ToEntity转换失败: {ex.Message}");
                LogUtil.Log($"原始内容: {val}");
                throw;
            }
        }

        public static List<T> ToEntityList<T>(this string val)
        {
            try
            {
                // 检查输入是否为空或null
                if (string.IsNullOrWhiteSpace(val))
                {
                    LogUtil.Log($"JSON列表解析警告: 输入字符串为空，返回空列表");
                    return new List<T>();
                }

                // 检查是否是HTML内容
                if (val.TrimStart().StartsWith("<"))
                {
                    LogUtil.Log($"JSON列表解析错误: 接收到HTML内容而不是JSON数据");
                    throw new InvalidOperationException("服务器返回了HTML内容而不是JSON数据，请检查服务器状态或网络连接");
                }

                return JsonConvert.DeserializeObject<List<T>>(val);
            }
            catch (JsonException ex)
            {
                LogUtil.Log($"JSON列表解析失败: {ex.Message}");
                LogUtil.Log($"原始内容: {val}");
                throw new InvalidOperationException($"JSON列表解析失败: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"ToEntityList转换失败: {ex.Message}");
                throw;
            }
        }

        public static string ToJson<T>(this T entity, Formatting formatting = Formatting.None)
        {
            try
            {
                return JsonConvert.SerializeObject(entity, formatting);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"对象序列化为JSON失败: {ex.Message}");
                throw;
            }
        }
    }
}
