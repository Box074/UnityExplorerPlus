
namespace UnityExplorerPlusMod;

static class WidgetManager
{
    private static Dictionary<Type, MethodInfo> typeMap = new();
    static WidgetManager()
    {
        Init();
    }
    public static void RegisterType(Type src, Type widget)
    {
        if(!typeof(UnityObjectWidget).IsAssignableFrom(widget)) return;
        var poolType = typeof(Pool<>).MakeGenericType(widget);
        var borrow = poolType.GetMethod("Borrow");
        typeMap[src] = borrow;
    }
    private static void Init()
    {
        HookEndpointManager.Add(
            typeof(UnityObjectWidget).GetMethod("GetUnityWidget"),
            (Func<object, Type, ReflectionInspector, UnityObjectWidget> orig,
                object target, Type targetType, ReflectionInspector inspector) =>
                {
                    if(typeMap.TryGetValue(targetType, out var borrow))
                    {
                        var r = (UnityObjectWidget)borrow.FastInvoke(null);
                        r.OnBorrowed(target, targetType, inspector);
                        return r;
                    }
                    else
                    {
                        return orig(target, targetType, inspector);
                    }
                }
            );
    }
}
