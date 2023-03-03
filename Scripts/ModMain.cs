
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBaseWithSettings<UnityExplorerPlus, UnityExplorerPlus.Settings, object>, IGlobalSettings<UnityExplorerPlus.Settings>
{
    public class Settings
    {
        public string fsmViewerPath = "";
        public int fsmViewerPort = 60023;
    }
    public static AssetsInfo prefabMap = JsonConvert.DeserializeObject<AssetsInfo>(
        Encoding.UTF8.GetString(ModResources.PREFABMAP)
        )!;
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public override void Initialize()
    {
        Init().StartCoroutine();

        _ = Executer.Instance;
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
        WidgetManager.Init();
        FsmUtils.Init();

        PatchGameObjectControls.Init();
    }

    public void InitPanel()
    {
        var uibase = UUIManagerR.UiBase;
        var UIPanels = UUIManagerR.UIPanels;
        var panels = new CustomPanel[]
        {
            new ModPanel(uibase)
        };
        
        foreach (var v in panels)
        {
            UIPanels.Add(v.PanelType.Reflect(), v.Reflect());
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

        if (inspectors.TryGetValue(index, out var _))
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
