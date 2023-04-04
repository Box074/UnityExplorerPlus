namespace UnityExplorerPlus;

static class ReferenceSearch
{
    public static GameObject refFilterRow = null!;
    public static GameObject uoRefRow = null!;
    public static InputFieldRef uoRefInputField = null!;
    public static GameObject depthRow = null!;
    public static InputFieldRef depthInputField = null!;
    public static RefFilterMode mode = RefFilterMode.None;
    public static Dropdown refFilterDropdown = null!;
    public static void Init()
    {
        On.UnityExplorer.ObjectExplorer.SearchProvider.UnityObjectSearch += (orig, input, customTypeInput, childFilter, sceneFilter) =>
        {
            var g = orig(input, customTypeInput, childFilter, sceneFilter);
            if (mode == RefFilterMode.None) return g;
            if (!int.TryParse(depthInputField.Text, out var depth))
            {
                depth = 1;
            }
            if (depth <= 0) depth = 1;
            if (mode >= RefFilterMode.FieldOnly)
            {
                if (!string.IsNullOrEmpty(uoRefInputField.Text))
                {
                    if (int.TryParse(uoRefInputField.Text, out var instId))
                    {
                        var obj = Resources.InstanceIDToObject(instId);
                        if (obj == null) return new();
                        HashSet<object> foundObject = new();

                        bool Check(object o, int d)
                        {
                            if (o is null || foundObject.Contains(o) || d == 0) return false;
                            foundObject.Add(o);
                            var type = o.GetType();
                            if (mode < RefFilterMode.PropertyOnly)
                            {
                                foreach (var v in type.GetFields(HReflectionHelper.Instance))
                                {
                                    var fv = v.GetValue(o);
                                    if (fv == null) continue;
                                    if (!v.FieldType.IsValueType)
                                    {
                                        if (fv == (object)obj)
                                        {
                                            return true;
                                        }
                                    }
                                    if (Check(fv, d - 1)) return true;
                                }
                            }
                            if (mode >= RefFilterMode.PropertyAndField)
                            {
                                foreach (var v in type.GetProperties(HReflectionHelper.Instance))
                                {
                                    if (v.PropertyType.IsValueType && v.PropertyType.FullName.StartsWith("System.")) continue;
                                    if (v.PropertyType.IsValueType && v.PropertyType.FullName.StartsWith("UnityEngine.")) continue;
                                    object fv;
                                    try
                                    {
                                        fv = v.GetValue(o);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                    if (fv == null) continue;
                                    if (!v.PropertyType.IsValueType)
                                    {
                                        if (fv == (object)obj)
                                        {
                                            return true;
                                        }
                                    }
                                    if (Check(fv, d - 1)) return true;
                                }
                            }
                            return false;
                        }
                        var result = new List<object>();
                        foreach (var v in g)
                        {
                            if (Check(v, depth)) result.Add(v);
                        }
                        return result;
                    }
                }
            }

            return g;
        };
        On.UnityExplorer.ObjectExplorer.ObjectSearch.ConstructUI += (orig, self, root) =>
        {
            orig(self, root);
            var nameInputRow = self.Reflect().nameInputRow;
            int siblingId = nameInputRow.transform.GetSiblingIndex() + 1;

            refFilterRow = UIFactory.CreateHorizontalGroup(nameInputRow.transform.parent.gameObject, "RefFilterRow", false, true, true, true, 2, new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(refFilterRow, minHeight: 25, flexibleHeight: 0);
            refFilterRow.transform.SetSiblingIndex(siblingId++);

            Text filterLbl = UIFactory.CreateLabel(refFilterRow, "RefLabel", "Reference filter:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(filterLbl.gameObject, minWidth: 110, flexibleWidth: 0);

            GameObject filterDropObj = UIFactory.CreateDropdown(refFilterRow, "RefFilterDropdown", out refFilterDropdown, null, 14, val =>
            {
                mode = (RefFilterMode)val;
                switch (mode)
                {
                    case RefFilterMode.None:
                        uoRefRow.SetActive(false);
                        depthRow.SetActive(false);
                        break;
                    default:
                        uoRefRow.SetActive(true);
                        depthRow.SetActive(true);
                        break;
                }
            });
            foreach (string name in Enum.GetNames(typeof(RefFilterMode)))
                refFilterDropdown.options.Add(new Dropdown.OptionData(name));
            UIFactory.SetLayoutElement(filterDropObj, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);

            #region Unity Object Ref

            uoRefRow = UIFactory.CreateHorizontalGroup(nameInputRow.transform.parent.gameObject
                , "UOReferenceRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            uoRefRow.transform.SetSiblingIndex(siblingId++);
            uoRefRow.SetActive(false);

            UIFactory.SetLayoutElement(uoRefRow, minHeight: 25, flexibleHeight: 0);

            Text nameLbl = UIFactory.CreateLabel(uoRefRow, "UORefFilterLabel", "Reference object:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLbl.gameObject, minWidth: 110, flexibleWidth: 0);

            uoRefInputField = UIFactory.CreateInputField(uoRefRow, "UORefFilterInput", "Unity Object Instance Id");
            UIFactory.SetLayoutElement(uoRefInputField.UIRoot, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);

            #endregion

            depthRow = UIFactory.CreateHorizontalGroup(nameInputRow.transform.parent.gameObject
                , "DepthRow", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            depthRow.transform.SetSiblingIndex(siblingId++);
            depthRow.SetActive(false);

            UIFactory.SetLayoutElement(depthRow, minHeight: 25, flexibleHeight: 0);

            Text nameLbl2 = UIFactory.CreateLabel(depthRow, "SearchDepthLabel", "Search Depth:", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLbl.gameObject, minWidth: 110, flexibleWidth: 0);

            depthInputField = UIFactory.CreateInputField(depthRow, "SearchDepthInput", "e.g. 1");
            UIFactory.SetLayoutElement(depthInputField.UIRoot, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);
        };

        On.UnityExplorer.UI.Widgets.UnityObjectWidget.CreateContent += (orig, self, root) =>
        {
            var result = orig(self, root);
            var findRef = UIFactory.CreateButton(result, "RefButton", "Search Reference", new Color(0.2f, 0.2f, 0.2f));
            UIFactory.SetLayoutElement(findRef.Component.gameObject, minHeight: 25, minWidth: 160);
            findRef.OnClick += () =>
            {
                if (mode == RefFilterMode.None)
                {
                    refFilterDropdown.value = (int)RefFilterMode.PropertyAndField;
                }
                uoRefInputField.Text = self.unityObject.GetInstanceID().ToString();
                UUIManager.GetPanel<ObjectExplorerPanel>(UUIManager.Panels.ObjectExplorer).SetTab(1);
                UUIManager.SetPanelActive(UUIManager.Panels.ObjectExplorer, true);
            };
            return result;
        };
        On.UnityExplorer.UI.Widgets.UnityObjectWidget.OnBorrowed += (orig, self, obj, type, inspector) =>
        {
            orig(self, obj, type, inspector);
            if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                self.UIRoot.FindChildWithPath("RefButton")?.SetActive(false);
            }
            else
            {
                self.UIRoot.FindChildWithPath("RefButton")?.SetActive(true);
            }
        };
    }
    public enum RefFilterMode
    {
        None,
        FieldOnly,
        PropertyAndField,
        PropertyOnly
    }
}
