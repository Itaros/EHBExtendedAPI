using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHBInjector.Injectors.API.Appenders
{
    /// <summary>
    /// Adds conditional API selector to computers.
    /// For use with screwdrivers or Relay database
    /// </summary>
    public class AppendAPIToComputerSelector
    {

        private TypeDefinition _typeDefComputer;
        private MethodReference _methodRefAccessor;

        private MethodDefinition _overrideForeignCall;

        private MethodDefinition _methodDefApiSelector;

        public AppendAPIToComputerSelector(TypeDefinition typeDefComputer, MethodReference methodRefAccessor, MethodDefinition overrideForeignCall)
        {
            _typeDefComputer = typeDefComputer;
            _methodRefAccessor = methodRefAccessor;

            _overrideForeignCall = overrideForeignCall;

            _methodDefApiSelector = _typeDefComputer.Methods.First(o => o.Name == "GenerateProgramAPI");
        }

        public void Execute()
        {

            MethodReference overrideCall = _typeDefComputer.Module.Import(_overrideForeignCall);

            var ilProc = _methodDefApiSelector.Body.GetILProcessor();
            Instruction origin = _methodDefApiSelector.Body.Instructions[_methodDefApiSelector.Body.Instructions.Count-4];

            Instruction ldargThis = Instruction.Create(OpCodes.Ldarg_0);
            Instruction callProp = Instruction.Create(OpCodes.Call, _methodRefAccessor);
            Instruction skipJmp = Instruction.Create(OpCodes.Brfalse, origin);//Will it work if I add more?

            Instruction ldargThisToPass = Instruction.Create(OpCodes.Ldarg_0);
            Instruction ldlocThisToPass = Instruction.Create(OpCodes.Ldloc_0);
            //TODO: Additional shit

            Instruction callOverride = Instruction.Create(OpCodes.Call,overrideCall);

            Instruction stlocThisStackRestore = Instruction.Create(OpCodes.Stloc_0);

            ilProc.InsertBefore(origin,ldargThis);
            ilProc.InsertBefore(origin,callProp);
            ilProc.InsertBefore(origin,skipJmp);
            ilProc.InsertBefore(origin,ldargThisToPass);
            ilProc.InsertBefore(origin,ldlocThisToPass);
            ilProc.InsertBefore(origin,callOverride);
            ilProc.InsertBefore(origin, stlocThisStackRestore);
            skipJmp.Operand = stlocThisStackRestore.Next;

        }

    }
}
