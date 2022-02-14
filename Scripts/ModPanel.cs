
namespace UnityExplorerPlusMod;

class ModPanel : CustomPanel , ICellPoolDataSource<ModCell>
{
    public override int MinHeight => 200;
    public override int MinWidth => 360;
    public override string Name => "Mods";
    public GameObject UiRoot;
    public List<IMod> modCache = new();
    public ScrollPool<ModCell> scrollPool;
    public static Type TModLoader = typeof(Mod).Assembly.GetType("Modding.ModLoader");
    public static PropertyInfo FModInstances = TModLoader.GetProperty("ModInstances");
    public static Type TModInstance = TModLoader.GetNestedType("ModInstance");
    public static FieldInfo FMod = TModInstance.GetField("Mod");
    public static Type TModInstanceSet = typeof(HashSet<>).MakeGenericType(TModInstance);
    public static MethodInfo MGetEnumerator = TModInstanceSet.GetMethod("GetEnumerator");
    public override void ConstructPanelContent()
    {
        /* UiRoot = UIFactory.CreateHorizontalGroup(uiContent, "TabBar", true, 
            true, true, true, 2, new Vector4(2f, 2f, 2f, 2f), default(Color), null);
        UIFactory.SetLayoutElement(UiRoot, null, 25, null, 0, null, null, null);*/
        scrollPool = UIFactory.CreateScrollPool<ModCell>(UIRoot, "ModList", out var root, 
            out var content, new Color(0.11f, 0.11f, 0.11f));
        UIFactory.SetLayoutElement(root, null, null, null, 9999, null, null, null);
        UIFactory.SetLayoutElement(content, null, 25, null, 9999, null, null, null);
        content.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
        //scrollPool.Initialize(this);
        modCache.Clear();
        var mods = (IEnumerator)MGetEnumerator.Invoke(FModInstances.GetValue(null, null), null);
        while (mods.MoveNext())
        {
            var mod = (IMod)FMod.GetValue(mods.Current);
            if (mod == null) continue;
            modCache.Add(mod);
        }
        typeof(ScrollPool<ModCell>).GetMethod("Initialize").Invoke(scrollPool, new object[] { this, null });
        Refresh();
    }
    protected override void DoSetDefaultPosAndAnchors()
    {
        Rect.localPosition = Vector2.zero;
        Rect.pivot = new Vector2(0f, 1f);
        Rect.anchorMin = new Vector2(0.125f, 0.175f);
        Rect.anchorMax = new Vector2(0.325f, 0.925f);
    }
    public void Refresh()
    {
        var mods = (IEnumerator)MGetEnumerator.Invoke(FModInstances.GetValue(null, null), null);
        modCache.Clear();
        while(mods.MoveNext()) {
            var mod = (IMod)FMod.GetValue(mods.Current);
            if(mod == null) continue;
            modCache.Add(mod);
        }
        scrollPool.Refresh(true, true);
    }
    public void OnCellBorrowed(ModCell cell)
    {

    }
    public void SetCell(ModCell cell, int index)
    {
        if(index < modCache.Count)
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
