# Office Dialogue Design

## Scope

Implement the Office onboarding stage only. The player can learn about the Memory Organizer role, explore the office, or start work. The crystal ball remains disabled until Start Work completes.

## Minimal Architecture

Only one new runtime script is added: `OfficeDialogueController`.

- `AssistantController` remains the shared assistant dialogue player. It gains a public `PlayDialogue` method so another controller can play any Inspector-configured line array, plus a fallback subtitle duration when a line has no audio.
- `OfficeDialogueController` owns the Office dialogue arrays, choice labels, three session flags, Canvas, three buttons, and button labels. It is the only script that manages Office choices and UI visibility.
- `GameStateController` gets one Office dialogue state. It starts the Office controller with the crystal ball disabled, and the Office controller changes to `AwaitCrystalBall` only after Start Work dialogue completes.

No separate dialogue-player, state-model, or UI-view script is created.

## Inspector Configuration

`AssistantController.DialogueLine` contains subtitle text, optional audio, `keepVisibleAfterLine`, and `fallbackSubtitleDuration`.

`OfficeDialogueController` exposes six dialogue arrays, three option labels, a World Space Canvas root, three `Button` references, three TMP label references, and references to `AssistantController` and `GameStateController`. All story content remains editable in the Inspector.

## Behavior

1. Entering `OfficeDialogue` activates `OfficeRoot`, disables the crystal ball, plays the greeting, then shows choices.
2. Job Explanation plays its array and is not shown again in the current session.
3. Explore hides choices and plays the departure array. A manually bound trigger/interactable calls `ReturnFromExploration()`, which marks exploration complete, plays the return array, and restores still-valid choices.
4. Start Work hides choices permanently, plays the start-work and crystal-ball instruction arrays, then enters `AwaitCrystalBall`.
5. Inputs while a dialogue is running, while exploration is active, or after Start Work are ignored.

The three flags are session-only: `HasHeardJobExplanation`, `HasExploredOffice`, and `IsReadyToStart`.

## Scene Binding

Create a World Space Canvas with three standard Unity UI Buttons and TMP labels. Configure the active EventSystem/XR UI input module and controller ray UI interaction. Assign all UI references to `OfficeDialogueController`. Create an exploration return trigger/interactable and bind its UnityEvent to `OfficeDialogueController.ReturnFromExploration()`.

## Tests

One Edit Mode test fixture validates duplicate input, used-option removal, explicit exploration return, and delayed transition to `AwaitCrystalBall`.
