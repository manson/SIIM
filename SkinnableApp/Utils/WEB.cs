using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace SIinformer.Utils
{
    public static class WEB
    {
        private static Logger _logger;
        private static ProxySetting _proxySetting;

        public static void Init(ProxySetting proxySetting, Logger logger)
        {
            _proxySetting = proxySetting;
            _logger = logger;
        }

        /// <summary>
        /// Синхронная или асинхронная загрузка данных
        /// </summary>
        /// <param name="url">Адрес</param>
        /// <param name="progress">Обработчик события прогресса загрузки для асинхронной загрузки (null для синхронной</param>
        /// <param name="complete">Обработчик события завершения загрузки</param>
        /// <returns>Результат синхронной загрузки или null при неудаче, всегда null при асинхронной</returns>
        public static string DownloadPageSilent(string url, DownloadProgressChangedEventHandler progress,
                                                DownloadDataCompletedEventHandler complete)
        {
            byte[] buffer = null;
            int tries = 0;

            var client = new WebClient();
            try
            {
                if (_proxySetting.UseProxy)
                {
                    IPAddress test;
                    if (!IPAddress.TryParse(_proxySetting.Address, out test))
                        throw new ArgumentException("Некорректный адрес прокси сервера");
                    client.Proxy = _proxySetting.UseAuthentification
                                       ? new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port),
                                             false,
                                             new string[0],
                                             new NetworkCredential(_proxySetting.UserName, _proxySetting.Password))
                                       : new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port));
                }
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка конструктора прокси", false, true);
            }

            while (tries < 3)
            {
                try
                {
                    SetHttpHeaders(client, null);
                    if (progress == null)
                        buffer = client.DownloadData(url);
                    else
                    {
                        client.DownloadProgressChanged += progress;
                        client.DownloadDataCompleted += complete;
                        client.DownloadDataAsync(new Uri(url));
                        return null;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Add(ex.StackTrace, false, true);
                    _logger.Add(ex.Message, false, true);
                    _logger.Add("Ошибка загрузки страницы", false, true);
                    tries++;
                }
            }

            return (buffer != null) ? ConvertPage(buffer) : null;
        }

        public static byte[] DownloadPageSilentBinary(string url, DownloadProgressChangedEventHandler progress,
                                                DownloadDataCompletedEventHandler complete)
        {
            byte[] buffer = null;
            int tries = 0;

            var client = new WebClient();
            try
            {
                if (_proxySetting.UseProxy)
                {
                    IPAddress test;
                    if (!IPAddress.TryParse(_proxySetting.Address, out test))
                        throw new ArgumentException("Некорректный адрес прокси сервера");
                    client.Proxy = _proxySetting.UseAuthentification
                                       ? new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port),
                                             false,
                                             new string[0],
                                             new NetworkCredential(_proxySetting.UserName, _proxySetting.Password))
                                       : new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port));
                }
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка конструктора прокси", false, true);
            }

            while (tries < 3)
            {
                try
                {
                    SetHttpHeaders(client, null);
                    if (progress == null)
                        buffer = client.DownloadData(url);
                    else
                    {
                        client.DownloadProgressChanged += progress;
                        client.DownloadDataCompleted += complete;
                        client.DownloadDataAsync(new Uri(url));
                        return null;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Add(ex.StackTrace, false, true);
                    _logger.Add(ex.Message, false, true);
                    _logger.Add("Ошибка загрузки страницы", false, true);
                    tries++;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Синхронная загрузка данных
        /// </summary>
        /// <param name="url">Адрес</param>
        /// <returns>Результат загрузки или null при неудаче</returns>
        public static string DownloadPageSilent(string url)
        {
            return DownloadPageSilent(url, null, null);
        }

        private static void SetHttpHeaders(WebClient client, string referer)
        {
            client.Headers.Clear();
            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1;)");
            client.Headers.Add("Accept-Charset", "windows-1251");
            if (!string.IsNullOrEmpty(referer))
            {
                client.Headers.Add("Referer", referer);
            }
        }

        public static string ConvertPage(byte[] data)
        {
            return Encoding.GetEncoding("windows-1251").GetString(data);
        }

        public static void OpenURL(string url)
        {
            Thread thread = new Thread(obj => Process.Start((string) obj)) {IsBackground = true};
            thread.Start(url);
        }

        public static string SendHttpPOSTRequest(string Url, Dictionary<string, string> postData)
        {
            WebRequest request = WebRequest.Create(Url);
            try
            {
                if (_proxySetting.UseProxy)
                {
                    IPAddress test;
                    if (!IPAddress.TryParse(_proxySetting.Address, out test))
                        throw new ArgumentException("Некорректный адрес прокси сервера");
                    request.Proxy = _proxySetting.UseAuthentification
                                       ? new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port),
                                             false,
                                             new string[0],
                                             new NetworkCredential(_proxySetting.UserName, _proxySetting.Password))
                                       : new WebProxy(
                                             new Uri("http://" + _proxySetting.Address + ":" + _proxySetting.Port));
                }
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка конструктора прокси", false, true);
            }

            string postDataString = string.Empty;
            foreach(KeyValuePair<string, string> item in postData)
            {
                if (postDataString.Length != 0)
                    postDataString += "&";
                postDataString += item.Key + "=" + item.Value;
            }

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataString.Length;
            using (Stream writeStream = request.GetRequestStream())
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(postDataString);
                writeStream.Write(bytes, 0, bytes.Length);
            }

            string result = string.Empty;
            try
            {
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка посылке POST запроса", false, true);
            }
            return result;
        }

    }
}