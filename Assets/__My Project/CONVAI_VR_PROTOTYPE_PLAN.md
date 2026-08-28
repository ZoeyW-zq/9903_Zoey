# Convai VR Prototype Plan

## Purpose

`scene_VR_ConvaiPrototype.unity` is an isolated copy of the current VR experience for validating Convai without changing `scene_VR.unity` or the current build entry.

The prototype proves one outcome first: in the Office, the player can talk naturally to the assistant and choose to learn about the job, explore the office, or start work without pressing a dialogue button.

The prototype does not make Convai the owner of gameplay progression. Convai owns speech and intent recognition. Unity owns the state machine, interactions, movement, crisis sequence, scene roots, and report outcome.

## Scope

### Included in the first prototype

- Quest/Android microphone permission and a live Convai conversation.
- Convai voice playback from the existing `AssistantRobot` position.
- A context-aware Office greeting.
- Natural-language handling for `office.explain_job`, `office.explore`, and `office.start_work`.
- A local validation layer that executes an Office action only when the current state permits it.
- Subtitle display for Convai replies, if the installed SDK exposes response text.
- A visible and non-blocking fallback when the service is unavailable.

### Explicitly excluded from the first prototype

- Deleting existing dialogue clips.
- Replacing Giant Crisis, swallow, mirror, placement, report, or ending dialogue.
- Giving Convai direct access to `GameStateController.SetState`.
- Replacing local ambience, footsteps, heartbeat, rumble, or swallow audio.
- Persisting API keys in a scene, prefab, ScriptableObject, or source file.

## Existing Project Boundaries

The primary narrative state is held by `GameStateController`. It must remain the only component that transitions the five-stage experience.

| Existing system | Remains responsible for |
| --- | --- |
| `GameStateController` | State transition, scene root activation, fades, player placement, and terminal state. |
| `OfficeDialogueController` | Office availability rules and the existing start/explore/job branches. |
| `MemoryPlacementController` | Object-placement progress and the local condition that starts Crisis. |
| `ClownController` / `SwallowController` | Crisis animation, player control, timing, and non-verbal audio. |
| `MemoryDialogueController` | Mirror completion and failure logic until a later, separately tested migration. |
| Convai | Player speech recognition, assistant speech generation, free-form answers, and structured intent proposals. |

## Target Architecture

```text
Player microphone
  -> ConvaiVoiceBridge
  -> Convai character
  -> response text + generated speech + requested action
  -> NarrativeActionRouter
  -> NarrativeOrchestrator validation
  -> existing Unity flow controller
```

`NarrativeOrchestrator` is the local authority. It knows the current `GameState`, whether a scripted beat is in progress, and which actions are valid. It rejects any Convai request that is out of stage, duplicated, or unsafe.

The game sends small context updates to Convai; Convai must not infer the entire plot from free conversation history.

```text
Unity event: office.entered
Unity context: state=OfficeDialogue, allowed_actions=[office.explain_job, office.explore, office.start_work]
Convai reply: speech + optional structured action
Unity result: action accepted or rejected
```

## New Components

Create these scripts only after importing and checking the installed Convai Unity SDK API. Names below describe project responsibilities, not SDK class names.

### `ConvaiVoiceBridge`

- Wraps the SDK character component and callback API.
- Starts/stops listening only when requested by `NarrativeOrchestrator`.
- Sends game context events to Convai.
- Receives response text, audio lifecycle events, and structured action payloads.
- Routes generated voice through an AudioSource located on `AssistantRobot` with spatial audio enabled.
- Never changes game state or calls gameplay controllers directly.

### `NarrativeOrchestrator`

- Reads `GameStateController.State`.
- Selects one of three modes: `FreeTalk`, `NarrativeBeat`, or `Locked`.
- Maintains the currently allowed action whitelist.
- Pauses input while Convai is speaking or while the game is in a non-interruptible sequence.
- Tells Convai when a Unity event has succeeded, failed, or is unavailable.

### `NarrativeActionRouter`

- Validates the requested action against the current whitelist.
- Applies idempotency locks so duplicate voice callbacks cannot repeat an event.
- Invokes narrowly scoped public methods on flow controllers.
- Logs rejected actions with state and reason for test diagnostics.

