using UnityExplorerPlus.Widgets.Sprites;
using UnityExplorerPlus.Widgets.tk2d;

namespace UnityExplorerPlus.Widgets;

static class WidgetManager
{
    private static Dictionary<Type, MethodInfo> typeMap = new();
    static WidgetManager()
    {
        Init();

        RegisterType(typeof(tk2dSpriteCollectionData), typeof(Tk2dSpriteDefWidget));
        RegisterType(typeof(Sprite), typeof(SpriteWidget));
        RegisterType(typeof(SpriteAtlas), typeof(SpriteAtlasWidget));
        RegisterType(typeof(tk2dSpriteAnimation), typeof(tk2dSpriteWidget));
        RegisterType(typeof(tk2dSpriteAnimator), typeof(tk2dSpriteWidget));
        RegisterType(typeof(tk2dSpriteAnimationClip), typeof(tk2dClipDumpWidget));
        RegisterType(typeof(PlayMakerFSM), typeof(FsmWidget));
    }
    public static void RegisterType(Type src, Type widget)
    {
        if (!typeof(UnityObjectWidget).IsAssignableFrom(widget)) return;
        var poolType = typeof(Pool<>).MakeGenericType(widget);
        var borrow = poolType.GetMethod("Borrow");
        typeMap[src] = borrow;
    }
    public static void Init()
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
                if (target is Renderer renderer && false)
                {
                    return Pool<RendererWidget>.Borrow().With(x => x.OnBorrowed(target, targetType, inspector));
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
