using Nexora.FPSDemo.Handhelds.RangedWeapon;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nexora.FPSDemo.Editor
{
    public interface IGunBehaviourSelector
    {
        int SelectedIndex { get; set; }
    }

    public sealed class GunBehaviourSelector<T> :
        IGunBehaviourSelector
        where T : GunBehaviour
    {
        public string HeaderName { get; }
        public string[] BehaviourNames { get; }
        public T[] AvailableBehaviours { get; }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if(_selectedIndex == value || value < 0 || value >= AvailableBehaviours.Length)
                {
                    return;
                }

                _selectedIndex = value;

                for (int i = 0; i < AvailableBehaviours.Length; i++)
                {
                    var behaviour = AvailableBehaviours[i];
                    if (behaviour.IsAttached && i != value)
                    {
                        DetachBehaviour(behaviour);
                    }
                }

                AttachBehaviour(AvailableBehaviours[_selectedIndex]);
            }
        }

        public GunBehaviourSelector(IGun gun, GunBehaviourType behaviourType)
        {
            AvailableBehaviours = gun.gameObject.GetComponentsInChildren<T>(true);
            HeaderName = behaviourType.ToString().AddSpacesToCamelCase();

            BehaviourNames = AvailableBehaviours.Select(
                behaviour => $" {behaviour.gameObject.name} ({behaviour.GetType().Name.Replace("Gun", "")})").ToArray();

            _selectedIndex = GetSelectedIndex();
        }

        private int GetSelectedIndex()
        {
            if(AvailableBehaviours.IsEmpty())
            {
                return -1;
            }

            int selectedIndex = -1;

            for(int i = 0; i< AvailableBehaviours.Length; i++)
            {
                var behaviour = AvailableBehaviours[i];
                if(behaviour.IsAttached)
                {
                    if(selectedIndex == -1)
                    {
                        selectedIndex = i;
                    }
                    else
                    {
                        DetachBehaviour(behaviour);
                    }
                }
            }

            if(selectedIndex != -1)
            {
                return selectedIndex;
            }

            AttachBehaviour(AvailableBehaviours.First());

            return 0;
        }

        private void DetachBehaviour(GunBehaviour behaviour)
        {
            behaviour.Detach();
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(behaviour);
            }
        }

        private void AttachBehaviour(GunBehaviour behaviour)
        {
            behaviour.Attach();
            if(Application.isPlaying == false)
            {
                EditorUtility.SetDirty(behaviour);
            }
        }
    }
}