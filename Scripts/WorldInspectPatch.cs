
namespace UnityExplorerPlusMod;

class WorldInspectPatch
{
    public static MethodInfo methodInfo_ClearHitData = typeof(MouseInspector)
        .GetMethod("ClearHitData", BindingFlags.Instance | BindingFlags.NonPublic);
}
