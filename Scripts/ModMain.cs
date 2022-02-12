
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase
{
    public static Dictionary<int, MouseInspectorBase> inspectors = new();
    public override void Initialize()
    {
        if (typeof(UExplorer.UExplorer)
            .GetMethod("PatchWorldInspector", BindingFlags.Static | BindingFlags.NonPublic) is null)
        {
            WorldInspectPatch.PatchWorldInspector();
        }
        Init().StartCoroutine();
    }
    public static void AddInspector(string name, MouseInspectorBase inspector)
    {
        int id  = InspectorPanel.Instance.MouseInspectDropdown.options.Count;
        InspectorPanel.Instance.MouseInspectDropdown.options.Add(new(name));
        inspectors.Add(id, inspector);
    }
    private IEnumerator Init()
    {
        while(UnityExplorer.UI.UIManager.Initializing) yield return null;
        AddInspector("Renderer", new RendererInspector());
        AddInspector("Enemy", new EnemyInspector());
        HookEndpointManager.Add(typeof(InspectUnderMouse).GetMethod("OnDropdownSelect"), PatchOnDropdownSelect);
        HookEndpointManager.Add(typeof(InspectUnderMouse).GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);
    }
    public MouseInspectorBase Patch_get_CurrentInspector(Func<InspectUnderMouse, MouseInspectorBase> orig,
        InspectUnderMouse self)
    {
        if(inspectors.TryGetValue((int)InspectUnderMouse.Mode, out var insp))
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
            InspectUnderMouse.Instance.StartInspect((MouseInspectMode)index);
        }
        else
        {
            orig(index);
        }
    }
}
