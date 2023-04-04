namespace UnityExplorerPlus;

class ModCell : ICell
{
    public bool Enabled { get; private set; }
    public RectTransform Rect { get; set; }
    public GameObject UIRoot { get; set; }
    public float DefaultHeight => 25;
    public ButtonRef nameButton;
    public Text nameText;
    public (IMod, Type) bindMod;
    public void Enable()
    {
        Enabled = true;
        UIRoot.SetActive(true);
    }
    public void Disable()
    {
        Enabled = false;
        UIRoot.SetActive(false);
    }
    public void OnClick()
    {
        InspectorManager.Inspect(bindMod.Item1 ?? (object)bindMod.Item2);
    }
    public void BindMod((IMod, Type) mod)
    {
        bindMod = mod;
        if (mod.Item1 != null)
        {
            nameButton.ButtonText.text = $"<i><color=green>{mod.Item1.GetName()}</color><color=grey>({mod.Item2.FullName})</color></i>";
        }
        else
        {
            nameButton.ButtonText.text = $"<i><color=red>Failed to load</color><color=grey>({mod.Item2.FullName})</color></i>";
        }
        nameButton.ButtonText.color = Color.white;
    }
    public GameObject CreateContent(GameObject root)
    {
        /*UIRoot = UIFactory.CreateUIObject("ModCell", root);
        UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(UIRoot, false, false, true, true, 2,
            null, null, null, null, TextAnchor.MiddleCenter);
        
        UIFactory.SetLayoutElement(UIRoot, 100, 25, 9999, 0, null, null, null);*/

        nameButton = UIFactory.CreateButton(root, "ModName", "Mod Name", null);
        UIRoot = nameButton.GameObject;
        nameText = nameButton.Component.GetComponentInChildren<Text>();
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RuntimeHelper.SetColorBlock(nameButton.Component, new Color(0.11f, 0.11f, 0.11f),
            new Color(0.25f, 0.25f, 0.25f),
            new Color(0.05f, 0.05f, 0.05f),
            new Color(1f, 1f, 1f, 0f));
        nameButton.Component.onClick.AddListener(OnClick);

        Rect = UIRoot.GetComponent<RectTransform>();
        Rect.anchorMin = new Vector2(0f, 1f);
        Rect.anchorMax = new Vector2(0f, 1f);
        Rect.pivot = new Vector2(0.5f, 1f);
        Rect.sizeDelta = new Vector2(25f, 25f);

        return UIRoot;
    }

}
