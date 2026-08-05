# Project Context for Future Codex Sessions

Last updated: 2026-07-30

This document is intended as the first file to read in future Codex sessions for this Unity project. It records the current understanding of the project, the gameplay flow, the key scripts, and the design decisions made so far.

## Project Summary

This is a Unity VR narrative/interactive experience built on top of the EZPZ Interaction Toolkit and Unity XR Interaction Toolkit samples.

The current custom game appears to be a memory/brain-themed VR experience. The player starts in an office-like space, interacts with a crystal ball, transitions into the Hippocampus area, places or reviews memory-related content, and eventually enters a Nightmare/Giant Clown crisis sequence. The Nightmare sequence includes a giant clown approaching, manipulating the roof, grabbing the player, moving the player toward the clown's mouth, then transitioning into a swallow/fall sequence and finally into a Mirror Chamber.

The current development focus is the Nightmare/Giant Clown sequence, especially:

- Making the giant's hand, IK target, grab anchor, roof, and player movement feel synchronized.
- Avoiding sudden jumps in VR player position or camera rotation.
- Making screen fade transitions support the intended narrative timing.
- Keeping assistant dialogue independent from physical/animation coroutine timing.

## Planned Final Narrative Design (Not Yet Implemented)

This section records the approved direction for the final assignment version. It is a design specification, not a description of the current Unity implementation. The technical sections later in this document still describe the currently implemented linear flow.

### Core Theme and Rules

- The player is a Memory Organizer who helps a client decide how much attention to give different memories.
- The player cannot turn memories into long-term or short-term memories and cannot delete real experiences. Those processes occur naturally in the client's life.
- Painful and pleasant memories continue to exist together. The problem occurs when painful memories collect, reinforce each other, and occupy too much attention.
- The Giant Clown is the physical form of accumulated anxiety, pressure, shame, and self-criticism.
- The player does not rewrite the past. They understand memories, respond to the client's current internal beliefs, and redistribute attention.
- The final outcome is based only on where the player places the seven memories during the final Memory Room stage.
- Failed dialogue choices in the Giant stage have immediate feedback but do not affect the final office report.

### Client Profile

The client is approximately 28 years old and works in an office. They are conscientious, sensitive, and inclined to please others. They find it difficult to refuse requests or ask for help, and they often connect personal worth to performance and other people's approval.

The central internalized belief connecting the seven memories is:

> Only when I perform perfectly, endure pressure, and avoid burdening other people am I worthy of recognition.

The two positive recent memories establish an alternative perspective:

> Some experiences deserve attention simply because they are beautiful or personally meaningful. Self-worth does not need to come from external approval.

### Planned Full Game Flow

#### Stage 1: Office

1. The computer is initially in a standby state and the crystal ball is not present.
2. The assistant greets the player and asks whether they are ready to begin work.
3. Three options appear:
   - Learn more about the Memory Organizer job.
   - Explore the office before starting.
   - Start work now.
4. Choosing the job explanation plays the explanatory dialogue. The assistant explains that a Memory Organizer can redistribute attention but cannot delete, alter, or recreate a client's memories. The already-used option is then removed.
5. Choosing office exploration makes the assistant wait. When the player returns:
   - All three choices appear if the player has not heard the job explanation.
   - Only office exploration and starting work appear if the explanation has already been heard.
6. The player may repeat office exploration any number of times.
7. Choosing to start causes the crystal ball to appear.
8. The office computer then loads the assigned client's information:
   - Client name and case ID.
   - `Session 01 / Initial Memory Organization` and first-visit status.
   - `Sleeping / Connection Available` and the current sleep phase, such as `REM Sleep`.
   - Heart rate, respiration, temperature, and overall physical stability.
   - A concise initial assessment and client statement.
9. The player reviews and confirms the client report. The crystal ball remains unavailable until this confirmation so the information cannot be skipped accidentally.
10. The assistant asks the player to place a hand on the crystal ball.
11. The existing white fade and teleport transition move the player and assistant to the Memory Room.

The Office choice flow supports these routes and any repeated exploration variants:

- Job explanation -> start.
- Job explanation -> explore -> start.
- Explore -> start.
- Explore -> job explanation -> start.
- Explore -> job explanation -> explore -> start.

Suggested state flags:

- `HasHeardJobExplanation`
- `HasExploredOffice`
- `IsReadyToStart`
- `HasConfirmedClientReport`

The Office choices let the player control onboarding pace. The primary narrative agency occurs later through attention placement.

#### Stage 2: Initial Memory Room Organization

1. On arrival, the assistant explains that the room and its existing objects are formed from the client's established long-term memories.
2. The assistant clarifies that memories cannot be deleted or rewritten. The player's task is to decide how much current attention each surfaced memory receives.
3. Three recent surface-memory objects appear: the water bottle, sunset photograph, and LEGO bricks.
4. The assistant introduces the three spatial attention categories:
   - `Focus`: high current attention.
   - `Context`: available in the client's active life context without dominating attention.
   - `Background`: still present and accessible, but outside current attention.
