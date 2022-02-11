
namespace UnityExplorerPlusMod;

class WorldInspectPatch
{
    public static MethodInfo methodInfo_OnHitGameObject = typeof(WorldInspector)
           .GetMethod("OnHitGameObject", BindingFlags.Instance | BindingFlags.NonPublic);
    public static MethodInfo methodInfo_ClearHitData = typeof(InspectUnderMouse)
        .GetMethod("ClearHitData", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void PatchWorldInspector()
    {
        var MUpdateMouseInspect = typeof(WorldInspector)
            .GetRuntimeMethod("UpdateMouseInspect", new Type[] { typeof(Vector2) });
        HookEndpointManager.Add(MUpdateMouseInspect,
            new Action<Action<WorldInspector, Vector2>, WorldInspector, Vector2>(UpdateMouseInspectHook));
        /*detour_UpdateMouseInspect = new Hook(MUpdateMouseInspect,
            typeof(UExplorer).GetMethod("UpdateMouseInspectHook", BindingFlags.Static | BindingFlags.NonPublic));*/

    }
    private static void UpdateMouseInspectHook(Action<WorldInspector, Vector2> _,
         WorldInspector self, Vector2 _1)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            InspectUnderMouse.Instance.StopInspect();
            return;
        }
        var mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
        var worldPos = cam.ScreenToWorldPoint(mousePos);
        var hit = Physics2D.OverlapPointAll(worldPos, Physics2D.AllLayers).FirstOrDefault(x =>
            x.transform.position.z > 0);

        if (hit == null)
        {
            //if (ReflectionHelper.GetField<WorldInspector, GameObject>(self, "lastHitObject") != null)
            //{
            methodInfo_ClearHitData.Invoke(InspectUnderMouse.Instance, Array.Empty<object>());
            //}
        }
        else
        {
            methodInfo_OnHitGameObject.Invoke(self, new object[] { hit.gameObject });
        }
    }
}
