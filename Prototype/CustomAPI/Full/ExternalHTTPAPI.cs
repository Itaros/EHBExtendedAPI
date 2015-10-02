using GameWorld2;
using ProgrammingLanguageNr1;
using System;
using System.Collections.Generic;
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

        [SprakAPI("Fetches HTTP message from specified host")]
        public string API_Fetch()
        {
            return string.Empty;
        }

    }
}
