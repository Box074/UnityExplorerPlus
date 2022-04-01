
namespace UnityExplorerPlusMod;

class SpriteWidget : Texture2DWidget
{
    public Texture2D viewTex;
    private ReflectionObject texWidget;
    //public static MethodInfo baseTypeOnBorrowed = typeof(UnityObjectWidget).GetMethod("OnBorrowed", HReflectionHelper.All);
    public SpriteWidget()
    {
        texWidget = this.CreateReflectionObject();
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        var sr = (Sprite)target;
        viewTex = SpriteUtils.ExtractTextureFromSprite(sr, false);
        viewTex.name = sr.name;
        base.OnBorrowed(viewTex, targetType, inspector);

        UnityObjectRef = (UnityEngine.Object)target;
        nameInput.Text = UnityObjectRef.name;
        instanceIdInput.Text = UnityObjectRef.GetInstanceID().ToString();
        gameObjectButton.Component.gameObject.SetActive(false);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        UnityEngine.Object.Destroy(viewTex);
    }
}
