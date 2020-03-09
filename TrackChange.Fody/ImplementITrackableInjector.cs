#pragma warning disable 618

using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeSystem = Mono.Cecil.TypeSystem;
using System;

public class ImplementITrackableInjector
{
    private readonly MsCoreReferenceFinder msCoreReferenceFinder;
    private readonly TypeSystem typeSystem;
    private readonly List<TypeDefinition> _allPocoTypes;
    private readonly ModuleWeaver moduleWeaver;

    private InterfaceImplementation trackInterfaceImplementation = null;

    private TypeReference trackableTypeRef = null;
    private TypeReference dicPropTypeRef = null;

    public ImplementITrackableInjector(ModuleWeaver moduleWeaver, MsCoreReferenceFinder msCoreReferenceFinder,
        TypeSystem typeSystem, List<TypeDefinition> allPocoTypes)
    {
        this.moduleWeaver = moduleWeaver;
        this.msCoreReferenceFinder = msCoreReferenceFinder;
        this.typeSystem = typeSystem;
        this._allPocoTypes = allPocoTypes;
    }


    private static IEnumerable<TypeDefinition> GetHierarchy(TypeDefinition type)
    {
        while (type != null)
        {
            yield return type;
            if (type.BaseType == null)
            {
                type = null;
            }
            else
            {
                try
                {
                    type = type.BaseType?.Resolve();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    type = null;
                }
            }
        }
    }

    public void Execute()
    {
        var orderdPocoTypes = _allPocoTypes.OrderBy(t => GetHierarchy(t).Count()).ToList();
        foreach (var type in orderdPocoTypes)
        {
            if (!type.IsInterface && !type.IsValueType && !type.IsEnum)
            {
                InjectImplInterface(type);
            }
        }
    }

    public bool HasInterface(TypeDefinition type, string interfaceFullName)
    {
        return (type.Interfaces.Any(i => i.InterfaceType.FullName.Equals(interfaceFullName))
                || type.NestedTypes.Any(t => HasInterface(t, interfaceFullName)));
    }

