using ProgrammingLanguageNr1;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameWorld2
{
    class GetMasterProgramHackdevOverride : MimanTing
    {

        private Program _program;

        public override Program masterProgram
        {
            get
            {
                if (this._program == null)
                {
                    this._program = base.EnsureProgram("MasterProgram", this.masterProgramName);
                    List<FunctionDefinition> list = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Hackdev)));
                    list.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, this._tingRunner, this.masterProgram), typeof(ConnectionAPI)));
                    this._program.FunctionDefinitions = list;
                }
                return this._program;
            }
        }

        public string masterProgramName { get; set; }


        public override bool DoesMasterProgramExist()
        {
            return true;
        }
    }
}
