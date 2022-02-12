
namespace UnityExplorerPlusMod;

class EnemyInspector : MouseInspectorBase
{
    public GameObject lastHit = null;
    public static Text objNameLabel =>
        ReflectionHelper.GetField<InspectUnderMouse, Text>(InspectUnderMouse.Instance, "objNameLabel");
    public static Text objPathLabel =>
        ReflectionHelper.GetField<InspectUnderMouse, Text>(InspectUnderMouse.Instance, "objPathLabel");
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
            WorldInspectPatch.methodInfo_ClearHitData.Invoke(InspectUnderMouse.Instance, null);
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
            InspectUnderMouse.Instance.StopInspect();
            return;
        }
        var mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
        var worldPos = cam.ScreenToWorldPoint(mousePos);
        var hit = Physics2D.OverlapPointAll(worldPos, Physics2D.AllLayers)
            .Where(x=> x.GetComponent<HealthManager>() != null)
            .FirstOrDefault(x =>
            x.transform.position.z > 0);
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
