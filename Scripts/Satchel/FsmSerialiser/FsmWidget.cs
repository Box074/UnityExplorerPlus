
namespace UnityExplorerPlusMod;

class FsmWidget : DumpWidgetBase<FsmWidget>
{
    public PlayMakerFSM fsm;
    protected override void OnSave(string savePath)
    {
        if (fsm.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("PlayMakerFSM is null, maybe it was destroyed?");
            return;
        }
        var data = Activator.CreateInstance(SatchelExt.TFsmDataInstance, (object)fsm);
        var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                Debug.Log(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            },
        });
        File.WriteAllText(
            savePath, json);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        fsm = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        fsm = (PlayMakerFSM)target;
        SetDefaultPath(fsm.gameObject.name + "-" + fsm.Fsm.Name, "json");
    }
}
