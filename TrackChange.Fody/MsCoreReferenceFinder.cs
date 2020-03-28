using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
#pragma warning disable 168

public class MsCoreReferenceFinder
{
    private readonly ModuleWeaver _moduleWeaver;


    public MethodReference CompilerGeneratedReference;

    public MethodReference NonSerializedReference;

    public MethodReference NotMappedAttributeReference;
    public MethodReference JsonIgnoreAttributeReference;

    public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
    {
        this._moduleWeaver = moduleWeaver;
    }

    public void Execute()
    {
        var module = _moduleWeaver.ModuleDefinition;

        CompilerGeneratedReference = GetTypeCtorReference(module, "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        NonSerializedReference = GetTypeCtorReference(module, "System.NonSerializedAttribute");
        NotMappedAttributeReference = GetTypeCtorReference(module, "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute");
        JsonIgnoreAttributeReference = GetTypeCtorReference(module, "System.Text.Json.Serialization.JsonIgnoreAttribute");

    }

    public MethodReference GetTypeCtorReference(ModuleDefinition module, string fullTypeName)
    {
        try
        {
            var typeReference = _moduleWeaver.FindTypeDefinition(fullTypeName);
            if (typeReference != null)
            {
                try
                {
                    var ctorReference = module.ImportReference(typeReference.Resolve().Methods.First(x => x.IsConstructor));
                    return ctorReference;
                }
                catch
                {
                }
            }
        }
        catch (Exception ex1)
        {
        }

        return null;
    }




}