### `OfficeConversationFlow`

- Defines the Office prompt and current allowed actions.
- Adapts the existing Office logic instead of duplicating it.
- Reopens listening after the assistant finishes a response.
- Handles ambiguity by asking a clarification question rather than selecting an action.

## Office Action Contract

Use structured actions/function calls supplied by Convai if the SDK supports them. Do not use raw keyword matching as the primary control path.

| Convai action | Valid local state | Unity operation |
| --- | --- | --- |
| `office.explain_job` | `OfficeDialogue`, explanation still available | `OfficeDialogueController.SelectJobExplanation()` |
| `office.explore` | `OfficeDialogue`, exploration available | `OfficeDialogueController.SelectExploreOffice()` |
| `office.start_work` | `OfficeDialogue`, start available | `OfficeDialogueController.SelectStartWork()` |
| `office.return_from_exploration` | Office exploration active | `OfficeDialogueController.ReturnFromExploration()` |
| `office.ask_question` | `OfficeDialogue` | No game action; Convai answers from Office context. |

Example protocol shape:

```json
{
  "action": "office.start_work",
  "confidence": 0.93,
  "speech": "Understood. I will bring up the client file now."
}
```

The router must not accept a confidence score alone as authority. The local state and action whitelist are always decisive.

## Conversation Modes

| Mode | Player can speak | Convai can propose action | Use in this project |
| --- | --- | --- | --- |
| `FreeTalk` | Yes | Only whitelisted actions | Office, calm Memory Room, final redistribution. |
| `NarrativeBeat` | Input may be queued or ignored | No gameplay action | A required assistant explanation or one-off story line. |
| `Locked` | No | No | Crisis, player grab, swallow, fades, scene moves, session completion. |

`GiantCrisis` must be driven by the existing third-memory placement condition. Unity may send `crisis.started` to Convai so it can deliver a contextual line, but Convai cannot start, stop, or complete the crisis.

## Audio Migration

Migrate spoken dialogue in stages. Do not delete any current audio until the corresponding Convai path has completed Quest testing.

1. Prototype only the Office assistant voice.
2. Replace the Memory Room assistant voice.
3. Replace spoken Crisis and Mirror narrative lines, while retaining Unity timing and local non-verbal SFX.
4. Keep a text-only fallback for service failure; retain old clips until full-flow sign-off.
5. Remove obsolete dialogue clips and Inspector fields only after the full build passes regression testing.

For generated speech, the `AssistantController` and Convai bridge must be coordinated. There may be only one active spoken source at a time. `NarrativeOrchestrator` is responsible for interrupting or suppressing output; it must never wait for a remote response before allowing an animation or transition to continue.

## Scene Setup Checklist

When the Convai package is imported into this prototype scene:

1. Add a `ConvaiExperiment` root object outside `OfficeRoot`, `HippoRoot`, and `DungeonRoot` so it survives scene-root switching.
2. Place the SDK character/listener component on `AssistantRobot` or a child named `Convai Voice`.
3. Add `ConvaiVoiceBridge`, `NarrativeOrchestrator`, and `NarrativeActionRouter` to `ConvaiExperiment`.
4. Reference the existing `GameStateController`, `OfficeDialogueController`, `AssistantController`, AssistantRobot AudioSource, and subtitle text.
5. Make the original Office dialogue buttons hidden in the prototype only after voice actions work. Keep the methods and their availability checks.
6. Add Android microphone usage text and verify runtime permission on a Quest device.
7. Configure Convai credentials through the SDK's approved secure configuration path, never in version-controlled scene data.
8. Keep `scene_VR.unity` as the enabled build scene until the prototype is approved.

## Acceptance Tests

The Office prototype is ready for expansion only when all checks pass on a Quest device:

- The assistant answers a free-form question without moving the game forward.
- “Start work”, equivalent phrases, and natural wording reach the same valid start branch once.
- “Explore the office” reaches the exploration branch once and does not show the old choice buttons.
- Ambiguous answers lead to a clarification, not an accidental branch.
- A repeated or delayed Convai callback cannot repeat Start Work or trigger a state transition twice.
- A network/API failure leaves the player in a usable Office state and exposes a fallback message.
- Convai voice is spatialized at the assistant and never overlaps a local scripted voice.
- Crisis still begins solely when Unity detects the third preliminary placement.
- The current `scene_VR.unity` remains behaviorally unchanged.

