
namespace UnityExplorerPlusMod;

public static class ParseManager
{
    private static HashSet<string> types = new();
    static ParseManager()
    {
        On.UnityExplorer.CacheObject.CacheObjectBase.SetDataToCell += (orig, self, cell) =>
            {
                orig(self, cell);
                if (!types.Contains(self.Value?.GetType()?.FullName)) return;
                self.Reflect().SetValueState(cell.Reflect(), new(valueRichText: true, inputActive: self.CanWrite, applyActive: self.CanWrite, inspectActive: true));
            };
    }
    public static void Register(Type type, ToStringMethodR toString, ParseMethodR parse)
    {
        types.Add(type.FullName);
        ParseUtilityR.customTypes.Add(type.FullName, parse);
        ParseUtilityR.customTypesToString.Add(type.FullName,
            toString);
    }
}
