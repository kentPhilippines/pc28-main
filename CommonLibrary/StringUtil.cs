using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class StringUtil
    {
        /// <summary>
        /// 提取数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double ExtractNumber(string input)
        {
            string n = Regex.Replace(input, "[^0-9.]+", "");
            double.TryParse(n, out double result);
            return result;
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            return int.TryParse(str, out int num);
        }

        /// <summary>
        /// 取两个标识之间的字符
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startToken"></param>
        /// <param name="endToken"></param>
        /// <returns></returns>
        public static string Extract(string source, string startToken, string endToken)
        {
            // 构建正则表达式模式
            string pattern = $@"(?<={Regex.Escape(startToken)})[^\{Regex.Escape(endToken)}]*";

            // 使用正则表达式匹配
            Match match = Regex.Match(source, pattern);

            // 返回匹配到的字符串，如果没有匹配则返回空字符串
            return match.Success ? match.Value : string.Empty;
        }

        /// <summary>
        /// 提取指定词组后的数字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static double ExtractNumberAfterWord(string input, string word)
        {
            string pattern = $@"{word}\s+(\d+(\.\d+)?)";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                return double.Parse(match.Groups[1].Value);
            }
            throw new InvalidOperationException("Number not found after the specified word.");
        }

        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    deflateStream.Write(buffer, 0, buffer.Length);
                    deflateStream.Close();
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// 解压缩字符串
        /// </summary>
        /// <param name="compressedText"></param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] buffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream(buffer))
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(deflateStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
