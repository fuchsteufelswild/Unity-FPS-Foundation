using Nexora.InventorySystem;
using UnityEngine;

namespace Nexora.FPSDemo.InventorySystem
{
    public interface ICharacterInventory : 
        IInventory, 
        ICharacterBehaviour
    {

    }

    public sealed class CharacterInventory : 
        Inventory,
        ICharacterInventory
    {
        
    }
}