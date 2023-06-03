using UnityExplorerPlus.LineDrawing;

namespace UnityExplorerPlus.Inspectors;

class RendererInspector : MouseInspectorBase, ILineProvider
{
    public RendererInspector()
    {
        LineRenderer2.Instance.providers.Add(this);
    }

    public RendererInspectorResultPanel resultPanel = new();
    private List<GameObject> currentGameObjects = new List<GameObject>();
    public Renderer[] rendererCache = null;
    public float cacheTime = 0;

    public List<LineData> Lines { get; } = new();

    public override void OnEndInspect()
    {
        Lines.Clear();
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
    private Vector2 LocalToScreenPoint(Vector3 point)
    {
        Vector2 result = CameraSwitcher.GetCurrentCamera().WorldToScreenPoint(point);
        return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
    }
    private void AddLine(Vector2 a, Vector2 b, float z)
    {
        if (!UnityExplorerPlus.Instance.settings.enableRendererBox.Value) return;
        var c = Mathf.RoundToInt((z * 50) % 255) / 255f;
        var color = new Color(1, c, c, 1);
        Lines.Add(new(LocalToScreenPoint(a), LocalToScreenPoint(b), color, Mathf.RoundToInt(z * 100)));
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
        Lines.Clear();

        var p = CameraSwitcher.GetCurrentMousePosition();

        foreach (var v in rendererCache
            .Where(x => x != null)
            .Where(x => x.isVisible)
            .Where(x => x.enabled)
            .Where(x => x.gameObject.activeInHierarchy)
            .OrderBy(x => x.transform.position.z))
        {
            Vector2 pos = v.transform.position;
            Vector3 pos3 = v.transform.position;
            var scale = new Vector3(v.transform.GetScaleX(), v.transform.GetScaleY(), 1);
            var vert = new List<(Vector3, Vector3, Vector3)>();
            if (v is MeshRenderer mr)
            {
                var filter = v.GetComponent<MeshFilter>();
                if (filter == null) continue;
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                var points = mesh.vertices;
                vert.Clear();
                bool isTouch = false;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    var trig = filter.sharedMesh.GetTriangles(i);
                    for (int i2 = 0; i2 < trig.Length; i2 += 3)
                    {
                        var a = points[trig[i]].MultiplyElements(scale) + (Vector3)pos;
                        var b = points[trig[i + 1]].MultiplyElements(scale) + (Vector3)pos;
                        var c = points[trig[i + 2]].MultiplyElements(scale) + (Vector3)pos;
                        vert.Add((a, b, c));
                        if (!isTouch && TestPointInTrig(a, b, c, p))
                        {
                            currentGameObjects.Add(v.gameObject);
                            isTouch = true;
                        }
                    }
                }
                if(isTouch)
                {
                    foreach (var (a, b, c) in vert)
                    {
                        AddLine(a, b, pos3.z);
                        AddLine(b, c, pos3.z);
                        AddLine(a, c, pos3.z);
                    }
                }
            }
            else if (v is SpriteRenderer sprite)
            {
                if (sprite.sprite == null) continue;
                var points = sprite.sprite.vertices;
                var trig = sprite.sprite.triangles;
                vert.Clear();
                bool isTouch = false;
                for (int i = 0; i < trig.Length; i += 3)
                {
                    var a = points[trig[i]].MultiplyElements(scale) + pos;
                    var b = points[trig[i + 1]].MultiplyElements(scale) + pos;
                    var c = points[trig[i + 2]].MultiplyElements(scale) + pos;
                    vert.Add((a, b, c));
                    if (!isTouch && TestPointInTrig(a, b, c, p))
                    {
                        currentGameObjects.Add(v.gameObject);
                        isTouch = true;
                    }
                }
                if (isTouch)
                {
                    foreach (var (a, b, c) in vert)
                    {
                        AddLine(a, b, pos3.z);
                        AddLine(b, c, pos3.z);
                        AddLine(a, c, pos3.z);
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
