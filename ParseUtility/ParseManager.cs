namespace UnityExplorerPlus.ParseUtility;

public static class ParseManager
{
    private static HashSet<string> types = new();
    private static Dictionary<Type, Func<object, string>> customToStrings = new();
    static ParseManager()
    {
        On.UnityExplorer.CacheObject.CacheObjectBase.SetDataToCell += (orig, self, cell) =>
            {
                orig(self, cell);
                if (!types.Contains(self.Value?.GetType()?.FullName)) return;
                self.Reflect().SetValueState(cell.Reflect(), new(valueRichText: true, inputActive: self.CanWrite, applyActive: self.CanWrite, inspectActive: true));
            };
        On.UniverseLib.Utility.ToStringUtility.ToStringWithType += ToStringUtility_ToStringWithType;
    }

    private static string ToStringUtility_ToStringWithType(On.UniverseLib.Utility.ToStringUtility.orig_ToStringWithType orig,
        object value, Type fallbackType, bool includeNamespace)
    {
        if (!value.IsNullOrDestroyed())
        {
            var t = value.GetType();
            if (!customToStrings.TryGetValue(t, out var p))
            {
                p = customToStrings.FirstOrDefault(x => x.Key.IsInstanceOfType(value)).Value;
            }
            if (p != null)
            {
                var result = p(value);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return orig(value, fallbackType, includeNamespace);
    }

    public static void Register(Type type, ToStringMethodR toString, ParseMethodR parse)
    {
        types.Add(type.FullName);
        ParseUtilityR.customTypes.Add(type.FullName, parse);
        ParseUtilityR.customTypesToString.Add(type.FullName,
            toString);
    }

    public static void RegisterToString(Type type, Func<object, string> toString)
    {
        customToStrings[type] = toString;
    }
    public static void RegisterToString<T>(Func<T, string> toString)
    {
        RegisterToString(typeof(T), val => toString((T)val));
    }
    public static void Init()
    {
        RegisterToString<tk2dSpriteAnimationClip>(
            clip => $"<color=grey>tk2dSpriteAnimationClip: </color><color=green>{clip.name}</color>");

        RegisterToString<tk2dSpriteDefinition>(
            def => $"<color=grey>tk2dSpriteDefinition: </color><color=green>{def.name}</color>");
    }
}
