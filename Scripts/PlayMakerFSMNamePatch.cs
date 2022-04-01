
namespace UnityExplorerPlusMod;

static class PlayMakerFSMNamePatch
{
    public static FieldInfo FGetEntries = typeof(ComponentList).GetField("GetEntries", HReflectionHelper.All);
    public static void Init()
    {
        HookEndpointManager.Add(
            typeof(ComponentList).GetMethod("SetComponentCell", HReflectionHelper.All),
            (Action<ComponentList, ComponentCell, int> orig, ComponentList self, ComponentCell cell, int index) =>
            {
                orig(self, cell, index);
                var gentries = (Func<List<Component>>)FGetEntries.FastGet(self);
                if(gentries is null) return;
                var data = gentries()[index];
                if(data is PlayMakerFSM pm)
                {
                    if(string.IsNullOrEmpty(pm.FsmName)) return;
                    cell.Button.ButtonText.text = cell.Button.ButtonText.text 
                        + "<color=grey>(</color><color=green>" 
                        + pm.FsmName
                        + "</color><color=grey>)</color>";
                }
            }
            );
        HookEndpointManager.Add(
            typeof(ToStringUtility).GetMethod("ToStringWithType"),
            (Func<object, Type, bool, string> orig, object value, Type fallbackType, bool includeNamespace) => {
                if(value is FsmState state)
                {
                    return $"<color=grey>Fsm State: </color><color=green>{state.Name}</color>";
                }
                if(value is FsmTransition tran)
                {
                    return $"<color=grey>Fsm Transition: </color><color=green>{tran.FsmEvent?.Name ?? tran.EventName}</color><color=grey> -> </color><color=green>{tran.ToFsmState?.Name ?? tran.ToState}</color>";
                }
                if(value is FsmEvent ev)
                {
                    return $"<color=grey>Fsm Event: </color><color=green>{ev.Name}</color>";
                }
                return orig(value, fallbackType, includeNamespace);
            }
        );
    }
}
