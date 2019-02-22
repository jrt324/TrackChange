using System.Collections.Generic;
using Mono.Cecil;

public class MethodFinder
{
    List<TypeDefinition> allTypes;
    public List<MethodDefinition> MethodsToProcess = new List<MethodDefinition>();

    public MethodFinder(List<TypeDefinition> allTypes)
    {
        this.allTypes = allTypes;
    }

    public void Execute()
    {
        foreach (var type in allTypes)
        {
            if (type.IsInterface)
            {
                continue;
            }
            if (type.IsEnum)
            {
                continue;
            }
            foreach (var method in type.Methods)
            {
                MethodsToProcess.Add(method);
            }
        }
    }
}