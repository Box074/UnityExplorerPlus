namespace UnityExplorerPlus.Inspectors;

class EnemyInspector : MouseInspectorBase
{
    public GameObject lastHit = null;
    public static Text objNameLabel => MouseInspector.Instance.Reflect().objNameLabel;
    public static Text objPathLabel => MouseInspector.Instance.Reflect().objPathLabel;
    public override void OnBeginMouseInspect()
    {
        lastHit = null;
    }
    public override void ClearHitData()
    {
        lastHit = null;
    }
    public void UpdateText(GameObject go)
    {
        if (go == null)
        {
            MouseInspector.Instance.Reflect().ClearHitData();
            return;
        }
        objNameLabel.text = "<b>Click to Inspect:</b> <color=cyan>" + go.name + "</color>";
        objPathLabel.text = "Path: " + go.transform.GetTransformPath(true);
    }
    public override void OnEndInspect()
    {

    }
    public override void UpdateMouseInspect(Vector2 _)
    {
        var cam = CameraSwitcher.GetCurrentCamera();
        if (cam == null)
        {
            MouseInspector.Instance.StopInspect();
            return;
        }

        var worldPos = CameraSwitcher.GetCurrentMousePosition();
        var hit = Physics2D.OverlapPointAll(worldPos, Physics2D.AllLayers)
            .Where(x => x.GetComponent<HealthManager>() is not null)
            .FirstOrDefault();
        var go = hit?.gameObject;
        if (go != lastHit)
        {
            lastHit = go;
            UpdateText(go);
        }
    }
    public override void OnSelectMouseInspect()
    {
        if (lastHit != null)
        {
            InspectorManager.Inspect(lastHit);
        }
    }
}
