using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

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
			FillProxy(client.Proxy);

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
			FillProxy(client.Proxy);

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

		private static void SetHttpHeaders(HttpWebRequest request, string referer)
		{
			request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
			request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)";
			request.Headers.Add("Accept-Encoding", "gzip, deflate");
			request.Headers.Add("Accept-Language", "en-us");
			request.Headers.Add("Pragma", "no-cache");
			if (!string.IsNullOrEmpty(referer))
			{
				request.Referer = referer;
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

		private static void FillProxy(IWebProxy Proxy)
		{
			try
			{
				if (_proxySetting.UseProxy)
				{
					IPAddress test;
					if (!IPAddress.TryParse(_proxySetting.Address, out test))
						throw new ArgumentException("Некорректный адрес прокси сервера");
					Proxy = _proxySetting.UseAuthentification
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
		}

		public static HttpWebResponse SendHttpGETRequest(string Url)
		{
			return SendHttpGETRequest(Url, null, null);
		}

		public static HttpWebResponse SendHttpGETRequest(string Url, string Referer, string Cookies)
		{
			HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
			FillProxy(request.Proxy);

			request.Method = "GET";
			request.Timeout = 60000;
			SetHttpHeaders(request, Referer);
			if (!string.IsNullOrEmpty(Cookies))
				request.Headers.Add("Cookie", Cookies);

			HttpWebResponse response = null;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (Exception ex)
			{
				_logger.Add(ex.StackTrace, false, true);
				_logger.Add(ex.Message, false, true);
				_logger.Add("Ошибка при посылке GET запроса", false, true);
			}
			return response;
		}

        public static byte[] SendPOSTRequest(string Url, Dictionary<string, string> postData, string Referer, string Cookies)
        {
            byte[] result = null;
            try
            {
                Encoding e = Encoding.GetEncoding("windows-1251");
                WebClient client = new WebClient();
                FillProxy(client.Proxy);
                SetHttpHeaders(client, Referer);
                client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                client.Headers["Cookie"] = Cookies;
                string postDataString = string.Empty;
                foreach (KeyValuePair<string, string> item in postData)
                {
                    if (postDataString.Length != 0)
                        postDataString += "&";
                    postDataString += HttpUtility.UrlEncode(item.Key, e) + "=" + HttpUtility.UrlEncode(item.Value, e);
                }

                byte[] bytes = Encoding.ASCII.GetBytes(postDataString);
                result = client.UploadData(Url, bytes);
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка при посылке POST запроса", false, true);
            }
            return result;
        }

		public static HttpWebResponse SendHttpPOSTRequest(string Url, Dictionary<string, string> postData, string Referer, string Cookies)
        {
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
			FillProxy(request.Proxy);

            string postDataString = string.Empty;
            foreach(KeyValuePair<string, string> item in postData)
            {
                if (postDataString.Length != 0)
                    postDataString += "&";
				postDataString += item.Key + "=" + HttpUtility.UrlEncode(item.Value, Encoding.GetEncoding("windows-1251"));
            }

			byte[] bytes = Encoding.GetEncoding("windows-1251").GetBytes(postDataString);

            request.Method = "POST";
            request.Timeout = 60000;
			SetHttpHeaders(request, Referer);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = bytes.Length;
			request.Headers.Add("Cookie", Cookies);

            using (Stream writeStream = request.GetRequestStream())
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }

			HttpWebResponse response = null;
            try
            {
				response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Ошибка при посылке POST запроса", false, true);
            }
            return response;
        }


		/// <summary>
		/// Метод расширяет класс HttpWebResponse
		/// Парсит полученные Set-Cookies хедеры и добавляет в список
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static CookieCollection GetCookies(this HttpWebResponse response)
		{
			if (response.Headers["Set-Cookie"] != null)
			{
				string cookies = Convert.ToString(response.Headers["Set-Cookie"], System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
				string[] lines = cookies.Split(';');
				string[] fl = lines[0].Split('=');
				response.Cookies.Add(new Cookie(fl[0], fl[1]));
				foreach(string line in lines)
				{
					if (line.IndexOf(",") > 0)
					{
						if (!(line.IndexOf("expires") > 0 && line.IndexOf(',') == line.LastIndexOf(',')))
						{
							fl = line.Substring(line.LastIndexOf(",") + 1).Split('=');
							response.Cookies.Add(new Cookie(fl[0], fl[1]));
						}
					}
				}
			}
			return response.Cookies;
		}

		public static string GetString(this CookieCollection collection)
		{
			string result = "";
			foreach (Cookie c in collection) 
			{
				result += c.Name + "=" + c.Value + "; ";
			}
			return result;
		}

    }
	
}