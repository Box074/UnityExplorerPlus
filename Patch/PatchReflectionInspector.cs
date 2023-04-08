namespace UnityExplorerPlus.Patch;

static class PatchReflectionInspector
{
    public static List<PropertyInfo> autoEvalBlacklist = new()
    {
        typeof(MeshFilter).GetProperty(nameof(MeshFilter.mesh)),
        typeof(Renderer).GetProperty(nameof(Renderer.material)),
        typeof(Renderer).GetProperty(nameof(Renderer.materials))
    };
    public static void Init()
    {
        On.UnityExplorer.CacheObject.CacheProperty.get_ShouldAutoEvaluate += CacheProperty_get_ShouldAutoEvaluate;
    }

    private static bool CacheProperty_get_ShouldAutoEvaluate(On.UnityExplorer.CacheObject.CacheProperty.orig_get_ShouldAutoEvaluate orig, 
        CacheProperty self)
    {
        if (autoEvalBlacklist.Contains(self.PropertyInfo)) return false;
        return orig(self);
    }
}
