using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class HttpHelper
    {      
        private static ConcurrentDictionary<string, HttpHelper> httpMap = new ConcurrentDictionary<string, HttpHelper>();
        public ConcurrentDictionary<string, string> Headers { get; } = new ConcurrentDictionary<string, string>();
        #region 单例
        public static HttpHelper Instance { get; } = new HttpHelper();
        private HttpHelper() { }
        #endregion

        public static HttpHelper GetInstance(string key)
        {
            if(httpMap.TryGetValue(key, out HttpHelper http))
            {
                return http;
            }
            else
            {
                HttpHelper httpHelper = new HttpHelper();
                httpMap.TryAdd(key, httpHelper);
                return httpHelper;
            }
        }
        /// <summary>
        /// 发起POST同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <returns></returns>
        public string HttpPost(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            postData = postData ?? "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeOut);
                    
                    if (Headers.Count >0 )
                    {
                        foreach (var header in Headers)
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                    using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                    {
                        if (contentType != null)
                            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                        HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                        
                        // 检查HTTP响应状态
                        if (!response.IsSuccessStatusCode)
                        {
                            LogUtil.Log($"HTTP POST请求失败: {url}");
                            LogUtil.Log($"状态码: {response.StatusCode} ({(int)response.StatusCode})");
                            LogUtil.Log($"响应内容: {responseContent}");
                            throw new HttpRequestException($"HTTP请求失败，状态码: {response.StatusCode}，响应: {responseContent}");
                        }
                        
                        return responseContent;
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                LogUtil.Log($"HTTP POST请求超时: {url}");
                throw new TimeoutException($"HTTP请求超时 ({timeOut}秒): {url}", ex);
            }
            catch (HttpRequestException ex)
            {
                LogUtil.Log($"HTTP POST请求异常: {url} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogUtil.Log($"HTTP POST请求发生未知错误: {url} - {ex.Message}");
                throw new HttpRequestException($"HTTP请求发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发起POST异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <returns></returns>
        public async Task<string> HttpPostAsync(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            postData = postData ?? "";
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut);
                if (Headers.Count > 0)
                {
                    foreach (var header in Headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = await client.PostAsync(url, httpContent);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// 发起GET同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public string HttpGet(string url, Dictionary<string, string> parameters, string contentType = "application/json")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (contentType != null)
                        client.DefaultRequestHeaders.Add("ContentType", contentType);
                    if (Headers.Count > 0)
                    {
                        foreach (var header in Headers)
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }                
                    
                    if (parameters?.Count > 0)
                    {
                        url += "?" + ToQueryString(parameters);
                    }
                    
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    
                    // 检查HTTP响应状态
                    if (!response.IsSuccessStatusCode)
                    {
                        LogUtil.Log($"HTTP GET请求失败: {url}");
                        LogUtil.Log($"状态码: {response.StatusCode} ({(int)response.StatusCode})");
                        LogUtil.Log($"响应内容: {responseContent}");
                        throw new HttpRequestException($"HTTP请求失败，状态码: {response.StatusCode}，响应: {responseContent}");
                    }
                    
                    return responseContent;
                }
            }
            catch (HttpRequestException ex)
            {
                LogUtil.Log($"HTTP GET请求异常: {url} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogUtil.Log($"HTTP GET请求发生未知错误: {url} - {ex.Message}");
                throw new HttpRequestException($"HTTP请求发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Dictionary 转url查询参数字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ToQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "";

            StringBuilder query = new StringBuilder();
            foreach (var pair in parameters)
            {
                if (query.Length > 0)
                    query.Append("&");

                query.Append(Uri.EscapeDataString(pair.Key));
                query.Append("=");
                query.Append(Uri.EscapeDataString(pair.Value));
            }

            return query.ToString();
        }

        /// <summary>
        /// 发起GET异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<string> HttpGetAsync(string url, string contentType = "application/json")
        {
            using (HttpClient client = new HttpClient())
            {
                if (contentType != null)
                    client.DefaultRequestHeaders.Add("ContentType", contentType);
                if (Headers.Count > 0)
                {
                    foreach (var header in Headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发起POST同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <param name="headers">填充消息头</param>
        /// <returns></returns>
        public T HttpPost<T>(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            return HttpPost(url, postData, contentType, timeOut).ToEntity<T>();
        }

        /// <summary>
        /// 发起POST异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <param name="headers">填充消息头</param>
        /// <returns></returns>
        public async Task<T> HttpPostAsync<T>(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            var res = await HttpPostAsync(url, postData, contentType, timeOut);
            return res.ToEntity<T>();
        }

        /// <summary>
        /// 发起GET同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public T HttpGet<T>(string url, Dictionary<string, string> parameters = null, string contentType = "application/json")
        {
            return HttpGet(url, parameters, contentType).ToEntity<T>();
        }

        /// <summary>
        /// 发起GET异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<T> HttpGetAsync<T>(string url, string contentType = "application/json")
        {
            var res = await HttpGetAsync(url, contentType);
            return res.ToEntity<T>();
        }

        /// <summary>
        /// 发起PUT同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <returns></returns>
        public string HttpPut(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            postData = postData ?? "";
            using (HttpClient client = new HttpClient())
            {
                if (Headers.Count > 0)
                {
                    foreach (var header in Headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = client.PutAsync(url, httpContent).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
        }

        /// <summary>
        /// 发起PUT同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <param name="headers">填充消息头</param>
        /// <returns></returns>
        public T HttpPut<T>(string url, string postData = null, string contentType = "application/json", int timeOut = 30)
        {
            return HttpPut(url, postData, contentType, timeOut).ToEntity<T>();
        }
    }
}