5. The player may inspect the three objects in any order. Picking up an object plays its memory clip.
6. After viewing a memory, the player places the object in one of the three categories. Objects may be moved again before confirmation.
7. No visible score, mood value, correct-answer color, or failure label appears.
8. After the second object is placed, the assistant notices that something is missing:

   > "Strange... these all seem to be memories that only recently surfaced. Where are the more painful memories that have been receiving so much attention?"

9. A low sound, brief light fluctuation, or subtle room vibration hints that the missing memories have collected elsewhere.
10. After all three objects are placed, the player may revise the layout and confirm the preliminary organization.
11. Confirmation disturbs the deeper painful memories that have collected in the client's subconscious.
12. The Giant Clown crisis begins. The giant grabs and swallows the player and assistant.

The crisis occurs on every path so the production needs only one main story spine. It is not a punishment for an incorrect preliminary layout. The final report is calculated only from the final seven-object placement later in the experience.

#### Stage 3: Giant Interior / Deep Memory Crisis

1. The player arrives in the enclosed room inside the Giant Clown.
2. Four mirrors represent amplified painful memories and repeatedly emit the client's negative internal statements.
3. The player may approach the mirrors in any order.
4. Approaching a mirror displays a `Start Conversation` button.
5. Selecting it starts a two-round conversation with the client's current internal voice.
6. The first round always contains three options:
   - Two options that can develop the conversation.
   - One plausible but harmful shortcut that reinforces avoidance, perfectionism, isolation, or self-criticism.
7. Selecting the harmful shortcut immediately ends the conversation:
   - The client speaks one more agitated line.
   - The voice and nearby environment briefly intensify.
   - The mirror returns to its idle state and displays `Start Conversation` again.
8. Selecting either constructive first-round response enters a second round.
9. Every second-round option is constructive. Any second-round choice successfully completes the memory, but the wording lets the player choose the perspective they prefer.
10. On success, the client forms a more flexible interpretation. The mirror stops echoing and returns to the original memory object.
11. After all four objects are restored, the painful memories no longer sustain the Giant Clown. The giant space dissolves.
12. The player, assistant, and four restored objects return to the Memory Room.

Dialogue failures have immediate local consequences only. They are not included in the final office report and do not permanently block progress.

#### Stage 4: Final Attention Redistribution

1. The three recent objects and four restored deep-memory objects are now present together.
2. The player has enough context to reconsider all seven memories.
3. The player places every object in `Focus`, `Context`, or `Background`.
4. No real memory can be deleted.
5. The player can revise the layout until confirming the final distribution.
6. The confirmed positions are stored for the office report.
7. The player and assistant return to the Office.

#### Stage 5: Office Report and Ending

1. The player and assistant return to the Office. The crystal ball stops glowing and the subconscious connection closes.
2. The computer displays `Processing Session Data`, followed by `Session Report Ready`.
3. The player selects `View Updated Report`.
4. The report reads only the seven confirmed final positions. Each object has a `Background`, `Context`, or `Focus` outcome, producing `3^7 = 2187` possible attention layouts.
5. The implementation does not require 2187 separately authored reports. It selects one of three outcome fragments for each of the seven objects, for a total of 21 authored object-level fragments, then combines them with an overall synthesis.
6. The report displays:
   - Client and session information.
   - The final attention position of all seven memories.
   - One position-specific finding for each memory.
   - A qualitative overall analysis and suggested direction.
   - A personal client message.
7. The player selects `Play Client Message` to hear or read the client's feedback.
8. The assistant states that the day's work is complete.
9. The player closes the report and the experience ends.

The report uses qualitative language, not scores, stars, percentages, or explicit success/failure colors.

## Planned Memory Set

### Recent Surface Memory 1: Water Bottle

- **Object:** A used insulated water bottle.
- **Memory:** The client has a fever but remains alone in the office to finish a project. They drink water, take medicine, and continue editing after everyone else has left.
- **Internal line:** "Just keep going. I can rest after I finish."
- **Emotional quality:** Painful/current negative pattern.
- **Meaning:** The client puts responsibility before physical needs and treats rest as irresponsibility.
- **Deep links:** The old alarm clock and old phone.

### Recent Surface Memory 2: Sunset Photograph

- **Object:** An orange-red instant photograph or a printed phone photograph of a sunset.
- **Memory:** While commuting home, the client stops checking work messages after noticing the sunset. They leave public transport one stop early, stand near a river or pedestrian bridge for several quiet minutes, and take the photograph.
- **Internal line:** "The sky looks beautiful today."
- **Emotional quality:** Purely pleasant.
- **Meaning:** An experience can deserve attention without proving ability, producing value, or receiving approval.
- **Deep link:** The old alarm clock. The memory counters the belief that stopping is wasted time.

