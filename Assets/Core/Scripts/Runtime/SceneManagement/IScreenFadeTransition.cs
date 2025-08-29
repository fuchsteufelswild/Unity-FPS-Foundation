using System.Collections;
using UnityEngine;

namespace Nexora
{
    public interface IScreenFadeTransition
    {
        IEnumerator Show(float speedMultiplier = 1f);
        IEnumerator Hide(float speedMultiplier = 1f);
    }
}