## SDK Verification Gate

Before implementation, verify from the installed Convai SDK documentation and a Quest build that it supports the needed Unity version, Android IL2CPP/ARM64 target, microphone capture, response text callbacks, controllable listening/TTS lifecycle, and structured actions or an equivalent reliable metadata channel. If structured actions are unavailable, use a constrained local intent parser on the Convai transcript, not unconstrained generated text as a command channel.

## Official Documentation Verification (2026-08-26)

The current Convai Unity SDK documentation confirms the following facts for this prototype:

- The current official installation page describes SDK version 4.5.0 through Unity Package Manager with package name `com.convai.convai-sdk-for-unity`, or through the Unity Asset Store. The Package Manager route still requires the Unity account to hold the Asset Store entitlement for the Convai product; it is not an anonymous public registry download. This project now resolves the Package Manager package at version `4.5.0`.
- The hard minimum Unity version is `6000.0.80f1`. This project uses Unity `6000.4.4f1`, so the project version is within the documented range.
- Built-in, URP, and HDRP render pipelines are supported. The project uses URP.
- Meta Quest is listed as fully supporting voice conversation, microphone capture, remote audio, spatial audio, actions, emotion, long-term memory, narrative design, and dynamic context. Quest passthrough vision is an optional feature limited to Quest 3 and Quest 3S; this prototype does not require it.
- The SDK requires an internet connection at edit time and runtime. Android must declare `android.permission.RECORD_AUDIO`; the SDK requests the runtime permission when recording starts.
- The four core scene components are `ConvaiManager`, `ConvaiRoomManager`, `ConvaiCharacter`, and `ConvaiPlayer`. The documented editor command is `GameObject > Convai > Setup Required Components`.
- `ConvaiRoomManager` supports both `HandsFree` and `PushToTalk`. This prototype intentionally uses `PushToTalk` so incidental speech does not consume interactions. Narrative Decisions are evaluated after the backend emits a processed-final player transcript, regardless of which input mode produced it.
- `ConvaiCharacter` requires a dashboard Character ID. `ConvaiAudioOutput` and an `AudioSource` on the same GameObject provide generated voice output; 3D audio is supported on Quest.
- The SDK exposes event relays for connection, character speech, transcript, turn, emotion, and errors. `ConvaiCharacter.OnActionsReceived` exposes action batches, while `ConvaiActionDispatcher` is optional.
- Character actions can be configured with `ConvaiActionConfigSource` and custom behavior can be implemented through `IConvaiActionExecutor`. For this project, a custom executor or raw action subscription should forward only to a local whitelist and never directly own `GameStateController`.
- Dynamic context is available through `ConvaiCharacter.DynamicContext`; tracked updates can be queued before the character is ready and are confirmed by the SDK. This is suitable for sending `office.entered`, current state, allowed actions, and `crisis.started`.
- Narrative Design provides dashboard-authored sections and named triggers. `ConvaiNarrativeDesignManager` listens for section changes and `ConvaiNarrativeDesignTrigger` sends named triggers. This can complement the Unity state machine, but the current project should keep Unity as the final authority for physical transitions and report outcomes.
- API Key mode stores an obfuscated but not encrypted key in `Assets/Resources/ConvaiSettings.asset` and is documented for local development only. A distributed build should use Auth Token mode with a project-controlled token endpoint.

## Subscription and Payment Decision

The documentation explicitly requires a Convai account, API key, and at least one dashboard character, but the SDK setup pages do not state a mandatory paid subscription for local development. Therefore:

1. Do not purchase a plan before the first editor/Quest proof of concept. Create the account, create one character, install the SDK, and check the dashboard's current free usage allowance.
2. A paid plan or credits may become necessary when the free quota is exhausted, when long sessions are needed, or when the prototype is shared with testers. The exact quota, billing unit, and current plan prices are account/dashboard data and must be checked there before payment.
3. Payment is not a technical compatibility requirement. It is a usage and deployment decision.
4. Never put the account API key in a distributed Quest build. For a private local prototype, API Key mode is acceptable; for any shared build, implement Auth Token mode first.

