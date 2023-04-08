
using UnityExplorerPlus.ParseUtility;

namespace UnityExplorerPlus;

static class FsmUtils
{

    public static void Init()
    {
        

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
