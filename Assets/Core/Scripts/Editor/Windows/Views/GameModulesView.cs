using System.Collections.Generic;
using UnityEngine;

namespace Nexora.Editor
{
    public class GameModulesView : AssetRootView<GameModule>
    {
        public GameModulesView(System.Action<IWindowView> selectOnWindow) 
            : base(selectOnWindow)
        {
        }

        public override int SortOrder => 0;

        public override string DisplayName => "Game Modules";

        protected override string ResourcePath => "GameModules/";

        public override bool CanHandleObject(Object targetObject) => false;
    }
}