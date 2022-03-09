
namespace UnityExplorerPlusMod;

class RendererInspector : MouseInspectorBase
{
    public GameObject lastHit = null;
    public Renderer[] rendererCache = null;
    public float cacheTime = 0;
    public static Text objNameLabel =>
        UnityExplorerPlus.mouseInspector["objNameLabel"].As<Text>();
    public static Text objPathLabel =>
        UnityExplorerPlus.mouseInspector["objPathLabel"].As<Text>();
    public override void OnEndInspect()
    {
        cacheTime = 0;
        rendererCache = null;
    }
    public override void OnBeginMouseInspect()
    {
        cacheTime = 0;
    }
    public override void ClearHitData()
    {
        lastHit = null;
    }
    public override void OnSelectMouseInspect()
    {
        if(lastHit != null)
        {
            InspectorManager.Inspect(lastHit);
        }
    }
    public void UpdateText(GameObject go)
    {
        if(go == null)
        {
            WorldInspectPatch.methodInfo_ClearHitData.Invoke(MouseInspector.Instance, null);
            return;
        }
        objNameLabel.text = "<b>Click to Inspect:</b> <color=cyan>" + go.name + "</color>";
        objPathLabel.text = "Path: " + go.transform.GetTransformPath(true);
    }
    public static bool TestPointInTrig(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        var signOfTrig = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        var signOfAB = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        var signOfCA = (a.x - c.x) * (p.y - c.y) - (a.y - c.y) * (p.x - c.x);
        var signOfBC = (c.x - b.x) * (p.y - c.y) - (c.y - b.y) * (p.x - c.x);
        var d1 = (signOfAB * signOfTrig > 0);
        var d2 = (signOfCA * signOfTrig > 0);
        var d3 = (signOfBC * signOfTrig > 0);
        return d1 && d2 && d3;
    }
    public override void UpdateMouseInspect(Vector2 _)
    {
        if (Time.unscaledTime - cacheTime > 0.5f || rendererCache is null)
        {
            rendererCache = UnityEngine.Object.FindObjectsOfType<Renderer>();
        }
        var cam = Camera.main;
        if (cam == null)
        {
            MouseInspector.Instance.StopInspect();
            return;
        }
        var mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
        var p = cam.ScreenToWorldPoint(mousePos);
        GameObject go = null;
        foreach (var v in rendererCache
            .Where(x => x != null)
            .Where(x => x.isVisible)
            .OrderBy(x => x.transform.position.z))
        {
            Vector2 pos = v.transform.position;
            var scale = new Vector3(v.transform.GetScaleX(), v.transform.GetScaleY(), 1);
            if (v is MeshRenderer mesh)
            {
                var filter = v.GetComponent<MeshFilter>();
                if (filter == null) continue;
                if(filter.mesh == null) continue;
                var points = filter.mesh.vertices;
                
                for (int i = 0; i < filter.mesh.subMeshCount; i++)
                {
                    var trig = filter.mesh.GetTriangles(i);
                    for (int i2 = 0; i2 < trig.Length; i2 += 3)
                    {
                        var a = points[trig[i]].MultiplyElements(scale)  + (Vector3)pos;
                        var b = points[trig[i + 1]].MultiplyElements(scale) + (Vector3)pos;
                        var c = points[trig[i + 2]].MultiplyElements(scale) + (Vector3)pos;

                        if (TestPointInTrig(a, b, c, p))
                        {
                            go = v.gameObject;
                            break;
                        }
                    }
                    if (go != null) break;
                }
            }
            else if (v is SpriteRenderer sprite)
            {
                if(sprite.sprite == null) continue;
                var points = sprite.sprite.vertices;
                var trig = sprite.sprite.triangles;
                for(int i = 0; i < trig.Length ; i+= 3)
                {
                    var a = points[trig[i]].MultiplyElements(scale) + pos;
                    var b = points[trig[i + 1]].MultiplyElements(scale) + pos;
                    var c = points[trig[i + 2]].MultiplyElements(scale) + pos;
                    
                    if(TestPointInTrig(a, b, c, p))
                    {
                        go = v.gameObject;
                        break;
                    }
                }
                if(go != null) break;
            }
        }
        if(lastHit != go)
        {
            lastHit = go;
            UpdateText(go);
        }
    }
}
