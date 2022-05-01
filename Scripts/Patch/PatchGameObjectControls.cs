
namespace UnityExplorerPlusMod;

static class PatchGameObjectControls 
{
    public class GOCInfo
    {
        public ButtonRef showPrefabBtn;
        public GameObject prefab;
    }
    public static Dictionary<GameObjectControls, GOCInfo> dict = new();
    public static void Init()
    {
        HookEndpointManager.Add(
            FindMethodBase("UnityExplorer.UI.Widgets.GameObjectControls::UpdateGameObjectInfo"),//typeof(GameObjectControls).GetMethod("UpdateGameObjectInfo"),
            (Action<GameObjectControls, bool, bool> orig, GameObjectControls self, bool firstUpdate, bool force) =>
            {
                orig(self, firstUpdate, force);
                var go = (GameObject)self.Parent.Target;
                
                var showExplorerBtn = self.Parent.Content.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(x => x.gameObject.name == "ExploreBtn").gameObject;
                var destroyBtn = self.Parent.Content.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(x => x.gameObject.name == "DestroyBtn").gameObject;
                var cp = showExplorerBtn.transform.parent;
                var sceneLabel = cp.Find("SceneLabel").gameObject.GetComponent<Text>();
                
                destroyBtn.SetActive(true);
                showExplorerBtn.SetActive(true);
                sceneLabel.text = "Scene:";
                GOCInfo info;
                if(!dict.TryGetValue(self, out info)) info = null;
                else info.showPrefabBtn.Component.gameObject.SetActive(false);
                if(!go.scene.IsValid())
                {
                    var root = go.transform.root.gameObject;
                    destroyBtn.SetActive(false);
                    showExplorerBtn.SetActive(false);
                    sceneLabel.text = "Assets:";
                    var text = (InputFieldRef)FindFieldInfo("UnityExplorer.UI.Widgets.GameObjectInfoPanel::SceneInput").FastGet(self.GameObjectInfo);
                    var map = UnityExplorerPlus.prefabMap;
                    var componentTable = root.GetComponents<Component>().Select(x => x.GetType().Name).ToArray();

                    if(map.resources.TryGetValue(root.name, out var res) && res.All(x => componentTable.Contains(x)))
                    {
                        text.Text = "resources.assets (Prefab)";
                        return;
                    }
                    if(map.sharedAssets.TryGetValue(root.name, out var sres) && sres.compoents.All(x => componentTable.Contains(x)))
                    {
                        text.Text = sres.assetFile + ".assets (Prefab)";
                        return;
                    }
                    text.Text = "Not recorded";
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
                    if(pb is null)
                    {
                        if(UnityExplorerPlus.prefabMap.resources.TryGetValue(prefabName, out var res)
                            && res.All(x => cs.Contains(x)))
                        {
                            pb = Resources.LoadAll<GameObject>("").FirstOrDefault(x => x.name == prefabName);
                        }
                    }
                    if(pb is not null)
                    {
                        if(info is null)
                        {
                            info = new();
                            var parent = cp.gameObject;
                            info.showPrefabBtn = UIFactory.CreateButton(parent, "PrefabBtn", "Prefab", new Color(0.15f, 0.15f, 0.15f));
                            UIFactory.SetLayoutElement(info.showPrefabBtn.Component.gameObject,
                                75, 25, null, null, null, null, null);
                            info.showPrefabBtn.ButtonText.fontSize = 12;
                            info.showPrefabBtn.Component.onClick.AddListener(() =>
                            {
                                if(info.prefab is not null)
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
            });
    }
}
