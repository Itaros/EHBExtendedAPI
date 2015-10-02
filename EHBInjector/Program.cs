using EHBInjector.Injectors.API;
using EHBInjector.Injectors.API.Appenders;
using EHBInjector.Injectors.Relay;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EHBInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetPath = @"_GameWorld2.dll";
            string newPath = @"GameWorld2.dll";

            Console.WriteLine("EHB Injector Version 1.0a by Itaros");

            if (args.Where(o => o == "-uninstall").Count() != 0)
            {
                Console.WriteLine("Restoring original Game Core:");
                Uninstall(newPath, targetPath);
                End(args);
                return;
            }

            Console.WriteLine("Deploying ExtendedAPIs Binary Modification:");


            CreateBackupCopyOfMasterDLL(newPath, targetPath);

            //Loading proto
            Console.WriteLine("Loading definition of Prototype.dll");
            AssemblyDefinition proto = AssemblyDefinition.ReadAssembly(@"Prototype.dll");
            TypeDefinition protoDefinition = proto.MainModule.GetType("Prototype", "Override");

            //Loading sprak!
            Console.WriteLine("Loading definition of ProgrammingLanguageNr1.dll(Sprak runtime)");
            AssemblyDefinition sprak = AssemblyDefinition.ReadAssembly(@"ProgrammingLanguageNr1.dll");
            
            //Loading Relay!
            Console.WriteLine("Loading definition of Relay.dll(Database)");
            AssemblyDefinition relay = AssemblyDefinition.ReadAssembly(@"Relay.dll");
            

            //Getting target
            Console.WriteLine("Loading definition of GameWorld2.dll(Game core)");
            AssemblyDefinition target = AssemblyDefinition.ReadAssembly(targetPath);
            Console.WriteLine("Attempting to perform modifications...");
            //target.MainModule.Import();
            TypeDefinition targetDefinition = target.MainModule.GetType("GameWorld2", "Hackdev");
            MethodDefinition targetMethod = targetDefinition.Methods.First(o => o.Name == "get_masterProgram");
            targetMethod.Body.Variables.Add(new VariableDefinition(target.MainModule.TypeSystem.Object));

            //Getting _tingRunner accessor
            Instruction ldfldToTingrunner = target.MainModule.GetType("GameWorld2", "Radio").Methods.First(o => o.Name == "get_masterProgram").Body.Instructions.Where(o=>o.OpCode==OpCodes.Ldfld).ElementAt(1);
            Instruction getNewTingRunner = Instruction.Create(ldfldToTingrunner.OpCode, (FieldReference)ldfldToTingrunner.Operand);

            var ilproc = targetMethod.Body.GetILProcessor();

            Instruction defset = null;
            //Looking for assigment
            for (int i = 0; i < targetMethod.Body.Instructions.Count; i++)
            {
                var instr = targetMethod.Body.Instructions[i];
                if (instr != null)
                {
                    if (instr.OpCode == OpCodes.Callvirt)
                    {
                        defset = instr;
                        break;
                    }
                }
            }

            if (defset == null)
            {
                throw new InvalidProgramException("Nowhere to attach!");
            }

            //Pushing calls

            var screwdriverAPIExtended = new InjectScrewdriverAPI(proto.MainModule.GetType("Prototype.CustomAPI.Injected", "ScrewdriverInjectoid"), target.MainModule.GetType("GameWorld2", "Screwdriver"));
            screwdriverAPIExtended.Sprak = sprak.MainModule;
            screwdriverAPIExtended.Execute();

            var injectCellHasExternalHTTPAPI = new InjectBoolEntry(target.MainModule.GetType("GameWorld2", "Computer"), "hasExternalHTTPAPI");
            injectCellHasExternalHTTPAPI.Execute(relay.MainModule);

            MethodDefinition appendApiToComputerSelectorExternalHTTP = proto.MainModule.GetType("Prototype.CustomAPI.Full", "ExternalHTTPAPI").Methods.First(o=>o.Name=="AssembleDefinitions");
            AppendAPIToComputerSelector selectorExternalHTTPAPI = new AppendAPIToComputerSelector(target.MainModule.GetType("GameWorld2", "Computer"), injectCellHasExternalHTTPAPI.AccessorGet, appendApiToComputerSelectorExternalHTTP);
            selectorExternalHTTPAPI.Execute();

            Console.WriteLine("Writing new version...");

            target.Write(newPath);

            End(args);
            //proto.Write(@"D:\SteamLibrary\steamapps\common\ElseHeartbreak\ElseHeartbreak_Data\Managed\Prototype.dll");

        }

        private static void End(string[] args)
        {
            Console.WriteLine("Done!");

            if (args.Where(o => o == "-silent").Count() == 0)
            {
                Console.WriteLine("Press any key to continue!");
                Console.ReadKey();
            }
        }

        private static void CreateBackupCopyOfMasterDLL(string origin, string target)
        {
            //If target exists there is no need to copy as it is origin already
            if (!File.Exists(target))
            {
                Console.WriteLine("Copying origin!");
                File.Copy(origin,target);
            }
        }

        private static void Uninstall(string origin, string target)
        {
            File.Delete(origin);
            File.Copy(target, origin);
            Console.WriteLine("Origin is restored!");
        }

    }
}
