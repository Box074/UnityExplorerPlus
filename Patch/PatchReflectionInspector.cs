using System.Runtime.CompilerServices;
using UnityExplorerPlus.Inspectors.Reflect;

namespace UnityExplorerPlus.Patch;

static class PatchReflectionInspector
{
    public static List<PropertyInfo> autoEvalBlacklist = new()
    {
        typeof(MeshFilter).GetProperty(nameof(MeshFilter.mesh)),
        typeof(Renderer).GetProperty(nameof(Renderer.material)),
        typeof(Renderer).GetProperty(nameof(Renderer.materials)),
        typeof(Material).GetProperty(nameof(Material.color)),
        typeof(Material).GetProperty(nameof(Material.mainTexture)),
        typeof(Material).GetProperty(nameof(Material.mainTextureOffset)),
        typeof(Material).GetProperty(nameof(Material.mainTextureScale))
    };
    private static ConditionalWeakTable<Material, Shader> shader_cache = new();
    public static void Init()
    {
        On.UnityExplorer.Inspectors.ReflectionInspector.SetTarget += ReflectionInspector_SetTarget;
        On.UnityExplorer.Inspectors.ReflectionInspector.Update += ReflectionInspector_Update;

        On.UnityExplorer.CacheObject.CacheProperty.get_ShouldAutoEvaluate += CacheProperty_get_ShouldAutoEvaluate;
    }

    private static void ReflectionInspector_Update(On.UnityExplorer.Inspectors.ReflectionInspector.orig_Update orig, ReflectionInspector self)
    {
        
        if(self.Target is Material mat)
        {
            if(shader_cache.TryGetValue(mat, out var shader))
            {
                if(mat.shader != shader)
                {
                    self.Reflect().refreshWanted = true;
                    RemoveShaderCache(self);
                    AttachShaderCache(mat, self);
                }
            }
        }
        orig(self);
    }

    private static void AttachShaderCache(Material mat, ReflectionInspector self)
    {
        shader_cache.Remove(mat);
        var shader = mat.shader;
        shader_cache.Add(mat, shader);

        if (shader == null) return;
        var list = self.Reflect().members;
        var l = new List<CacheMemberR>();
        for(int i = 0; i < shader.GetPropertyCount(); i++)
        {
            var name = shader.GetPropertyName(i);
            var cm = new CacheShaderProp();
            cm.BindShaderProp(name);
            cm.SetInspectorOwner(self, null);
            l.Add(cm.Reflect());
        }
        list.InsertRange(0, l);
        
    }
    private static void RemoveShaderCache(ReflectionInspector self)
    {
        self.Reflect().members.RemoveAll(x => x.ToOriginal() is CacheShaderProp || 
            x.ToOriginal() is CacheShaderKeywords);
    }

    private static void ReflectionInspector_SetTarget(On.UnityExplorer.Inspectors.ReflectionInspector.orig_SetTarget orig, 
        ReflectionInspector self, object target)
    {
        orig(self, target);
        if(target is Material mat)
        {
            AttachShaderCache(mat, self);
        }
    }

    private static bool CacheProperty_get_ShouldAutoEvaluate(On.UnityExplorer.CacheObject.CacheProperty.orig_get_ShouldAutoEvaluate orig, 
        CacheProperty self)
    {
        if (autoEvalBlacklist.Contains(self.PropertyInfo)) return false;
        return orig(self);
    }
}
