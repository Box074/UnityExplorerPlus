namespace UnityExplorerPlus.Inspectors;

class RendererInspector : MouseInspectorBase
{
    public RendererInspectorResultPanel resultPanel = new();
    private List<GameObject> currentGameObjects = new List<GameObject>();
    public Renderer[] rendererCache = null;
    public float cacheTime = 0;


    public override void OnEndInspect()
    {
        cacheTime = 0;
        rendererCache = null;
    }
    public override void OnBeginMouseInspect()
    {
        currentGameObjects.Clear();
        cacheTime = 0;
    }
    private IEnumerator SetPanelActiveCoro()
    {
        yield return null;
        resultPanel.SetActive(true);
        resultPanel.ShowResults();
        yield break;
    }
    public override void ClearHitData()
    {
        currentGameObjects.Clear();
    }
    public override void OnSelectMouseInspect()
    {
        resultPanel.Result.Clear();
        resultPanel.Result.AddRange(currentGameObjects);
        RuntimeHelper.StartCoroutine(SetPanelActiveCoro());
    }

    public static bool TestPointInTrig(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        var signOfTrig = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        var signOfAB = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        var signOfCA = (a.x - c.x) * (p.y - c.y) - (a.y - c.y) * (p.x - c.x);
        var signOfBC = (c.x - b.x) * (p.y - c.y) - (c.y - b.y) * (p.x - c.x);
        var d1 = signOfAB * signOfTrig > 0;
        var d2 = signOfCA * signOfTrig > 0;
        var d3 = signOfBC * signOfTrig > 0;
        return d1 && d2 && d3;
    }
    public override void UpdateMouseInspect(Vector2 _)
    {
        if (Time.unscaledTime - cacheTime > 0.5f || rendererCache is null)
        {
            rendererCache = UnityEngine.Object.FindObjectsOfType<Renderer>();
        }
        var cam = CameraSwitcher.GetCurrentCamera();
        if (cam == null)
        {
            MouseInspector.Instance.StopInspect();
            return;
        }
        currentGameObjects.Clear();

        var p = CameraSwitcher.GetCurrentMousePosition();

        foreach (var v in rendererCache
            .Where(x => x != null)
            .Where(x => x.isVisible)
            .Where(x => x.enabled)
            .Where(x => x.gameObject.activeInHierarchy)
            .OrderBy(x => x.transform.position.z))
        {
            Vector2 pos = v.transform.position;
            var scale = new Vector3(v.transform.GetScaleX(), v.transform.GetScaleY(), 1);
            if (v is MeshRenderer mr)
            {
                var filter = v.GetComponent<MeshFilter>();
                if (filter == null) continue;
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                var points = mesh.vertices;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    var trig = filter.sharedMesh.GetTriangles(i);
                    for (int i2 = 0; i2 < trig.Length; i2 += 3)
                    {
                        var a = points[trig[i]].MultiplyElements(scale) + (Vector3)pos;
                        var b = points[trig[i + 1]].MultiplyElements(scale) + (Vector3)pos;
                        var c = points[trig[i + 2]].MultiplyElements(scale) + (Vector3)pos;

                        if (TestPointInTrig(a, b, c, p))
                        {
                            currentGameObjects.Add(v.gameObject);
                            break;
                        }
                    }
                }
            }
            else if (v is SpriteRenderer sprite)
            {
                if (sprite.sprite == null) continue;
                var points = sprite.sprite.vertices;
                var trig = sprite.sprite.triangles;
                for (int i = 0; i < trig.Length; i += 3)
                {
                    var a = points[trig[i]].MultiplyElements(scale) + pos;
                    var b = points[trig[i + 1]].MultiplyElements(scale) + pos;
                    var c = points[trig[i + 2]].MultiplyElements(scale) + pos;

                    if (TestPointInTrig(a, b, c, p))
                    {
                        currentGameObjects.Add(v.gameObject);
                        break;
                    }
                }
            }
        }

        if (currentGameObjects.Count > 0)
        {
            MouseInspectorR.Instance.objNameLabel.text = $"Click to view renderers under mouse{(Vector2)p}: {currentGameObjects.Count}";
        }
        else
        {
            MouseInspectorR.Instance.objNameLabel.text = "No renderers under mouse.";
        }

    }
}
