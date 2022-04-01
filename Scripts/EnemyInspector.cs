
namespace UnityExplorerPlusMod;

class EnemyInspector : MouseInspectorBase
{
    public GameObject lastHit = null;
    public static Text objNameLabel =>
        UnityExplorerPlus.mouseInspector["objNameLabel"].As<Text>();
    public static Text objPathLabel =>
        UnityExplorerPlus.mouseInspector["objPathLabel"].As<Text>();
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
            UnityExplorerPlus.mouseInspector.InvokeMethod("ClearHitData");
            //WorldInspectPatch.methodInfo_ClearHitData.FastInvoke(MouseInspector.Instance);
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
        var cam = Camera.main;
        if (cam == null)
        {
            UnityExplorerPlus.mouseInspector.InvokeMethod("StopInspect");
            return;
        }
        var mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
        var worldPos = cam.ScreenToWorldPoint(mousePos);
        var hit = Physics2D.OverlapPointAll(worldPos, Physics2D.AllLayers)
            .Where(x=> x.GetComponent<HealthManager>() is not null)
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
        if(lastHit != null)
        {
            InspectorManager.Inspect(lastHit);
        }
    }
}
