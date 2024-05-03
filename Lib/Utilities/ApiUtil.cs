using Lib.Models;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Lib.Utilities
{
    public class ApiUtil
    {
        public static string SetQueryParams(dynamic? queryParams = null)
        {
            string query = string.Empty;
            string result = string.Empty;

            if (queryParams != null)
            {
                if (queryParams.GetType().Name == typeof(Dictionary<,>).Name)
                {
                    foreach (KeyValuePair<object, object> param in queryParams)
                        query += $"&{param.Key}={param.Value}";
                    query = query.TrimStart('&');
                }
                else
                { // anonymous type
                    Func<PropertyInfo, bool> predicate = prop => !prop.GetIndexParameters().Any(); // exclude index property
                    IEnumerable<PropertyInfo> props = queryParams.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    props = props.Where(predicate);

                    foreach (var prop in props)
                        query += $"&{prop.Name}={prop.GetValue(queryParams)}";
                    query = query.TrimStart('&');
                }
                result = query != string.Empty ? "?" + query : query;
            }

            return result;
        }

        public static TResult? HttpClientEx<TResult>(string service, string requestUri = "",
            object? body = null, dynamic? queryParams = null, HttpVerbs method = HttpVerbs.Post,
            BasicAuth? basicAuth = null, string token = "") where TResult : new()
        {
            // 安裝套件：Microsoft.AspNet.WebApi.Client
            TResult? result = default;

            try
            {
                requestUri += SetQueryParams(queryParams);

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(service);
                    client.SetBasicAuth(basicAuth);
                    client.SetBearerAuth(token);
                    // Accept 用於宣告客戶端要求服務端回應的文件型態
                    //client.DefaultRequestHeaders.Accept.Add(
                    //new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    switch (method)
                    {
                        case HttpVerbs.Get:
                            using (HttpResponseMessage response = client.GetAsync(requestUri).Result)
                                result = response.Content.ReadAsAsync<TResult>().Result;
                            break;
                        case HttpVerbs.Post:
                            using (HttpResponseMessage response = client.PostAsJsonAsync(requestUri, body).Result)
                                result = response.Content.ReadAsAsync<TResult>().Result;
                            break;
                        case HttpVerbs.Put:
                            using (HttpResponseMessage response = client.PutAsJsonAsync(requestUri, body).Result)
                                result = response.Content.ReadAsAsync<TResult>().Result;
                            break;
                        case HttpVerbs.Patch:
                            using (HttpResponseMessage response = client.PatchAsJsonAsync(requestUri, body).Result)
                                result = response.Content.ReadAsAsync<TResult>().Result;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (typeof(TResult).Name == typeof(ApiResult<>).Name && result == null)
                {
                    result = new TResult();
                    (result as dynamic).Succ = false;
                    (result as dynamic).Msg = ex.ToString();
                }
                else if (typeof(TResult).Name == typeof(ApiResult<>).Name && result != null)
                {
                    (result as dynamic).Msg = ex.ToString() + Environment.NewLine + (result as dynamic).Msg;
                }
                else
                {
                    throw new Exception($"{nameof(HttpClientEx)} Exception", ex);
                }
            }

            return result;
        }

        public static async Task<TResult?> HttpClientExAsync<TResult>(string service, string requestUri = "",
            object? body = null, dynamic? queryParams = null, HttpVerbs method = HttpVerbs.Post,
            BasicAuth? basicAuth = null, string token = "",
            CancellationToken cancellationToken = default) where TResult : new()
        {
            // 安裝套件：Microsoft.AspNet.WebApi.Client
            TResult? result = default;

            try
            {
                requestUri += SetQueryParams(queryParams);

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(service);
                    client.SetBasicAuth(basicAuth);
                    client.SetBearerAuth(token);
                    //client.DefaultRequestHeaders.Accept.Add(
                    //new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    switch (method)
                    {
                        case HttpVerbs.Get:
                            using (HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken))
                                result = await response.Content.ReadAsAsync<TResult>(cancellationToken);
                            break;
                        case HttpVerbs.Post:
                            using (HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, body, cancellationToken))
                                result = await response.Content.ReadAsAsync<TResult>(cancellationToken);
                            break;
                        case HttpVerbs.Put:
                            using (HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, body, cancellationToken))
                                result = await response.Content.ReadAsAsync<TResult>(cancellationToken);
                            break;
                        case HttpVerbs.Patch:
                            using (HttpResponseMessage response = await client.PatchAsJsonAsync(requestUri, body, cancellationToken))
                                result = await response.Content.ReadAsAsync<TResult>(cancellationToken);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (typeof(TResult).Name == typeof(ApiResult<>).Name && result == null)
                {
                    result = new TResult();
                    (result as dynamic).Succ = false;
                    (result as dynamic).Msg = ex.ToString();
                }
                else if (typeof(TResult).Name == typeof(ApiResult<>).Name && result != null)
                {
                    (result as dynamic).Msg = ex.ToString() + Environment.NewLine + (result as dynamic).Msg;
                }
                else
                {
                    throw new Exception($"{nameof(HttpClientExAsync)} Exception", ex);
                }
            }

            return result;
        }

        /// <summary>
        /// 確認 Http 可否連線
        /// </summary>
        public static async Task<ApiResult<bool>> CheckHttpAvalible(string uri, double timeout = 5000)
        {
            ApiResult<bool> result;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout);
                    using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)))
                    {
                        result = new ApiResult<bool>(response.IsSuccessStatusCode, data: response.IsSuccessStatusCode, code: response.StatusCode);
                        result.Msg += $"({response.ReasonPhrase})";
                    }
                }
            }
            catch
            {
                result = new ApiResult<bool>(false, data: false);
            }

            return result;
        }

        /// <summary>
        /// Get string content like html
        /// </summary>
        /// <remarks>https://www.cych.org.tw/pharm/searchdrugdetailmis.aspx?drug_serial3=APT</remarks>
        public static async Task<ApiResult<string>> HttpClientGetString(string service, string requestUri = "", dynamic? queryParams = null)
        {
            ApiResult<string> result;

            try
            {
                requestUri += SetQueryParams(queryParams);

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(service);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    //string data = await client.GetStringAsync(requestUri); // 可簡化為此
                    using (HttpResponseMessage response = await client.GetAsync(requestUri))
                    {
                        string data = string.Empty;
                        if (response.IsSuccessStatusCode) data = await response.Content.ReadAsStringAsync();
                        result = new ApiResult<string>(response.IsSuccessStatusCode, data: data, code: response.StatusCode);
                        result.Msg += $"({response.ReasonPhrase})";
                    }
                }
            }
            catch
            {
                result = new ApiResult<string>(false, data: string.Empty);
            }

            return result;
        }

        /// <summary>
        /// 下載檔案
        /// </summary>
        public static async Task<ApiResult<bool>> DownloadFile(string uri, string savePath, string saveFileName = "")
        {
            ApiResult<bool> result;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                saveFileName = saveFileName.IsNullOrWhiteSpace() ? Path.GetFileName(uri) : saveFileName;
                                string saveFilePath = Path.Combine(savePath, saveFileName);
                                using (FileStream fileStream = File.Create(saveFilePath))
                                    await contentStream.CopyToAsync(fileStream);
                            }
                        }
                        result = new ApiResult<bool>(response.IsSuccessStatusCode, data: response.IsSuccessStatusCode, code: response.StatusCode);
                        result.Msg += $"({response.ReasonPhrase})";
                    }
                }
            }
            catch
            {
                result = new ApiResult<bool>(false, data: false);
            }

            return result;
        }

    }

    public static class ApiExUtil
    {
        /// <summary>
        /// Set HTTP Basic Auth
        /// </summary>
        public static void SetBasicAuth(this HttpClient client, BasicAuth? basicAuth)
        {
            if (basicAuth != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicAuth.UserId}:{basicAuth.Password}")));
        }

        /// <summary>
        /// Set HTTP Bearer Auth
        /// </summary>
        public static void SetBearerAuth(this HttpClient client, string token)
        {
            if (!token.IsNullOrWhiteSpace())
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken = default)
        {
            var content = new ObjectContent<T>(value, new JsonMediaTypeFormatter());
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return client.SendAsync(request, cancellationToken);
        }

    }

    /// <summary>
    /// Enumerates the HTTP verbs.
    /// </summary>
    [Flags]
    public enum HttpVerbs
    {
        /// <summary>
        /// Retrieves the information or entity that is identified by the URI of the request.
        /// </summary>
        Get = 1,
        /// <summary>
        /// Posts a new entity as an addition to a URI.
        /// </summary>
        Post = 2,
        /// <summary>
        /// Replaces an entity that is identified by a URI.
        /// </summary>
        Put = 4,
        /// <summary>
        /// Requests that a specified URI be deleted.
        /// </summary>
        Delete = 8,
        /// <summary>
        /// Retrieves the message headers for the information or entity that is identified by the URI of the request.
        /// </summary>
        Head = 16,
        /// <summary>
        /// Requests that a set of changes described in the request entity be applied to the resource identified by the Request- URI.
        /// </summary>
        Patch = 32,
        /// <summary>
        /// Represents a request for information about the communication options available on the request/response chain identified by the Request-URI.
        /// </summary>
        Options = 64
    }

}
