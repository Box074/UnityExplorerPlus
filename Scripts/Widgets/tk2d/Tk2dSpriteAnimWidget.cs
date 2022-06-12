
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
        foreach(var v in animation.clips)
        {
            var p = Path.Combine(savePath, v.name);
            Directory.CreateDirectory(p);
            int i = 0;
            foreach(var f in v.frames)
            {
                var tex = SpriteUtils.ExtractTk2dSprite(f.spriteCollection, f.spriteId);
                File.WriteAllBytes(Path.Combine(p, v.name + "-" + i.ToString() + ".png"), tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
                i++;
            }
        }
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
