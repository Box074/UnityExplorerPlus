
namespace UnityExplorerPlusMod;

class tk2dClipDumpWidget : Texture2DWidget
{
    public tk2dSpriteAnimationClip clip;
    public ButtonRef playStopButton;
    public Text progressLabel;
    public InputFieldRef savePlus;
    public List<Texture2D> frames = new();
    private Coroutine CurrentlyPlayingCoroutine;
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var ret = base.CreateContent(uiRoot);
        GetFieldRef<InputFieldRef, Texture2DWidget>(this, "savePathInput").Transform.parent.gameObject.SetActive(false);
        GameObject playerRow = UIFactory.CreateHorizontalGroup(GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot"), "PlayerWidget", false, false, true, true,
                spacing: 5, padding: new() { x = 3f, w = 3f, y = 3f, z = 3f });
        playerRow.transform.SetAsFirstSibling();

        playStopButton = UIFactory.CreateButton(playerRow, "PlayerButton", "Play", normalColor: new(0.2f, 0.4f, 0.2f));
        playStopButton.OnClick += OnPlayStopClicked;
        UIFactory.SetLayoutElement(playStopButton.GameObject, minWidth: 60, minHeight: 25);

        progressLabel = UIFactory.CreateLabel(playerRow, "ProgressLabel", "0 / 0");
        UIFactory.SetLayoutElement(progressLabel.gameObject, flexibleWidth: 9999, minHeight: 25);



        var saveRow = UIFactory.CreateHorizontalGroup(GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot"), "SpriteSaveRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
        saveRow.transform.SetSiblingIndex(1);
        UIFactory.SetLayoutElement(saveRow, minHeight: 30, flexibleWidth: 9999);

        Text saveLabel = UIFactory.CreateLabel(saveRow, "SpriteSaveLabel", "Save Clip:");
        UIFactory.SetLayoutElement(saveLabel.gameObject, minWidth: 75, minHeight: 25);

        savePlus = UIFactory.CreateInputField(saveRow, "SpriteSaveInput", "...");
        UIFactory.SetLayoutElement(savePlus.UIRoot, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);

        ButtonRef saveBtn = UIFactory.CreateButton(saveRow, "SaveButton", "Save Folder", new Color(0.2f, 0.25f, 0.2f));
        UIFactory.SetLayoutElement(saveBtn.Component.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 0);
        saveBtn.OnClick += () =>
        {
            var sp = savePlus.Text;
            if (string.IsNullOrEmpty(sp))
            {
                ExplorerCore.LogWarning("Save path cannot be empty!");
                return;
            }
            Directory.CreateDirectory(sp);
            int i = 0;
            foreach (var v in clip.frames)
            {
                var tex = SpriteUtils.ExtractTk2dSprite(v.spriteCollection, v.spriteId);
                File.WriteAllBytes(Path.Combine(sp, clip.name + "-" + i.ToString() + ".png"), tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
                i++;
            }
        };

        return ret;
    }
    static string GetLengthString(float seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds(seconds);

        StringBuilder sb = new();

        if (ts.Hours > 0)
            sb.Append($"{ts.Hours}:");

        sb.Append($"{ts.Minutes:00}:");
        sb.Append($"{ts.Seconds:00}:");
        sb.Append($"{ts.Milliseconds:000}");

        return sb.ToString();
    }
    private void ResetProgressLabel()
    {
        progressLabel.text = $"{GetLengthString(0)} / {GetLengthString((float)clip.frames.Length / clip.fps)}";
    }

    private void OnPlayStopClicked()
    {
        if (CurrentlyPlayingCoroutine != null) StopClip();
        else CurrentlyPlayingCoroutine = RuntimeHelper.StartCoroutine(PlayClipCoroutine());
    }
    private void SetTex(tk2dSpriteCollectionData col, int id)
    {
        var tex = SpriteUtils.ExtractTk2dSprite(col, id);
        SetTex(tex);
    }
    private void SetTex(Texture2D tex)
    {
        this.private_texture() = tex;
        this.SetupTextureViewer();
        SetImageSize(tex);
    }
    private void SetImageSize(Texture2D prevTex)
    {
        RectTransform imageRect = InspectorPanel.Instance.Rect;
        var imageLayout = this.private_imageLayout();

        float rectWidth = imageRect.rect.width - 25;
        float rectHeight = imageRect.rect.height - 196;

        // If our image is smaller than the viewport, just use 100% scaling
        if (prevTex.width < rectWidth && prevTex.height < rectHeight)
        {
            imageLayout.minWidth = prevTex.width;
            imageLayout.minHeight = prevTex.height;
        }
        else // we will need to scale down the image to fit
        {
            // get the ratio of our viewport dimensions to width and height
            float viewWidthRatio = (float)((decimal)rectWidth / (decimal)prevTex.width);
            float viewHeightRatio = (float)((decimal)rectHeight / (decimal)prevTex.height);

            // if width needs to be scaled more than height
            if (viewWidthRatio < viewHeightRatio)
            {
                imageLayout.minWidth = prevTex.width * viewWidthRatio;
                imageLayout.minHeight = prevTex.height * viewWidthRatio;
            }
            else // if height needs to be scaled more than width
            {
                imageLayout.minWidth = prevTex.width * viewHeightRatio;
                imageLayout.minHeight = prevTex.height * viewHeightRatio;
            }
        }
    }
    private IEnumerator PlayClipCoroutine()
    {
        playStopButton.ButtonText.text = "Stop Clip";
        var ws = 1f / clip.fps;
        do
        {
            yield return null;
            var st = Time.unscaledTime;
            Texture2D prevTex = null;
            foreach (var v in clip.frames)
            {
                if(prevTex != null) UnityEngine.Object.Destroy(prevTex);
                prevTex = SpriteUtils.ExtractTk2dSprite(v.spriteCollection, v.spriteId);
                SetTex(prevTex);
                var fst = Time.unscaledTime;
                while (Time.unscaledTime - fst <= ws)
                {
                    progressLabel.text = $"{GetLengthString(Time.unscaledTime - st)} / {GetLengthString((float)clip.frames.Length / clip.fps)}";
                    yield return null;
                }
            }
        } while (clip.wrapMode switch
        {
            tk2dSpriteAnimationClip.WrapMode.Loop => true,
            tk2dSpriteAnimationClip.WrapMode.LoopSection => true,
            tk2dSpriteAnimationClip.WrapMode.RandomLoop => true,
            _ => false
        }
        );

        CurrentlyPlayingCoroutine = null;
        StopClip();
    }
    private void StopClip()
    {
        if (CurrentlyPlayingCoroutine != null)
            RuntimeHelper.StopCoroutine(CurrentlyPlayingCoroutine);


        CurrentlyPlayingCoroutine = null;
        playStopButton.ButtonText.text = "Play Clip";

        ResetProgressLabel();
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        StopClip();
        foreach (var v in frames)
        {
            UnityEngine.Object.Destroy(v);
        }
        frames.Clear();
        clip = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        clip = (tk2dSpriteAnimationClip)target;
        target = SpriteUtils.ExtractTk2dSprite(clip.frames[0].spriteCollection, clip.frames[0].spriteId);
        base.OnBorrowed(target, targetType, inspector);
        savePlus.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value, "tk2dSpriteAnimationClip-" + clip.name);
        ResetProgressLabel();
    }
}
