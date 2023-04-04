namespace UnityExplorerPlus.Widgets;

class RendererWidget : Texture2DWidget
{
    public Renderer renderer;
    private Coroutine refreshCor;
    private int width = 2048;
    private int height = 1024;
    private Texture2D prevTex = null;
    private bool autoRefresh = false;
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        renderer = (Renderer)target;
        var t = (UnityEngine.Object)target;
        ResetSize();
        target = renderer.gameObject.Render(width, height)!;
        base.OnBorrowed(target, typeof(Texture2D), inspector);
        unityObject = t;

        var resb = ((UnityObjectWidget)this).Reflect();
        resb.nameInput.Text = unityObject.name;
        resb.instanceIdInput.Text = unityObject.GetInstanceID().ToString();
        component = renderer;
        resb.gameObjectButton.Component.gameObject.SetActive(true);
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
        prevTex = null!;
        if (width == 0 || height == 0 || width > 10000 || height > 10000) return;
        prevTex = renderer.gameObject.Render(width, height, false)!;
        this.Reflect().texture = prevTex;
        this.Reflect().SetupTextureViewer();
        //FindMethodBase("UnityExplorer.UI.Widgets.Texture2DWidget::SetupTextureViewer").Invoke(this, new object[0]);
        SetImageSize();
    }
    private IEnumerator Refresh()
    {
        yield return null;
        var root = this.Reflect().textureViewerRoot;
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
        var imageLayout = this.Reflect().imageLayout;

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
            float viewWidthRatio = (float)((decimal)rectWidth / prevTex.width);
            float viewHeightRatio = (float)((decimal)rectHeight / prevTex.height);

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
        var saveRow = this.Reflect().textureViewerRoot.transform.GetChild(0).gameObject;
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

        return ret;
    }
}

