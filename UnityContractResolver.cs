namespace UnityExplorerPlus;

public class UnityContractResolver : DefaultContractResolver
{
    private IEnumerable<FieldInfo> GetAllInstanceFields(Type type)
    {
        IEnumerable<FieldInfo> t = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        while ((type = type.BaseType) != null)
        {
            t = t.Union(type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }
        return t;
    }
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = GetAllInstanceFields(type)
                    .Where(x => x is not null)
                    .Where(x => !x.IsDefined(typeof(NonSerializedAttribute)))
                    .Where(x => x.IsPublic || x.IsDefined(typeof(SerializeField)) || x.Name.StartsWith("m_"))
                    .Where(x => x.FieldType.IsDefined(typeof(SerializableAttribute)) ||
                        x.FieldType.IsValueType || x.FieldType.IsArray || typeof(UnityEngine.Object).IsAssignableFrom(x.FieldType))
                    .Select(f => CreateProperty(f, memberSerialization))
                    .ToList();
        props.ForEach(p => { p.Writable = true; p.Readable = true; UnityExplorerPlus.Instance.LogFine($"{type.FullName}.{p.PropertyName}"); });
        return props;
    }
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);
        var nativeName = member.GetCustomAttribute(HReflectionHelper.FindType("UnityEngine.Bindings.NativeNameAttribute"))?.FastGet("<Name>k__BackingField") as string;
        if (!string.IsNullOrEmpty(nativeName))
        {
            UnityExplorerPlus.Instance.LogFine($"Change name: {prop.PropertyName} -> {nativeName}");
            prop.PropertyName = nativeName;
        }

        return prop;
    }
}
