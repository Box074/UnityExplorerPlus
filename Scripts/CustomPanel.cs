
namespace UnityExplorerPlusMod;

abstract class CustomPanel : UEPanel
{
    protected CustomPanel(UIBase owner) : base(owner)
    {

    }
    internal UnityExplorer.UI.UIManager.Panels panelType = (UnityExplorer.UI.UIManager.Panels)id++;
    private static int id = 10;
    public override UnityExplorer.UI.UIManager.Panels PanelType => panelType;
    public override bool ShouldSaveActiveState => true;
    public override bool ShowByDefault => true;
    public override void ApplySaveData()
    {

    }
}
