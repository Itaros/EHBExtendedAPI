using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EHBInjector.Injectors.API
{
    public class InjectScrewdriverAPI : InjectSprakAPI
    {

        public InjectScrewdriverAPI(TypeDefinition source, TypeDefinition target):base(source,target){

        }

        public override void Execute()
        {
            //Adding RemoveWatchdog()!
            ILProcessor ilProc;

            MethodDefinition apiRemoveWatchdog = CreateAPIDummy(out ilProc, "RemoveWatchdog", new string[]{"Makes computer to run forever"});
            FillRemoveWatchdog(ilProc);

            MethodDefinition apiSetWatchdog = CreateAPIDummy(out ilProc, "SetWatchdog", new string[] { "Sets maximum execution time in seconds" });
            apiSetWatchdog.Parameters.Add(new ParameterDefinition("time", ParameterAttributes.None, _target.Module.TypeSystem.Single));
            FillSetWatchdog(ilProc);

            MethodDefinition apiResetWatchdog = CreateAPIDummy(out ilProc, "ResetWatchdog", new string[] { "Resets watchdog timer, but keeps max time unchanged" });
            FillResetWatchdog(ilProc);

            _target.Methods.Add(apiRemoveWatchdog);
            _target.Methods.Add(apiSetWatchdog);
            _target.Methods.Add(apiResetWatchdog);
        }

        private void FillResetWatchdog(ILProcessor ilProc)
        {
            Instruction ldarg = Instruction.Create(OpCodes.Ldarg_0);

            FieldReference targetField = new FieldReference("_computerTarget", GetCommonTingComputer(), _target);
            Instruction targetToStack = Instruction.Create(OpCodes.Ldfld, targetField);

            MethodReference injectoid = _source.Methods.First(o => o.Name == "ResetWatchdog");
            injectoid = _target.Module.Import(injectoid);
            Instruction callInjectoid = Instruction.Create(OpCodes.Call, injectoid);

            Instruction ret = Instruction.Create(OpCodes.Ret);

            ilProc.Append(ldarg);
            ilProc.Append(targetToStack);
            ilProc.Append(callInjectoid);
            ilProc.Append(ret);
        }

        private void FillSetWatchdog(ILProcessor ilProc)
        {
            Instruction ldarg = Instruction.Create(OpCodes.Ldarg_0);

            FieldReference targetField = new FieldReference("_computerTarget", GetCommonTingComputer(), _target);
            Instruction targetToStack = Instruction.Create(OpCodes.Ldfld, targetField);

            Instruction ldargArg1 = Instruction.Create(OpCodes.Ldarg_1);

            MethodReference injectoid = _source.Methods.First(o => o.Name == "SetWatchdog");
            injectoid = _target.Module.Import(injectoid);
            Instruction callInjectoid = Instruction.Create(OpCodes.Call, injectoid);

            Instruction ret = Instruction.Create(OpCodes.Ret);

            ilProc.Append(ldarg);
            ilProc.Append(targetToStack);
            ilProc.Append(ldargArg1);
            ilProc.Append(callInjectoid);
            ilProc.Append(ret);

        }

        private void FillRemoveWatchdog(Mono.Cecil.Cil.ILProcessor ilRemoveWatchdog)
        {
            Instruction ldarg = Instruction.Create(OpCodes.Ldarg_0);

            FieldReference targetField = new FieldReference("_computerTarget", GetCommonTingComputer(), _target);
            Instruction targetToStack = Instruction.Create(OpCodes.Ldfld, targetField);

            MethodReference injectoid = _source.Methods.First(o=>o.Name == "RemoveWatchdog");
            injectoid = _target.Module.Import(injectoid);
            Instruction callInjectoid = Instruction.Create(OpCodes.Call, injectoid);

            Instruction ret = Instruction.Create(OpCodes.Ret);

            ilRemoveWatchdog.Append(ldarg);
            ilRemoveWatchdog.Append(targetToStack);
            ilRemoveWatchdog.Append(callInjectoid);
            ilRemoveWatchdog.Append(ret);
        }
    }
}
