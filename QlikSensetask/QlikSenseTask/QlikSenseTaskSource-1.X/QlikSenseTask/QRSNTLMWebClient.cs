using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Principal;

namespace QVnextDemoBuilder
{
    public class QRSNTLMWebClient
    {
        private readonly CookierAwareWebClient _client;
        private readonly NameValueCollection _queryStringCollection;
        private string serverURL;

        public QRSNTLMWebClient(string QRSserverURL, Int32 requesttimeout)
        {
            _client = new CookierAwareWebClient { Encoding = Encoding.UTF8 };
            _client.UseDefaultCredentials = true;
            _client.Timeout = requesttimeout;

            //_client.Credentials = CredentialCache.DefaultCredentials;

            _queryStringCollection = new NameValueCollection { { "xrfkey", "ABCDEFG123456789" } };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            serverURL = QRSserverURL;

            //do a simple first GET to set cookies for subsequent actions (otherwise POST commands wont work)
            try
            {
                string resp = Get("/qrs/about");
            }
            catch (Exception ex)
            {
                throw new Exception("Couldnt connect to the server at " + QRSserverURL + " , check that the Proxy and QRS are running.");
            }
        }



        public string Put(string endpoint, string content)
        {
            SetHeaders();
            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            _client.QueryString = queryStringCollection;

            try
            {
                return _client.UploadString(serverURL + endpoint, "Put", content);
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string Put(string endpoint, Dictionary<string, string> queries)
        {
            SetHeaders();
            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            if (queries != null)
            {
                foreach (KeyValuePair<string, string> query in queries)
                    queryStringCollection.Add(query.Key, query.Value);
            }

            _client.QueryString = queryStringCollection;

            try
            {
                return _client.UploadString(serverURL + endpoint, "Put", "");
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string Post(string endpoint, Dictionary<string, string> queries)
        {
            SetHeaders();
            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            if (queries != null)
            {
                foreach (KeyValuePair<string, string> query in queries)
                    queryStringCollection.Add(query.Key, query.Value);
            }

            _client.QueryString = queryStringCollection;

            try
            {
                

                return _client.UploadString(serverURL + endpoint, "Post", "");
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public byte[] PutFile(string endpoint, string filepath)
        {
            SetHeaders();
            _client.QueryString = _queryStringCollection;

            try
            {
                return _client.UploadFile(serverURL + endpoint, "Put", filepath);
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string Post(string endpoint, string content)
        {
            SetHeaders();
            //NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            //_client.QueryString = queryStringCollection;

            try
            {
                return _client.UploadString(serverURL + endpoint, "Post", content);
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string PostFile(string endpoint, string filepath, Dictionary<string, string> queries)
        {
            SetHeaders();

            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            if (queries != null)
            {
                foreach (KeyValuePair<string, string> query in queries)
                    queryStringCollection.Add(query.Key, query.Value);
            }
            _client.QueryString = queryStringCollection;

            try
            {
                byte[] result = _client.UploadFile(serverURL + endpoint, "Post", filepath);
                return Encoding.UTF8.GetString(result, 0, result.Length);
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string Delete(string endpoint)
        {
            SetHeaders();
            _client.QueryString = _queryStringCollection;

            try
            {
                return _client.UploadString(serverURL + endpoint, "DELETE", "");
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public string Get(string url)
        {
            return Get(url, null);
        }



        public string Get(string endpoint, Dictionary<string, string> queries)
        {
            SetHeaders();
            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);
            if (queries != null)
            {
                foreach (KeyValuePair<string, string> query in queries)
                    queryStringCollection.Add(query.Key, query.Value);
            }
            
            _client.QueryString = queryStringCollection;

            try
            {
                string response = _client.DownloadString(serverURL + endpoint);
                return response;
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }

        public void GetFile(string endpoint, string filepath)
        {
            SetHeaders();

            NameValueCollection queryStringCollection = new NameValueCollection(_queryStringCollection);

            _client.QueryString = queryStringCollection;

            try
            {
                _client.DownloadFile(serverURL + endpoint, filepath);
                return;
            }
            catch (WebException ex)
            {
                throw new Exception(ParseWebException(ex));
            }
        }



        private void SetHeaders()
        {
            _client.Headers.Clear();
            _client.Headers.Add("Accept-Charset", "utf-8");
            _client.Headers.Add("Accept", "application/json");
            _client.Headers.Add("Content-Type", "application/json");
            _client.Headers.Add("X-Qlik-xrfkey", "ABCDEFG123456789");
            _client.Headers.Add("X-QlikView-xrfkey", "ABCDEFG123456789");
            _client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36");

            

            //_client.Headers.Add("X-Qlik-User", "UserDirectory=QTSEL; UserId=msi");

        }




        private static string ParseWebException(WebException exception)
        {
            if (exception.Status == WebExceptionStatus.ConnectFailure || exception.Status == WebExceptionStatus.Timeout)
                return exception.Status + ": " + exception.Message;

            HttpWebResponse webResponse = (HttpWebResponse)exception.Response;
            Stream responseStream = webResponse.GetResponseStream();
            return webResponse.StatusDescription + (responseStream != null ? ": " + new StreamReader(responseStream).ReadToEnd() : string.Empty);
        }
    }


    public class CookierAwareWebClient : WebClient
    {

        public CookieContainer QRSCookieContainer = new CookieContainer();

        private Int32 timeout;
        public Int32 Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = QRSCookieContainer;
            request.Timeout = this.timeout;
            
            return request;
        }


    }


}

