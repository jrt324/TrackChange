using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CecilExtensions
{

    public static bool IsRefOrOut(this Instruction next)
    {
        if (next.OpCode != OpCodes.Call && next.OpCode != OpCodes.Calli)
        {
            return false;
        }
        var methodReference = next.Operand as MethodReference;
        if (methodReference == null)
        {
            return false;
        }
        return methodReference.Parameters.Any(x => x.IsOut || x.ParameterType.Name.EndsWith("&"));
    }

    public static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
    {
        if (self.GenericParameters.Count != arguments.Length)
            throw new ArgumentException();

        var instance = new GenericInstanceType(self);
        foreach (var argument in arguments)
            instance.GenericArguments.Add(argument);

        return instance;
    }

    public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType)
        {
            DeclaringType = self.DeclaringType.MakeGenericType(arguments),
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            CallingConvention = self.CallingConvention,
        };

        foreach (var parameter in self.Parameters)
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

        foreach (var generic_parameter in self.GenericParameters)
            reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

        return reference;
    }

    public static MethodReference MakeSpecialTypeGeneric(this MethodReference self, TypeReference ownRealType, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType)
        {
            DeclaringType = ownRealType,
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            CallingConvention = self.CallingConvention,
        };

        foreach (var parameter in self.Parameters)
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

        foreach (var generic_parameter in self.GenericParameters)
            reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

        return reference;
    }

}