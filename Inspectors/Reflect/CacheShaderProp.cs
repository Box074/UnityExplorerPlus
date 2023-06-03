using UnityEngine.Rendering;

namespace UnityExplorerPlus.Inspectors.Reflect
{
    internal class CacheShaderProp : CacheProperty
    {
        public readonly static Dictionary<ShaderPropertyType, MethodInfo> shaderPropGetter = 
            (new (ShaderPropertyType, string)[]
            {
                (ShaderPropertyType.Texture, nameof(Material.GetTexture)),
                (ShaderPropertyType.Range, nameof(Material.GetInt)),
                (ShaderPropertyType.Float, nameof(Material.GetFloat)),
                (ShaderPropertyType.Color, nameof(Material.GetColor)),
                (ShaderPropertyType.Vector, nameof(Material.GetVector))
            }).ToDictionary(val => val.Item1, val => typeof(Material).GetMethod(val.Item2, new Type[]
            {
                typeof(int)
            }));

        public readonly static Dictionary<ShaderPropertyType, MethodInfo> shaderPropSetter =
            (new (ShaderPropertyType, string)[]
            {
                (ShaderPropertyType.Texture, nameof(Material.SetTexture)),
                (ShaderPropertyType.Range, nameof(Material.SetInt)),
                (ShaderPropertyType.Float, nameof(Material.SetFloat)),
                (ShaderPropertyType.Color, nameof(Material.SetColor)),
                (ShaderPropertyType.Vector, nameof(Material.SetVector))
            }).ToDictionary(val => val.Item1, val => typeof(Material).GetMethods()
                .First(x => x.Name == val.Item2 && x.GetParameters()[0].ParameterType == typeof(int)));
        public readonly static Dictionary<ShaderPropertyType, Type> shaderPropType = new()
        {
            [ShaderPropertyType.Range] = typeof(int),
            [ShaderPropertyType.Float] = typeof(float),
            [ShaderPropertyType.Color] = typeof(Color),
            [ShaderPropertyType.Vector] = typeof(Vector4),
            [ShaderPropertyType.Texture] = typeof(Texture)
        };
        public override Type DeclaringType => typeof(Material);

        public override bool IsStatic => false;

        public override bool ShouldAutoEvaluate => true;

        public override bool CanWrite => true;

        private string m_Name = "";
        private int m_Index = -1;

        public CacheShaderProp() : base(null)
        {
        }

        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            Owner = inspector;
            StringBuilder sb = new();
            sb
                .Append("<color=")
                .Append(SignatureHighlighter.PROP_INSTANCE)
                .Append('>')
                .Append("[Shader Prop]")
                .Append("</color>")
                .Append("<color=")
                .Append(SignatureHighlighter.FIELD_INSTANCE)
                .Append('>')
                .Append(m_Name)
                .Append("</color>")
            ;
            NameLabelText = sb.ToString();
            NameForFiltering = m_Name;
            NameLabelTextRaw = NameForFiltering;
        }

        public void BindShaderProp(string name)
        {
            m_Name = name;
            m_Index = Shader.PropertyToID(name);

            FallbackType = typeof(object);
        }

        protected override object TryEvaluate()
        {
            var mat = (Material)DeclaringInstance;
            var shader = mat.shader;
            if (shader == null) return null;
            var pid = shader.FindPropertyIndex(m_Name);
            var type = shader.GetPropertyType(pid);
            FallbackType = shaderPropType[type];
            var result = shaderPropGetter[type].Invoke(mat, new object[]
            {
                m_Index
            });
            if(result == null && type == ShaderPropertyType.Texture)
            {
                result = shader.GetPropertyTextureDefaultName(pid) switch
                {
                    "white" => Texture2D.whiteTexture,
                    "red" => Texture2D.redTexture,
                    "black" => Texture2D.blackTexture,
                    "gray" => Texture2D.grayTexture,
                    _ => null
                };
            }
            return result;
        }

        protected override void TrySetValue(object value)
        {
            var mat = (Material)DeclaringInstance;
            var shader = mat.shader;
            if (shader == null) return;
            shaderPropSetter[shader.GetPropertyType(shader.FindPropertyIndex(m_Name))].Invoke(mat, new object[]
            {
                m_Index,
                value
            });
        }
    }
}
