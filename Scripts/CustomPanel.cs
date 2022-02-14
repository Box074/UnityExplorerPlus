
namespace UnityExplorerPlusMod;

abstract class CustomPanel : UIPanel
{
    internal UnityExplorer.UI.UIManager.Panels panelType;
    public override UnityExplorer.UI.UIManager.Panels PanelType => panelType;
    public override bool ShouldSaveActiveState => true;
    public override bool ShowByDefault => true;
}
