# TrackChange
This is an add-in for [Fody](https://github.com/Fody/Home/). 

Tracking POCO properties changes , easy to access what properties changed.

TrackChange will process any POCO class that mark as `TrackingAttribute`.

All trackable POCOs will be inject to implement `ITrackable` iterface , you can copy follow code in your project

```csharp
public class TrackingAttribute : Attribute
{
}


public interface ITrackable
{
    /// <summary>
    /// Modified name of Properties 
    /// </summary>
    Dictionary<string, bool> ModifiedProperties { get; set; }

    /// <summary>
    /// Is Track properties change state
    /// </summary>
    bool IsTracking { get; set; }
}
```

# Installation

```powershell
PM> Install-Package TrackChange.Fody
```

# How it works
### Your Code

```csharp
[Tracking]
public class Class3
{
    public DateTime? Prop1 { get; set; }
    public string Test2 { get; set; }
    public int IntVal1 { get; set; }
    public int? IntVal2 { get; set; }
}
```

### What gets compiled
```csharp
[Tracking]
public class Class3 : ITrackable
{
    [NonSerialized]
    public virtual bool IsTracking { get; set; }

    [NonSerialized]
    public virtual Dictionary<string, bool> ModifiedProperties { get; set; } = new Dictionary<string, bool>();

    public DateTime? Prop1
    {
        [CompilerGenerated]
        get
        {
            return this.<Prop1>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            bool flag = object.Equals(this.Prop1, value);
            bool flag2 = !flag;
            if (flag2)
            {
                this.ModifiedProperties["Prop1"] = true;
            }
            this.<Prop1>k__BackingField = value;
        }
    }

    public string Test2
    {
        [CompilerGenerated]
        get
        {
            return this.<Test2>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            bool flag = object.Equals(this.Test2, value);
            bool flag2 = !flag;
            if (flag2)
            {
                this.ModifiedProperties["Test2"] = true;
            }
            this.<Test2>k__BackingField = value;
        }
    }

    public int IntVal1
    {
        [CompilerGenerated]
        get
        {
            return this.<IntVal1>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            bool flag = object.Equals(this.IntVal1, value);
            bool flag2 = !flag;
            if (flag2)
            {
                this.ModifiedProperties["IntVal1"] = true;
            }
            this.<IntVal1>k__BackingField = value;
        }
    }

    public int? IntVal2
    {
        [CompilerGenerated]
        get
        {
            return this.<IntVal2>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            bool flag = object.Equals(this.IntVal2, value);
            bool flag2 = !flag;
            if (flag2)
            {
                this.ModifiedProperties["IntVal2"] = true;
            }
            this.<IntVal2>k__BackingField = value;
        }
    }


}

```