### Recent Surface Memory 3: LEGO Bricks

- **Object:** A gradually completed LEGO model and several loose bricks.
- **Memory:** The client spends a long evening assembling a LEGO model. Watching it take shape piece by piece draws them into a calm flow state. Completing it brings private satisfaction and a genuine sense of achievement without requiring external approval.
- **Internal line:** "It took a long time, but I like seeing what I built piece by piece."
- **Emotional quality:** Positive self-recognition.
- **Meaning:** Patient creation, flow, and gradual progress can provide achievement and personal value without external evaluation.
- **Deep links:** The second-place medal and red correction pen.

### Deep Memory 1: Second-Place Medal

- **Object:** A worn silver medal with the client's childhood name engraved on the back.
- **Memory:** As a child, the client excitedly shows their parents a second-place medal. The parents first ask who came first and why the client did not beat them.
- **Repeated negative lines:**
  - "There is nothing worth celebrating about second place."
  - "If you had worked harder, you would not have lost."
- **Internalized belief:** Only the highest achievement has value.
- **Emotions:** Inferiority, perfectionism, fear of failure.
- **Surface link:** The LEGO model demonstrates that gradual, imperfect progress can still deserve personal recognition.

### Deep Memory 2: Old Alarm Clock

- **Object:** An old red alarm clock whose hands repeatedly jump back to 6:00 a.m.
- **Memory:** While sick as a student, the client asks to stay home. Their parents, who are also under work and financial pressure, say that everyone is tired but responsibilities still need to be completed.
- **Repeated negative lines:**
  - "Other people can keep going. Why can't you?"
  - "Stopping means you are irresponsible."
- **Internalized belief:** Illness and exhaustion are not acceptable reasons to rest.
- **Emotions:** Guilt, anxiety, discomfort with rest.
- **Surface links:** The water bottle repeats this behavior; the sunset photograph shows that stopping can have value.

### Deep Memory 3: Old Phone

- **Object:** An old phone with a cracked screen showing an unsent request for help.
- **Memory:** During a university group project, the overloaded client asks a teammate for help. The teammate responds, "Everyone is busy. Don't hold the group back." The client deletes the message and stays up alone to complete the work.
- **Repeated negative lines:**
  - "Do not burden other people."
  - "Needing help means you are unreliable."
- **Internalized belief:** Asking for help causes disappointment and proves incompetence.
- **Emotions:** Shame, isolation, fear of becoming a burden.
- **Surface link:** The client again works alone while sick in the water-bottle memory.

### Deep Memory 4: Red Correction Pen

- **Object:** A red pen carrying the logo of the client's former employer.
- **Memory:** Early in the client's career, they make a data error in a report. A supervisor circles the error in front of colleagues and says, "If you make mistakes like this, I cannot trust you with more important work."
- **Repeated negative lines:**
  - "You are not capable."
  - "You will make the same mistake again."
- **Internalized belief:** One mistake proves total incompetence.
- **Emotions:** Self-doubt, shame, fear of authority judgment.
- **Surface link:** A misplaced LEGO piece can be corrected without erasing the value of the completed model.

### Memory Relationship Summary

```text
Second-place medal
  -> I must be the best to have value
  -> LEGO bricks: gradual progress can still deserve self-recognition

Old alarm clock
  -> Rest is irresponsible
  -> Water bottle: the client works while sick
  -> Sunset photograph: stopping can be valuable in itself

Old phone
  -> Asking for help makes me a burden
  -> Water bottle: the client continues alone while sick

Red correction pen
  -> One mistake proves I have no ability
  -> LEGO bricks: one misplaced piece does not erase the value of the whole model
```

## Planned Deep-Memory Dialogue Scripts

### Dialogue 1: Second-Place Medal

**Initial internal voice:**

> "I came second, but the first thing they asked was why I was not first. Maybe only first place is worth celebrating."

**First-round choices:**

1. "You wanted them to see your effort first. It makes sense that their reaction hurt."
2. "Second place still reflects your effort. A rank cannot decide whether the experience has value."
3. "Keep working harder. Next time, win first place and prove it to them." *(Harmful shortcut)*

**Harmful response:**

> "Right... as long as I am not first, I am still not good enough. I cannot stop."

The dialogue closes, the voice intensifies, and `Start Conversation` returns.

**If choice 1 was selected:**

Client:

> "But if I am satisfied with second place, does that mean I have no ambition?"

Second-round success choices:

- "Recognizing what you have achieved does not prevent you from continuing to grow."
- "You can want to improve while still allowing yourself to be happy with what you did."

**If choice 2 was selected:**

Client:

