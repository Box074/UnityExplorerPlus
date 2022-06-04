
namespace UnityExplorerPlusMod;

static class FsmUtils
{
    public static FieldInfo FGetEntries = FindFieldInfo("UnityExplorer.UI.Widgets.ComponentList::GetEntries")!;
    public static void Init()
    {
        On.UnityExplorer.UI.Widgets.ComponentList.SetComponentCell += (orig, self, cell, index) =>
            {
                orig(self, cell, index);
                var gentries = (Func<List<Component>>)FGetEntries.FastGet(self);
                if (gentries is null) return;
                var data = gentries()[index];
                if (data is PlayMakerFSM pm)
                {
                    if (string.IsNullOrEmpty(pm.FsmName)) return;
                    cell.Button.ButtonText.text = cell.Button.ButtonText.text
                        + "<color=grey>(</color><color=green>"
                        + pm.FsmName
                        + "</color><color=grey>)</color>";
                }
            };
        On.UniverseLib.Utility.ToStringUtility.ToStringWithType += (orig, value, fallbackType, includeNamespace) =>
            {
                if (value is FsmState state)
                {
                    return $"<color=grey>Fsm State: </color><color=green>{state.Name}</color>";
                }
                if (value is FsmTransition tran)
                {
                    return $"<color=grey>Fsm Transition: </color><color=green>{tran.FsmEvent?.Name ?? tran.EventName}</color><color=grey> -> </color><color=green>{tran.ToFsmState?.Name ?? tran.ToState}</color>";
                }
                if (value is FsmEvent ev)
                {
                    return $"<color=grey>Fsm Event: </color><color=green>{ev.Name}</color>";
                }
                if (value is NamedVariable v)
                {
                    return $"<color=grey>Fsm Variable(</color><color=green>{v.VariableType.ToString()}</color><color=grey>): </color><color=green>{v.Name}</color>";
                }
                return orig(value, fallbackType, includeNamespace);
            };
        

        ParseManager.Register(typeof(FsmEvent), (ev) => ((FsmEvent)ev).Name,
            (name) => FsmEvent.GetFsmEvent(name));
    }
}
