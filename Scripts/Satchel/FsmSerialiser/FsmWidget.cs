
namespace UnityExplorerPlusMod;

class FsmWidget : UnityObjectWidget
{
    public Fsm fsm;
    public InputFieldRef savePathInput;
    public GameObject panel;
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);
        var toggleButton = UIFactory.CreateButton(UIRoot, "FsmWidgetToggleButton", "Dump", new Color?(new Color(0.2f, 0.3f, 0.2f)));
        toggleButton.Transform.SetSiblingIndex(0);
        toggleButton.Component.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
        });
        UIFactory.SetLayoutElement(toggleButton.Component.gameObject, 170, 25, null, null, null, null, null);
        var parent = UIFactory.CreateHorizontalGroup(uiRoot, "Fsm Save Row", false, false, true, true, 2, new Vector4(2f, 2f, 2f, 2f), new Color(0.1f, 0.1f, 0.1f), null);
        UIFactory.SetLayoutElement(parent, null, null, 9999, 35, null, null, null);
        panel = parent;

        savePathInput = UIFactory.CreateInputField(parent, "SaveInput", "...");
        UIFactory.SetLayoutElement(savePathInput.UIRoot, 100, 25, 9999, null, null, null, null);
        var buttonRef = UIFactory.CreateButton(parent, "SaveButton", "Dump", new Color?(new Color(0.2f, 0.25f, 0.2f)));
        buttonRef.Component.onClick.AddListener(OnClickSave);
        UIFactory.SetLayoutElement(buttonRef.Component.gameObject, 100, 25, 0, null, null, null, null);
        parent.SetActive(false);
        return result;
    }
    private void OnClickSave()
    {
        if (fsm?.FsmComponent is null)
        {
            ExplorerCore.LogWarning("PlayMakerFSM is null, maybe it was destroyed?");
            return;
        }
        if (string.IsNullOrEmpty(savePathInput.Text))
        {
            ExplorerCore.LogWarning("Save path cannot be empty!");
            return;
        }
        var sp = savePathInput.Text;
        if (!sp.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            sp = sp + ".json";
        }
        var dir = Path.GetDirectoryName(sp);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var data = Activator.CreateInstance(SatchelExt.TFsmDataInstance, (object)fsm.FsmComponent);
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
            sp, json);
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        fsm = null;
        panel.transform.SetParent(Pool<tk2dSpriteWidget>.Instance.InactiveHolder.transform);
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);
        panel.transform.SetParent(inspector.UIRoot.transform);
        panel.transform.SetSiblingIndex(inspector.UIRoot.transform.childCount - 2);

        string text;
        if (target is PlayMakerFSM pm)
        {
            fsm = pm.Fsm;
            if (fsm is null) return;
            text = pm.name + "-" + fsm.Name;
        }
        else
        {
            fsm = (Fsm)target;
            text = fsm.Name;
        }
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }

        savePathInput.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value,
            text + ".json");
    }
}
