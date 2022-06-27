
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase<UnityExplorerPlus>
{
    public static AssetsInfo prefabMap = JsonConvert.DeserializeObject<AssetsInfo>(ModRes.PREFAB_INFO);
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public override void Initialize()
    {
        Init().StartCoroutine();
    }
    public static void AddInspector(string name, MouseInspectorBase inspector)
    {
        int id = InspectorPanel.Instance.MouseInspectDropdown.options.Count;
        InspectorPanel.Instance.MouseInspectDropdown.options.Add(new(name));
        inspectors.Add(id, inspector);
    }
    public override void OnCheckDependencies()
    {
        CheckAssembly("UnityExplorer.Standalone.Mono", new Version(4,8,2));
    }
    private IEnumerator Init()
    {
        PatchReflectionInspector.Init();
        ReferenceSearch.Init();
        while (UnityExplorer.UI.UIManager.Initializing) yield return null;

        InitPanel();

        //AddInspector("Renderer", new RendererInspector());
        AddInspector("Enemy", new EnemyInspector());
        AddInspector("World Position", new WorldPositionPin());
        On.UnityExplorer.Inspectors.MouseInspector.OnDropdownSelect += PatchOnDropdownSelect;
        HookEndpointManager.Add(typeof(MouseInspector).GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);

        FsmUtils.Init();
        
        if (HaveAssembly("Satchel"))
        {
            SatchelExt.Init();
        }

        PatchGameObjectControls.Init();
    }

    public void InitPanel()
    {
        var uibase = UnityExplorer.UI.StaticUIManager.get_UiBase();
        var UIPanels = UnityExplorer.UI.StaticUIManager.private_UIPanels();
        var panels = new CustomPanel[]
        {
            new ModPanel(uibase)
        };
        
        foreach (var v in panels)
        {
            UIPanels.Add((UnityExplorer.UI.UIManager.Panels)v.PanelType, v);
            try
            {
                UnityExplorer.UI.UIManager.SetPanelActive(v, false);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }

    public MouseInspectorBase Patch_get_CurrentInspector(Func<object, MouseInspectorBase> orig,
        object self)
    {
        if (inspectors.TryGetValue((int)MouseInspector.Mode, out var insp))
        {
            return insp;
        }
        else
        {
            return orig(self);
        }
    }
    public void PatchOnDropdownSelect(On.UnityExplorer.Inspectors.MouseInspector.orig_OnDropdownSelect orig, int index)
    {
        if (inspectors.TryGetValue(index, out var insp))
        {
            InspectorPanel.Instance.MouseInspectDropdown.value = 0;
            MouseInspector.Instance.StartInspect((MouseInspectMode)index);
        }
        else
        {
            orig(index);
        }
    }
}
