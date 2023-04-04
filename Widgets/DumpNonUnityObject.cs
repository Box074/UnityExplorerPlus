namespace UnityExplorerPlus.Widgets;

public abstract class DumpNonUnityObject<T> : NonUnityObjectWidget where T : DumpNonUnityObject<T>
{
    public InputFieldRef savePathInput;
    private string ext;
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);


        savePathInput = UIFactory.CreateInputField(UIRoot, "SaveInput", "...");
        UIFactory.SetLayoutElement(savePathInput.UIRoot, 100, 25, 9999, null, null, null, null);
        var buttonRef = UIFactory.CreateButton(UIRoot, "SaveButton", "Dump", new Color?(new Color(0.2f, 0.25f, 0.2f)));
        buttonRef.Component.onClick.AddListener(OnClickSave);
        UIFactory.SetLayoutElement(buttonRef.Component.gameObject, 100, 25, 0, null, null, null, null);
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
        if (!string.IsNullOrEmpty(ext) && !sp.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase)) sp = sp + "." + ext;
        OnSave(sp);
    }
    protected void SetDefaultPath(string fn, string ext = "")
    {
        if (string.IsNullOrEmpty(fn))
        {
            fn = "untitled";
        }
        this.ext = ext;
        savePathInput.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value,
            string.IsNullOrEmpty(ext) ? fn : fn + "." + ext);
    }
}