> "They looked so disappointed. It is hard to believe second place really had value."

Second-round success choices:

- "Their expectations belong to them. They cannot replace your judgment of your own value."
- "Like your LEGO model, gradual progress can deserve recognition before everything is perfect."

**Resolution line:**

> "I can keep growing without waiting until I am first to recognize what I have already done."

The mirror becomes the second-place medal.

### Dialogue 2: Old Alarm Clock

**Initial internal voice:**

> "I only wanted to rest, but everyone was tired. Maybe stopping makes me selfish and irresponsible."

**First-round choices:**

1. "Your exhaustion and illness were real. They were not excuses."
2. "Being responsible does not mean that you can never stop and rest."
3. "Keep enduring it. You can rest after everything is finished." *(Harmful shortcut)*

**Harmful response:**

> "But the work never ends... does that mean I can never stop?"

The dialogue closes, the clock accelerates, and `Start Conversation` returns.

**If choice 1 was selected:**

Client:

> "But if I rest, the work may fall on someone else."

Second-round success choices:

- "You can communicate and request support instead of continuing alone until you cannot function."
- "Caring for your body is also part of being able to meet future responsibilities."

**If choice 2 was selected:**

Client:

> "Every time I stop, I feel guilty, as if I have done something wrong."

Second-round success choices:

- "That guilt comes from an old rule. It does not prove that you did something wrong."
- "You once stopped to watch the sunset. Those few minutes did not harm anyone."

**Resolution line:**

> "Rest is not avoiding responsibility. I can care for myself and then decide what to do next."

The mirror becomes the old alarm clock and its hands stop looping.

### Dialogue 3: Old Phone

**Initial internal voice:**

> "I asked for help and they said I was holding everyone back. Maybe reliable people should solve everything alone."

**First-round choices:**

1. "That rejection made you feel ashamed. It makes sense that asking again became difficult."
2. "One person's response cannot represent everyone or prove that asking for help means incompetence."
3. "Never depend on anyone again. Doing everything alone is safest." *(Harmful shortcut)*

**Harmful response:**

> "Right. If I carry everything alone, no one can reject me. I cannot ask, no matter how exhausted I become."

The dialogue closes, unsent messages multiply, and `Start Conversation` returns.

**If choice 1 was selected:**

Client:

> "What if I ask again and I am rejected again?"

Second-round success choices:

- "You can choose people you trust. One refusal cannot decide whether you deserve support."
- "Making a request gives another person a choice. It does not force your responsibility onto them."

**If choice 2 was selected:**

Client:

> "Needing help still makes me feel unreliable."

Second-round success choices:

- "Being reliable does not mean doing everything alone. It includes knowing when collaboration is needed."
- "When you worked alone while sick, continuing by yourself did not make you safer."

**Resolution line:**

> "I can try to ask. How another person responds should not decide whether I am allowed to need help."

The mirror becomes the cracked old phone.

### Dialogue 4: Red Correction Pen

**Initial internal voice:**

> "That mistake showed everyone that I was unprofessional. Maybe they will eventually discover that I am not capable."

**First-round choices:**

1. "Being criticized in front of everyone was humiliating and frightening. Your reaction makes sense."
2. "One mistake means that the report needed correction. It does not mean that you have no ability."
3. "Check more carefully next time and make sure you never make another mistake." *(Harmful shortcut)*

**Harmful response:**

> "Right. Everything must be perfect. If I make one more mistake, they will know I am unreliable."

The dialogue closes, red marks spread, and `Start Conversation` returns.

**If choice 1 was selected:**

Client:

> "Every time I submit something now, I still remember that red pen."

Second-round success choices:

- "The memory can remind you to check, but it does not have to decide the result in advance."
- "You can take your work seriously while accepting that mistakes remain possible."

**If choice 2 was selected:**

Client:

> "But what if the next mistake has real consequences?"

Second-round success choices:

- "Taking responsibility and correcting a consequence is different from denying your entire ability."
- "Like one misplaced piece in your LEGO model, one error does not remove the value of the whole work."

**Resolution line:**

> "I can check carefully and correct mistakes without using perfect performance to prove my ability."

The mirror becomes the red correction pen.

## Planned Office Outcome Reports

### Outcome Model

The previous three-template ending calculation is obsolete. The approved design uses compositional object-level feedback.

- The final report reads only the seven confirmed Memory Room positions.
- Each object has a `Background`, `Context`, and `Focus` feedback fragment.
- Seven objects multiplied by three positions require 21 authored fragments.
- The seven ternary positions produce `3^7 = 2187` possible final layouts.
- The report selects seven fragments and combines them with an overall synthesis and personal client message.
- Dialogue failures in the Giant Interior and the preliminary three-object layout do not affect this report.

These labels describe current attention rather than memory storage:

