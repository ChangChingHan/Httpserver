using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web;

namespace Httpserver
{
    public class ClientRequest
    {
        public bool RequestString = false;
        public string Method, Url, Protocol;
        public Dictionary<string, string> Headers;
        public ClientRequest(StreamReader sr)
        {
            string firstLine = sr.ReadLine();
            if (firstLine != null)
            {
                RequestString = true;
                string[] p = firstLine.Split(' ');
                Method = p[0];
                Url = (p.Length > 1) ? p[1] : "NA";
				Url = HttpUtility.UrlDecode(Url);
                Protocol = (p.Length > 2) ? p[2] : "NA";
                string line = null;
                Headers = new Dictionary<string, string>();
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    int pos = line.IndexOf(":");
                    if (pos > -1)
                        Headers.Add(line.Substring(0, pos),
                            line.Substring(pos + 1));
                }
            }
        }
    }

    public class ClientResponse
    {
        public class HttpStatus
        {
            public static string Http200 = "200 OK";
            public static string Http404 = "404 Not Found";
            public static string Http500 = "500 Error";
        }
        public string StatusText = HttpStatus.Http200;
        public string ContentType = "text/plain";
        public Dictionary<string, string> Headers
            = new Dictionary<string, string>();
        public byte[] Data = new byte[] { };
        public string StringData = "";
    }

    public class MicroHttpServer
    {
        private Thread serverThread;
        TcpListener listener;
        public MicroHttpServer(int port,
            Func<ClientRequest, ClientResponse> reqProc)
        {
			IPAddress ipAddr = IPAddress.Any;
            listener = new TcpListener(ipAddr, port);
            serverThread = new Thread(() =>
            {
				byte[] buffer = new byte[1024];
                listener.Start();
                while (true)
                {
                    Socket s = listener.AcceptSocket();
                    NetworkStream ns = new NetworkStream(s);
                    StreamReader sr = new StreamReader(ns);
                    ClientRequest req = new ClientRequest(sr);
                    if (req.RequestString == true)
                    {
                        ClientResponse resp = reqProc(req);
                        StreamWriter sw = new StreamWriter(ns);

						resp.StringData = req.Url;
                        sw.WriteLine("HTTP/1.1 {0}", resp.StatusText);
                        sw.WriteLine("Content-Type: " + resp.ContentType);
                        foreach (string k in resp.Headers.Keys)
                            sw.WriteLine("{0}: {1}", k, resp.Headers[k]);

						byte[] senddata = Encoding.UTF8.GetBytes(resp.StringData);
						sw.WriteLine("Content-Length: {0}", senddata.Length);
                        sw.WriteLine("Connection: Keep-Alive");
                        sw.WriteLine();
                        sw.Flush();

						s.Send(senddata);
                    }
                    //s.Shutdown(SocketShutdown.Both);
                    //ns.Close();
                    Thread.Sleep(1000);
                }
            });
            serverThread.Start();
        }
        public void Stop()
        {
            listener.Stop();
            serverThread.Abort();
        }
    }
}
