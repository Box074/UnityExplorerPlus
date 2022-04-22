
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
            FindMethodBase("UnityExplorer.Inspectors.GameObjectControls::UpdateGameObjectInfo"),//typeof(GameObjectControls).GetMethod("UpdateGameObjectInfo"),
            (Action<GameObjectControls, bool, bool> orig, GameObjectControls self, bool firstUpdate, bool force) =>
            {
                orig(self, firstUpdate, force);
                var go = self.Parent.GOTarget;
                
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
                    destroyBtn.SetActive(false);
                    showExplorerBtn.SetActive(false);
                    sceneLabel.text = "Assets:";
                    var text = Modding.ReflectionHelper.GetField<GameObjectControls, InputFieldRef>(self, "SceneInput");
                    if(UnityExplorerPlus.prefabMap.TryGetValue(go.transform.root.gameObject.name, out var n))
                    {
                        text.Text = n + ".assets (Prefab)";
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
                            .Select(x => x.GetType().AssemblyQualifiedName)
                            .ToArray();
                    var pb = Resources.FindObjectsOfTypeAll<GameObject>()
                            .Where(
                                x => x.name == prefabName && !x.scene.IsValid() && x.transform.parent == null
                            )
                            .FirstOrDefault(
                                x => x.GetComponents<Component>().All(x => cs.Contains(x.GetType().AssemblyQualifiedName))
                            );
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