- `Background / Low`: the memory remains present and accessible but rarely enters current attention. It does not mean deletion, denial, suppression, or forgetting.
- `Context / Medium`: the memory remains available as useful context or guidance without dominating the present.
- `Focus / High`: the memory frequently enters current awareness and significantly influences feelings and behavior.

There is no single correct final layout. `Background` and `Context` can both be reasonable for reframed painful memories, with different benefits and costs. Positive restorative memories may benefit from `Focus`. High attention to the four deep painful memories risks allowing the original pain to regain control even though the new interpretation remains present.

Report language should describe tendencies using phrases such as "may," "is more likely to," or "the current distribution suggests." It should not present a deterministic psychological diagnosis.

### Object-Level Outcome Fragments

| Memory object | Background / Low attention | Context / Medium attention | Focus / High attention |
| --- | --- | --- | --- |
| **Water Bottle** | The client spends less time revisiting the distress of working while ill, reducing the immediate emotional burden. However, they may continue to minimize physical symptoms and delay rest or support. | The client recognizes signs of illness and overwork and is more likely to rest, take medication, or request support without allowing physical discomfort to dominate their attention. | Physical discomfort and the experience of having to continue working remain highly active. The client may repeatedly focus on pain, resentment, and fear of being unable to continue. |
| **Sunset Photograph** | The memory remains available but rarely enters the client's current attention. Work and responsibility may continue to overshadow small restorative experiences. | The client occasionally returns to this peaceful memory and allows themselves to pause without abandoning their responsibilities. | The client actively notices and creates similar restorative moments. Daily life is no longer defined only by work, productivity, and responsibility. |
| **LEGO Bricks** | The satisfaction of completing the model remains accessible but is rarely used to support the client's self-image. External judgment may still have a stronger influence on their sense of ability. | The client can return to this private achievement when facing criticism. It balances external judgment without turning the hobby into another test of performance. | Patient creation and personal judgment receive greater attention. The client's sense of ability becomes less dependent on external recognition, and the hobby becomes a stable source of satisfaction and self-worth. |
| **Second-Place Medal** | The parents' reaction no longer drives constant comparison. However, the achievement itself is also less available as a source of recognition for the client's effort. | The client remembers both the pain and the new understanding. They can value their effort and continue improving without needing first place to prove their worth. | Despite the new understanding, the ranking and the parents' disappointment frequently return to attention. Achievement remains closely tied to proving personal worth. |
| **Old Alarm Clock** | Guilt associated with rest becomes less active. However, the client may be less likely to use this experience as a reminder to notice exhaustion and physical limits early. | The client recognizes physical limits and understands rest as part of responsible self-care rather than an escape from responsibility. | The client repeatedly evaluates whether they are tired enough or have earned the right to rest. Self-care risks becoming another responsibility that must be performed correctly. |
| **Old Phone** | The rejection no longer controls future decisions about asking for help. The client is more willing to approach trusted people without treating their response as a judgment of personal worth. | The client remembers both the risk of rejection and the legitimacy of asking for support. They choose whom to trust carefully, although some hesitation may remain. | The client frequently anticipates rejection. They overexplain, repeatedly edit, or delete requests for help and are more likely to carry pressure alone. |
| **Red Correction Pen** | The humiliation no longer defines the client's ability. They can correct routine mistakes without repeatedly returning to the event, although similar public criticism may still catch them unprepared. | The client uses the experience as a limited reminder to review work, accept consequences, and correct mistakes without treating an error as proof of total incompetence. | The client remains highly alert to mistakes and authority judgment. Repeated checking may develop into perfectionism, delay, and renewed self-doubt. |

### General Interpretation

| Memory type | Background / Low | Context / Medium | Focus / High |
| --- | --- | --- | --- |
| Positive or restorative memories | Remain accessible but may be overshadowed by pressure. | Provide consistent emotional support without dominating attention. | Become active sources of restoration, flow, self-recognition, and personal meaning. |
| Immediate health or warning memories | Cause less distress but may receive insufficient practical attention. | Support proportionate awareness and action. | May lead to repeated worry or excessive monitoring. |
| Reframed painful memories | No longer dominate the client's present life. | Remain available as useful experience and understanding. | Risk allowing the original pain to regain control of current attention. |


## Important Files

Custom scripts are under:

- `Assets/__My Project/scripts/`

Important scripts:

- `GameStateController.cs`
  - Owns the main game state machine.
  - Starts the office intro, crystal ball wait state, Hippocampus transition, Giant Crisis, Swallow Transition, Mirror Chamber intro, and Back To Office return.
  - Also owns coarse scene-root activation for performance on headset builds.
  - Controls Office, Hippocampus, and Nightmare Global Volume weights through state changes.
  - Owns the shared player movement lock entry point used by the clown grab and swallow/fall flow.
  - Fades the assigned Hippocampus Particle System emission rate to 0 when Giant Crisis begins.
  - Plays the assigned heartbeat AudioSource just before the clown crisis sequence starts.

