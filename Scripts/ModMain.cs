
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase<UnityExplorerPlus>
{
    private static Type _TMouseInspector;
    public static Type MouseInspectorType
    {
        get
        {
            if(_TMouseInspector is null)
            {
                _TMouseInspector = 
                    HKTool.Reflection.ReflectionHelper.FindType("UnityExplorer.Inspectors.MouseInspector") ??
                    HKTool.Reflection.ReflectionHelper.FindType("UnityExplorer.Inspectors.InspectUnderMouse");
            }
            return _TMouseInspector;
        }
    }
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public static ReflectionObject mouseInspector = null;
    public readonly static ReflectionObject RTMouseInspactor = MouseInspectorType.CreateReflectionObject();
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
    private IEnumerator Init()
    {
        while (UnityExplorer.UI.UIManager.Initializing) yield return null;

        mouseInspector = RTMouseInspactor["Instance"];

        InitPanel();

        //AddInspector("Renderer", new RendererInspector());
        AddInspector("Enemy", new EnemyInspector());
        HookEndpointManager.Add(MouseInspectorType.GetMethod("OnDropdownSelect"), PatchOnDropdownSelect);
        HookEndpointManager.Add(MouseInspectorType.GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);
    }

    public void InitPanel()
    {
        var panels = new CustomPanel[]
        {
            new ModPanel()
        };
        var UIPanels = (Dictionary<UnityExplorer.UI.UIManager.Panels, UIPanel>)typeof(UnityExplorer.UI.UIManager)
            .GetField("UIPanels", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null);
        int id = UIPanels.Count;
        foreach (var v in panels)
        {
            UIPanels.Add((UnityExplorer.UI.UIManager.Panels)id, v);
            v.panelType = (UnityExplorer.UI.UIManager.Panels)id;
            try
            {
                v.ConstructUI();
                UnityExplorer.UI.UIManager.SetPanelActive(v, false);
                v.ApplySaveData();
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
