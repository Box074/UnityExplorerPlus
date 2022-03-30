
namespace UnityExplorerPlusMod;

static class FsmDumpExt
{
    public static Type TFsmDataInstance = HReflectionHelper.FindType("Satchel.Futils.Serialiser.FsmDataInstance");
    public static void Init()
    {
        if(TFsmDataInstance is null) return;
        WidgetManager.RegisterType(typeof(PlayMakerFSM), typeof(FsmWidget));
    }
}
