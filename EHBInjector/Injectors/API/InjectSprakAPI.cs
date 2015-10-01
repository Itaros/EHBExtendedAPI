using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EHBInjector.Injectors.API
{
    public abstract class InjectSprakAPI
    {

        protected TypeDefinition _source;
        protected TypeDefinition _target;

        public InjectSprakAPI(TypeDefinition source, TypeDefinition target)
        {
            _source = source;
            _target = target;
            Validate();
        }

        private void Validate()
        {
            if (_source == null | _target == null)
            {
                throw new ArgumentException("Typedefs are null!");
            }
        }


        protected TypeReference TypesystemBlob
        {
            get { return _target.Module.TypeSystem.Byte; }
        }
        protected TypeReference TypesystemSprakdefArray
        {
            get { return _sprakAttribute.Parameters[0].ParameterType; }
            //get { return _target.Module.TypeSystem.String; }
            //get { return _target.Module.Import(typeof(string[])); }
        }

        protected TypeReference TypesystemVoid{
            get{return _target.Module.TypeSystem.Void;}
        }

        private MethodReference _sprakAttribute;
        protected MethodReference TypesystemSprakApiArgument
        {
            get
            {
                return _sprakAttribute;
            }
        }

        private ModuleDefinition _sprak;
        public ModuleDefinition Sprak
        {
            protected get { return _sprak; }
            set { _sprak = value; FillTypesystem(); }
        }

        private void FillTypesystem()
        {
            var typedef = _sprak.GetType("ProgrammingLanguageNr1", "SprakAPI");
            var method = typedef.Methods[0];
            _sprakAttribute = _target.Module.Import(method);

            //var md = _sprakAttribute.Resolve();
            //md.Parameters[0].ParameterType = TypesystemSprakdefArray;
        }

        public abstract void Execute();


        protected TypeReference GetCommonTingComputer()
        {
            return _target.Module.GetType("GameWorld2","Computer");
        }

        protected CustomAttributeArgument GetSprakDocs(params string[] text)
        {
            var prm = new CustomAttributeArgument[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                prm[i] = new CustomAttributeArgument(_target.Module.TypeSystem.String, text[i]);
            }

            var root = new CustomAttributeArgument(TypesystemSprakdefArray,prm);

            return root;
        }

        protected MethodDefinition CreateAPIDummy(out ILProcessor ilProc, string apitoken, string[] man)
        {
            CustomAttribute defRemoveWatchdog = new CustomAttribute(TypesystemSprakApiArgument)
            {
                ConstructorArguments = { GetSprakDocs(man) }//new CustomAttributeArgument[] { new CustomAttributeArgument(_target.Module.TypeSystem.String,"well") })
            };
            MethodDefinition methodDef = new MethodDefinition("API_" + apitoken, MethodAttributes.Public, TypesystemVoid);
            methodDef.CustomAttributes.Add(defRemoveWatchdog);
            ilProc = methodDef.Body.GetILProcessor();
            return methodDef;
        }

    }
}
