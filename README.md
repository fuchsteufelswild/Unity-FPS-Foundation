#  Unity FPS Foundation

A robust, modular data-driven foundation for building first-person shooter games in Unity. Built upon the systems provided in [**Unity Core Utilities**](https://github.com/fuchsteufelswild/Unity-Core-Utilities), this framework provides a complete suite of AAA-quality systems - from procedural animation and weapon handling to inventory and interaction, with extensibility and easy-to-use kept as main objective. 

## Video Demo

Demo of the project showcasing the main features as a small video, there can be small visual defects due to **Free** assets used in the project.

[**YouTube Link**](https://youtu.be/YNK7uSQGYds)

---

# Features

- **Procedural FPS Animation:** A comprehensive system for head and hands motion (Bobbing, Strafe, Jump, Look, and more).
- **Modular Ranged Weapon System:** Guns built from 10+ decoupled subsystems (Aim, Magazine, Recoil, etc.) for unparalleled flexibility with easy plug in-out effector system.
- **Data-Driven Inventory & Items:** A robust inventory system with dynamic properties, constraints, and actions.
- **State-Based Character Controller:** A fluid movement system built with state machine (Idle, Walk, Run, Crouch etc.) and synced step-cycle for footsteps along with advanced character controller.
- **Interaction System:** A standalone, interface-based interaction system with no dependencies, ready to use for any project.
- **Surface & Impact System:** Define *data-based* surfaces and detect or play appropriate effects (audio, VFX, decals) on impact.
- **Input Layer:** Built on Unity's New Input System with a clean abstraction for easy management.

# Architecture Overview
This project exemplifies a composition-over-inheritance architecture. Core systems are broken into small, single-responsibility components that communicate through interfaces, making them highly reusable and easy to test.

# Systems Deep Dive

## Procedural Animation System
Extends the system from **Unity Core Utilities**. Powers all character camera and weapon movement for a cinematic, responsive feel.

- **Motions Included:** Bob, Offset (e.g Weapon Offset), Look, Strafe, Fall, Jump, Noise, Landing, Height (for adjustment), Head Tilt, and Close-Range Retraction (to avoid clipping into walls).
- **Fully Extensible:** Easily create and blend new motion types.

## Ranged Weapon System (Gun)
The core of the framework. Weapons are assembled by combining specialized subsystems, which are further specialized through small action classes that handles *single responsibility* (**Effectors**), enabling rapid creation of everything from pistols to shotguns to sci-fi blaster to even medieval bows.

| Subsystem               | Responsibility |
|-------------------------|----------------|
| `AimBehaviour`          | Controls FOV adjustment, motion damping, scope overlay, and crosshair behavior. |
| `MagazineBehaviour`     | Manages ammo count in magazine and reload logic (full mag vs. progressive). |
| `RecoilSystem`          | Handles visual and positional recoil, including camera shake and return smoothing. |
| `TriggerMechanism`      | Defines fire mode: Semi-Auto, Full-Auto, Burst, and Charged shot support. |
| `FiringMechanism`       | Executes shot logic (projectile spawn or hitscan) based on fire mode and feeds them firing parameters such as spread, shot count. |
| `ProjectileLogic`       | Manages projectile lifecycle: movement, collision, damage, and visual effects. |
| `MuzzleEffectBehaviour` | Spawns muzzle flashes, sparks, light pulses, and sound on fire. |
| `ImpactEffect`          | Applies damage, spawns decals, particle effects, and sounds at hit location. |
| `ShellEjector`          | Simulates physical ejection of shell casings with physics. |
| `CartridgeVisualizer`   | Manages visible cartridges (e.g., in revolver cylinder or transparent mags). |
| `DryFireEffect`         | Plays a "click" sound and animation when attempting to fire with empty magazine. |

## Inventory & Item System
*A standalone assembly with no dependencies on other FPS systems*

- **ItemDefinition (ScriptableObject):** The base data for any item, with unique IDs, tags, shared item behaviours(cookable, craftable), and dynamic properties (e.g ammo count in magazine, durability).
- **nventory:** A **MonoBehaviour**, consists of collection **Container**s.
- **Container:** Manages collections of **Slot**s that hold **ItemStack**s.
- **ContainerAddConstraint:** Restrict which items can be placed in specific containers (e.g "Only magazines in vest pouches")
- **ItemAction:** Define actions like **Drop**, **Equip**, or **Use** that can be performed on items.
- **IItemActionContext:** Solves dependencies without hard references, keeping the system decoupled. 

## Handheld System
The bridge between the Inventory and equippable items.

- **Handheld:** The base class for any equippable weapon, providing **Equip**/**Holster** methods and action handlers.
- **HandheldsInventory:** Keeps track of a character's loadout (e.g a 6-slot radial menu).
- **HandheldsManager:** Manages the **Handheld**s that a character use and controls the **Equip**/**Holster** routines.
- **ActionBlocker:** A system to block actions (e.g. shooting, reloading) based on duration or a persistent block through an **Object** which can only be unblocked again with the same **Object**.

## Interaction System
*A standalone assembly.* A clean, interface-based system for player interaction with the world.

- **IInteractable:** Implement this on any object that can be interacted with or directly use general ready solution **Interactable**.
- **IInteractor:** Implement this on the player or NPCs that can perform interactions.
- **InteractorCore:** Provides base functionality for **IInteractor**.
- **IInteractorContext:** Solves dependencies without hard references, keeping the system decoupled. 

## Other Core Systems
- **Movement:** A state-driven **CharacterMovementController** (Idle, Walk, Run, Crouch etc.) paired with a **CharacterMotor** that handles the movement input acquired from the states and moves the character.
- **Surface System:** Maps **PhysicsMaterial**s to **SurfaceDefinition** assets to retrieve correct surface and to play appropriate impact effects (sound/VFX/Decal).
- **Input:** Extends the **Unity Core Utilities** **Input System** with FPS-specific **InputHandler** classes for movement, looking, interaction, and handheld actions.

---

# Getting Started
1. **Explore the Demo Scene:** Open *Scenes/Demo.scene* to see all systems working together. Examine pre-configured Character prefab and Gun examples.

## Creating Your First Weapon
Note that ammo can be created the same way but in a simpler form, and should be set to different category.

1. Duplicate and existing weapon already existing under Character prefab.
2. Modify its subsystems (e.g change the **TriggerMechanism** from **FullAuto** to **BurstFire**).
3. Create new **ItemDefinition** asset for your Gun and sets its Category as **ItemCategory_Handheld**, then sets its **DynamicItemProperties** like **Ammo In Magazine**.
4. Under **GunItem** select the your newly created **ItemDefinition**.
5. Do not forget to reassign any transforms like **CasingEject** or **MagazineEject** for the new object.
6. All subsystems can be controlled through attachments, in the **Gun** inspector you can see and select which one you want to pick. **AR** gun is a good place to start check its **TriggerMechanism** to see how it both handles **BurstFire** and **FullAuto** at the same time.
7. Do not try to drop an item if it does not have a **PickupItem** included in the **ItemDefinition**.
8. If you want to have a different model, do not forget to create a **CharacterMotionDataPreset** by duplicating *Motion_AR* asset and making changes on it.

## Create a New Surface
1. From the create asset menu select *Nexora/Surfaces/Create Surface*.
2. Set parameters that defines characteristics of the surface along with **PhysicsMaterial** that defines the relationship.
3. Add effect mappings for different surface types.
4. In any **MeshRenderer**, add **MeshSurfaceIdentifier** with the corresponding **Surface Definition**, and also corresponding **PhysicsMaterial** on the **Collider**.

## Additional Actions
1. On *Character/Systems/Inventory*, can edit initial contents and properties *backpack* and *loadout* of the character.
2. On *Character/Systems/Stamina*, can change the states and their stamina usage.
3. On *Character/Systems/Movement*, can inspect states and their corresponding characteristics.
4. On ***Hands*** you can control the block states.
5. In *Resources/Options* folder, can inspect different **Options\<T>** and modify them. *Note that if you already playing it might have retrieved already data and behaviour will not change.*
6. In *FPSDemo/Demo/Data/MotionPresets* you can change and see how **MotionData** works.

---

# Links
**Foundations Library:** [**Unity Core Utilities**](https://github.com/fuchsteufelswild/Unity-Core-Utilities) 

# License
The project is licensed under the **MIT License.**
