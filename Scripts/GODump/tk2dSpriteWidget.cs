
namespace UnityExplorerPlusMod;

class tk2dSpriteWidget : DumpWidgetBase<tk2dSpriteWidget>
{
    public tk2dSpriteAnimation animation;
    protected override void OnSave(string savePath)
    {
        if (animation.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("Animation is null, maybe it was destroyed?");
            return;
        }
        Directory.CreateDirectory(savePath);
        GODump.GODump.Instance.LoadSettings();
        Dump.DumpSpriteInUExplorer(animation, savePath).StartCoroutine();
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        animation = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        string text;
        if (target is tk2dSpriteAnimator anim)
        {
            animation = anim.Library;
            if (animation is null) return;
            text = anim.name;
        }
        else
        {
            animation = (tk2dSpriteAnimation)target;
            text = animation.name;
        }
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }
        SetDefaultPath("tk2d-" + text);
    }
}
