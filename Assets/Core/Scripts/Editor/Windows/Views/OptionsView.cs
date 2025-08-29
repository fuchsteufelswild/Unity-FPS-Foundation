using Nexora.Options;
using UnityEngine;

namespace Nexora.Editor
{
    public class OptionsView : AssetRootView<Options.Options>
    {
        public OptionsView(System.Action<IWindowView> selectOnWindow) 
            : base(selectOnWindow)
        {
        }

        public override int SortOrder => 1;

        public override string DisplayName => "Options";

        protected override string ResourcePath => "Options/";

        public override bool CanHandleObject(Object targetObject) => false;
    }
}