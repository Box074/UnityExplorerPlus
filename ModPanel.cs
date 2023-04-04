namespace UnityExplorerPlus;

class ModPanel : CustomPanel, ICellPoolDataSource<ModCell>
{
    public override int MinHeight => 200;
    public override int MinWidth => 360;
    public override string Name => "Mods";
    public List<(IMod, Type)> modCache = new();
    public ScrollPool<ModCell> scrollPool;
    public ModPanel(UIBase owner) : base(owner) { }
    protected override void ConstructPanelContent()
    {
        /* UiRoot = UIFactory.CreateHorizontalGroup(uiContent, "TabBar", true, 
            true, true, true, 2, new Vector4(2f, 2f, 2f, 2f), default(Color), null);
        UIFactory.SetLayoutElement(UiRoot, null, 25, null, 0, null, null, null);*/
        var title = UIRoot.FindChildWithPath("Content", "TitleBar").transform;
        title.parent = UIRoot.transform;
        title.SetAsFirstSibling();
        UnityEngine.Object.Destroy(UIRoot.FindChildWithPath("Content"));
        scrollPool = UIFactory.CreateScrollPool<ModCell>(UIRoot, "ModList", out var root,
            out var content, new Color(0.11f, 0.11f, 0.11f));
        UIFactory.SetLayoutElement(root, null, null, null, 9999, null, null, null);
        UIFactory.SetLayoutElement(content, null, 25, null, 9999, null, null, null);
        content.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
        //scrollPool.Initialize(this);
        Refresh();
        scrollPool.Initialize(this);
        //typeof(ScrollPool<ModCell>).GetMethod("Initialize").Invoke(scrollPool, new object[] { this, null });
        Refresh();
    }
    public override Vector2 DefaultAnchorMin
    {
        get
        {
            return new Vector2(0, -0.1744325f);
        }
    }
    public override Vector2 DefaultAnchorMax
    {
        get
        {
            return new Vector2(0.3215279f, 0.4169654f);
        }
    }

    public void Refresh()
    {
        modCache.Clear();
        foreach (var mod in ModLoaderR.ModInstanceTypeMap)
        {
            modCache.Add((mod.Value.Mod.ToOriginal(), mod.Key));
        }
        scrollPool.Refresh(true, true);
    }
    public void OnCellBorrowed(ModCell cell)
    {

    }
    public void SetCell(ModCell cell, int index)
    {
        if (index < modCache.Count)
        {
            cell.BindMod(modCache[index]);
            cell.Enable();
        }
        else
        {
            cell.Disable();
        }
    }
    public int ItemCount => modCache.Count;
}
