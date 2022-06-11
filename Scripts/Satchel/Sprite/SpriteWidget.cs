
namespace UnityExplorerPlusMod;

class SpriteWidget : Texture2DWidget
{
    public Texture2D viewTex;
    private ReflectionObject texWidget;
    private Sprite sprite;
    private SpriteAtlas atlas;
    private ButtonRef atlasButton;

    public SpriteWidget()
    {
        texWidget = this.CreateReflectionObject();
    }
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var r = base.CreateContent(uiRoot);
        atlasButton = UIFactory.CreateButton(UIRoot, "AtlasButton", "Inspect SpriteAtlas", new Color(0.2f, 0.2f, 0.2f));
        var but = atlasButton;
        UIFactory.SetLayoutElement(but.Component.gameObject, 160, 25, null, null, null, null, null);
        but.Component.transform.SetSiblingIndex(3);
        but.Component.onClick.AddListener(() =>
        {
            if(atlas is not null)
            {
                InspectorManager.Inspect(atlas);
                return;
            }
            ExplorerCore.LogWarning("SpriteAtlas reference is null or destroyed!");
        });
        gameObjectButton.Component.gameObject.SetActive(false);
        return r;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        var sr = (Sprite)target;
        sprite = sr;
        atlasButton.Component.gameObject.SetActive(false);
        if(sr.packed)
        {
            atlas = Resources.FindObjectsOfTypeAll<SpriteAtlas>()
                .FirstOrDefault(x => x.CanBindTo(sr));
            if(atlas is not null)
            {
                atlasButton.Component.gameObject.SetActive(true);
            }
        }
        viewTex = SpriteUtils.ExtractTextureFromSprite(sr, false);
        viewTex.name = sr.name;
        base.OnBorrowed(viewTex, targetType, inspector);

        unityObject = (UnityEngine.Object)target;
        nameInput.Text = unityObject.name;
        instanceIdInput.Text = unityObject.GetInstanceID().ToString();
        gameObjectButton.Component.gameObject.SetActive(false);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        UnityEngine.Object.Destroy(viewTex);
    }
}
