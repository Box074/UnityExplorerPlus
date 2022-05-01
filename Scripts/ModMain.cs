
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase<UnityExplorerPlus>
{
    public static Type MouseInspectorType => typeof(MouseInspector);
    public static AssetsInfo prefabMap = null;
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public static ReflectionObject mouseInspector = null;
    public readonly static ReflectionObject RTMouseInspactor = MouseInspectorType.CreateReflectionObject();
    public override void Initialize()
    {
        prefabMap =
            JsonConvert.DeserializeObject<AssetsInfo>(
                System.Text.Encoding.UTF8.GetString(this.GetEmbeddedResource("UnityExplorerPlus.Prefabs.json")));
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
        CheckAssembly("UnityExplorer.Standalone.Mono", new Version(4,7,12));
    }
    private IEnumerator Init()
    {
        while (UnityExplorer.UI.UIManager.Initializing) yield return null;

        mouseInspector = MouseInspector.Instance.CreateReflectionObject();

        InitPanel();

        //AddInspector("Renderer", new RendererInspector());
        AddInspector("Enemy", new EnemyInspector());
        AddInspector("World Position", new WorldPositionPin());
        HookEndpointManager.Add(MouseInspectorType.GetMethod("OnDropdownSelect"), PatchOnDropdownSelect);
        HookEndpointManager.Add(MouseInspectorType.GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);

        FsmUtils.Init();

        if (HaveAssembly("GODump"))
        {
            GODumpExt.Init();
        }
        if (HaveAssembly("Satchel"))
        {
            SatchelExt.Init();
        }

        PatchGameObjectControls.Init();
    }

    public void InitPanel()
    {

        var uibase = (UIBase)FindFieldInfo("UnityExplorer.UI.UIManager::<UiBase>k__BackingField").FastGet((object)null);
        var panels = new CustomPanel[]
        {
            new ModPanel(uibase)
        };
        var UIPanels = (Dictionary<UnityExplorer.UI.UIManager.Panels, UEPanel>)typeof(UnityExplorer.UI.UIManager)
            .GetField("UIPanels", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null);
        int id = UIPanels.Count;
        foreach (var v in panels)
        {
            UIPanels.Add((UnityExplorer.UI.UIManager.Panels)id, v);
            v.panelType = (UnityExplorer.UI.UIManager.Panels)id;
            try
            {
                UnityExplorer.UI.UIManager.SetPanelActive(v, false);
            }
            catch (Exception e)
            {
                LogError(e);
            }
            id++;
        }
    }

    public MouseInspectorBase Patch_get_CurrentInspector(Func<object, MouseInspectorBase> orig,
        object self)
    {
        if (inspectors.TryGetValue((int)RTMouseInspactor["Mode"].As<MouseInspectMode>(), out var insp))
        {
            return insp;
        }
        else
        {
            return orig(self);
        }
    }
    public void PatchOnDropdownSelect(Action<int> orig, int index)
    {
        if (inspectors.TryGetValue(index, out var insp))
        {
            InspectorPanel.Instance.MouseInspectDropdown.value = 0;
            mouseInspector.InvokeMethod("StartInspect", (MouseInspectMode)index);
        }
        else
        {
            orig(index);
        }
    }
}
