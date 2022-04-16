
namespace UnityExplorerPlusMod;

class tk2dClipDumpWidget : DumpNonUnityObject<tk2dClipDumpWidget>
{
    public tk2dSpriteAnimationClip clip;
    protected override void OnSave(string savePath)
    {
        if (clip.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("Clip is null");
            return;
        }
        Directory.CreateDirectory(savePath);
        DumpClip(savePath).StartCoroutine();
    }
    private IEnumerator DumpClip(string savePath)
    {
        var info = new SpriteInfo();
        GODump.GODump.Instance.LoadSettings();
        yield return Dump.DumpClip(clip, savePath, info);
        if(GODump.GODump.Settings.DumpSpriteInfo)
        {
            var path = Path.Combine(savePath, "0.Atlases", "SpriteInfo.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(info, Formatting.Indented));
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        clip = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        clip = (tk2dSpriteAnimationClip)target;
        
        string text = clip.name;
        
        if (string.IsNullOrEmpty(text))
        {
            text = "untitled";
        }
        SetDefaultPath("tk2dClip-" + text);
    }
}
