
namespace UnityExplorerPlusMod;

class UnityExplorerPlus : ModBase
{
    public static int rendererInspectorId = -1;
    public static RendererInspector rendererInspector = new();
    public override void Initialize()
    {
        if (typeof(UExplorer.UExplorer)
            .GetMethod("PatchWorldInspector", BindingFlags.Static | BindingFlags.NonPublic) is null)
        {
            WorldInspectPatch.PatchWorldInspector();
        }
        Init().StartCoroutine();
    }
    private IEnumerator Init()
    {
        while(UnityExplorer.UI.UIManager.Initializing) yield return null;
        rendererInspectorId = InspectorPanel.Instance.MouseInspectDropdown.options.Count;
        InspectorPanel.Instance.MouseInspectDropdown.options.Add(new("Renderer Inspector"));
        HookEndpointManager.Add(typeof(InspectUnderMouse).GetMethod("OnDropdownSelect"), PatchOnDropdownSelect);
        HookEndpointManager.Add(typeof(InspectUnderMouse).GetMethod("get_CurrentInspector"), Patch_get_CurrentInspector);
    }
    public MouseInspectorBase Patch_get_CurrentInspector(Func<InspectUnderMouse, MouseInspectorBase> orig,
        InspectUnderMouse self)
    {
        if((int)InspectUnderMouse.Mode == rendererInspectorId)
        {
            return rendererInspector;
        }
        else
        {
            return orig(self);
        }
    }
    public void PatchOnDropdownSelect(Action<int> orig, int index)
    {
        if (index == rendererInspectorId)
        {
            InspectorPanel.Instance.MouseInspectDropdown.value = 0;
            InspectUnderMouse.Instance.StartInspect((MouseInspectMode)rendererInspectorId);
        }
        else
        {
            orig(index);
        }
    }
}
