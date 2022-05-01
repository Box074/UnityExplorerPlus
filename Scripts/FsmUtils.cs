
namespace UnityExplorerPlusMod;

static class FsmUtils
{
    public static FieldInfo FGetEntries =  FindFieldInfo("UnityExplorer.UI.Widgets.ComponentList::GetEntries")!;
    public static void Init()
    {
        HookEndpointManager.Add(
            FindMethodBase("UnityExplorer.UI.Widgets.ComponentList::SetComponentCell"),
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
                if(value is NamedVariable v)
                {
                    return $"<color=grey>Fsm Variable(</color><color=green>{v.VariableType.ToString()}</color><color=grey>): </color><color=green>{v.Name}</color>";
                }
                return orig(value, fallbackType, includeNamespace);
            }
        );

        ParseManager.Register(typeof(FsmEvent), (ev) => ((FsmEvent)ev).Name,
            (name) => FsmEvent.GetFsmEvent(name));
    }
}
