# Project Context for Future Codex Sessions

Last updated: 2026-07-02

This document is intended as the first file to read in future Codex sessions for this Unity project. It records the current understanding of the project, the gameplay flow, the key scripts, and the design decisions made so far.

## Project Summary

This is a Unity VR narrative/interactive experience built on top of the EZPZ Interaction Toolkit and Unity XR Interaction Toolkit samples.

The current custom game appears to be a memory/brain-themed VR experience. The player starts in an office-like space, interacts with a crystal ball, transitions into the Hippocampus area, places or reviews memory-related content, and eventually enters a Nightmare/Giant Clown crisis sequence. The Nightmare sequence includes a giant clown approaching, manipulating the roof, grabbing the player, moving the player toward the clown's mouth, then transitioning into a swallow/fall sequence and finally into a Mirror Chamber.

The current development focus is the Nightmare/Giant Clown sequence, especially:

- Making the giant's hand, IK target, grab anchor, roof, and player movement feel synchronized.
- Avoiding sudden jumps in VR player position or camera rotation.
- Making screen fade transitions support the intended narrative timing.
- Keeping assistant dialogue independent from physical/animation coroutine timing.

## Important Files

Custom scripts are under:

- `Assets/__My Project/scripts/`

Important scripts:

- `GameStateController.cs`
  - Owns the main game state machine.
  - Starts the office intro, crystal ball wait state, Hippocampus transition, Giant Crisis, Swallow Transition, and Mirror Chamber intro.

- `CrystalBall.cs`
  - Detects when the player's hand stays near the crystal ball.
  - Drives the white screen fade before transitioning to Hippocampus.
  - If the player removes their hand before the hold time completes, the fade returns to transparent and the timer resets.

- `ScreenFadeController.cs`
  - Controls fade image alpha and color.
  - Supports setting fade color and alpha directly.
  - Used for both white crystal-ball transition and black Nightmare swallow transition.

- `AssistantController.cs`
  - Controls assistant dialogue lines and stages.
  - Has Nightmare warning dialogue and Swallow Transition dialogue.
  - Assistant dialogue should remain independent from the clown crisis coroutine timing.

- `ClownController.cs`
  - Main controller for the Giant Clown / Nightmare crisis sequence.
  - Handles audio order, giant animation trigger, right-arm IK weight, roof grab/drop, player grab/follow, hand-to-mouth movement, and state transition into `SwallowTransition`.

- `SwallowController.cs`
  - Handles the black screen fade, black hold time, assistant swallow dialogue, teleport to pipe/fall start, fade back in, controlled fall, and transition into Mirror Chamber.

- `MemoryContentDisplay.cs`
  - Related to memory content UI/display.

Main scene:

- `Assets/__My Project/scene.unity`

## Game State Flow

Current high-level state order in `GameStateController`:

1. `OfficeIntro`
   - Assistant plays intro.

2. `AwaitCrystalBall`
   - Crystal ball interaction becomes active.

3. `TransitionToHippocampus`
   - Triggered after the player's hand stays on/near the crystal ball long enough.
   - White screen fade is already fully opaque when this state begins.
   - Player and assistant are moved to Hippocampus spawn points.
   - Screen fades back to transparent.

4. `Hippocampus`
   - Crystal ball disabled.
   - Assistant plays Hippocampus intro.

5. `AwaitMemoryPlacement`
   - Memory placement/review stage.

6. `GiantCrisis`
   - Calls `ClownController.StartCrisisSequence()`.

7. `SwallowTransition`
   - Calls `SwallowController.StartSwallowTransition()`.

8. `MirrorChamber`
   - Assistant moved to mirror chamber spawn point.
   - Assistant plays Mirror Chamber intro.

## Crystal Ball Transition

Target behavior:

- Player places hand on the crystal ball.
- Screen fades to white over 2 seconds.
- If the hand leaves before 2 seconds, fade returns to transparent and timer resets.
- Once complete, player transfers to Hippocampus.

Current implementation:

- `CrystalBall` drives white fade progress while the hand is in range.
- `GameStateController.TransitionToHippocampusRoutine()` assumes the screen is already fully white, teleports player/assistant, then fades back to transparent.

## Nightmare / Giant Clown Sequence

Current `ClownController.CrisisRoutine()` intent:

