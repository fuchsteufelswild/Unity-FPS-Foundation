using Nexora.Editor;
using Nexora.FPSDemo.Handhelds.RangedWeapon;
using System.Linq;
using Toolbox.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Nexora.FPSDemo.Editor
{
    [CustomEditor(typeof(Gun))]
    public sealed class GunEditor : ToolboxEditor
    {
        private Gun _gun;

        IGunBehaviourSelector[] behaviourSelectors;
        GunBehaviourPanelDrawer[] panelDrawers;

        private void OnEnable()
        {
            _gun = target as Gun;
        }

        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();


            behaviourSelectors ??= CreateBehaviourSelectors(_gun);
            panelDrawers ??= CreatePanelDrawers(behaviourSelectors);

            foreach(var drawer in panelDrawers)
            {
                ToolboxEditorGui.DrawLine();
                drawer.Draw();
            }
        }

        private static IGunBehaviourSelector[] CreateBehaviourSelectors(Gun gun)
        {
            return new IGunBehaviourSelector[]
            {
                new GunBehaviourSelector<GunAimBehaviour>(gun, GunBehaviourType.AimingSystem),
                new GunBehaviourSelector<GunTriggerBehaviour>(gun, GunBehaviourType.TriggerMechanism),
                new GunBehaviourSelector<GunShellEjectionBehaviour>(gun, GunBehaviourType.EjectionSystem),
                new GunBehaviourSelector<GunMuzzleEffectBehaviour>(gun, GunBehaviourType.MuzzleEffect),
                new GunBehaviourSelector<GunRecoilBehaviour>(gun, GunBehaviourType.RecoilSystem),
                new GunBehaviourSelector<GunMagazineBehaviour>(gun, GunBehaviourType.MagazineSystem),
                new GunBehaviourSelector<MagazineCartridgeVisualizer>(gun, GunBehaviourType.CartridgeVisualizer),
                new GunBehaviourSelector<GunFiringMechanism>(gun, GunBehaviourType.FiringMechanism),
                new GunBehaviourSelector<GunAmmoStorageBehaviour>(gun, GunBehaviourType.AmmunitionSystem),
                new GunBehaviourSelector<GunDryFireEffectBehaviour>(gun, GunBehaviourType.DryFireEffect),
                new GunBehaviourSelector<GunImpactEffectBehaviour>(gun, GunBehaviourType.ImpactSystem),
            };
        }

        private static GunBehaviourPanelDrawer[] CreatePanelDrawers(IGunBehaviourSelector[] gunBehaviourSelectors)
        {
            return gunBehaviourSelectors.Select(behaviourSelector => new  GunBehaviourPanelDrawer(behaviourSelector)).ToArray();
        }
    }
}