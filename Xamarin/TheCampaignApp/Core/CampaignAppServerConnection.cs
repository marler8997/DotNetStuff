using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace CampaignApp
{
	public class CampaignAppServerConnection
	{
		public static Boolean TrustEveryoneValidator(Object sender, X509Certificate certificate,
		                                             X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		//Socket insecureSocket;
		//Socket secureSocket;

		String sessionID;

		public CampaignAppServerConnection()
		{
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

		// Returns null on success and a message on failure
		public String SignIn(String usernameOrEmail, String password)
		{
			//
			// For Testing
			//
			ServicePointManager.ServerCertificateValidationCallback = TrustEveryoneValidator;


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
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (String.Format("https://{0}/login.php", CampaignAppServer.HostName));
			request.Method = "POST";
			request.AllowAutoRedirect = false;
			request.UserAgent = "TheCampaignApp";
			request.ContentType = "application/x-www-form-urlencoded";
			Stream requestStream = request.GetRequestStream();
			requestStream.Write(postData, 0, postData.Length);
			requestStream.Close();

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream stream = response.GetResponseStream();
			String responseText = new StreamReader(stream).ReadToEnd();

			Cookie sessionIDCookie = response.Cookies["sid"];
			if(sessionIDCookie == null)
			{
				return responseText;
				//return "Invalid Email/Username or Password";
			}

			this.sessionID = sessionIDCookie.Value;
			return null;
		}
	}
}

