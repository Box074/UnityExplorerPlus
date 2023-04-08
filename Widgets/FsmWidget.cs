using UnityExplorerPlus.FSMViewer;
using UnityExplorerPlus.Patch;

namespace UnityExplorerPlus.Widgets;

class FsmWidget : DumpWidgetBase<FsmWidget>
{
    class UnityObjectConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType) => typeof(UnityEngine.Object).IsAssignableFrom(objectType);
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return existingValue;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var uobj = (UnityEngine.Object)value;
            string name = uobj.name;
            string file = "";
            writer.WriteStartObject();
            if(uobj is GameObject go)
            {
                name = go.GetPath();
                UnityExplorerPlus.Instance.LogFine($"{name} (GO)");
                if(go.scene.IsValid())
                {
                    file = go.scene.name;
                }
                else
                {
                    file = PatchGameObjectControls.GetPrefabFile(go);
                }
            }
            else if(uobj is Component c)
            {
                UnityExplorerPlus.Instance.LogFine($"{uobj.name} (C)");
                go = c.gameObject;
                name = go.GetPath() + $" ({c.GetType().FullName})";
                UnityExplorerPlus.Instance.LogFine($"{name} (GO)");
                if(go.scene.IsValid())
                {
                    file = go.scene.name;
                }
                else
                {
                    file = PatchGameObjectControls.GetPrefabFile(go);
                }
            }
            writer.WritePropertyName("objName");
            writer.WriteValue(name);
            writer.WritePropertyName("objFile");
            writer.WriteValue(file);
            writer.WritePropertyName("objId");
            writer.WriteValue(uobj.GetInstanceID());
            writer.WritePropertyName("objType");
            writer.WriteValue(value.GetType().FullName);
            writer.WriteEndObject();
            UnityExplorerPlus.Instance.LogFine($"{name} ({file})");
            //writer.Flush();
        }
    }
    public PlayMakerFSM fsm;
    protected override void OnSave(string savePath)
    {
        if (fsm.IsNullOrDestroyed())
        {
            ExplorerCore.LogWarning("PlayMakerFSM is null, maybe it was destroyed?");
            return;
        }

        File.WriteAllText(savePath, GetFsmJson(fsm));
    }
    private string GetFsmJson(PlayMakerFSM fsm)
    {
        var f = fsm.Fsm;
        foreach (var v in f.States)
        {

            v.Reflect().actionData = v.ActionData.Reflect() ?? new();
            v.SaveActions();
        }

        var token = JToken.Parse(JsonConvert.SerializeObject(f, Formatting.Indented, new JsonSerializerSettings()
        {
            ContractResolver = new UnityContractResolver(),
            Converters = new List<JsonConverter>()
            {
                new UnityObjectConverter()
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
        token["goName"] = fsm.gameObject.name;
        token["goPath"] = fsm.gameObject.GetPath();
        token["fsmId"] = fsm.GetInstanceID();
        return token.ToString(Formatting.Indented);
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        fsm = null;
    }
    public override GameObject CreateContent(GameObject uiRoot)
    {
        var result = base.CreateContent(uiRoot);
        var btn = UIFactory.CreateButton(UIRoot, "OpenInFSMViewer", "Open", new Color?(new Color(0.2f, 0.3f, 0.2f)));
        btn.Transform.SetSiblingIndex(0);
        btn.Component.onClick.AddListener(() =>
        {
            FSMViewerManager.OpenJsonFsm(GetFsmJson(fsm));
        });
        UIFactory.SetLayoutElement(btn.Component.gameObject, 100, 25, null, null, null, null, null);
        return result;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        fsm = (PlayMakerFSM)target;
        SetDefaultPath(fsm.gameObject.name + "-" + fsm.Fsm.Name, "json");
    }
}
