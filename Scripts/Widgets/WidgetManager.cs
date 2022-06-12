
namespace UnityExplorerPlusMod;

static class WidgetManager
{
    private static Dictionary<Type, MethodInfo> typeMap = new();
    static WidgetManager()
    {
        Init();

        RegisterType(typeof(tk2dSpriteCollectionData), typeof(Tk2dSpriteDefWidget));
        WidgetManager.RegisterType(typeof(Sprite), typeof(SpriteWidget));
        WidgetManager.RegisterType(typeof(SpriteAtlas), typeof(SpriteAtlasWidget));
        WidgetManager.RegisterType(typeof(tk2dSpriteAnimation), typeof(tk2dSpriteWidget));
        WidgetManager.RegisterType(typeof(tk2dSpriteAnimator), typeof(tk2dSpriteWidget));
        WidgetManager.RegisterType(typeof(tk2dSpriteAnimationClip), typeof(tk2dClipDumpWidget));
    }
    public static void RegisterType(Type src, Type widget)
    {
        if (!typeof(UnityObjectWidget).IsAssignableFrom(widget)) return;
        var poolType = typeof(Pool<>).MakeGenericType(widget);
        var borrow = poolType.GetMethod("Borrow");
        typeMap[src] = borrow;
    }
    private static void Init()
    {
        On.UnityExplorer.UI.Widgets.UnityObjectWidget.GetUnityWidget += (orig, target, targetType, inspector) =>
        {
            try
            {
                if (typeMap.TryGetValue(targetType, out var borrow))
                {
                    var r = (UnityObjectWidget)borrow.FastInvoke(null);
                    r.OnBorrowed(target, targetType, inspector);
                    return r;
                }
            }
            catch (Exception e)
            {
                UnityExplorerPlus.Instance.LogError(e);
            }
            return orig(target, targetType, inspector);
        };

    }
}