- `CrystalBall.cs`
  - Detects when the player's hand stays near the crystal ball.
  - Drives the white screen fade before transitioning to Hippocampus.
  - Sends configurable XR controller haptic feedback while the player's hand is held on/near the crystal ball.
  - If the player removes their hand before the hold time completes, the fade returns to transparent and the timer resets.

- `ScreenFadeController.cs`
  - Controls fade image alpha and color.
  - Supports setting fade color and alpha directly.
  - Used for both white crystal-ball transition and black Nightmare swallow transition.

- `AssistantController.cs`
  - Controls assistant dialogue lines and stages.
  - Has Nightmare warning dialogue and Swallow Transition dialogue.
  - Has Mirror Chamber intro, Break Glass prompt, Glass Broken Praise, and Return To Office dialogue stages for the final glass sequence.
  - Assistant dialogue should remain independent from the clown crisis coroutine timing.

- `ClownController.cs`
  - Main controller for the Giant Clown / Nightmare crisis sequence.
  - Handles audio order, giant animation trigger, right-arm IK weight, roof grab/drop, player grab/follow, hand-to-mouth movement, and state transition into `SwallowTransition`.
  - Exposes `ReleasePlayerControl()` so the swallow transition can stop hand-following once the screen is fully black.

- `SwallowController.cs`
  - Handles the black screen fade, black hold time, assistant swallow dialogue, teleport to pipe/fall start, fade back in, controlled fall, and transition into Mirror Chamber.

- `PlayerMovementLockController.cs`
  - Shared movement-lock bridge for VR and WebGL.
  - For VR, disables manually assigned movement/teleport `Behaviour` components while leaving turn providers unassigned so turning can remain available.
  - For WebGL, calls `FirstPersonController.SetMovementInputEnabled(false)` so WASD/jump/sprint movement stops while mouse look remains active.

- `MemoryContentDisplay.cs`
  - Memory content no longer displays text or images.
  - It now only plays the currently attached `VideoPlayer` when any memory content show method is triggered.
  - The old public methods are kept so existing UnityEvent bindings in the scene do not break.
  - It prepares a `RawImage` and runtime `RenderTexture` if the scene does not provide a visible video output surface.
  - On WebGL runtime, it switches the `VideoPlayer` to URL mode and loads `Application.streamingAssetsPath + "/video.mp4"` by default, because WebGL does not support playing assigned `VideoClip` assets reliably.

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

9. `BreakGlass`
   - Triggered when the assistant begins the Glass Broken Praise lines.
   - Keeps `DungeonRoot` active.
   - Fades the Nightmare Global Volume weight to 0.

10. `BackToOffice`
   - Intended to be triggered after the Tall Stylized Breakable Glass in the Chamber is broken.
   - Fades the screen to black over 1 second.
   - Activates `OfficeRoot`.
   - Moves player and assistant robot to manually assigned office return spawn transforms while the screen is black.
   - Fades the screen back to transparent over 1 second.

## Scene Root Activation

To reduce headset runtime cost, the main scene is organized into three coarse root GameObjects:

- `OfficeRoot`
  - Contains first-stage office construction, objects, and building/environment content.

- `HippoRoot`
  - Contains Hippocampus-related scene content and the Nightmare/Giant Crisis content that still happens before the dungeon/fall transition.

- `DungeonRoot`
  - Contains the scene content used after the player enters the dungeon/fall sequence, including later dungeon and Mirror Chamber content.

`GameStateController` has Inspector references for these three roots and activates only one root at a time:

- `OfficeIntro` and `AwaitCrystalBall` activate `OfficeRoot`.
- `TransitionToHippocampus`, `Hippocampus`, `AwaitMemoryPlacement`, and `GiantCrisis` activate `HippoRoot`.
- `SwallowTransition` currently does not switch roots directly. This lets the black fade / swallow hand-to-mouth overlap begin while the previous root remains active.
- `MirrorChamber` activates `DungeonRoot`.
- `BackToOffice` activates `OfficeRoot` after the screen has faded to black.

Persistent gameplay objects such as the XR Origin/player, `GameStateController`, screen fade, assistant robot, and global event/input objects should stay outside these three roots so they remain active throughout the experience.

## Global Volume State Control

`GameStateController` exposes Inspector references for the Office, Hippocampus, and Nightmare Global Volumes, plus a shared Global Volume fade duration.

