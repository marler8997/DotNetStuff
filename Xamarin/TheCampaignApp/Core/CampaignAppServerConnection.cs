using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

using More;

namespace CampaignApp
{
    public delegate void FailCallback(String message);

	public class CampaignAppServerConnection
	{
		public static Boolean TrustEveryoneValidator(Object sender, X509Certificate certificate,
		                                             X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
        static String CookieValue(String cookies, String cookieName)
        {
            if (String.IsNullOrEmpty(cookies))
                return null;

            Int32 indexOfCookie = cookies.IndexOf(cookieName);

            if (indexOfCookie < 0)
                return null;
            indexOfCookie += cookieName.Length;

            Int32 indexOfSemicolon = cookies.IndexOf(';');
            if (indexOfSemicolon < 0)
            {
                return cookies.Substring(indexOfCookie);
            }
            return cookies.Substring(indexOfCookie, indexOfSemicolon - indexOfCookie);
        }

        readonly WorkerThread workerThread;

		//Socket insecureSocket;
		//Socket secureSocket;

		String sessionID;

		public CampaignAppServerConnection()
        {
            ServicePointManager.ServerCertificateValidationCallback = TrustEveryoneValidator;
            this.workerThread = new WorkerThread();
		}
		/*
		void SecureConnect()
		{
			if (secureSocket == null || !secureSocket.Connected)
			{
				secureSocket = new Socket (CampaignAppServer.httpsCampaignAppServerEndPoint.AddressFamily,
				                          SocketType.Stream, ProtocolType.Tcp);
			}
		}
		*/

		public void SignIn(String usernameOrEmail, String password, Action successCallback, FailCallback failCallback)
		{
            if (sessionID != null)
            {
                failCallback("You are already signed in");
                return;
            }
            workerThread.Add(new SignInObject(this, usernameOrEmail, password,
                successCallback, failCallback).SignIn);
		}



        
        class SignInObject
        {
            readonly CampaignAppServerConnection connection;
            readonly String usernameOrEmail, password;
            readonly Action successCallback;
            readonly FailCallback failCallback;
            public SignInObject(CampaignAppServerConnection connection, String usernameOrEmail,
                String password, Action successCallback, FailCallback failCallback)
            {
                this.connection = connection;
                this.usernameOrEmail = usernameOrEmail;
                this.password = password;
                this.successCallback = successCallback;
                this.failCallback = failCallback;
            }
            public void SignIn()
            {
                try
                {
                    if (connection.sessionID != null)
                    {
                        failCallback("You are already signed in");
                        return;
                    }

                    //
                    // Create post data
                    //
                    Byte[] postData = Encoding.ASCII.GetBytes(String.Format (
                        "email={0}&password={1}",
                    HttpUtility.UrlEncode(usernameOrEmail),
                    HttpUtility.UrlEncode(password)));

                    //
                    // Create Http Request
                    //
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("https://{0}/login.php", CampaignAppServer.HostName));
                    request.Method = "POST";
                    request.AllowAutoRedirect = false;
                    request.UserAgent = "TheCampaignApp";
                    request.ContentType = "application/x-www-form-urlencoded";
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(postData, 0, postData.Length);
                    requestStream.Close();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    String sessionID = CookieValue(response.GetResponseHeader("Set-Cookie"), "sid=");
                    if(String.IsNullOrEmpty(sessionID))
                    {
                        failCallback("Invalid Email/Username or Password");
                        return;
                    }
                    connection.sessionID = sessionID;
                    successCallback();
                }
                catch(Exception e)
                {
                    failCallback(e.ToString());
                }
            }
        }

    }

}

