using GameWorld2;
using ProgrammingLanguageNr1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototype
{
    public class Override
    {
        public static object AddAPIConnection(MimanTing ting, object list, TingTing.TingRunner runner)
        {
            //List<FunctionDefinition> list
            List<FunctionDefinition> fd = (List<FunctionDefinition>)list;
            fd.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(ting, runner, ting.masterProgram), typeof(ConnectionAPI)));
            return fd;
        }
        //public static void AddAPIConnection(MimanTing ting, List<FunctionDefinition> list, TingTing.TingRunner runner )
        //{
        //    list.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(ting, runner, ting.masterProgram), typeof(ConnectionAPI)));
        //}

    }
}
