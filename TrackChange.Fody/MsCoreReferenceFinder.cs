using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class MsCoreReferenceFinder
{
    private readonly ModuleWeaver _moduleWeaver;


    public MethodReference CompilerGeneratedReference;

    public MethodReference NonSerializedReference;


    public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
    {
        this._moduleWeaver = moduleWeaver;
    }
    
    public void Execute()
    {
        var module = _moduleWeaver.ModuleDefinition;
        
        var compilerGeneratedDefinition = _moduleWeaver.FindType("System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        CompilerGeneratedReference = module.ImportReference(compilerGeneratedDefinition.Resolve().Methods.First(x => x.IsConstructor));

        var nonSerializedReference = _moduleWeaver.FindType("System.NonSerializedAttribute");
        NonSerializedReference = module.ImportReference(nonSerializedReference.Resolve().Methods.First(x => x.IsConstructor));

    }

   

}