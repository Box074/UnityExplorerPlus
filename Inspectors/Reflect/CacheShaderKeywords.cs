namespace UnityExplorerPlus.Inspectors.Reflect
{
    internal class CacheShaderKeywords : CacheProperty
    {
        public CacheShaderKeywords() : base(null)
        {

        }
        public override bool HasArguments => false;
        public override bool CanWrite => true;
        public override bool IsStatic => false;
        public override bool ShouldAutoEvaluate => true;
        private string m_Name;
        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            Owner = inspector;

            StringBuilder sb = new();
            sb
                .Append("<color=")
                .Append(SignatureHighlighter.PROP_INSTANCE)
                .Append('>')
                .Append("[Shader Keywords]")
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

        public void BindShaderKeyword(string name)
        {
            m_Name = name;

            FallbackType = typeof(bool);
        }

        protected override object TryEvaluate()
        {
            var mat = (Material)DeclaringInstance;
            var shader = mat.shader;
            if (shader == null) return null;
            return mat.IsKeywordEnabled(m_Name);
        }
        protected override void TrySetValue(object value)
        {
            var mat = (Material)DeclaringInstance;
            var shader = mat.shader;
            var val = (bool)value;
            if (shader == null) return;
            if(val)
            {
                mat.EnableKeyword(m_Name);
            }
            else
            {
                mat.DisableKeyword(m_Name);
            }
        }
    }
}
