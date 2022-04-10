
namespace UnityExplorerPlusMod;

public abstract class DumpWidgetBase<T> : UnityObjectWidget where T : DumpWidgetBase<T>
{
    public InputFieldRef savePathInput;
    public GameObject panel;
    private string ext;
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);
        var toggleButton = UIFactory.CreateButton(UIRoot, "DumpWidgetToggleButton", "Dump", new Color?(new Color(0.2f, 0.3f, 0.2f)));
        toggleButton.Transform.SetSiblingIndex(0);
        toggleButton.Component.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
        });
        UIFactory.SetLayoutElement(toggleButton.Component.gameObject, 100, 25, null, null, null, null, null);
        var parent = UIFactory.CreateHorizontalGroup(uiRoot, "Save Row", false, false, true, true, 2, new Vector4(2f, 2f, 2f, 2f), new Color(0.1f, 0.1f, 0.1f), null);
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
    protected abstract void OnSave(string savePath);
    private void OnClickSave()
    {
        var sp = savePathInput.Text;
        if (string.IsNullOrEmpty(savePathInput.Text))
        {
            ExplorerCore.LogWarning("Save path cannot be empty!");
            return;
        }
        if(!string.IsNullOrEmpty(ext) && !sp.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase)) sp = sp + "." + ext;
        OnSave(sp);
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        panel.transform.SetParent(Pool<T>.Instance.InactiveHolder.transform);
    }
    protected void SetDefaultPath(string fn, string ext = "")
    {
        if (string.IsNullOrEmpty(fn))
        {
            fn = "untitled";
        }
        this.ext = ext;
        savePathInput.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value,
            string.IsNullOrEmpty(ext) ? fn : (fn + "." + ext));
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);
        panel.transform.SetParent(inspector.UIRoot.transform);
        panel.transform.SetSiblingIndex(inspector.UIRoot.transform.childCount - 2);

        
    }
}
