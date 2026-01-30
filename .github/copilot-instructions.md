# AI Coding Instructions for Jam Game

## Project Overview
**Jam Game - Carnival of Masks** is a third-person carnival game built in Unity 6 (2024.1+) featuring anthropomorphic animal characters with magical mask-based abilities. The project is in early development as a game jam entry.

## Architecture & Major Components

### Project Structure
- **Assets/Scripts/Player/** - Player character mechanics and controls
- **Assets/Scenes/** - Game scenes (currently SampleScene)
- **Assets/ScriptableObjects/** - Gameplay configuration and data
- **Assets/Art/** - Sprites, models, and visual assets
- **Assets/Settings/** - Volume profiles and rendering settings (using URP)

### Technology Stack
- **Engine**: Unity 6.0.44f1
- **Rendering**: Universal Render Pipeline (URP) v17.0.4
- **Input System**: New Input System (1.13.1) via InputSystem_Actions.inputactions
- **IDE Support**: Rider and Visual Studio integration configured
- **Testing**: Unity Test Framework (1.5.1)
- **VFX/Animation**: Visual Scripting, Timeline, Particle System

## Critical Workflows

### Build & Development
- **Solution**: JamGame.sln (Visual Studio/Rider compatible)
- **Build Targets**: Assembly-CSharp.csproj (runtime), Assembly-CSharp-Editor.csproj (editor tools)
- **Scene Entry**: SampleScene.unity is the primary game scene

### Running & Debugging
1. Open JamGame.sln in your IDE or use Unity Editor directly
2. Use Unity Play mode (Ctrl+P) to test from SampleScene
3. Check Logs/ folder for runtime errors
4. Monitor Library/SceneVisibilityState and PlayModeViewStates for debugging state

## Project-Specific Patterns & Conventions

### Gameplay Architecture
- **Mini-Games as Standalone Modules**: Each carnival game (Dunk Tank, Balloon Pop, Strength Meter, Bowling) should be implementable as isolated scenes or prefab systems with clear entry/exit points
- **Mask Ability System**: Characters possess mask-based abilities split into two categories:
  - Buff abilities (enhanced stats)
  - Quirk abilities (unusual mechanics/gameplay glitches)
- These should use a scriptable object or state machine pattern for data-driven behavior

### MonoBehaviour Usage
- Follow standard Unity patterns: Start() initialization, Update() frame logic
- Player controller scripts go in `Assets/Scripts/Player/`
- Use serialized fields sparingly; prefer scriptable objects for configuration
- Avoid tight coupling between mini-games and global systems

### Input Handling
- Use **New Input System** (InputSystem_Actions.inputactions configured)
- All player controls must reference this centralized input configuration, not legacy InputManager
- Example: Input events should map to carnival game-specific actions

### Asset Organization
- Prefabs and configurations in `Assets/ScriptableObjects/`
- Visual effects and animations managed through Timeline
- URP rendering configured in `Assets/Settings/DefaultVolumeProfile.asset`

## Integration Points & Dependencies

### Key Package Dependencies
- **com.unity.inputsystem** - Centralized input handling (required for cross-platform carnival controls)
- **com.unity.render-pipelines.universal** - Post-processing and visual effects
- **com.unity.ai.navigation** - Character AI pathfinding (for carnival NPCs/mechanics)
- **com.unity.visualscripting** - Visual behavior graphs for complex game logic
- **com.unity.timeline** - Cinematic sequences and mini-game scripting

### External Tool Integration
- **IDE Setup**: Rider/Visual Studio configured through IDE-specific packages
- **Multiplayer Center**: Project has multiplayer packages but unclear if implemented—check for usage before adding network code

## Dark Theme & Carnival Atmosphere
- The project emphasizes "darkly-themed" carnival setting with "twisted challenges"
- Art direction should maintain this unsettling ambiance throughout all mini-games
- "Hidden surprises and dark twists" suggests branching/event-driven narrative systems (not yet visible in codebase)

## Development Notes
- Early development stage—expect incomplete systems
- Jam project context means rapid iteration over polish
- Test scripts (Player/Test.cs) are placeholder stubs—implement actual gameplay logic progressively
- No existing gameplay loop visible yet—core game state management needs implementation
