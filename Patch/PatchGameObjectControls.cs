using System.Runtime.CompilerServices;

namespace UnityExplorerPlus.Patch;

static class PatchGameObjectControls
{
    public class GOCInfo
    {
        public ButtonRef showPrefabBtn;
        public GameObject prefab;
    }

    public static Dictionary<Type, PropertyInfo> enabledProp = new();
    public static ConditionalWeakTable<GameObjectControls, GOCInfo> dict = new();
    public static ConditionalWeakTable<GameObjectControls, InputFieldRef> assetDict = new();
    public static string GetPrefabFile(GameObject go)
    {
        var root = go.transform.root.gameObject;
        var map = UnityExplorerPlus.prefabMap;
        var componentTable = root.GetComponents<Component>().Select(x => x.GetType().Name).ToArray();
        if (map.resources.TryGetValue(root.name, out var res) && res.All(x => componentTable.Contains(x)))
        {
            return "resources.assets";
        }
        if (map.sharedAssets.TryGetValue(root.name, out var sres) && sres.compoents.All(x => componentTable.Contains(x)))
        {
            return sres.assetFile + ".assets";
        }
        return "Not recorded";
    }
    public static void Init()
    {
        On.UnityExplorer.UI.Widgets.GameObjectControls.UpdateGameObjectInfo += GameObjectControls_UpdateGameObjectInfo;
        On.UnityExplorer.UI.Widgets.ComponentList.SetComponentCell += ComponentList_SetComponentCell;
        On.UnityExplorer.UI.Widgets.ComponentList.OnBehaviourToggled += ComponentList_OnBehaviourToggled;
    }

    private static void ComponentList_OnBehaviourToggled(On.UnityExplorer.UI.Widgets.ComponentList.orig_OnBehaviourToggled orig, ComponentList self, bool value, int index)
    {
        orig(self, value, index);
        var data = self.Parent.Reflect().GetComponentEntries()[index];
        var type = data.GetType();

        if(enabledProp.TryGetValue(type, out var p))
        {
            p.SetValue(data, value);
        }

    }

    private static void ComponentList_SetComponentCell(On.UnityExplorer.UI.Widgets.ComponentList.orig_SetComponentCell orig, ComponentList self, ComponentCell cell, int index)
    {
        orig(self, cell, index);

        var data = self.Parent.Reflect().GetComponentEntries()[index];
        var type = data.GetType();
        if (data is PlayMakerFSM pm)
        {
            if (string.IsNullOrEmpty(pm.FsmName)) return;
            cell.Button.ButtonText.text = cell.Button.ButtonText.text
                + "<color=grey>(</color><color=green>"
                + pm.FsmName
                + "</color><color=grey>)</color>";
        }
        if(data is not Behaviour)
        {
            if(!enabledProp.TryGetValue(type, out var enabled))
            {
                enabled = type.GetProperties().FirstOrDefault(x => x.Name.Equals("enabled",
                    StringComparison.OrdinalIgnoreCase) && x.PropertyType == typeof(bool)
                    && x.CanWrite && x.CanRead);
                enabledProp[type] = enabled;
            }
            if(enabled is not null)
            {
                var e = (bool)enabled.GetValue(data);
                cell.BehaviourToggle.interactable = true;
                cell.BehaviourToggle.SetIsOnWithoutNotify(e);
                cell.BehaviourToggle.graphic.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            }
        }
    }

    private static void GameObjectControls_UpdateGameObjectInfo(On.UnityExplorer.UI.Widgets.GameObjectControls.orig_UpdateGameObjectInfo orig, GameObjectControls self, bool firstUpdate, bool force)
    {
        orig(self, firstUpdate, force);
        var go = self.Parent.Target;

        var showExplorerBtn = self.Parent.Content.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(x => x.gameObject.name == "ExploreBtn").gameObject;
        var destroyBtn = self.Parent.Content.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(x => x.gameObject.name == "DestroyBtn").gameObject;
        var cp = showExplorerBtn.transform.parent;
        var sceneLabel = cp.Find("SceneLabel").gameObject.GetComponent<Text>();
        var sceneBtn = self.GameObjectInfo.Reflect().SceneButton;

        if (!assetDict.TryGetValue(self, out var assetText))
        {
            var text = UIFactory.CreateInputField(sceneBtn.GameObject.transform.parent.gameObject, "AssetsFileText", "Not recorded");
            UIFactory.SetLayoutElement(text.Component.gameObject, minHeight: 25, minWidth: 120, flexibleWidth: 999);
            text.Component.readOnly = true;
            text.Component.textComponent.color = new Color(0.7f, 0.7f, 0.7f);
            text.Transform.SetSiblingIndex(sceneBtn.Transform.GetSiblingIndex());
            assetText = text;
            assetDict.Add(self, assetText);
        }


        destroyBtn.SetActive(true);
        showExplorerBtn.SetActive(true);
        sceneBtn.GameObject.SetActive(true);
        assetText.GameObject.SetActive(false);
        sceneLabel.text = "Scene:";
        if (!dict.TryGetValue(self, out GOCInfo info)) info = null;
        else info.showPrefabBtn.Component.gameObject.SetActive(false);
        if (!go.scene.IsValid())
        {
            var root = go.transform.root.gameObject;
            destroyBtn.SetActive(false);
            showExplorerBtn.SetActive(false);
            assetText.GameObject.SetActive(true);
            sceneLabel.text = "Assets:";
            sceneBtn.GameObject.SetActive(false);
            assetText.Text = GetPrefabFile(root);
        }
        else
        {
            //var prefabName = go.name.Replace("(Clone)", "");
            var t0 = go.name.IndexOf('(');

            var prefabName = t0 == -1 ? go.name : go.name.Substring(0, t0).Trim();
            var cs = go.GetComponents<Component>()
                    .Select(x => x.GetType().Name)
                    .ToArray();
            var pb = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(
                        x => x.name == prefabName && !x.scene.IsValid() && x.transform.parent == null
                    )
                    .FirstOrDefault(
                        x => x.GetComponents<Component>().All(x => cs.Contains(x.GetType().Name))
                    );
            if (pb is null)
            {
                if (UnityExplorerPlus.prefabMap.resources.TryGetValue(prefabName, out var res)
                    && res.All(x => cs.Contains(x)))
                {
                    pb = Resources.LoadAll<GameObject>("").FirstOrDefault(x => x.name == prefabName);
                }
            }
            if (pb is not null)
            {
                if (info is null)
                {
                    info = new();
                    var parent = cp.gameObject;
                    info.showPrefabBtn = UIFactory.CreateButton(parent, "PrefabBtn", "Prefab", new Color(0.15f, 0.15f, 0.15f));
                    UIFactory.SetLayoutElement(info.showPrefabBtn.Component.gameObject,
                        75, 25, null, null, null, null, null);
                    info.showPrefabBtn.ButtonText.fontSize = 12;
                    info.showPrefabBtn.Component.onClick.AddListener(() =>
                    {
                        if (info.prefab is not null)
                        {
                            InspectorManager.Inspect(info.prefab);
                        }
                    });
                    info.showPrefabBtn.Component.transform.SetSiblingIndex(0);
                    info.showPrefabBtn.Component.gameObject.SetActive(false);
                    dict.Add(self, info);
                }


                info.prefab = pb;
                info.showPrefabBtn.Component.gameObject.SetActive(true);
                //showExplorerBtn.SetActive(false);

                return;
            }
        }
    }
}
