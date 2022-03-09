
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase
{
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public static ReflectionObject mouseInspector = null;
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

        mouseInspector = MouseInspector.Instance.CreateReflectionObject();

        InitPanel();

        //AddInspector("Renderer", new RendererInspector());
        AddInspector("Enemy", new EnemyInspector());
        HookEndpointManager.Add(typeof(MouseInspector).GetMethod("OnDropdownSelect"), PatchOnDropdownSelect);
        HookEndpointManager.Add(typeof(MouseInspector).GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);
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

    public MouseInspectorBase Patch_get_CurrentInspector(Func<MouseInspector, MouseInspectorBase> orig,
        MouseInspector self)
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
    public void PatchOnDropdownSelect(Action<int> orig, int index)
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
