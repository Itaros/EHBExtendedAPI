using GameWorld2;
using ProgrammingLanguageNr1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Prototype.CustomAPI.Full
{
    public class ExternalHTTPAPI
    {

        public static object AssembleDefinitions(object ting, object list)
        {
            List<FunctionDefinition> fd = (List<FunctionDefinition>)list;
            fd.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ExternalHTTPAPI(ting as Computer), typeof(ExternalHTTPAPI)));
            return fd;
        }

        private Computer _pComputer;

        public ExternalHTTPAPI(Computer pComputer)
        {
            if (pComputer == null)
            {
                throw new ArgumentException("Computer was expected!");
            }
            _pComputer = pComputer;
        }

        [SprakAPI("Activates HTTP Modem to connect to specified uri")]
        public int API_HTTPConnect(string uri)
        {
            return MasterHost.TryAcquire(_pComputer, uri);
        }

        [SprakAPI("Checks if HTTP Modem has received data")]
        public bool API_HTTPIsReady(float id)
        {
            return MasterHost.AcquireIsReady(_pComputer, (int)id);
        }

        [SprakAPI("Gets how much lines HTTP Modem has received")]
        public float API_HTTPCount(float id)
        {
            return MasterHost.AcquireCount(_pComputer, (int)id);
        }
        [SprakAPI("Query HTTP Modem to receive a line")]
        public string API_HTTPDemux(float id, float line)
        {
            int intline = (int)line;
            return MasterHost.AcquireLine(_pComputer, (int)id, intline);
        }

        [SprakAPI("Query HTTP Modem to close external connections")]
        public void API_HTTPCloseAll()
        {
            MasterHost.CloseAll(_pComputer);
        }

        private static HttpHost MasterHost { get; set; }

        static ExternalHTTPAPI()
        {
            MasterHost = new HttpHost();
        }


        /// <summary>
        /// Master class to maintain centralized control over HTTP queries in the game
        /// </summary>
        public class HttpHost
        {

            private WebClient _client;

            public HttpHost()
            {
                _client = new WebClient();
                _client.Headers.Add("user-agent", "DorisburgTerminalModem/1.0");
                _client.OpenReadCompleted += OnDoneReading;
            }

            private void OnDoneReading(object sender, OpenReadCompletedEventArgs e)
            {
                HttpReadQueueElement rqe = e.UserState as HttpReadQueueElement;
                rqe.Result = e.Result;
                if (!rqe.RequestDiscard)
                {
                    rqe.Parse();
                }
                _current = null;
                TryQueueNext();
            }

            public int TryAcquire(Computer owner, string uri)
            {
                return TryAcquire(owner, new Uri(uri));
            }

            public int TryAcquire(Computer owner, Uri uri)
            {
                HttpReadQueueElement rqe = new HttpReadQueueElement(owner, uri);
                rqe.QueryID = GetUniqueID();
                //_client.OpenReadAsync(uri, rqe);
                _fastlook.Add(rqe.QueryID, rqe);
                lock (_lockQueue)
                {
                    _awaitingProcess.Enqueue(rqe);
                }
                TryQueueNext();
                return rqe.QueryID;
            }

            private void TryQueueNext()
            {
                if (_current == null)
                {
                    if (_awaitingProcess.Count > 0)
                    {
                        lock (_lockQueue)
                        {
                            if (_current == null)
                            {
                                if (_awaitingProcess.Count > 0)
                                {
                                    HttpReadQueueElement dec = _awaitingProcess.Dequeue();
                                    _current = dec;
                                    _client.OpenReadAsync(_current.Uri,_current);
                                }
                            }
                        }
                    }
                }
            }

            private int GetUniqueID()
            {
                int candidate = 1;
                foreach(HttpReadQueueElement element in _fastlook.Values)
                {
                    if (element.QueryID >= candidate)
                    {
                        candidate = element.QueryID + 1;
                    }
                }
                return candidate;
            }

            private object _lockQueue = new object();
            private Queue<HttpReadQueueElement> _awaitingProcess = new Queue<HttpReadQueueElement>();
            private Dictionary<int, HttpReadQueueElement> _fastlook = new Dictionary<int, HttpReadQueueElement>();
            private HttpReadQueueElement _current;

            private class HttpReadQueueElement
            {

                public bool RequestDiscard { get; set; }

                private Stream _result;
                public Stream Result { set { _result = value; } }
                public string[] Lines { get { return _lines; } }
                public bool IsReady { get { return _result != null || _lines!=null; } }

                private string[] _lines;

                private Computer _owner;
                public Computer Owner { get { return _owner; } }
                private Uri _uri;

                public Uri Uri { get { return _uri; } }

                public int QueryID { get; set; }

                public HttpReadQueueElement(Computer owner, Uri uri)
                {
                    _owner = owner;
                    _uri = uri;
                }


                public bool Security(Computer computer)
                {
                    return computer == _owner;
                }

                public void Parse()
                {
                    StreamReader str = new StreamReader(_result);
                    List<string> rslt = new List<string>();
                    while (!str.EndOfStream)
                    {
                        rslt.Add(str.ReadLine());
                    }
                    _result = null;
                    _lines = rslt.ToArray();
                }
            }


            public bool AcquireIsReady(Computer computer, int id)
            {
                HttpReadQueueElement rqe;
                //computer.API_Print("Debug: Acquiring QueueElement...");
                if (_fastlook.TryGetValue(id, out rqe))
                {
                    if (rqe.Security(computer))
                    {
                        return rqe.IsReady;
                    }
                    else
                    {
                        return false;//No acccess!
                    }
                }
                else
                {
                    return true;
                }
            }

            public int AcquireCount(Computer computer, int id)
            {
               HttpReadQueueElement rqe;
               if (_fastlook.TryGetValue(id, out rqe))
               {
                   if (rqe.Security(computer))
                   {
                       if (rqe.Lines != null)
                       {
                           return rqe.Lines.Length;
                       }
                       else
                       {
                           return -3;//Data is gone
                       }
                   }
                   else
                   {
                       return -2;//No access
                   }
               }
               else
               {
                   return -1;//No connection
               }
            }

            public string AcquireLine(Computer computer, int id, int intline)
            {
               HttpReadQueueElement rqe;
               if (_fastlook.TryGetValue(id, out rqe))
               {
                   if (rqe.Security(computer))
                   {
                       return rqe.Lines[intline];
                   }
                   else
                   {
                       return string.Empty;//No access
                   }
               }
               else
               {
                   return string.Empty;//No connection
               }
            }

            public void CloseAll(Computer computer)
            {
                List<HttpReadQueueElement> deletionQueue = new List<HttpReadQueueElement>();
                foreach (var item in _fastlook.Values)
                {
                    if (item.Owner == computer)
                    {
                        item.RequestDiscard = true;
                        deletionQueue.Add(item);
                    }
                }
                foreach (var item in deletionQueue)
                {
                    _fastlook.Remove(item.QueryID);
                }
            }
        }

    }
}
