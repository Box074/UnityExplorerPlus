
namespace UnityExplorerPlusMod;

static class GODumpExt
{
    public static void Init()
    {
        WidgetManager.RegisterType(typeof(tk2dSpriteAnimation), typeof(tk2dSpriteWidget));
        WidgetManager.RegisterType(typeof(tk2dSpriteAnimator), typeof(tk2dSpriteWidget));
        if(typeof(Dump).GetMethod("DumpClip") is not null)
        {
            WidgetManager.RegisterType(typeof(tk2dSpriteAnimationClip), typeof(tk2dClipDumpWidget));
        }
    }
}
