namespace UnityExplorerPlus;

abstract class CustomPanel : UEPanel
{
    protected CustomPanel(UIBase owner) : base(owner)
    {

    }
    internal UUIManager.Panels panelType = (UUIManager.Panels)id++;
    private static int id = 100;
    public override UUIManager.Panels PanelType => panelType;
    public override bool ShouldSaveActiveState => true;
    public override bool ShowByDefault => true;
}
