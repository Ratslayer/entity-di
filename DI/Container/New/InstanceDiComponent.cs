using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace BB.Di
{
    public sealed class InstanceDiComponent : BaseDiComponent
    {
        readonly object _instance;
        public InstanceDiComponent(in DiComponentContext context, object instance) : base(context)
        {
            _instance = instance;
        }

        public override object Create(in DiComponentCreateContext context)
            => _instance;

        public override bool Validate(IEntityInstaller installer)
        {
            if (_instance is null)
            {
                LogError(installer, "attempted to bind null instance");
                return false;
            }
            return true;
        }
    }
}