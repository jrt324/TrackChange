using System.Linq;
using Mono.Cecil;

public static class CustomAttributeChecker
{
    public static CustomAttribute GetTrackAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        return customAttributes.FirstOrDefault(x => x.AttributeType.Name == "TrackingAttribute");
    }

    public static bool ContainsTrackAttribute(this ICustomAttributeProvider definition)
    {
        return GetTrackAttribute(definition) != null;
    }

    public static bool IsCompilerGenerated(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        return customAttributes.Any(x => x.AttributeType.Name == "CompilerGeneratedAttribute");
    }

    public static void RemoveTrackAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        var timeAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "TrackingAttribute");

        if (timeAttribute != null)
        {
            customAttributes.Remove(timeAttribute);
        }

    }
}