    /// <summary>
    /// Process POCO Type implement ITrackable interface
    /// </summary>
    /// <param name="type">POCO Type</param>
    private void InjectImplInterface(TypeDefinition type)
    {
        moduleWeaver.LogInfo("Process Type impl Interface");

        var attr = type.GetTrackAttribute();
        if (attr != null)
        {
            // Check Type is Implement the ITrackable interface
            var hasITrackableInterface = type.Interfaces.Any(i => i.InterfaceType.Name == "ITrackable");
            if (hasITrackableInterface)
            {
                return;
            }

            if (trackableTypeRef == null)
            {
                var bronzeAssam = attr.AttributeType.Module.Assembly;
                var assName = attr.AttributeType.Scope.Name;
                var reference = attr.AttributeType.Module.AssemblyReferences.SingleOrDefault(a => a.Name == assName);
                if (reference != null)
                {
                    bronzeAssam = moduleWeaver.ModuleDefinition.AssemblyResolver.Resolve(reference);
                }

                TypeReference trackableType = bronzeAssam.MainModule.GetTypes().FirstOrDefault(t => t.Name.EndsWith(".ITrackable") || t.Name == "ITrackable");
                trackableTypeRef = this.moduleWeaver.ModuleDefinition.ImportReference(trackableType);
            }

            if (dicPropTypeRef == null)
            {
                dicPropTypeRef = trackableTypeRef.Resolve().Properties.SingleOrDefault(p => p.Name == "ModifiedProperties").PropertyType;
                dicPropTypeRef = moduleWeaver.ModuleDefinition.ImportReference(dicPropTypeRef);
            }
            var dicTypeDefinition = dicPropTypeRef.Resolve();

            // Add IsTracking property
            var isTrackingProp = InjectProperty(type, "IsTracking", typeSystem.Boolean, true);
            if (!isTrackingProp.FromBaseClass)
            {
                isTrackingProp.Prop.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.NonSerializedReference));
                if (msCoreReferenceFinder.JsonIgnoreAttributeReference!=null)
                {
                    isTrackingProp.Prop.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.JsonIgnoreAttributeReference));
                }
            }

            // Add ModifiedProperties property
            var modifiedPropertiesProp = InjectProperty(type, "ModifiedProperties", dicPropTypeRef, true);
            if (!modifiedPropertiesProp.FromBaseClass)
            {
                modifiedPropertiesProp.Prop.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.NonSerializedReference));
                if (msCoreReferenceFinder.JsonIgnoreAttributeReference != null)
                {
                    modifiedPropertiesProp.Prop.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.JsonIgnoreAttributeReference));
                }
            }

            // Implement the ITrackable interface
            if (trackInterfaceImplementation == null)
            {
                trackInterfaceImplementation = new InterfaceImplementation(trackableTypeRef);
            }
            type.Interfaces.Add(trackInterfaceImplementation);

            if (modifiedPropertiesProp.Field != null)
            {
                var dicCtor = dicTypeDefinition.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 0);

                var dicConstructorInfoRef = type.Module.ImportReference(dicCtor);
                dicConstructorInfoRef = dicConstructorInfoRef.MakeGeneric(typeSystem.String, typeSystem.Boolean);

                var currentTypeCtor = type.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 0);

                if (currentTypeCtor != null)
                {
                    var processor = currentTypeCtor.Body.GetILProcessor();
                    var firstInstruction = currentTypeCtor.Body.Instructions[1];

                    var instructions = new List<Instruction>() {
                        processor.Create(OpCodes.Ldarg_0),
                        processor.Create(OpCodes.Newobj, dicConstructorInfoRef),
                        processor.Create(OpCodes.Stfld, modifiedPropertiesProp.Field),
                    };

                    foreach (var instruction in instructions)
                    {
                        processor.InsertBefore(firstInstruction, instruction);
                    }
                }
            }


            foreach (var property in type.Properties)
            {
                if (property.Name != "IsTracking" && property.Name != "ModifiedProperties"
                    && property.SetMethod != null && property.SetMethod.IsPublic)
                {
                    var propFieldStr = $"<{property.Name}>k__BackingField";
                    var propFieldDef = type.Fields.SingleOrDefault(f => f.Name == propFieldStr);
                    if (propFieldDef == null)
                    {
                        continue;
                    }

                    var md = property.SetMethod;
                    md.Body.Instructions.Clear();

                    var ins1 = md.Body.Instructions;
                    moduleWeaver.ModuleDefinition.ImportReference(property.PropertyType);

                    // .locals init( [0]bool V_0 )
                    // 如果InitLocals=false，则会变成.locals( [0]bool V_0 ) 
                    md.Body.InitLocals = true;

                    //定义变量
                    md.Body.Variables.Add(new VariableDefinition(typeSystem.Boolean));
                    md.Body.Variables.Add(new VariableDefinition(typeSystem.Boolean));
                    //end 定义变量

                    //IL_0000: nop
                    ins1.Add(Instruction.Create(OpCodes.Nop));

                    //IL_0001: ldarg.0
                    ins1.Add(Instruction.Create(OpCodes.Ldarg_0));

                    //IL_0002: call instance valuetype [mscorlib]System.Nullable`1<valuetype [mscorlib]System.DateTime> AssemblyToProcess.Class1::get_Prop1()
                    ins1.Add(Instruction.Create(OpCodes.Call, property.GetMethod));

                    //IL_0007: box valuetype [mscorlib]System.Nullable`1<valuetype [mscorlib]System.DateTime>
                    ins1.Add(Instruction.Create(OpCodes.Box, property.PropertyType));

                    //IL_000c: ldarg.1
                    ins1.Add(Instruction.Create(OpCodes.Ldarg_1));

                    //IL_000d: box valuetype [mscorlib]System.Nullable`1<valuetype [mscorlib]System.DateTime>
                    ins1.Add(Instruction.Create(OpCodes.Box, property.PropertyType));


                    var objectDefinition = moduleWeaver.FindType("System.Object");
                    var objectEqualsMethodDefinition = objectDefinition.Methods.First(x => x.Name == "Equals" && x.Parameters.Count == 2);
                    moduleWeaver.ModuleDefinition.ImportReference(objectEqualsMethodDefinition);

                    //IL_0012: call bool [mscorlib]System.Object::Equals(object, object)
                    var EqualsMd = type.Module.ImportReference(objectEqualsMethodDefinition);

                    ins1.Add(Instruction.Create(OpCodes.Call, EqualsMd));

                    //IL_0017: stloc.0
                    ins1.Add(Instruction.Create(OpCodes.Stloc_0));

                    //IL_0018: ldloc.0
                    ins1.Add(Instruction.Create(OpCodes.Ldloc_0));

                    //IL_0019: ldc.i4.0
                    ins1.Add(Instruction.Create(OpCodes.Ldc_I4_0));

                    //IL_001a: ceq
                    ins1.Add(Instruction.Create(OpCodes.Ceq));

                    //IL_001c: stloc.1
                    ins1.Add(Instruction.Create(OpCodes.Stloc_1));

                    //IL_001d: ldloc.1
                    ins1.Add(Instruction.Create(OpCodes.Ldloc_1));

                    //IL_001e: brfalse.s IL_0034
                    var IL_0034 = Instruction.Create(OpCodes.Ldarg_0);
                    ins1.Add(Instruction.Create(OpCodes.Brfalse_S, IL_0034));

                    //IL_0020: nop
                    ins1.Add(Instruction.Create(OpCodes.Nop));

                    //IL_0021: ldarg.0
                    ins1.Add(Instruction.Create(OpCodes.Ldarg_0));

                    //IL_0022: call instance class [mscorlib]System.Collections.Generic.Dictionary`2<string, bool> AssemblyToProcess.Class1::get_ModifiedProperties()

                    var getModifiedPropertiesMethod = moduleWeaver.ModuleDefinition.ImportReference(modifiedPropertiesProp.Prop.GetMethod);
                    ins1.Add(Instruction.Create(OpCodes.Call, getModifiedPropertiesMethod));

                    //IL_0027: ldstr "Prop1"
                    ins1.Add(Instruction.Create(OpCodes.Ldstr, property.Name));

                    //IL_002c: ldc.i4.1
                    ins1.Add(Instruction.Create(OpCodes.Ldc_I4_1));

                    //IL_002d: callvirt instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, bool>::set_Item(!0, !1)
                    //var _set_ItemMothed = typeof(Dictionary<string, bool>).GetMethod("set_Item", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

                    var _set_ItemMothed = dicTypeDefinition.Methods.FirstOrDefault(m => m.Name == "set_Item");
                    var set_ItemMethodRef = type.Module.ImportReference(_set_ItemMothed);
                    set_ItemMethodRef = set_ItemMethodRef.MakeGeneric(typeSystem.String, typeSystem.Boolean);

                    ins1.Add(Instruction.Create(OpCodes.Callvirt, set_ItemMethodRef));

                    //IL_0032: nop
                    ins1.Add(Instruction.Create(OpCodes.Nop));

                    //IL_0033: nop
                    ins1.Add(Instruction.Create(OpCodes.Nop));

                    //IL_0034: ldarg.0
                    ins1.Add(IL_0034);

                    //IL_0035: ldarg.1
                    ins1.Add(Instruction.Create(OpCodes.Ldarg_1));

                    //IL_0036: stfld valuetype [mscorlib]System.Nullable`1<valuetype [mscorlib]System.DateTime> AssemblyToProcess.Class1::k__BackingField
                    ins1.Add(Instruction.Create(OpCodes.Stfld, propFieldDef));

                    //IL_003b: ret
                    ins1.Add(Instruction.Create(OpCodes.Ret));

                }
            }

        }
    }

    public class PropAndField
    {
        public bool FromBaseClass { get; set; }

        public PropertyDefinition Prop { get; set; }
        public FieldDefinition Field { get; set; }

        public PropAndField()
        {

        }

        public PropAndField(PropertyDefinition prop, FieldDefinition field, bool fromBaseClass)
        {
            this.Prop = prop;
            this.Field = field;
            this.FromBaseClass = fromBaseClass;
        }
    }

    private PropAndField InjectProperty(TypeDefinition typeDefinition, string propName, TypeReference propType, bool isPublic)
    {
        var name = propName;
        if (typeDefinition.HasGenericParameters)
        {
            var message =
                $"Skipped public field '{typeDefinition.Name}.{name}' because generic types are not currently supported. You should make this a public property instead.";
            moduleWeaver.LogWarning(message);
            return null;
        }

        // 获取本类或基类是否存在 相同的属性
        Func<TypeDefinition, PropAndField> GetExistProp = null;
        GetExistProp = (TypeDefinition type) =>
        {
            try
            {
                var prop = type.Properties.SingleOrDefault(p => p.Name == propName);
                if (prop != null)
                {
                    return new PropAndField(prop, null, true);
                }
                else
                {
                    if (type.BaseType.FullName != "System.Object")
                    {
                        var baseType = type.BaseType.Resolve();
                        return GetExistProp(baseType);
                    }
                }
            }
            catch (System.Exception ex1)
            {
                var ss = ex1.Message;
            }

            return null;
        };


        var result = GetExistProp(typeDefinition);
        if (result != null)
        {
            return result;
        }

        FieldDefinition field = new FieldDefinition(name, FieldAttributes.Private, propType);
        field.Name = $"<{name}>k__BackingField";
        field.IsPublic = false;
        field.IsPrivate = true;

        typeDefinition.Fields.Add(field);

        var get = InjectPropertyGet(field, name, isPublic);
        typeDefinition.Methods.Add(get);

        var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, field.FieldType)
        {
            HasThis = false,//这个地方默认为true，正常属性应该为true
            GetMethod = get
        };


        var isReadOnly = field.Attributes.HasFlag(FieldAttributes.InitOnly);
        if (!isReadOnly)
        {
            var set = InjectPropertySet(field, name, isPublic);
            typeDefinition.Methods.Add(set);
            propertyDefinition.SetMethod = set;
        }
        foreach (var customAttribute in field.CustomAttributes)
        {
            propertyDefinition.CustomAttributes.Add(customAttribute);
        }
        field.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.CompilerGeneratedReference));

        typeDefinition.Properties.Add(propertyDefinition);
        propertyDefinition.HasThis = false;
        return new PropAndField(propertyDefinition, field, false);
    }

    private MethodDefinition InjectPropertyGet(FieldDefinition field, string name, bool isPublic = true)
    {
        MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
        if (isPublic)
        {
            methodAttributes = MethodAttributes.Public | methodAttributes;
        }
        else
        {
            methodAttributes = MethodAttributes.Private | methodAttributes;
        }

        var get = new MethodDefinition("get_" + name, methodAttributes, field.FieldType);
        var instructions = get.Body.Instructions;
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
        instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        var inst = Instruction.Create(OpCodes.Ldloc_0);
        instructions.Add(Instruction.Create(OpCodes.Br_S, inst));
        instructions.Add(inst);
        instructions.Add(Instruction.Create(OpCodes.Ret));
        get.Body.Variables.Add(new VariableDefinition(field.FieldType));
        get.Body.InitLocals = true;
        get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
        get.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.CompilerGeneratedReference));
        return get;
    }

    private MethodDefinition InjectPropertySet(FieldDefinition field, string name, bool isPublic = true)
    {
        MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
        if (isPublic)
        {
            methodAttributes = MethodAttributes.Public | methodAttributes;
        }
        else
        {
            methodAttributes = MethodAttributes.Private | methodAttributes;
        }

        var set = new MethodDefinition("set_" + name, methodAttributes, typeSystem.Void);
        var instructions = set.Body.Instructions;
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        instructions.Add(Instruction.Create(OpCodes.Stfld, field));
        instructions.Add(Instruction.Create(OpCodes.Ret));
        set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, field.FieldType));
        set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
        set.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.CompilerGeneratedReference));
        return set;
    }
}
#pragma warning restore 618