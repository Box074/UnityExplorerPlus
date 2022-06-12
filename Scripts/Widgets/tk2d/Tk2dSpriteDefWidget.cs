
namespace UnityExplorerPlusMod;

class Tk2dSpriteDefWidget : Texture2DWidget
{
    public InputFieldRef spriteIdInput;
    public InputFieldRef savePlus;
    public tk2dSpriteCollectionData collection;
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        collection = (tk2dSpriteCollectionData)target;
        target = SpriteUtils.ExtractTk2dSprite(collection, 0);
        savePlus.Text = Path.Combine(UnityExplorer.Config.ConfigManager.Default_Output_Path.Value, "tk2dSpriteCollectionData-" + collection.name);
        base.OnBorrowed(target, typeof(Texture2D), inspector);
        unityObject = collection;
        instanceIdInput.Text = collection.GetInstanceID().ToString();
    }
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var ret = base.CreateContent(uiRoot);

        var saveRow = UIFactory.CreateHorizontalGroup(GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot"), "SpriteSaveRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
        saveRow.transform.SetSiblingIndex(1);
        UIFactory.SetLayoutElement(saveRow, minHeight: 30, flexibleWidth: 9999);

        Text saveLabel = UIFactory.CreateLabel(saveRow, "SpriteSaveLabel", "Save All:");
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
            for (int i = 0; i < collection.spriteDefinitions.Length; i++)
            {
                var name = collection.spriteDefinitions[i].name;
                var tex = SpriteUtils.ExtractTk2dSprite(collection, i);
                File.WriteAllBytes(Path.Combine(sp, name + ".png"), tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
            }
        };

        
        var idRow = UIFactory.CreateHorizontalGroup(GetFieldRef<GameObject, Texture2DWidget>(this, "textureViewerRoot").transform.GetChild(0).gameObject,
            "SpriteIDRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
        idRow.transform.SetAsFirstSibling();
        UIFactory.SetLayoutElement(idRow, minHeight: 30, flexibleWidth: 9999);

        Text spriteIdLabel = UIFactory.CreateLabel(idRow, "SpriteIDLabel", "SpriteId:");
        UIFactory.SetLayoutElement(spriteIdLabel.gameObject, minWidth: 75, minHeight: 25);

        spriteIdInput = UIFactory.CreateInputField(idRow, "SpriteIDInput", "e.g. 0");
        UIFactory.SetLayoutElement(spriteIdInput.UIRoot, minHeight: 25, minWidth: 75, flexibleWidth: 250);
        spriteIdInput.Component.GetOnEndEdit().AddListener(val =>
        {
            if (!int.TryParse(val, out var id)) return;
            if (id < 0)
            {
                id = 0;
                spriteIdInput.Text = "0";
            }
            if (id >= collection.spriteDefinitions.Length)
            {
                id = collection.spriteDefinitions.Length - 1;
                spriteIdInput.Text = id.ToString();
            }
            var tex = SpriteUtils.ExtractTk2dSprite(collection, id);
            tex.name = collection.spriteDefinitions[id].name;
            GetFieldRef<Texture2D, Texture2DWidget>(this, "texture") = tex;
            FindMethodBase("UnityExplorer.UI.Widgets.Texture2DWidget::SetupTextureViewer").Invoke(this, new object[0]);
            RuntimeHelper.StartCoroutine(
                (IEnumerator)FindMethodBase("UnityExplorer.UI.Widgets.Texture2DWidget::SetImageSizeCoro").Invoke(this, new object[0])
            );
        });

        return ret;
    }
}