1. Start assistant Nightmare warning dialogue immediately.
   - Assistant is independent.
   - The coroutine does not wait for assistant dialogue to finish.

2. Play footsteps.
   - `footstepsAudio.Play()`.
   - The crisis coroutine waits for this AudioSource to stop before continuing.
   - Important: if `footstepsAudio` is set to Loop, this will wait forever unless something external stops it.

3. After footsteps end, play rumble/roof vibration audio.
   - `rumbleAudio.Play()`.
   - Rumble starts after footsteps, not at the same time.
   - Rumble does not block the rest of the crisis sequence.

4. Trigger giant animation.
   - Animator trigger name is `BendPick`.
   - Then wait `animationLeadTime` before raising IK weight.

5. Blend right arm Rig weight to 1.
   - `rightArmRig.weight` is controlled, not the `TwoBoneIKConstraint` component's own weight.
   - The Two Bone IK Constraint may still show weight 1 in Inspector; this is expected.

6. Move hand IK target to `roofPoint`.

7. Attach roof to grab anchor.
   - Roof no longer lifts upward first.
   - Roof now smoothly moves/settles onto `grabAnchor` over `roofAttachDuration`.
   - By default roof rotation is not forced to match the hand, to avoid sudden flipping.
   - `alignRoofRotationToGrabAnchor` can be enabled if rotation alignment is desired later.

8. Move hand IK target to `dropRoofPoint`.
   - Because the roof is parented to `grabAnchor`, the hand carries the roof to the drop point.

9. Release roof.
   - Roof is unparented.
   - Rigidbody is set non-kinematic and gravity is enabled.

10. Reach toward player.
   - This stage no longer uses a grab-arrival timeout.
   - The hand must actually reach the player before continuing.
   - The IK target moves toward `playerHead`, but completion is based on `grabAnchor` / grab reference distance to `playerHead`.
   - This prevents the script from continuing just because `handIKTarget` reached the player while the visible hand/grab point has not.

11. Attach player to hand.
   - Player is not parented to the hand.
   - Instead, `LateUpdate()` moves `xrOrigin.position` to follow `grabAnchor.position + playerGrabOffset`.
   - Player rotation is intentionally not inherited, because forced camera rotation is bad for VR comfort and player agency.

12. Blend right arm Rig weight to 0.
   - This lets the giant clown's original animation take over for a while after grabbing the player.

13. Wait `postGrabAnimationHoldDuration`.
   - This is the duration where the original clown animation runs after the player is attached.

14. Sync IK target to visible hand position.
   - `SyncHandIKTargetToVisibleHand()` sets `handIKTarget.position = handTipReference.position`.
   - This avoids a visible snap when IK weight is raised back to 1.
   - It no longer changes player rotation.

15. Blend right arm Rig weight back to 1.

16. Move hand IK target toward `mouthPoint`.
   - `mouthPoint` may be attached to an animated head/mouth bone, so movement samples the target transform continuously.

17. Wait until visible hand reference reaches mouth.
   - Uses `handTipReference` if assigned, otherwise `grabAnchor`, otherwise `handIKTarget`.
   - `mouthArrivalTimeout` still exists as a safety timeout.
   - If the desired final behavior is "must always reach mouth before transition", this timeout can also be removed later.

18. Detach player from hand following.

19. Set game state to `SwallowTransition`.

## Swallow Transition

Target behavior:

- Nightmare mouth transition uses black screen fade.
- Fade to black over about 1 second.
- Once fully black, stay black for a configurable hold time.
- During the full-black hold, assistant enters Swallow Transition dialogue and can output text/audio.
- Then player is transferred to the pipe/fall start point.
- Fade returns to transparent and controlled fall begins.

Current implementation:

- `SwallowController` has:
  - `fadeOutDuration`
  - `blackHoldDuration`
  - `fadeInDuration`
  - `swallowFadeColor`
- `swallowFadeColor` is black.
- Assistant `PlaySwallowTransition()` is called during the black hold period.

## Important ClownController Parameters

Current major Inspector-facing fields:

- `animationLeadTime`
  - Delay after giant animation trigger before IK begins taking over.

- `rigBlendDuration`
  - Initial blend from Rig weight 0 to 1.

- `moveToRoofDuration`
  - Time for hand IK target to move to roof grab point.

- `roofAttachDuration`
  - Time for roof to smoothly settle onto `grabAnchor`.

