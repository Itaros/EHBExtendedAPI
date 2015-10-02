using GameWorld2;
using ProgrammingLanguageNr1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototype.CustomAPI.Full
{
    public class ExternalHTTPAPI
    {

        private Computer _pComputer;

        public ExternalHTTPAPI(Computer pComputer)
        {
            _pComputer = pComputer;
        }

        [SprakAPI("Fetches HTTP message from specified host")]
        public string API_Fetch()
        {
            return string.Empty;
        }

    }
}
