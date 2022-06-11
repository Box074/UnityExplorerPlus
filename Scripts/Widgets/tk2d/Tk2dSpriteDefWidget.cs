
namespace UnityExplorerPlusMod;

class Tk2dSpriteDefWidget : Texture2DWidget
{
    public static Camera cacheCamera = null!;
    public static MeshRenderer cacheMeshR = null!;
    public static MeshFilter cacheMeshF = null!;
    public static GameObject cacheHolder = null!;
    public static tk2dSprite cacheTk2d = null!;
    public static GameObject rGO = null!;
    public static Mesh mesh = null!;
    public static void PrepareCamera()
    {
        cacheHolder = new GameObject("UEP tk2d Cache Holder");
        cacheHolder.transform.position = new Vector3(41563, 46689, 0);
        cacheHolder.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(cacheHolder);

        var camGO = new GameObject("Cache Camera");
        camGO.transform.parent = cacheHolder.transform;
        cacheCamera = camGO.AddComponent<Camera>();
        cacheCamera.orthographic = true;
        cacheCamera.clearFlags = CameraClearFlags.Nothing;

        rGO = new GameObject("Cache Renderer");
        rGO.transform.parent = cacheHolder.transform;
        rGO.transform.localPosition = new Vector3(0, 0, 0);
        cacheMeshF = rGO.AddComponent<MeshFilter>();
        cacheMeshR = rGO.AddComponent<MeshRenderer>();
    }
    public static void BuildCamera(tk2dSpriteCollectionData def, int id)
    {
        if (cacheHolder == null) PrepareCamera();
        /*if (mesh == null)
        {
            mesh = new();
            mesh.MarkDynamic();
            mesh.hideFlags = HideFlags.DontSave;
            cacheMeshF.mesh = mesh;
        }
        mesh.Clear();
        mesh.vertices = def.positions;
        mesh.colors = Enumerable.Repeat(Color.white, def.positions.Length).ToArray();
        mesh.uv = def.uvs;
        mesh.triangles = def.indices;
        mesh.bounds = new(def.boundsData[0], def.boundsData[1]);

        
        cacheMeshR.material = def.materialInst;*/

        if(cacheTk2d == null) cacheTk2d = tk2dSprite.AddComponent(cacheMeshF.gameObject, def, id);
        cacheTk2d.SetSprite(def, id);
        //Only Mesh
        var size = (Vector2)cacheMeshR.bounds.size;
        cacheCamera.orthographicSize = size.y / 2;
        cacheCamera.aspect = size.x / size.y;
        cacheCamera.transform.position = cacheMeshR.bounds.center.With((ref Vector3 x) => x.z = -1);
    }
    public static Texture2D Render(tk2dSpriteCollectionData def, int id)
    {
        BuildCamera(def, id);
        var sdef = def.spriteDefinitions[id];
        var width = (int)((sdef.uvs.Max(x => x.x) - sdef.uvs.Min(x => x.x)) * sdef.material.mainTexture.width) + 1;
        var height = (int)((sdef.uvs.Max(x => x.y)  - sdef.uvs.Min(x => x.y)) * sdef.material.mainTexture.height) + 1;
        var rtex = new RenderTexture(width, height, 0);
        rtex.Create();
        cacheCamera.targetTexture = rtex;
        cacheHolder.SetActive(true);
        cacheCamera.Render();
        cacheHolder.SetActive(false);

        var tex2d = new Texture2D(width, height);
        var prev = RenderTexture.active;
        RenderTexture.active = rtex;
        tex2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = prev;

        rtex.Release();
        return tex2d;
    }

    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        target = Render((tk2dSpriteCollectionData)target, 0);
        base.OnBorrowed(target, typeof(Texture2D), inspector);
    }
}

