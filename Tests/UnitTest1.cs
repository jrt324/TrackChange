using System;
using System.Collections.Generic;
using System.IO;
using AssemblyToProcess;
using Fody;
using Xunit;

#pragma warning disable 618
#region WeaverTests
public class WeaverTests
{
    static Fody.TestResult testResult;

    static WeaverTests()
    {
        var weavingTask = new ModuleWeaver();
    
        testResult = weavingTask.ExecuteTestRun(@"AssemblyToProcess.dll", runPeVerify: false);
    }




    [Fact]
    public void ValidateHelloWorldIsInjected()
    {
        var instance = testResult.GetInstance("AssemblyToProcess.Class3");
        instance.IntVal1 = 1;
        instance.IntVal2 = 2;
        Dictionary<string, bool> changes = instance.ModifiedProperties;
        Assert.True(changes.ContainsKey("IntVal1"));
        Assert.True(changes.ContainsKey("IntVal2"));
    }

    [Fact]
    public void ValidateInherited()
    {
        var instance = testResult.GetInstance("AssemblyToProcess.InheritedClass");
        instance.BaseProp1 = "ddd";

        Dictionary<string, bool> changes = instance.ModifiedProperties;
        Assert.True(changes.ContainsKey("BaseProp1"));

    }

    [Fact]
    public void ValidateInterfacePropsMarkAsNonSerialized()
    {
        var instance = testResult.GetInstance("AssemblyToProcess.Class3");
        var type = testResult.Assembly.GetType("AssemblyToProcess.Class3");
        var attrs = type.GetProperty("ModifiedProperties").GetCustomAttributes(typeof(NonSerializedAttribute), true);
        Assert.True(attrs.Length == 1);
    }
}
#endregion