## Current Repository Import Status (2026-08-26)

The SDK upgrade is complete and the official Sample scene has passed the user's conversation test. Unity has generated the Convai assemblies under `Library/ScriptAssemblies` (including `Convai.Runtime.dll`, module assemblies, transports, and LiveKit), and no Convai C# compilation error was found in the available Unity logs.

- `Packages/manifest.json` contains `com.convai.convai-sdk-for-unity: 4.5.0`, and `Packages/packages-lock.json` resolves the same version.
- The old `Assets/Convai SDK For Unity` copy has been removed, so the project does not intentionally contain two full Convai SDK copies. A separate `Assets/Convai/Plugins` directory remains in the repository; leave it in place until Unity's package validation confirms whether those native plugins are still required by the project.
- `com.unity.nuget.newtonsoft-json` `3.2.2` was added to `Packages/manifest.json` and `Packages/packages-lock.json`, satisfying the embedded SDK dependency.
- `Assets/Resources/ConvaiSettings.asset` exists and has working local-development credentials. Never print or commit the credential in plain text; use Auth Token mode before distributing a Quest build.
- `scene_VR_ConvaiPrototype.unity` contains the required Convai manager, room, player, character, audio, Narrative, and transcript UI components. The production `scene_VR.unity` remains on the recorded assistant flow.

## Recommended First Pass

Use the installed 4.5.0 SDK's `PushToTalk` mode and `ConvaiCharacter` on the existing `AssistantRobot`. Start with one dashboard character and one Office conversation. Use Dynamic Context to provide the current Office state, and use either a Convai character action with a custom local executor or transcript-to-intent routing for the three Office actions. Keep all original scene logic and audio assets until the Quest test passes. Only then migrate spoken lines and consider a paid plan based on measured usage.

## Narrative Setup Status (2026-08-26)

As of 2026-08-27, the formal configuration has moved from the original Mio backend character to the fresh character `kiki` (`33eac386-a1c6-11f1-a23e-42010a7be02f`) after kiki passed a minimal Narrative session/trigger test. Character text, voice, language, Personality, State Of Mind, all formal Sections, Triggers, and Decisions were migrated with new character-scoped IDs. Unity's Narrative Manager is synced to kiki's 15 formal Sections and the Office bridge is enabled. The `probe_*` backend nodes remain dormant for diagnostics; the Unity probe Trigger and Collider are disabled.

As of 2026-08-28, the transition contract is stricter. Unity remains the sole authority for physical stage completion. Convai Section speech completion may release an already validated Unity transition, but generated text never creates that validation. The prototype waits for kiki to finish the missing-memory aside before allowing the pending crisis, waits for the All Released praise before returning to the Memory Room, and waits for Distribution Confirmation before returning to the Office. Each gate has a 25-second fallback. The Office exploration return zone must see the player leave after selecting Explore before its next entry counts as a return, and post-Office Unity states reject stale Office Section side effects.

The Swallow and ending flow now use separate runtime-confirmed Sections and Triggers: `swallow_started -> swallow_transition`, `player_swallowed -> mirror_chamber_introduction`, `final_distribution_confirmed -> final_distribution_confirmation`, `office_returned -> office_report`, and `report_closed -> session_complete`. The final black fade starts only after the `session_complete` farewell turn finishes or its 25-second safety timeout expires. Push To Talk remains intentional; the prototype uses lower explicit VAD thresholds and a shorter release tail to improve quiet/short utterance capture without enabling Hands Free.

For standalone Meta Quest builds, use the SDK's built-in XR mapping rather than a custom Input Action relay. The prototype room's push-to-talk key is `KeyCode.JoystickButton2`, which Convai 4.5.0 maps to `XRNode.LeftHand + CommonUsages.primaryButton` (the X button). Hold X while speaking and release it to send the turn.