- `alignRoofRotationToGrabAnchor`
  - Whether roof rotation should also align to hand/grab anchor.
  - Currently default should stay off unless a specific visual need appears.

- `pullRoofDuration`
  - Time for hand to move the attached roof to drop roof point.

- `grabDistance`
  - Distance between grab reference and player head required before player is considered grabbed.

- `reachToPlayerSpeed`
  - Speed factor for hand IK target chasing the player head.

- `rigBlendOutDuration`
  - Time for Rig weight to blend to 0 after player is grabbed.

- `postGrabAnimationHoldDuration`
  - How long the clown's original animation runs after player is grabbed and Rig weight is off.

- `rigBlendInDuration`
  - Time for Rig weight to blend back to 1 before moving toward mouth.

- `moveToMouthDuration`
  - Time for hand target to move toward mouth point.

- `mouthArrivalDistance`
  - Distance threshold for hand/mouth arrival.

- `mouthArrivalTimeout`
  - Safety timeout for hand-to-mouth arrival. Still present.

- `mouthTrackingCatchupSpeed`
  - Speed at which IK target keeps chasing the animated mouth while waiting for arrival.

## Scene Binding Notes

From current inspection of `scene.unity`, `ClownController` is bound roughly as follows:

- `rightArmRig` points to `RightHandRig`.
- `handIKTarget` points to the `RightHandRig` transform used by the Two Bone IK target.
- `handTipReference` points to the visible hand/wrist/finger-related bone used as arrival reference.
- `grabAnchor` is a manually added transform under the giant clown prefab hierarchy.
- `roofPoint` and `roofPiece` currently both reference the roof/floor object being moved.
- `dropRoofPoint` is the point the hand carries the roof toward before release.
- `mouthPoint` is attached under an animated mouth/head transform.
- `xrOrigin` is the player root.
- `playerHead` is the player's head/camera transform.

Important IK note:

- `RightHandRig` has a `TwoBoneIKConstraint`.
- Root, mid, tip, and target are configured in scene.
- Hint is currently removed/unused.
- `TargetRotationWeight` is currently 0, so hand rotation is not driven by the IK target.
- The script controls the outer `Rig.weight`, not the constraint's `m_Weight`.

## Current Design Decisions

- The player's VR camera should not be forcibly rotated during grab/mouth movement.
- Player follows the hand by position only, via `xrOrigin.position`.
- Roof and player share the same `grabAnchor` concept to keep the hand/roof/player relationship consistent.
- Grab completion is determined by the grab reference reaching the player, not by `handIKTarget` alone.
- Grab-to-player has no timeout now; the giant hand must reach the player before the sequence continues.
- Mouth arrival still has a timeout for safety, but this may be removed later if strict arrival is desired.
- Assistant dialogue is independent and should not block the clown crisis coroutine unless explicitly requested.
- Footsteps and rumble are sequential: footsteps finish first, rumble starts afterward.

## Known Risks / Things to Check in Unity

- If `footstepsAudio` is looping, the crisis coroutine will wait forever before rumble/animation starts.
- If `handIKTarget` or `playerHead` is missing, the grab stage now stops and logs an error instead of continuing.
- If `grabAnchor` is placed poorly on the hand skeleton, the player/roof may appear offset even if script logic is correct.
- If the visible hand does not reach mouth before `mouthArrivalTimeout`, the flow may continue with a warning.
- If the roof seems to rotate unnaturally, keep `alignRoofRotationToGrabAnchor` off.
- If the roof seems not to align with the hand enough, tune `roofAttachDuration` and the local position of `grabAnchor`.
- If Rig weight appears not to change, verify that the Inspector is showing the outer `Rig` component weight, not only the `TwoBoneIKConstraint` weight.

## Verification Pattern

For script-only changes, use:

```powershell
dotnet build Assembly-CSharp.csproj --no-restore
```

Recent builds passed with 0 errors. The project currently still reports existing warnings:

- Duplicate `using` directives in `AssistantController.cs`.
- Obsolete `FindObjectsSortMode` usage in EZPZ Interaction Toolkit scripts.

These warnings are pre-existing and not directly related to the Nightmare sequence work.

## Future Update Rule

When making meaningful changes to game flow, scene bindings, or major script behavior, update this document before finishing the task. Future Codex sessions should read this file before doing broad project analysis.
