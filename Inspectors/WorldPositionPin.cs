namespace UnityExplorerPlus.Inspectors;

class WorldPositionPin : MouseInspectorBase
{
    public static Text objNameLabel => MouseInspector.Instance.Reflect().objNameLabel;
    public static Text objPathLabel => MouseInspector.Instance.Reflect().objPathLabel;
    public override void OnBeginMouseInspect()
    {

    }
    public override void ClearHitData()
    {

    }
    public override void OnEndInspect()
    {

    }
    public override void UpdateMouseInspect(Vector2 _)
    {
        objNameLabel.text = "<b>World Position: </b><color=cyan>" + 
            CameraSwitcher.GetCurrentMousePosition() + "</color>";
        objPathLabel.text = "";
    }
    public override void OnSelectMouseInspect()
    {
        var pos = Input.mousePosition;
        pos.z = CameraSwitcher.GetCurrentCamera().WorldToScreenPoint(Vector3.zero).z;
        pos = CameraSwitcher.GetCurrentCamera().ScreenToWorldPoint(pos);
        ClipboardPanel.Copy((Vector2)pos);
        UUIManager.SetPanelActive(UUIManager.Panels.Clipboard, true);
    }
}