kiki's conversation policy is now layered instead of treating every Section Objective as a closed script. Global behavior permits concise general-knowledge answers, focused clarification of fragmented transcripts, and one gentle return to the current task. Stage-specific facts remain in the active Section: the initial Memory Room knows the three shelves and three surface memories, the final Memory Room knows all seven memories and placement tradeoffs, and the Mirror Chamber can answer active environment/glass questions without interrupting local mirror dialogue or predicting future stages.

`ConvaiNarrativeBridge` sends silent Dynamic Context for current room, Unity state, available memory objects, and current task. `GameStateController.MirrorMemoryResolved` publishes a clamped 1-4 count for every successful local mirror resolution; the bridge sends the count and the confirmed mirror-shatter/release fact without triggering a generated turn. This grounds reactive answers while preserving Unity as the sole progression authority.

The bridge now covers the post-Office physical flow as well: second initial placement, Giant Crisis start, completed swallow arrival, all mirror memories resolved, return to the Memory Room, final distribution confirmation, and session completion each map to their matching saved Narrative Trigger or local close action. These transitions originate only from validated Unity state/events and have once-only guards.

Report completion also has a deterministic fallback: when a processed-final transcript in `office_report_reading` explicitly confirms reading is complete, the bridge waits for the normal Decision and then sends `office_reading_finished` only if `office_crystal_ball` did not arrive. Ordinary `Debug.Log` stack traces are disabled to reduce Editor Play Mode stalls; Warning/Error/Exception traces remain available.

`scene_VR_ConvaiPrototype.unity` now contains `ConvaiManager`, `ConvaiRoomManager`, `ConvaiPlayer`, `ConvaiCharacter`, `ConvaiAudioOutput`, and `ConvaiNarrativeDesignManager`. The character is configured as `Mio`, PushToTalk and Connect On Start are enabled, 3D voice output is enabled, and the Narrative Manager is bound to the same Character ID. PushToTalk is intentional for this prototype so incidental speech does not consume interactions.

Fifteen unique backend sections are synced with no orphaned entries and no recorded fetch error:

`runtime_bootstrap`, `office_introduction`, `office_job_explanation`, `office_exploration`, `office_start_work`, `office_report_reading`, `office_crystal_ball`, `memory_room_initial_placement`, `memory_room_warning`, `giant_crisis`, `mirror_chamber_introduction`, `all_memories_released`, `final_memory_placement`, `office_report`, and `session_complete`.

The remaining gap is Unity integration. The scene currently has no `ConvaiNarrativeDesignTrigger`, no project script calling `InvokeTrigger`, `InvokeEvent`, or `InvokeSpeech`, and no listener translating Office section changes into local game actions. The Narrative Manager's section UnityEvents and template-key list are empty. `GameStateController` also has no `AssistantController` or `OfficeDialogueController` reference in the prototype, while its Hippocampus and Mirror Chamber state handlers still directly call `AssistantController`.

## Office Integration Decision

Keep `AssistantController` unchanged for `scene_VR.unity` and other existing scenes. It is a pre-recorded audio/subtitle player and should not become the Convai integration layer.

Reuse `OfficeDialogueController` as the owner of Office business rules, but extend it with an opt-in external-dialogue mode that defaults to off. In external mode it initializes and validates Office state, controls report/crystal-ball visibility, and exposes action-completion methods without playing local clips or showing the three choice buttons. Existing scenes retain the current behavior because their default mode remains local/pre-recorded.

Add one prototype-specific `ConvaiNarrativeBridge` that:

- references `ConvaiCharacter`, `ConvaiNarrativeDesignManager`, `GameStateController`, and `OfficeDialogueController`;
- listens to Narrative section changes and Convai turn/speech completion;
- maps only `office_job_explanation`, `office_exploration`, `office_start_work`, and `office_crystal_ball` to validated Office operations;
- writes `office_entry_reason` and `has_heard_job_explanation` through Dynamic Context or Narrative template keys;
- invokes saved Narrative triggers only after matching Unity events occur;
- applies once-only guards and timeouts;
- never calls `GameStateController.SetState` from arbitrary generated text.

Add a state-change event to `GameStateController` so the bridge can observe Unity state without polling. Existing scene behavior remains unchanged. During migration, each direct `AssistantController` call must either use the old controller when assigned or notify the Convai bridge when the prototype uses Convai.

### Office Proof Flow

