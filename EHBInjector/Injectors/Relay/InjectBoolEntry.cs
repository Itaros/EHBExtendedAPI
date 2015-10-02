using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHBInjector.Injectors.Relay
{
    public class InjectBoolEntry
    {

        protected TypeDefinition _target;

        protected string _tokenName;

        public MethodReference AccessorGet { get; protected set; }

        public InjectBoolEntry(TypeDefinition target, string tokenName)
        {
            _target = target;

            _tokenName = tokenName;
        }

        public void Execute(ModuleDefinition relay)
        {
            //Example signature:
            //.field private class [Relay]RelayLib.ValueEntry`1<bool> CELL_hasTrapAPI
            //Adding field
            var fieldExampleDef = _target.Fields.First(o => o.FieldType.FullName.Contains("<System.Boolean>"));//Hackery
            //Typedef
            TypeDefinition entryValueTypedef = fieldExampleDef.FieldType.Resolve();
            GenericInstanceType entryValueTypedefInstance = new GenericInstanceType(entryValueTypedef);
            entryValueTypedefInstance.GenericArguments.Add(relay.TypeSystem.Boolean);
            TypeReference entryValueDeclaringGenericType = _target.Module.Import(entryValueTypedefInstance);

            //Field
            FieldDefinition cellDef = new FieldDefinition("CELL_" + _tokenName, FieldAttributes.Private, fieldExampleDef.FieldType);
            _target.Fields.Add(cellDef);
            //cellDef.FieldType.GenericParameters.Add(new GenericParameter(_target.Module.TypeSystem.Boolean));
            //Attempting to inject into SetupCells()
            MethodDefinition methodSC = _target.Methods.First(o => o.Name == "SetupCells");
            if (methodSC != null)
            {
                Instruction ret = methodSC.Body.Instructions.First(o => o.OpCode == OpCodes.Ret);
                var ilProc = methodSC.Body.GetILProcessor();
                //We need to make sure there are two 'this'es on stack first
                Instruction ldargThis1 = Instruction.Create(OpCodes.Ldarg_0);
                Instruction ldargThis2 = Instruction.Create(OpCodes.Ldarg_0);
                //Then we need to load literal describing field name
                Instruction strLiteralFname = Instruction.Create(OpCodes.Ldstr,_tokenName);
                //Then we need to load literal of default value
                Instruction defvalLiteral = Instruction.Create(OpCodes.Ldc_I4_1);//True
                //Then we need to call EnsureCell which is generic.
                TypeDefinition trDeclaringType = _target.Module.Import(relay.GetType("RelayLib","RelayObjectTwo")).Resolve();
                MethodReference mrEnsureCell = _target.Module.Import(trDeclaringType.Methods.First(o=>o.Name.Contains("EnsureCell")));//new MethodReference("EnsureCell",trValueEntryBool,trDeclaringType);
                var mrGenericEnsureCell = new GenericInstanceMethod(mrEnsureCell);
                mrGenericEnsureCell.GenericArguments.Add(_target.Module.TypeSystem.Boolean);
                //mrEnsureCell.GenericParameters.Add(new GenericParameter())
                Instruction callEnsureCell = Instruction.Create(OpCodes.Call,mrGenericEnsureCell);
                //Setting variable
                Instruction stfld = Instruction.Create(OpCodes.Stfld, cellDef);

                //Filling
                ilProc.InsertBefore(ret,ldargThis1);
                ilProc.InsertBefore(ret, ldargThis2);
                ilProc.InsertBefore(ret, strLiteralFname);
                ilProc.InsertBefore(ret, defvalLiteral);
                ilProc.InsertBefore(ret, callEnsureCell);
                ilProc.InsertBefore(ret, stfld);
            }
            //Creating accessors
            PropertyDefinition accessorDef = new PropertyDefinition(_tokenName, PropertyAttributes.None, _target.Module.TypeSystem.Boolean);
            accessorDef.GetMethod = new MethodDefinition("get_" + _tokenName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, _target.Module.TypeSystem.Boolean);
            accessorDef.SetMethod = new MethodDefinition("set_" + _tokenName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, _target.Module.TypeSystem.Void);
            accessorDef.SetMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, _target.Module.TypeSystem.Boolean));
            accessorDef.GetMethod.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            accessorDef.SetMethod.SemanticsAttributes = MethodSemanticsAttributes.Setter;

            var ilProcGet = accessorDef.GetMethod.Body.GetILProcessor();
            var ilProcSet = accessorDef.SetMethod.Body.GetILProcessor();

            //GET IL
            Instruction getLdarg = Instruction.Create(OpCodes.Ldarg_0);
            Instruction getLdfld = Instruction.Create(OpCodes.Ldfld, cellDef);

            MethodReference getGetDataRef = _target.Module.Import(entryValueTypedef.Properties[0].GetMethod);
            getGetDataRef.DeclaringType = entryValueDeclaringGenericType;
            Instruction getCallvirt = Instruction.Create(OpCodes.Callvirt, getGetDataRef);
            Instruction getRet = Instruction.Create(OpCodes.Ret);
            ilProcGet.Append(getLdarg);
            ilProcGet.Append(getLdfld);
            ilProcGet.Append(getCallvirt);
            ilProcGet.Append(getRet);
            //SET IL
            Instruction setLdarg = Instruction.Create(OpCodes.Ldarg_0);
            Instruction setLdfld = Instruction.Create(OpCodes.Ldfld, cellDef);
            Instruction setLdargVal = Instruction.Create(OpCodes.Ldarg_1);
            MethodReference setGetDataRef = _target.Module.Import(entryValueTypedef.Properties[0].SetMethod);
            setGetDataRef.DeclaringType = entryValueDeclaringGenericType;            
            Instruction setCallvirt = Instruction.Create(OpCodes.Callvirt, setGetDataRef);
            Instruction setRet = Instruction.Create(OpCodes.Ret);
            ilProcSet.Append(setLdarg);
            ilProcSet.Append(setLdfld);
            ilProcSet.Append(setLdargVal);
            ilProcSet.Append(setCallvirt);
            ilProcSet.Append(setRet);

            //Adding
            _target.Methods.Add(accessorDef.GetMethod);
            _target.Methods.Add(accessorDef.SetMethod);
            _target.Properties.Add(accessorDef);

            AccessorGet = accessorDef.GetMethod;

        }

    }
}
