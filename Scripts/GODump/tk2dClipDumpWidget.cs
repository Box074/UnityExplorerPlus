
namespace UnityExplorerPlusMod;

class tk2dClipDumpWidget : DumpWidgetBase<tk2dSpriteWidget>
{
    public static UnityEngine.Object fakeUObject = new FakeUObject();
    static tk2dClipDumpWidget() 
    {
        UnityEngine.Object.DontDestroyOnLoad(fakeUObject);
    }
    public tk2dSpriteAnimationClip clip;
    protected override void OnSave(string savePath)
    {
        if (clip.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("Clip is null");
            return;
        }
        Directory.CreateDirectory(savePath);
        Dump.DumpClip(clip, savePath).StartCoroutine();
    }

    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);
        UnityEngine.Object.Destroy(UIRoot.FindChild("InstanceLabel"));
        instanceIdInput.Component.gameObject.SetActive(false);
        return result;
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        clip = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(fakeUObject, targetType, inspector);

        instanceIdInput.Component.gameObject.SetActive(false);

        clip = (tk2dSpriteAnimationClip)target;
        
        string text = clip.name;
        
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }
        nameInput.Text = text;
        SetDefaultPath("tk2dClip-" + text);
    }
}