- On startup and when entering `OfficeIntro`, Office Global Volume is weight 1 while Hippocampus and Nightmare are weight 0.
- During `TransitionToHippocampus`, once the screen is fully white, Office Global Volume is set to weight 0 and Hippocampus Global Volume is set to weight 1 before the fade back in.
- Entering `GiantCrisis` first fades the Hippocampus Global Volume weight to 0, then activates the Nightmare Global Volume and fades its weight to 1.
- The Giant Crisis sequence starts after the Nightmare Global Volume fade-in completes.
- Entering `BreakGlass` fades the Nightmare Global Volume weight to 0.
- During `BackToOffice`, once the screen is fully black, Office Global Volume is restored to weight 1 while Hippocampus and Nightmare are set to weight 0.

## Crisis Particle State Control

`GameStateController` exposes an optional Hippocampus Particle System reference and a particle fade duration.

- Entering `Hippocampus` restores that particle system's cached emission rate and plays it if needed.
- Entering `GiantCrisis` fades the particle system's Rate over Time from its current value to 0.
- The fade stops new particle emission with `StopEmitting`, allowing existing particles to expire naturally instead of clearing instantly.

## Crisis Audio State Control

`GameStateController` exposes an optional heartbeat AudioSource.

- Entering `GiantCrisis` plays the heartbeat after the Hippocampus and Nightmare Global Volume transition completes, immediately before `ClownController.StartCrisisSequence()`.

## Crystal Ball Transition

Target behavior:

- Player places hand on the crystal ball.
- Screen fades to white over 2 seconds.
- If the hand leaves before 2 seconds, fade returns to transparent and timer resets.
- Once complete, player transfers to Hippocampus.

Current implementation:

- `CrystalBall` drives white fade progress while the hand is in range.
- While either hand is in range, `CrystalBall` sends repeated haptic impulses to the matching left/right XR controller using configurable amplitude, duration, and interval values.
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
   - When the reach begins, `ClownController` asks `GameStateController` to lock player movement input.
   - The IK target samples `playerHead.position` every frame, so if the player moves during the grab attempt the hand keeps chasing the player's current position.
   - Completion is based on `grabAnchor` / grab reference distance to `playerHead`.
   - This prevents the script from continuing just because `handIKTarget` reached the player while the visible hand/grab point has not.

11. Attach player to hand.
   - Player is not parented to the hand.
   - On the grab frame, the player's tracked head is immediately aligned to `grabAnchor`.
   - During follow, `LateUpdate()` moves `xrOrigin.position` by the delta between `grabAnchor.position` and the current `playerHead.position`, so headset movement does not create a growing offset from the hand.
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

16. Set game state to `SwallowTransition`.
   - This now happens before the hand finishes moving to `mouthPoint`.
   - `SwallowController` begins its black fade while the clown hand can still continue the mouth movement.
   - `GameStateController` does not switch to `DungeonRoot` in `SwallowTransition`; the root switch happens later when `SwallowController` sets the state to `MirrorChamber`.

17. Move hand IK target toward `mouthPoint`.
   - `mouthPoint` may be attached to an animated head/mouth bone, so movement samples the target transform continuously.

18. Wait until visible hand reference reaches mouth.
   - Uses `handTipReference` if assigned, otherwise `grabAnchor`, otherwise `handIKTarget`.
   - `mouthArrivalTimeout` is a safety timeout so the sequence can continue with a warning if the hand never reaches the mouth.

19. Detach player from hand following.

## Swallow Transition

Target behavior:

- Nightmare mouth transition uses black screen fade.
- Fade to black over about 1 second.
- Once fully black, stay black for a configurable hold time.
- During the full-black hold, assistant enters Swallow Transition dialogue and can output text/audio.
- Then player is transferred to the pipe/fall start point.
- `SwallowController` switches the game state to `MirrorChamber` while the screen is still black.
- `MirrorChamber` activates `DungeonRoot`, moves the assistant to the Mirror Chamber spawn point, and starts Mirror Chamber intro.
- Fade returns to transparent and controlled fall begins after the state has already switched to `MirrorChamber`.

Current implementation:

- `SwallowController` has:
  - `fadeOutDuration`
  - `blackHoldDuration`
  - `fadeInDuration`
  - `swallowFadeColor`
- `swallowFadeColor` is black.
- Assistant `PlaySwallowTransition()` is called during the black hold period.
- Once the screen is fully black, `SwallowController` asks `GameStateController` to release the clown's player-follow control before teleporting the XR Origin to `pipeStartPoint`.
- `GameStateController.SetState(MirrorChamber)` is called after the player is teleported to `pipeStartPoint`, before the fade-in and before `ControlledFall()`.
- At the end of `ControlledFall()`, `SwallowController` asks `GameStateController` to unlock player movement input.

## Player Movement Locking

`GameStateController` exposes a `PlayerMovementLockController` reference.

- `ClownController.ReachTowardPlayer()` calls `GameStateController.SetPlayerMovementLocked(true)` once the grab reach starts.
- `SwallowController.ControlledFall()` calls `GameStateController.SetPlayerMovementLocked(false)` after the player reaches the stomach landing point.
- The lock controller caches each assigned component's previous state before locking and restores that cached state when unlocking.

