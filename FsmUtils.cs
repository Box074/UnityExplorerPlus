
using UnityExplorerPlus.ParseUtility;
using UniverseLib.Utility;

namespace UnityExplorerPlus;

static class FsmUtils
{

    public static void Init()
    {
        On.UnityExplorer.UI.Widgets.ComponentList.SetComponentCell += (orig, self, cell, index) =>
            {
                orig(self, cell, index);
                var gentries = (Func<List<Component>>)self.FastGet("GetEntries");
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

        ParseManager.RegisterToString<FsmState>(
            state => $"<color=grey>Fsm State: </color><color=green>{state.Name}</color>");

        ParseManager.RegisterToString<FsmTransition>(
            tran => $"<color=grey>Fsm Transition: </color><color=green>{tran.FsmEvent?.Name ?? tran.EventName}</color><color=grey> -> </color><color=green>{tran.ToFsmState?.Name ?? tran.ToState}</color>");

        ParseManager.RegisterToString<FsmEvent>(
            ev => $"<color=grey>Fsm Event: </color><color=green>{ev.Name}</color>");

        ParseManager.RegisterToString<NamedVariable>(
            v => $"<color=grey>Fsm Variable(</color><color=green>{v.VariableType}</color><color=grey>): </color><color=green>{v.Name}</color> | " + ToStringUtility.ToStringWithType(v.RawValue, typeof(object))
            );

        ParseManager.Register(typeof(FsmEvent), (ev) => ((FsmEvent)ev).Name,
            FsmEvent.GetFsmEvent);
    }
}
