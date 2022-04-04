
namespace UnityExplorerPlusMod;

class SpriteAtlasWidget : UnityObjectWidget
{
    public SpriteAtlas atlas;
    public InputFieldRef savePathInput;
    public GameObject panel;
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);
        var toggleButton = UIFactory.CreateButton(UIRoot, "SRWidgetToggleButton", "Dump", new Color?(new Color(0.2f, 0.3f, 0.2f)));
        toggleButton.Transform.SetSiblingIndex(0);
        toggleButton.Component.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
        });
        UIFactory.SetLayoutElement(toggleButton.Component.gameObject, 170, 25, null, null, null, null, null);
        var parent = UIFactory.CreateHorizontalGroup(uiRoot, "Sprite Save Row", false, false, true, true, 2, new Vector4(2f, 2f, 2f, 2f), new Color(0.1f, 0.1f, 0.1f), null);
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
        if (atlas is null)
        {
            ExplorerCore.LogWarning("SpriteAtlas is null, maybe it was destroyed?");
            return;
        }
        if (string.IsNullOrEmpty(savePathInput.Text))
        {
            ExplorerCore.LogWarning("Save path cannot be empty!");
            return;
        }
        Directory.CreateDirectory(savePathInput.Text);
        var sr = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sr);
        foreach(var v in sr)
        {
            var p = Path.Combine(savePathInput.Text, v.name.Replace("(Clone)", "") + ".png");
            var tex = SpriteUtils.ExtractTextureFromSprite(v, false);
            File.WriteAllBytes(p, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(v);
        }
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        atlas = null;
        panel.transform.SetParent(Pool<tk2dSpriteWidget>.Instance.InactiveHolder.transform);
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);
        panel.transform.SetParent(inspector.UIRoot.transform);
        panel.transform.SetSiblingIndex(inspector.UIRoot.transform.childCount - 2);

        atlas = (SpriteAtlas)target;
        var text = atlas.name;
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }
        
        savePathInput.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value,
            "Atlas-" + text);
    }
}
