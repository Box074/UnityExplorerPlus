
namespace UnityExplorerPlusMod;

public static class ParseManager
{
    private static Type T_ParseUtility = typeof(ParseUtility);
    private static Type DT_ParseMethod = FindType("UniverseLib.Utility.ParseUtility+ParseMethod");
    private static Type DT_ToStringMethod = FindType("UniverseLib.Utility.ParseUtility+ToStringMethod");
    private static FieldInfo F_customTypes = FindFieldInfo("UniverseLib.Utility.ParseUtility::customTypes");
    private static FieldInfo F_customTypesToString = FindFieldInfo("UniverseLib.Utility.ParseUtility::customTypesToString");
    private static MethodInfo M_customTypes_Add = F_customTypes.FieldType.GetMethod("Add");
    private static MethodInfo M_customTypesToString_Add = F_customTypesToString.FieldType.GetMethod("Add");

    private static MethodInfo M_SetValueState = (MethodInfo)FindMethodBase("UnityExplorer.CacheObject.CacheObjectBase::SetValueState");
    private static HashSet<string> types = new();
    static ParseManager()
    {
        On.UnityExplorer.CacheObject.CacheObjectBase.SetDataToCell += (orig, self, cell) =>
            {
                orig(self, cell);
                if (!types.Contains(self.Value?.GetType()?.FullName)) return;
                M_SetValueState.FastInvoke(self, cell,
                new CacheObjectBase.ValueStateArgs(valueRichText: true, inputActive: self.CanWrite, applyActive: self.CanWrite, inspectActive: true));
            };
    }
    public static void Register(Type type, Func<object, string> toString, Func<string, object> parse)
    {
        types.Add(type.FullName);
        M_customTypes_Add.FastInvoke(F_customTypes.FastGet((object)null), type.FullName,
            parse.Method.CreateDelegate(DT_ParseMethod, parse.Target)
        );
        M_customTypesToString_Add.FastInvoke(F_customTypesToString.FastGet((object)null), type.FullName,
            toString.Method.CreateDelegate(DT_ToStringMethod, toString.Target)
        );
    }
}
