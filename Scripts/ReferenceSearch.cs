
namespace UnityExplorerPlusMod;

static class ReferenceSearch
{
    public static GameObject referenceRow = null!;
    public static InputFieldRef refInputField = null!;
    public static GameObject depthRow = null!;
    public static InputFieldRef depthInputField = null!;
    public static void Init()
    {
        On.UnityExplorer.ObjectExplorer.SearchProvider.UnityObjectSearch += (orig, input, customTypeInput, childFilter, sceneFilter) =>
        {
            var g = orig(input, customTypeInput, childFilter, sceneFilter);
            if (!string.IsNullOrEmpty(refInputField.Text))
            {
                if(!int.TryParse(depthInputField.Text, out var depth))
                {
                    depth = 1;
                }
                if(depth <= 0) depth = 1;
                if (int.TryParse(refInputField.Text, out var instId))
                {
                    var obj = Resources.InstanceIDToObject(instId);
                    if(obj == null) return new();
                    HashSet<object> foundObject = new();

                    bool Check(object o, int d, bool isStatic = false)
                    {
                        if (o is null || foundObject.Contains(o) || d == 0) return false;
                        foundObject.Add(o);
                        var type = isStatic ? (o as Type) ?? o.GetType() : o.GetType();
                        foreach (var v in type.GetFields((isStatic && (object)type == o) ? HReflectionHelper.Static : HReflectionHelper.All))
                        {
                            if((object)type == o && !v.IsStatic) continue;
                            var fv = v.GetValue((object)type == o ? null : o);
                            if(fv == null) continue;
                            if(v.FieldType.IsValueType) continue;
                            if (ReferenceEquals(fv, obj))
                            {
                                return true;
                            }
                            else
                            {
                                if (Check(fv,d - 1)) return true;
                            }
                        }
                        return false;
                    }
                    var result = new List<object>();
                    foreach (var v in g)
                    {
                        if (Check(v, depth, result?.GetType() == typeof(Type))) result.Add(v);
                    }
                    return result;
                }
            }

            return g;
        };
        On.UnityExplorer.ObjectExplorer.ObjectSearch.ConstructUI += (orig, self, root) =>
        {
            orig(self, root);
            var nameInputRow = GetFieldRef<GameObject, ObjectSearch>(self, "nameInputRow");

            referenceRow = UIFactory.CreateHorizontalGroup(nameInputRow.transform.parent.gameObject
                , "ReferenceRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            referenceRow.transform.SetSiblingIndex(nameInputRow.transform.GetSiblingIndex() + 1);

            UIFactory.SetLayoutElement(referenceRow, minHeight: 25, flexibleHeight: 0);

            Text nameLbl = UIFactory.CreateLabel(referenceRow, "RefFilterLabel", "Reference object:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLbl.gameObject, minWidth: 110, flexibleWidth: 0);

            refInputField = UIFactory.CreateInputField(referenceRow, "RefFilterInput", "...");
            UIFactory.SetLayoutElement(refInputField.UIRoot, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);

            depthRow = UIFactory.CreateHorizontalGroup(nameInputRow.transform.parent.gameObject
                , "DepthRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            depthRow.transform.SetSiblingIndex(referenceRow.transform.GetSiblingIndex() + 1);

            UIFactory.SetLayoutElement(depthRow, minHeight: 25, flexibleHeight: 0);

            Text nameLbl2 = UIFactory.CreateLabel(depthRow, "SearchDepthLabel", "Search Depth:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLbl.gameObject, minWidth: 110, flexibleWidth: 0);

            depthInputField = UIFactory.CreateInputField(depthRow, "SearchDepthInput", "e.g. 1");
            UIFactory.SetLayoutElement(depthInputField.UIRoot, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);
        };
    }
}
