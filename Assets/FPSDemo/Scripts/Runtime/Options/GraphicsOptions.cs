using Nexora.Options;
using UnityEngine;

namespace Nexora.FPSDemo.Options
{
    [CreateAssetMenu(menuName = CreateAssetMenuPath + "Graphics Options", fileName = nameof(GraphicsOptions))]
    public sealed class GraphicsOptions : Options<GraphicsOptions>
    {
        [SerializeField]
        private Option<float> _fieldOfView = new(60f);

        public Option<float> FieldOfView => _fieldOfView;
    }
}