VR setup:

- Add `PlayerMovementLockController` to a persistent object in the VR scene, such as the same object that hosts `GameStateController` or another always-active manager object.
- Drag that component into `GameStateController.playerMovementLockController`.
- In `PlayerMovementLockController.disableWhenMovementLocked`, drag movement and teleport components only, such as `DynamicMoveProvider`, `ContinuousMoveProvider`, teleport providers/interactors/rays, or other movement-only scripts.
- Do not drag turn providers such as `SnapTurnProvider` or `ContinuousTurnProvider` if turning should remain enabled during the grab.

WebGL setup:

- Add `PlayerMovementLockController` to a persistent object in the WebGL scene.
- Drag that component into `GameStateController.playerMovementLockController`.
- In `PlayerMovementLockController.firstPersonControllers`, drag the WebGL player's `FirstPersonController`.
- `FirstPersonController` now exposes `SetMovementInputEnabled(bool)`. When movement is locked it clears WASD movement, jump, sprint, speed, and vertical velocity, but still allows mouse look through `LateUpdate()`.

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
  - Safety timeout for hand-to-mouth arrival.

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
- Player follows the hand by position only, via `xrOrigin.position`; while grabbed, the tracked head is kept aligned to `grabAnchor` without parenting or forced rotation.
- Roof and player share the same `grabAnchor` concept to keep the hand/roof/player relationship consistent.
- Grab completion is determined by the grab reference reaching the player, not by `handIKTarget` alone.
- Grab-to-player has no timeout now; the giant hand must reach the player before the sequence continues.
- Mouth arrival waits on `WaitForHandNear(...)`; if the hand never reaches the mouth, `mouthArrivalTimeout` lets the sequence continue with a warning instead of getting stuck forever.
- Assistant dialogue is independent and should not block the clown crisis coroutine unless explicitly requested.
- Footsteps and rumble are sequential: footsteps finish first, rumble starts afterward.
- `SwallowTransition` starts before the hand-to-mouth movement is complete, allowing the fade to black to overlap with the final mouth movement for a smoother visual transition.
- Memory content display is video-only now; it should play its assigned/current `VideoPlayer` instead of controlling text or image UI.
- A `VideoPlayer` under a world-space Canvas still needs a visible output surface. `MemoryContentDisplay` now creates/uses a `RawImage` plus a runtime `RenderTexture` so video can render on the Canvas.
- WebGL video playback uses StreamingAssets URL loading. Keep the playable WebGL file at `Assets/StreamingAssets/video.mp4` unless `MemoryContentDisplay.webGLStreamingAssetsVideoFileName` is changed.
- Breaking the final Chamber glass should eventually call `GameStateController.SetState(GameState.BackToOffice)`. The return spawn transforms are exposed on `GameStateController` for manual scene assignment.
- Current final Chamber glass flow:
  - `MirrorChamber` plays the assistant Mirror Chamber intro.
  - After the intro completes, `AssistantController` plays the Break Glass prompt.
  - The scene instance named `Tall Stylized Breakable Glass` has `notifyGameStateControllerOnShatter` enabled.
  - The same scene instance also has `shatterOnAllowedContact` enabled, so any collider on its allowed contact/impact mask can force shatter on contact instead of needing to satisfy the normal impact-speed thresholds.
  - When `StylizedBreakableGlass.Shatter()` runs, it invokes `GameStateController.HandleFinalChamberGlassShattered()`.
  - The controller asks the assistant to play the Glass Broken Praise line(s), then the Return To Office line(s).
  - Only after those lines finish does the assistant call `GameStateController.SetState(GameState.BackToOffice)`.
  - `BackToOffice` now runs a black screen transition before moving the player and assistant back to the office.
  - `StylizedBreakableGlass` also exposes `onShattered` for optional UnityEvent bindings.
  - `StylizedBreakableGlass` exposes `ForceShatter()` for explicit script or UnityEvent driven shatter without requiring a minimum impact speed.

## Known Risks / Things to Check in Unity

- If `footstepsAudio` is looping, the crisis coroutine will wait forever before rumble/animation starts.
- If `handIKTarget` or `playerHead` is missing, the grab stage now stops and logs an error instead of continuing.
- If `grabAnchor` is placed poorly on the hand skeleton, the player/roof may appear offset even if script logic is correct.
- If `mouthPoint`, `handIKTarget`, `handTipReference`, or `grabAnchor` are poorly placed, the mouth arrival wait may last until `mouthArrivalTimeout` and then continue with a warning.
- During `SwallowTransition`, `SwallowController` releases the clown's player-follow control once the screen is fully black and before moving the XR Origin to the pipe start point.
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
