
namespace UnityExplorerPlusMod;

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
            UnityExplorerPlus.Instance.LogFine($"{uobj.name} (OBJ)");
            string name = uobj.name;
            string file = "";
            writer.WriteStartObject();
            UnityExplorerPlus.Instance.LogFine($"{uobj.name} (WSO)");
            /*if(uobj is GameObject go)
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
            }*/
            writer.WritePropertyName("objName");
            writer.WriteValue(name);
            writer.WritePropertyName("objFile");
            writer.WriteValue(file);
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

        var f = fsm.Fsm;
        foreach (var v in f.States)
        {
            v.private_actionData() = v.ActionData ?? new();
            v.SaveActions();
        }
        
        File.WriteAllText(savePath, JsonConvert.SerializeObject(f, Formatting.Indented, new JsonSerializerSettings()
        {
            ContractResolver = new UnityContractResolver(),
            Converters = new List<JsonConverter>()
            {
                new UnityObjectConverter()
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        fsm = null;
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        base.OnBorrowed(target, targetType, inspector);

        fsm = (PlayMakerFSM)target;
        SetDefaultPath(fsm.gameObject.name + "-" + fsm.Fsm.Name, "json");
    }
}
