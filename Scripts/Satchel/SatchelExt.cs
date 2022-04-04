namespace UnityExplorerPlusMod;

static class SatchelExt
{
    public static Type TFsmDataInstance = HReflectionHelper.FindType("Satchel.Futils.Serialiser.FsmDataInstance");

    public static void Init()
    {
        WidgetManager.RegisterType(typeof(Sprite), typeof(SpriteWidget));
        WidgetManager.RegisterType(typeof(SpriteAtlas), typeof(SpriteAtlasWidget));
        if(TFsmDataInstance is not null) 
        {
            WidgetManager.RegisterType(typeof(PlayMakerFSM), typeof(FsmWidget));
        }
        
    }
}
