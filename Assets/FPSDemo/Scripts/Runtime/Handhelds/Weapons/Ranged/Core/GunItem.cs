using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    [RequireComponent(typeof(Gun))]
    [DefaultExecutionOrder(ExecutionOrder.EarlyGameLogic)]
    public sealed class GunItem : HandheldItem
    {
    }
}