1. Unity enters `OfficeDialogue`; `OfficeDialogueController` initializes external mode and the bridge sends `office_entry_reason=first_entry` and `has_heard_job_explanation=false`.
2. Convai begins silently in `runtime_bootstrap`; Unity sends `office_entered` to perform the first real transition into `office_introduction`, which then uses Decisions for job explanation, exploration, or start work.
3. The bridge observes Section changes. Job explanation updates the local flag; exploration enters local exploration state; start work marks the local flow busy.
4. When Mio finishes the `office_start_work` turn, Unity shows the client report, then the bridge invokes the saved report-ready trigger to enter `office_report_reading`.
5. A Decision to `office_crystal_ball` causes Unity to finish report reading, enable the crystal ball, and keep listening available.
6. The physical crystal-ball interaction remains Unity-controlled. After the transition reaches `Hippocampus`, Unity invokes the saved memory-room trigger to enter `memory_room_initial_placement`.
7. Returning physically from exploration first sets `office_entry_reason=exploration_return`, then invokes the saved return trigger to `office_introduction`; the assistant does not repeat its first greeting.

Do not use Section changes alone as proof that a Unity action succeeded. The bridge acknowledges actions only after the corresponding local method completes.

## Implementation Status (2026-08-26)

The Office prototype bridge is now implemented in `scripts/ConvaiNarrativeBridge.cs` and is attached to `AssistantRobot` in `scene_VR_ConvaiPrototype.unity`. `OfficeDialogueController` has an opt-in `ExternalConvai` mode; the prototype uses it while the regular VR scene remains on the recorded mode.

The existing Office exploration Area Trigger is preserved. Its persistent callback targets `OfficeDialogueController.ReturnFromExploration()`. In ExternalConvai mode that callback updates `office_entry_reason=exploration_return` and invokes `office_exploration_returned`, which lets the `office_introduction` Section produce the assistant's proactive return greeting.

The backend Start Section is the silent `runtime_bootstrap`. Office first initialization uses the separate saved `office_entered` trigger after setting `office_entry_reason=first_entry`. The bridge holds that Trigger locally until `ConvaiCharacter.OnCharacterReady`, then waits one frame for the SDK to flush the queued template keys before invoking it. It performs the first real transition from `runtime_bootstrap` to `office_introduction`, ensuring Convai can return a `behavior-tree-response` rather than relying on a same-section self-transition. `office_exploration_returned` remains exclusive to the physical return Area Trigger, preserving a different Message and greeting.

The bridge currently handles these Section changes: `office_job_explanation`, `office_exploration`, `office_start_work`, and `office_crystal_ball`. It sends these Unity-confirmed triggers: `office_exploration_returned`, `office_report_ready`, and `memory_room_entered`. Start Work uses the Convai `OnTurnCompleted` callback with a timeout before the report is shown. The production scene is not changed.

For Start Work, the backend also contains `office_start_work_requested -> office_start_work`. Natural-language Decisions remain the primary path. If the SDK emits a processed-final player transcript matching the restricted start-work intent whitelist but no `office_start_work` Section arrives within 2.5 seconds, the bridge sends this saved Trigger as a deterministic fallback. The fallback reads only player transcript events from `ConvaiManager.Events`, never Mio's generated response text.

The three values above are Saved Trigger **names**, not Trigger Messages or destination Section IDs. SDK 4.5.0 stores all three fields separately. Confirm the backend Trigger Name exactly matches the bridge field; use the `ConvaiNarrativeDesignTrigger` Inspector's Fetch Triggers dropdown to inspect the names returned by the backend when the Dashboard UI does not show them clearly.

The bridge registers itself with `ConvaiNarrativeDesignManager.OnAnySectionChanged` at runtime. Do not manually populate every Section's On Section Start/End UnityEvents; doing both would execute Office actions twice. Start Work now waits for actual `OnSpeechStarted`/`OnSpeechStopped`, with `OnTurnCompleted` and a timeout as fallbacks, before showing the PC report.

`office_crystal_ball` must not connect directly to `memory_room_initial_placement` through a Decision. It has no outgoing conversational Decision. The only entry to `memory_room_initial_placement` is the saved `memory_room_entered` Trigger sent after Unity completes the white transition and changes its state to `Hippocampus`. The bridge logs an error if the backend enters the memory-room Section early.

