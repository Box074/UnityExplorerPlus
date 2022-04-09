
namespace UnityExplorerPlusMod;

public static class ParseManager
{
    private static Type T_ParseUtility = typeof(ParseUtility);
    private static Type DT_ParseMethod = T_ParseUtility.GetNestedType("ParseMethod", HReflectionHelper.All);
    private static Type DT_ToStringMethod = T_ParseUtility.GetNestedType("ToStringMethod", HReflectionHelper.All);
    private static FieldInfo F_customTypes = T_ParseUtility.GetField("customTypes", HReflectionHelper.All);
    private static FieldInfo F_customTypesToString = T_ParseUtility.GetField("customTypesToString", HReflectionHelper.All);
    private static MethodInfo M_customTypes_Add = F_customTypes.FieldType.GetMethod("Add");
    private static MethodInfo M_customTypesToString_Add = F_customTypesToString.FieldType.GetMethod("Add");

    private static MethodInfo M_SetValueState = typeof(CacheObjectBase).GetMethod("SetValueState", HReflectionHelper.All);
    private static HashSet<string> types = new();
    static ParseManager()
    {
        HookEndpointManager.Add(
            typeof(CacheObjectBase).GetMethod("SetDataToCell"),
            (Action<CacheObjectBase, CacheObjectCell> orig, CacheObjectBase self, CacheObjectCell cell) =>
            {
                orig(self, cell);
                if(!types.Contains(self.Value?.GetType()?.FullName)) return;
                M_SetValueState.FastInvoke(self, cell, 
                new CacheObjectBase.ValueStateArgs(valueRichText: true, inputActive: self.CanWrite, applyActive: self.CanWrite, inspectActive: true));
            }
            );
    }
    public static void Register(Type type, Func<object, string> toString, Func<string, object> parse)
    {
        types.Add(type.FullName);
        M_customTypes_Add.FastInvoke(F_customTypes.FastGet(null), type.FullName, 
            parse.Method.CreateDelegate(DT_ParseMethod, parse.Target)
        );
        M_customTypesToString_Add.FastInvoke(F_customTypesToString.FastGet(null), type.FullName, 
            toString.Method.CreateDelegate(DT_ToStringMethod, toString.Target)
        );
    }
}
