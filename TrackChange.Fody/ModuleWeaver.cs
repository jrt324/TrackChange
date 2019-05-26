using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

public class ModuleWeaver: BaseModuleWeaver
{
    public override void Execute()
    {
        var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver);
        msCoreReferenceFinder.Execute();

        var allPocoTypes = ModuleDefinition.GetTypes().ToList();
        var finder = new MethodFinder(allPocoTypes);
        finder.Execute();
        var converter = new ImplementITrackableInjector(this, msCoreReferenceFinder, ModuleDefinition.TypeSystem, allPocoTypes);
        converter.Execute();
   
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System";
        yield return "System.Runtime";
        yield return "System.Core";
        yield return "netstandard";
        yield return "System.Collections";
        yield return "System.ObjectModel";
        yield return "System.Threading";
        yield return "System.ComponentModel.DataAnnotations";
    }

    public override bool ShouldCleanReference => true;
}