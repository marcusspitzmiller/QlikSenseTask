using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace SenseBuilder
{
    public class QRSWebClient
    {
        private readonly CertificateWebClient _client;
        private readonly NameValueCollection _queryStringCollection;
		private string serverURL;
        public string ExecuteAs;

        public QRSWebClient(string QRSserverURL)
        {
            _client = new CertificateWebClient { Encoding = Encoding.UTF8 };
            _client.Certificate = GetClientCertificate();
            _queryStringCollection = new NameValueCollection { { "xrfkey", "ABCDEFG123456789" } };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            serverURL = QRSserverURL;
			
            ExecuteAs = "UserDirectory=" + Environment.UserDomainName + "; UserId=" + Environment.UserName;
			//do a simple first GET to set cookies for subsequent actions (otherwise POST commands wont work)
            try
            {
                string resp = Get("/qrs/about");
            }
            catch (Exception ex)
            {
                throw new Exception("Couldnt connect to the QRS at " + QRSserverURL + " , check the QRS is running at this address");
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
            _client.QueryString = _queryStringCollection;

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
                return _client.UploadString(serverURL + endpoint, "Delete", "");
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
            _client.Headers.Add("X-Qlik-User", ExecuteAs);
        }

        private static X509Certificate2 GetClientCertificate()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (cert.FriendlyName == "QlikClient")
                {
                    store.Close();
                    return cert;
                }
            }

            throw new Exception("Could not find QlikClient certificate");
        }


        private static string ParseWebException(WebException exception)
        {
            if (exception.Status == WebExceptionStatus.ConnectFailure)
                return exception.Status + ": " + exception.Message;

            HttpWebResponse webResponse = (HttpWebResponse)exception.Response;
            Stream responseStream = webResponse.GetResponseStream();
            return webResponse.StatusDescription + (responseStream != null ? ": " + new StreamReader(responseStream).ReadToEnd() : string.Empty);
        }
    }


    class CertificateWebClient : WebClient
    {
        public X509Certificate2 Certificate { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.ClientCertificates.Add(Certificate);
            return request;
        }
    }

}
