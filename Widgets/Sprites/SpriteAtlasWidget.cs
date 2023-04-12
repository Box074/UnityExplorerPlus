namespace UnityExplorerPlus.Widgets.Sprites;

class SpriteAtlasWidget : DumpWidgetBase<SpriteAtlasWidget>
{
    public SpriteAtlas atlas;
    protected override void OnSave(string savePath)
    {
        if (atlas.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("SpriteAtlas is null, maybe it was destroyed?");
            return;
        }

        Directory.CreateDirectory(savePath);
        var sr = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sr);
        foreach (var v in sr)
        {
            var p = Path.Combine(savePath, v.name.Replace("(Clone)", "") + ".png");
            var tex = SpriteUtils.ExtractSprite(v, true);
            File.WriteAllBytes(p, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(v);
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        atlas = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        atlas = (SpriteAtlas)target;
        var text = atlas.name;
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }
        SetDefaultPath("Atlas-" + text);
    }
}