For concise VR delivery, ordinary questions and repeated interactions should use one to three short sentences. First greetings and first confirmed arrivals in genuinely new locations may use four to six concise sentences when their Section Objective explicitly requests an introduction. These introductions should be proactive after the matching Unity Trigger, while return visits remain brief and avoid repeating earlier material. Sample Dialogues should remain short. Speaking speed is controlled by the selected Convai voice/provider settings, not by `OfficeDialogueController`; do not increase Unity AudioSource pitch because it distorts streamed audio rather than changing TTS generation speed.

Do not put the full Giant Crisis, swallowing, Mirror Chamber, return, or ending sequence in the global Character Background or Sample Dialogues. Global character fields are visible in every Section and can cause Mio to narrate future transitions before Unity performs them. Add this global restriction: `Mio only knows the currently active Narrative Section. Mio never narrates an environmental change, transition, arrival, object appearance, or completed game action unless runtime context explicitly confirms that it has happened. Mio does not foreshadow future Sections.`

The bridge polls `CurrentSectionID` as a fallback in addition to subscribing to `OnAnySectionChanged`. An empty ID immediately after Character Ready is valid because SDK 4.5.0 populates it only after the server sends a `NarrativeSectionChanged` event; the initial backend Section is not guaranteed to arrive as a change event. Validate Narrative by making the first Office Decision and checking that `office_start_work`, `office_exploration`, or `office_job_explanation` is reported.

The C# project builds with zero errors. Quest runtime behavior, backend action/decision wiring, and exact Dashboard Section content still require Play Mode testing in the prototype scene.

The prototype scene contains no `AssistantController` component and its External Office dialogue arrays are empty, so it cannot play the old recorded Office clips. An earlier Play Mode run used an unsynchronized in-memory scene and logged calls to the old recorded path; a later run used the External configuration and no longer produced those calls. Reload the scene from disk after external scene edits before testing.

Unity Play Mode may report `Loaded scene 'Temp/__Backupscenes/0.backup'` when the active scene has unsaved in-memory changes. That backup can predate external scene updates and omit the bridge wiring even though the disk scene is correct. Before testing, preserve any wanted unsaved work in a separate scene, then reopen `scene_VR_ConvaiPrototype.unity` from disk. A valid run logs `ConvaiNarrativeBridge active: Office flow is connected to Narrative Design.` before any Section changes.

Convai Diagnostics are set to Warning with stack traces disabled for normal iteration. Info logging with stack traces produced thousands of Unity Console frames during live transcription and caused substantial Editor stutter. Temporarily enable verbose diagnostics only for a focused capture, then restore the lighter setting.

## Transcript UI Status (2026-08-26)

`TranscriptUI_Chat` is present in `scene_VR_ConvaiPrototype.unity` and its `ChatTranscriptUI` references are intact: the chat container, character/player message prefabs, input field, `CanvasFader`, and `CanvasGroup` are assigned. The canvas is configured for World Space and is parented to `AssistantRobot`.

The UI previously stayed blank because the project-wide Convai setting `Assets/Resources/ConvaiSettings.asset > Transcript System Enabled` was false. SDK 4.5.0 still creates the transcript facade in this configuration, but presentation is disabled, so `ChatTranscriptUI` clears its rows and does not subscribe to transcript changes. The setting is now enabled.

The prefab's initial `CanvasGroup` alpha of zero is intentional. `ChatTranscriptUI.Start()` calls `CanvasFader.StartFadeIn`, so do not force alpha to one in the scene. Verify in a fresh Play Mode connection that both the player's committed speech and Mio's replies create rows and that the canvas fades to alpha one.

The Console `KeyNotFoundException` naming `TranscriptUI_Chat` comes from XR Interaction Toolkit's `TrackedDeviceGraphicRaycaster.OnDisable()` while leaving Play Mode. It also occurs for unrelated canvases such as `SingleButton`; it is a teardown bug and is not the reason transcript rows were absent. Treat it separately if it begins occurring during active Play Mode or affects a Quest build.
