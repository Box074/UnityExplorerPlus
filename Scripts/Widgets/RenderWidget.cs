
namespace UnityExplorerPlusMod;

class RendererWidget : Texture2DWidget
{
    public Renderer renderer;
    private Coroutine refreshCor;
    private int width = 2048;
    private int height = 1024;
    private Texture2D prevTex = null;
    private bool autoRefresh = false;
    private bool includeChildren = false;
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        renderer = (Renderer)target;
        var t = (UnityEngine.Object)target;
        ResetSize();
        target = renderer.gameObject.Render(width, height);
        base.OnBorrowed(target, typeof(Texture2D), inspector);
        ((UnityObjectWidget)this).direct_OnBorrowed(target, targetType, inspector);
        unityObject = t;
        instanceIdInput.Text = t.GetInstanceID().ToString();

        refreshCor = RuntimeHelper.StartCoroutine(Refresh());
    }
    private void ResetSize()
    {
        width = (int)(renderer.bounds.size.x * 100);
        height = (int)(renderer.bounds.size.y * 100);
    }
    private void DoRefresh()
    {
        ResetSize();
        if (prevTex != null) UnityEngine.Object.Destroy(prevTex);
        prevTex = renderer.gameObject.Render(width, height, includeChildren);
        GetFieldRef<Texture2D, Texture2DWidget>(this, "texture") = prevTex;
        this.SetupTextureViewer();
        //FindMethodBase("UnityExplorer.UI.Widgets.Texture2DWidget::SetupTextureViewer").Invoke(this, new object[0]);
        SetImageSize();
    }
    private IEnumerator Refresh()
    {
        yield return null;
        var root = GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot");
        while (true)
        {
            while (!root.activeInHierarchy || !autoRefresh) yield return null;
            DoRefresh();
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    private void SetImageSize()
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
    public override void OnReturnToPool()
    {
        if (refreshCor != null)
        {
            RuntimeHelper.StopCoroutine(refreshCor);
            refreshCor = null;
        }
        if (prevTex != null)
        {
            UnityEngine.Object.Destroy(prevTex);
            prevTex = null;
        }
        base.OnReturnToPool();
    }
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var ret = base.CreateContent(uiRoot);
        var saveRow = GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot").transform.GetChild(0).gameObject;
        var btn = UIFactory.CreateButton(saveRow, "RefreshBtn", "Refresh", new Color(0.2f, 0.25f, 0.2f));
        UIFactory.SetLayoutElement(btn.GameObject, minHeight: 25, minWidth: 100, flexibleWidth: 99999);
        btn.Transform.SetAsLastSibling();
        btn.OnClick += () => DoRefresh();

        var toggle = UIFactory.CreateToggle(saveRow, "AutoRefresh", out var o_toggle, out var text);
        UIFactory.SetLayoutElement(toggle, minHeight: 25, minWidth: 150, flexibleWidth: 99999);
        toggle.transform.SetAsLastSibling();
        text.text = "Auto Refresh";
        o_toggle.onValueChanged.AddListener(val =>
        {
            autoRefresh = val;
        });
        o_toggle.isOn = false;

        var toggle0 = UIFactory.CreateToggle(saveRow, "IncludeChildren", out var o_toggle0, out var text0);
        UIFactory.SetLayoutElement(toggle0, minHeight: 25, minWidth: 150, flexibleWidth: 99999);
        toggle0.transform.SetAsLastSibling();
        text0.text = "Include Children";
        o_toggle0.onValueChanged.AddListener(val =>
        {
            includeChildren = val;
        });
        o_toggle0.isOn = false;

        return ret;
    }
}

