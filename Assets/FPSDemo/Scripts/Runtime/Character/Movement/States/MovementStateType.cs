using System;
using System.Linq;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public enum MovementStateType : int
    {
        /// <summary>
        /// Mainly used for denoting <b>All</b> states, like if you want to listen to changes for 
        /// all states, then you need to use this.
        /// </summary>
        None = 0,
        Idle = 1,
        Walk = 2,
        Run = 3,
        Slide = 4,
        Crouch = 5,
        Prone = 6,
        Jump = 7,
        Airborne = 8,
        Swim = 9
    }

    public enum MovementStateTransitionType
    {
        Enter = 0,
        Exit = 1,
        Both = 2
    }
}