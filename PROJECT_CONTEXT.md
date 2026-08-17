# Project Context for Future Codex Sessions

Last updated: 2026-08-16

This document is intended as the first file to read in future Codex sessions for this Unity project. It records the current understanding of the project, the gameplay flow, the key scripts, and the design decisions made so far.

## Project Summary

This is a Unity VR narrative/interactive experience built on top of the EZPZ Interaction Toolkit and Unity XR Interaction Toolkit samples.

The current custom game appears to be a memory/brain-themed VR experience. The player starts in an office-like space, interacts with a crystal ball, transitions into the Hippocampus area, places or reviews memory-related content, and eventually enters a Nightmare/Giant Clown crisis sequence. The Nightmare sequence includes a giant clown approaching, manipulating the roof, grabbing the player, moving the player toward the clown's mouth, then transitioning into a swallow/fall sequence and finally into a Mirror Chamber.

The current development focus (as of 2026-08-16) is end-to-end WebGL validation and presentation polish. The main narrative spine is now implemented through the final report:

- **Stage 1 (Office)**: Branching dialogue, Start Work, gated report reading, crystal-ball instructions, and Office → Hippocampus transition are implemented and bound in `scene_WebGL.unity`. Start Work lines finish before the computer screen and Finish Reading button appear; the crystal ball appears only after Finish Reading is selected. Office uses Skybox environment lighting, while every non-Office scene root uses Gradient environment lighting.
- **Stage 2 (Initial Memory Room)**: Three surface memories and the Focus / Context / Background zones are connected. The second placed object triggers the missing-painful-memories cue; the third placed object may interrupt that cue and automatically starts the Giant Crisis. Confirm has no effect during this preliminary stage.
- **Stage 3 (Giant Interior)**: Four two-round mirror conversations, one-second-gap idle/failure audio loops, mirror completion events, the first released-memory response, the all-four counter, and the return to the Memory Room are connected. Return points, Nightmare Volume fade-out, Giant Clown hiding, restored-object Holdable activation/gravity, and the replacement Skybox are bound.
- **Stage 4 (Final Redistribution)**: Implemented using the same `MemoryPlacementController`. The original three positions are preserved across root deactivation, all seven object references are configured, incomplete Confirm feedback remains active, and successful Confirm plays the assistant response before returning to the Office.
- **Stage 5 (Office Report and Ending)**: The Office return transition, return/report dialogue, page-1 `View Report` button, dynamic ItemCycler report page, placement summary, 21 concise object-level outcome fragments, and Close Session ending are implemented. Each playthrough displays one of three outcomes per object; Close Session plays neutral closing lines and then ends on a black screen.

The Nightmare/Giant Clown crisis sequence (roof grab, player grab, swallow transition) is implemented but still needs full-flow timing and visual tuning. The remaining priority is WebGL Play Mode testing from Office through the report, plus final Canvas readability and presentation polish.

## Narrative Design and Current Implementation

This section records the approved direction for the final assignment version. The core five-stage flow is now implemented in `scene_WebGL.unity`; details explicitly marked TBD or described as optional polish are still design targets rather than completed behavior.

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

### Full Game Flow

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
7. Choosing to start plays the Start Work lines while the computer content and crystal ball remain hidden.
8. When the Start Work lines finish, the office computer loads the assigned client's information and the Finish Reading button appears:
   - Client name and case ID.
   - `Session 01 / Initial Memory Organization` and first-visit status.
   - `Sleeping / Connection Available` and the current sleep phase, such as `REM Sleep`.
   - Heart rate, respiration, temperature, and overall physical stability.
   - A concise initial assessment and client statement.
9. The player reviews the report and selects Finish Reading. Only then does the crystal ball appear and the assistant play the crystal-ball instruction lines.
10. The player activates the crystal ball.
11. The existing white fade and teleport transition move the player and assistant to the Memory Room.

The Office choice flow supports these routes and any repeated exploration variants:

- Job explanation -> start.
- Job explanation -> explore -> start.
- Explore -> start.
- Explore -> job explanation -> start.
- Explore -> job explanation -> explore -> start.

Current state flags:

- `HasHeardJobExplanation`
- `IsReadyToStart`
- Internal `isExploring` and `waitingForReadingConfirmation` flags cover temporary Office states; no persistent `HasExploredOffice` flag is needed.

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
6. After viewing a memory, the player places the object in one of the three categories. Existing placements may be revised until the third distinct object is placed and the crisis begins.
7. No visible score, mood value, correct-answer color, or failure label appears.
8. After the second object is placed, the assistant notices that something is missing:

   > "Strange... these all seem to be memories that only recently surfaced. Where are the more painful memories that have been receiving so much attention?"

9. A low sound, brief light fluctuation, or subtle room vibration hints that the missing memories have collected elsewhere.
10. Placing the third object can interrupt the missing-memory cue; the interrupted cue is treated as complete so progression cannot stall.
11. As soon as all three objects are placed, the deeper painful memories are disturbed and the Giant Crisis starts automatically. Preliminary Confirm is intentionally ignored.
12. The Giant Clown grabs and swallows the player and assistant.

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
9. Every second round contains two options:
   - One constructive response that successfully completes the memory.
   - One harmful response that reinforces the same negative belief as the first-round harmful shortcut.
10. Selecting the second-round harmful response uses the same agitated client line and failure behavior as the first-round harmful response. The environment intensifies, the dialogue closes, and `Start Conversation` returns.
11. Selecting the constructive second-round response lets the client form a more flexible interpretation. **The mirror auto-shatters** — the painful echo dissipates, the container cracks open on its own, and the original memory object (medal, clock, phone, or pen) is released. No player action is needed to break the mirror.
12. After all four mirrors have shattered and their memory objects are freed, the echoes no longer sustain the Giant Clown. The giant space begins to dissolve.
13. With the interference gone, the assistant's teleport signal is restored. The assistant locks onto the Memory Room coordinates and actively teleports the player, themselves, and the four freed memory objects back to the Memory Room.

Dialogue failures have immediate local consequences only. They are not included in the final office report and do not permanently block progress.

#### Stage 4: Final Attention Redistribution

1. The player and assistant return to the Memory Room. The ceiling shows damage from the giant's emergence (the assistant notes it is outside their job description).
2. The three recent objects and four restored deep-memory objects are now present together.
3. The player has enough context to reconsider all seven memories.
4. The player places every object in `Focus`, `Context`, or `Background`.
5. No real memory can be deleted.
6. The player can revise the layout until confirming the final distribution.
7. Upon confirmation, the assistant response plays and the black fade transition returns the player to the Office.
8. The confirmed positions are stored for the office report.

#### Stage 5: Office Report and Ending

> The Office Return and Report lines and a neutral two-line Close Session ending are implemented and have audio clips assigned. The closing lines note that the client is still asleep and may notice a change after waking; they deliberately leave the exact result open. A separate personal client message remains optional/TBD.

1. After final placement confirmation and the assistant's confirmation response, the player and assistant fade back to the Office. The subconscious connection closes.
2. The computer returns to page 1 with a `View Report` button visible; after the fade-in, the assistant plays the Office Return and Report lines.
3. The player selects `View Report` to open the dynamically registered final report page.
4. The report reads only the seven confirmed final positions. Each object has a `Background`, `Context`, or `Focus` outcome, producing `3^7 = 2187` possible attention layouts.
5. The implementation does not require 2187 separately authored reports. It selects one of three concise outcome fragments for each of the seven objects, for a total of 21 authored object-level fragments.
6. The implemented report displays the processed count, Focus / Context / Background totals, each memory's final attention label, and one position-specific finding per memory.
7. A qualitative overall synthesis and personal client message remain optional/TBD and are not part of the current report implementation.
8. The former Back button is relabeled `Close Session`. Selecting it immediately disables the button and plays the assistant's neutral closing lines while movement remains available.
9. When those lines finish, the existing transition Canvas fades to black. Only after the fade is fully opaque is movement locked; all three scene roots are then disabled and the game remains in the terminal `SessionComplete` state. WebGL does not call `Application.Quit()`.

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

This same harmful response and reset behavior are used if the harmful option is selected during either second-round branch.

**If choice 1 was selected:**

Client:

> "But if I am satisfied with second place, does that mean I have no ambition?"

Second-round choices:

- "Recognizing what you have achieved does not prevent you from continuing to grow." *(Constructive; succeeds)*
- "Maybe if you stop pushing yourself to be first, you really will lose your ambition." *(Harmful; triggers the shared harmful response and exits)*

**If choice 2 was selected:**

Client:

> "They looked so disappointed. It is hard to believe second place really had value."

Second-round choices:

- "Like your LEGO model, gradual progress can deserve recognition before everything is perfect." *(Constructive; succeeds)*
- "If they were disappointed, perhaps only first place can make the achievement valuable." *(Harmful; triggers the shared harmful response and exits)*

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

This same harmful response and reset behavior are used if the harmful option is selected during either second-round branch.

**If choice 1 was selected:**

Client:

> "But if I rest, the work may fall on someone else."

Second-round choices:

- "Caring for your body is also part of being able to meet future responsibilities." *(Constructive; succeeds)*
- "To avoid leaving work to other people, you should keep going even when you are exhausted." *(Harmful; triggers the shared harmful response and exits)*

**If choice 2 was selected:**

Client:

> "Every time I stop, I feel guilty, as if I have done something wrong."

Second-round choices:

- "You once stopped to watch the sunset. Those few minutes did not harm anyone." *(Constructive; succeeds)*
- "If stopping makes you feel guilty, finish everything before you allow yourself to rest." *(Harmful; triggers the shared harmful response and exits)*

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

This same harmful response and reset behavior are used if the harmful option is selected during either second-round branch.

**If choice 1 was selected:**

Client:

> "What if I ask again and I am rejected again?"

Second-round choices:

- "You can choose people you trust. One refusal cannot decide whether you deserve support." *(Constructive; succeeds)*
- "To avoid being rejected again, it may be safest never to ask for help." *(Harmful; triggers the shared harmful response and exits)*

**If choice 2 was selected:**

Client:

> "Needing help still makes me feel unreliable."

Second-round choices:

- "Being reliable does not mean doing everything alone. It includes knowing when collaboration is needed." *(Constructive; succeeds)*
- "Reliable people should solve their own problems without needing anyone else's help." *(Harmful; triggers the shared harmful response and exits)*

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

This same harmful response and reset behavior are used if the harmful option is selected during either second-round branch.

**If choice 1 was selected:**

Client:

> "Every time I submit something now, I still remember that red pen."

Second-round choices:

- "You can take your work seriously while accepting that mistakes remain possible." *(Constructive; succeeds)*
- "Use that anxiety as a warning and keep checking until you can guarantee there are no mistakes." *(Harmful; triggers the shared harmful response and exits)*

**If choice 2 was selected:**

Client:

> "But what if the next mistake has real consequences?"

Second-round choices:

- "Taking responsibility and correcting a consequence is different from denying your entire ability." *(Constructive; succeeds)*
- "If mistakes can have real consequences, you must make sure that you never make another one." *(Harmful; triggers the shared harmful response and exits)*

**Resolution line:**

> "I can check carefully and correct mistakes without using perfect performance to prove my ability."

The mirror becomes the red correction pen.

## Office Outcome Report

### Outcome Model

The previous three-template ending calculation is obsolete. The implemented report uses compositional object-level feedback.

- The final report reads only the seven confirmed Memory Room positions.
- Each object has a `Background`, `Context`, and `Focus` feedback fragment.
- Seven objects multiplied by three positions require 21 authored fragments.
- The seven ternary positions produce `3^7 = 2187` possible final layouts.
- The report selects seven concise fragments and combines them with the final Focus / Context / Background counts.
- Dialogue failures in the Giant Interior and the preliminary three-object layout do not affect this report.

Implementation status (2026-08-16):

- `FinalReportController` contains all 21 concise English fragments and selects exactly one fragment for each final memory placement.
- The current WebGL scene has seven distinct final required items with matching IDs: `bottle`, `sunsetPhoto`, `legoBricks`, `pen`, `clock`, `phone`, and `medal`.
- `OutcomeText` is one TMP block rather than three variable-length columns, preventing a single attention category from overflowing its own narrow column.
- The report uses colored attention labels but deliberately does not assign success/failure colors or scores.
- The long-form table below remains the narrative source. Runtime text is a condensed version designed to fit the in-world computer screen.
- The 2026-08-16 cleanup removed unused Office/placement state APIs, the unused WebGL Crystal Ball fade-reset coroutine, duplicate mirror-conversation ending coroutines, and the no-op multi-slot Volume coroutine layer. Inspector-bound fields and UnityEvent entry points were preserved.

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
  - Starts the office intro (routed through `OfficeDialogueController`), crystal ball wait state, Hippocampus transition, Giant Crisis, Swallow Transition, Mirror Chamber intro, final memory placement, and Back To Office return.
  - Also owns coarse scene-root activation for performance on headset builds.
  - Controls Office, Hippocampus, and Nightmare Global Volume weights through state changes.
  - Switches Environment Lighting Source with the active scene root: Office uses `AmbientMode.Skybox`; Hippo, Dungeon, and no-active-root states use `AmbientMode.Trilight` (the Inspector's Gradient option). The Office Skybox material is cached on startup and restored after the Memory Room replacement Skybox has been used.
  - Owns the shared player movement lock entry point used by the clown grab and swallow/fall flow.
  - Fades the assigned Hippocampus Particle System emission rate to 0 when Giant Crisis begins.
  - Plays the assigned heartbeat AudioSource just before the clown crisis sequence starts.
  - Handles the first released-memory response and the all-four return sequence.
  - On return to the Memory Room, hides the Giant Clown, changes the Skybox, fades Nightmare Volume weight to 0, restores the four deep memories at explicit return points, enables their Holdable components, and restores gravity.
  - After final Confirm, waits for the assistant confirmation response, fades back to Office, restores the Office Volume, prepares the final report, and plays the Office Return and Report lines.

- `OfficeDialogueController.cs`
  - Controls the office-stage branching dialogue and choice system.
  - Uses five named references (not a dynamic array): the three initial choice buttons, `finishReadingButton`, and `computerScreenContent`.
  - Button text is pre-set in the hierarchy — code only controls visibility, never changes labels.
  - Implements the Planned Design flow: greetings → choices → job explanation / explore / start work.
  - `exploreOfficeButton` serves double duty as both initial exploration and re-exploration (merged with the old `waitALittleButton`).
  - `IsJobExplanationAvailable` only requires `!HasHeardJobExplanation` — exploring no longer hides the "Learn about the role" button.
  - `SelectStartWork()` hides all buttons and plays start-work dialogue. When that dialogue completes, `computerScreenContent` is activated and `finishReadingButton` becomes visible. `FinishReading()` is the only path that then enters `AwaitCrystalBall` and starts the crystal-ball instruction dialogue.
  - The computer screen and reading-confirmation UI are local Office dialogue concerns. The Start Work and Finish Reading buttons should have empty persistent `Button.onClick` lists because `OfficeDialogueController.Awake()` registers both callbacks in code.

- `CrystalBall.cs` (VR)
  - Detects when the player's hand stays near the crystal ball.
  - Drives the white screen fade before transitioning to Hippocampus.
  - Sends configurable XR controller haptic feedback while the player's hand is held on/near the crystal ball.
  - If the player removes their hand before the hold time completes, the fade returns to transparent and the timer resets.

- `CrystalBall_WebGL.cs` (WebGL)
  - Simpler click-to-transition crystal ball for WebGL (no hand-tracking / haptic logic).
  - Public `Transition()` method starts a white fade and then calls `SetState(TransitionToHippocampus)`.
  - Implements `ICrystalBallEntry` so `GameStateController.SetCrystalBallEnabled()` works identically to VR.
  - Crystal-ball availability remains owned by `GameStateController`: entering `AwaitCrystalBall` calls `SetEnabled(true)`, which activates the WebGL crystal-ball GameObject; leaving that state disables and hides it.

- `ScreenFadeController.cs`
  - Controls fade image alpha and color.
  - Supports setting fade color and alpha directly.
  - Used for both white crystal-ball transition and black Nightmare swallow transition.

- `AssistantController.cs`
  - Controls assistant dialogue lines and stages.
  - **Implemented dialogue arrays**: Hippocampus intro, per-object memory reveal (water bottle / sunset photo / LEGO bricks, each with a one-shot guard), missing painful memories, Nightmare warning, Swallow Transition, Mirror Chamber intro, `memoryReleasedLines`, `allMemoriesReleasedLines`, `returnToMemorySpaceLines`, final confirmation response, and office return/report review.
  - The four greeting clips preload their audio data to prevent WebGL first-use decoding gaps between opening lines. `greeting1.mp3` itself contains approximately 0.34 seconds of trailing silence; no extra greeting delay is added by the dialogue coroutine.
  - `swallowTransitionLines` is populated in `scene_WebGL.unity` with the "OH NO NO NO" subtitle and an assigned audio clip.
  - `sessionClosingLines` contains two neutral fallback-text lines; audio clips must be assigned in the Inspector. `PlaySessionClosing()` invokes the supplied callback after both lines complete.
  - **Optional Stage 5 work**: a separate personal client message or outcome-specific synthesis is still TBD.
  - The old `breakGlassLines` and `glassBrokenPraiseLines` arrays are deprecated — the player no longer manually breaks a glass wall. Each mirror auto-shatters when its conversation resolves successfully.

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

- `MemoryPlacementItem.cs`
  - Marks the root of a memory object that can be placed in an attention zone.
  - Exposes `memoryId` for report lookup.
  - Saves parent/local transform and Rigidbody state before the original Memory Room root is disabled, then restores the surface memory to the same assigned position on return.

- `MemoryPlacementZone.cs`
  - Represents a `Focus`, `Context`, or `Background` trigger zone.
  - Finds `MemoryPlacementItem` on the entering collider or its parents and reports placement changes to `MemoryPlacementController`.
  - Tracks collider counts per item so a memory with multiple colliders is not counted more than once inside the same zone.

- `MemoryPlacementController.cs`
  - Owns separate initial (three-object) and final (seven-object) required-item lists.
  - Tracks each required item's current zone by object reference rather than using the old EZPZ `NumberHolder` / `NumberCheckUtillity` counter approach.
  - Plays the missing-painful-memories Assistant cue once two required objects are in valid zones.
  - Automatically calls `ClownController.TriggerCrisis()` once all three initial objects are in valid zones; preliminary Confirm is no longer required.
  - Picking up the third object may interrupt the missing-memory cue; interruption counts as completing that cue so automatic crisis progression cannot stall.
  - Saves the three initial zone assignments and object transforms for the return to the Memory Room.
  - `BeginFinalPlacement()` changes the same controller to its final phase, restores the initial three assignments, and waits for all seven objects.
  - During the final phase, an incomplete Confirm only shows `incompleteMessage`; completion is accepted once all seven unique objects have a valid zone.
  - Successful final Confirm calls `GameStateController.HandleFinalPlacementConfirmed()`.
  - Exposes `FinalRequiredItems` and `TryGetFinalPlacementZone()` for report generation.

- `FinalReportController.cs`
  - Lives on the Office `PC` and is connected to its existing `ItemCycler`.
  - Keeps the page-1 `View Report` button hidden until the final return to Office.
  - Dynamically appends `FinalReportPage` to `ItemCycler.items`; the page should not be added manually to the original list.
  - `PrepareReportReady()` enables the screen, builds the report, shows the button, and returns the computer to page index 0.
  - `ShowReport()` selects the already-built final report page. Final placement is immutable after confirmation, so rebuilding on every click is unnecessary.
  - The former Back reference migrates to `closeSessionButton`. Clicking it disables repeat input and invokes the close-session callback supplied by `GameStateController`.
  - Generates a processed-count/distribution summary and seven object outcomes from the final placement dictionary.
  - Contains 21 concise English fragments: Background, Context, and Focus variants for each of the seven memory IDs.
  - Uses one `OutcomeText` TMP block with rich-text colored attention labels.

- **`MemoryDialogueController.cs`** (NEW — 2026-08-09)
  - Implements the two-round mirror conversation system for Stage 3 (Giant Interior / Deep Memory Crisis).
  - Data-driven: all dialogue text and audio clips are configured through serialized fields in the Inspector.
  - While an unresolved deep-memory controller is active, its assigned `openingClip` repeats as the memory's ambient internal voice with a configurable `openingLoopInterval` (currently one second) between plays. Selecting `Start Conversation` leaves that loop playing while the first choices are visible. The loop stops only when the player selects a choice and its next-round or failure audio begins. After any failed Round 1 or Round 2 response finishes, closing the dialogue restores the same interval-based loop. Successful completion and GameObject deactivation stop it permanently.
  - Conversation flow:
    1. Player enters the mirror's trigger area — a `Start Conversation` button appears.
    2. `BeginConversation()` displays the opening text and shows three Round 1 choices while the opening audio loop continues.
    3. Round 1: three choices —
       - Two constructive responses that enter Round 2 (`goesToRound2 = true`, `nextBranchIndex` selects which Round 2 branch).
       - One harmful shortcut (`goesToRound2 = false`) that plays a rejection response, briefly intensifies the environment, then returns to idle.
    4. Round 2: two choices per branch —
       - One constructive response (`completesMemory = true`) that resolves the mirror successfully.
       - One harmful response (`completesMemory = false`) that triggers the same rejection behavior as the Round 1 shortcut.
    5. Completing the conversation invokes `onComplete` (a `UnityEvent`) — used to deactivate the mirror and return the memory object.
    6. Harmful exits call `CloseConversation()` — the `Start Conversation` button reappears.
  - Exposes `SetPlayerInArea(bool)` — call from trigger enter/exit to control button visibility.
  - Exposes `SelectChoice(int)` — called by `MemoryDialogueChoiceRelay` from UI buttons.
  - Normalizes array sizes to 3 (Round 1) and 2 (Round 2 branches × 2 choices) via `NormalizeData()` in `Awake()` and `OnValidate()`, so the arrays auto-size correctly in the Inspector.
  - UI references: `startButtonRoot`, `dialogueRoot`, `round1ChoicesRoot`, `dialogueText` (TMP), `dialogueAudioSource`.
  - Per-branch UI: each Round 2 branch has `choicesRoot` — a unique `GameObject` parent for its two choice buttons.
  - Design rationale: two rounds with harmful shortcuts in *both* rounds (not just Round 1), as documented in the narrative design section. This gives the player more opportunities to choose the wrong path and see the immediate feedback.

- **`MemoryDialogueChoiceRelay.cs`** (NEW — 2026-08-09)
  - Lightweight bridge: a UI button's `onClick` → `MemoryDialogueChoiceRelay.TriggerChoice()` → `MemoryDialogueController.SelectChoice(choiceIndex)`.
  - Each relay carries a `choiceIndex` (0–2 for Round 1, 0–1 for Round 2).
  - Auto-finds the parent `MemoryDialogueController` if not manually assigned.

- **`MemoryContentDisplay.cs` — DELETED (2026-08-05).**
  - Its runtime RawImage/RenderTexture creation and WebGL URL switching are no longer needed.
  - The new approach: pre-configure `Image` or `RawImage` + `VideoPlayer` directly on the Canvas in the hierarchy, and use Holdable UnityEvents to toggle visibility.

Main scene:

- `Assets/__My Project/scene.unity`

WebGL scene:

- `Assets/__My Project/scene_WebGL.unity`

## Imported Model Assets (2026-08-05 to 2026-08-09)

New 3D models imported for the deep-memory objects (Stage 3) and the surface-memory content:

### Deep Memory Objects (Stage 3)

- **Second-Place Medal**: `Assets/__My Project/model/Ace Combat 7 Medals ACE/`
  - FBX + silver-medal texture (PNG).
  - Represents the "second-place medal" deep memory.
  - Still needs prefab creation and scene placement as a mirror-linked object.

- **Old Alarm Clock**: `Assets/__My Project/model/clock/`
  - FBX + Albedo / Normal / MetallicSmoothness / AO textures (PNG).
  - Represents the "old alarm clock" deep memory.
  - Still needs prefab creation, scene placement, and looping-hand animation.

- **Old Phone**: `Assets/__My Project/model/cell-phone/`
  - FBX + diffuse / AO textures (JPG).
  - Represents the "old phone with cracked screen" deep memory.
  - Still needs cracked-screen variant and prefab creation.

- **Red Correction Pen**: `Assets/Detailed ballpoint pens/`
  - 5 color prefabs (black, blue, green, red, white).
  - Includes `AudioOnCollision` script for sound effects (cap on/off, hit).
  - Red pen represents the "red correction pen" deep memory.
  - Also useful as a generic office prop.

### Surface Memory Content (Stage 2)

- **Medicine**: `Assets/__My Project/media/medicine.mp4` + `medicine.png`
  - Video + thumbnail for the water-bottle memory's visual content.

- **Sunset Photograph**: `Assets/__My Project/media/sunset photo.png` + `sunset.png`
  - Two sunset images for the sunset-photograph memory's visual content.

- **Brick Toy**: `Assets/__My Project/media/brick toy.png`
  - Image for the LEGO-bricks memory's visual content.

### Breakable Containers Asset Pack

- `Assets/Breakable Containers/`
  - Comprehensive package with Normal and Cracked variants of: Clay Vase, Clay Vase Tall, Clay Vase Cubic, Clay Jar, Clay Kettle, Jar (BlueRed), Jar (BrownPurple), Vase (WhiteBrown), Cauldron, Flower Pot.
  - Includes demo scene (`Demo Scene/Demo.unity`).
  - Likely imported for the auto-shatter visual effect on resolved mirrors in the Mirror Chamber.
  - Not yet integrated into the main game flow.

## Additional Package Imports

- **XR Hands (1.8.0)**: HandVisualizer sample + Hands Interaction Demo (3.4.1) with hand tracking, poke interaction, affordances, and shader graphs.
- **BrickToy 3D LowPoly WarriorRobots**: Low-poly robot prefabs (may be for the LEGO-bricks memory or future use).

## WebGL Memory Placement and Confirmation

Current `scene_WebGL.unity` setup:

- The water bottle, sunset photograph, and LEGO bricks are assigned to `initialRequiredItems`.
- `finalRequiredItems` contains exactly seven distinct `MemoryPlacementItem` references in this order: water bottle, sunset photograph, LEGO bricks, red correction pen, old alarm clock, old phone, and second-place medal.
- The three attention areas each have `MemoryPlacementZone` connected to the same controller:
  - `Focus` uses `MemoryPlacementZoneType.Focus`.
  - `Context` uses `MemoryPlacementZoneType.Context`.
  - `Background` uses `MemoryPlacementZoneType.Background`.
- The existing EZPZ Confirm button calls `MemoryPlacementController.ConfirmPlacement()`.
- During the initial phase that method immediately returns, so pressing Confirm has no effect.
- The initial crisis is driven by placement-zone occupancy: two unique objects trigger the missing-memory cue and all three trigger the crisis.
- During the final phase, the same Confirm binding checks all seven objects. Incomplete placement displays the existing English `incompleteMessage`; complete placement starts the final assistant response and Office transition.
- `ClownController.TriggerCrisis()` guards against duplicate activation and changes the game state to `GiantCrisis`.
- `MemoryPlacementZone` tracks collider counts per object, so multi-collider memories do not exit a zone until their final collider leaves.
- A disabled Holdable is ignored by the modified EZPZ `RaycastInteractor`; the four deep memories cannot be picked up inside the Giant while their Holdable components are disabled.
- On return, the four deep memories are placed at four assigned return points, set non-kinematic with gravity enabled, and have Holdable enabled.
- The three surface-memory transforms are captured before `HippoRoot` is disabled, reparented under the configured memory-room root, and restored at their player-assigned positions.

This setup is connected in the scene, but the full interaction must still be verified in WebGL Play Mode: enter and leave each zone, move an item between zones, confirm the second placement starts the missing-memory cue, pick up/place the third distinct object while that cue is playing, confirm the crisis begins exactly once, verify all seven restored items can be reassigned, and test both incomplete and complete final Confirm.

Deferred Stage 2 detail:

- After the second surface-memory object is placed, the assistant should notice that the painful memories are missing.
- A supporting light fluctuation, low sound, or subtle room vibration could still reinforce that those memories have collected elsewhere.
- The missing-memory dialogue cue is implemented; environmental reinforcement remains optional polish.

## Final Redistribution and Office Report

Current implemented sequence:

1. The DungeonRoot counter reaches four and invokes `GameStateController.HandleAllMemoriesResolved()`.
2. The Nightmare Global Volume begins fading toward weight 0 while the assistant plays the all-memories-released sequence.
3. The screen fades out; the Giant Clown is hidden; `HippoRoot` is activated; the return Skybox and Hippocampus Volume are applied.
4. Player and assistant return to their Memory Room spawn points. The four deep memories move to the four assigned return points and become holdable with gravity.
5. `FinalMemoryPlacement` calls `MemoryPlacementController.BeginFinalPlacement(this)`. The three surface memories return to their saved transforms and all seven objects can be redistributed.
6. Final Confirm is blocked with the configured English feedback until all seven items are in valid zones.
7. Successful Confirm plays `confirmationResponseLines`; its completion changes state to `BackToOffice`.
8. The screen fades out, `OfficeRoot` activates, Office Volume returns to weight 1, and player/assistant move to assigned Office return points.
9. `FinalReportController.PrepareReportReady()` enables the PC screen, generates the report, reveals the page-1 `View Report` button, and leaves the ItemCycler on page 1.
10. After fade-in, the assistant plays `officeReturnAndReportLines`.
11. Selecting `View Report` opens `FinalReportPage`; the relabeled `Close Session` button begins the terminal ending sequence.

Current report scene bindings:

- `FinalReportController` is on `PC` and is explicitly referenced by `GameStateController`.
- `reportEntryPageIndex` is 0.
- `FinalReportPage`, `View Report`, `Close Session`, `DistributionSummary`, and `OutcomeText` references are assigned. The serialized button reference migrates from the former Back field via `FormerlySerializedAs`.
- The `View Report` Button's persistent `On Click` list is empty; `FinalReportController.Awake()` adds its runtime callback, avoiding a duplicate `ItemCycler.NextItem` jump.
- The three attention-label colors are serialized and assigned.
- `FinalReportPage` is registered dynamically at runtime rather than stored in the initial `ItemCycler.items` array.
- `OutcomeText` replaced the old Focus/Context/Background/Initial Changes blocks. The obsolete text objects have been removed from the current scene.

## Game State Flow

Current high-level state order in `GameStateController`:

1. `OfficeDialogue`
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

5. `GiantCrisis`
   - Calls `ClownController.StartCrisisSequence()`.

6. `SwallowTransition`
   - Calls `SwallowController.StartSwallowTransition()`.

7. `MirrorChamber`
   - Player and assistant arrive inside the Giant Clown interior.
   - Assistant moved to mirror chamber spawn point.
   - Assistant plays Mirror Chamber intro.
   - Four mirrors are present, each broadcasting the client's negative internal voice.
   - Player approaches each mirror in any order, starts a conversation (via `MemoryDialogueController`), and either resolves or fails the dialogue.
   - **On resolution success**: the mirror auto-shatters — the echo stops, the container cracks open, and the original memory object (medal, clock, phone, pen) is revealed. No player action is needed to break the mirror.
   - **On dialogue failure**: the voice intensifies, the dialogue closes, and the `Start Conversation` button returns. The mirror stays intact.
   - After all four mirrors are resolved, the painful echoes no longer sustain the Giant Clown. The giant space dissolves automatically.
   - Player, assistant, and the four freed memory objects return to the Memory Room (HippoRoot).

8. `FinalMemoryPlacement`
   - Activates `HippoRoot` after the return from the Mirror Chamber.
   - Calls `MemoryPlacementController.BeginFinalPlacement(this)`.
   - Restores the original three objects at their saved placements and tracks all seven memories.
   - Waits for a valid seven-object Confirm.

9. `BackToOffice`
   - Triggered after the assistant's final confirmation response completes.
   - Fades the screen to black over 1 second.
   - Activates `OfficeRoot`.
   - Moves player and assistant robot to manually assigned office return spawn transforms while the screen is black.
   - Restores Office Global Volume and prepares the compositional report.
   - Fades the screen back to transparent over 1 second.
   - Plays the Office Return and Report assistant lines.

10. `SessionComplete`
   - Entered only after `Close Session` is selected and both assistant closing lines finish.
   - Clears the subtitle, fades the persistent transition Canvas to opaque black, and disables OfficeRoot, HippoRoot, and DungeonRoot.
   - The button is disabled immediately to prevent duplicate requests. Player movement remains available through the closing lines and fade, then locks after the Canvas reaches opaque black.
   - This is a terminal in-app state; WebGL does not attempt to close the browser tab.

## Scene Root Activation

To reduce headset runtime cost, the main scene is organized into three coarse root GameObjects:

- `OfficeRoot`
  - Contains first-stage office construction, objects, and building/environment content.

- `HippoRoot`
  - Contains Hippocampus-related scene content and the Nightmare/Giant Crisis content that still happens before the dungeon/fall transition.

- `DungeonRoot`
  - Contains the scene content used after the player enters the dungeon/fall sequence, including later dungeon and Mirror Chamber content.

`GameStateController` has Inspector references for these three roots and activates only one root at a time:

- `OfficeDialogue` and `AwaitCrystalBall` activate `OfficeRoot`.
- `TransitionToHippocampus`, `Hippocampus`, and `GiantCrisis` activate `HippoRoot`.
- `SwallowTransition` currently does not switch roots directly. This lets the black fade / swallow hand-to-mouth overlap begin while the previous root remains active.
- `MirrorChamber` activates `DungeonRoot`.
- When the giant dissolves after all mirrors resolve, the player returns to the Memory Room and enters `FinalMemoryPlacement` — `HippoRoot` is re-activated.
- `BackToOffice` activates `OfficeRoot` after the screen has faded to black (triggered after Stage 4 final confirmation).

Persistent gameplay objects such as the XR Origin/player, `GameStateController`, screen fade, assistant robot, and global event/input objects should stay outside these three roots so they remain active throughout the experience.

## Global Volume State Control

`GameStateController` exposes Inspector references for the Office, Hippocampus, and Nightmare Global Volumes, plus a shared Global Volume fade duration.

- On startup and when entering `OfficeDialogue`, Office Global Volume is weight 1 while Hippocampus and Nightmare are weight 0.
- During `TransitionToHippocampus`, once the screen is fully white, Office Global Volume is set to weight 0 and Hippocampus Global Volume is set to weight 1 before the fade back in.
- Entering `GiantCrisis` first fades the Hippocampus Global Volume weight to 0, then activates the Nightmare Global Volume and fades its weight to 1.
- The Giant Crisis sequence starts after the Nightmare Global Volume fade-in completes.
- When the giant dissolves after all four mirrors are resolved, the Nightmare Global Volume fades to 0 as the player returns to the Memory Room.
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
- `MirrorChamber` activates `DungeonRoot`, moves the player and assistant to the Mirror Chamber spawn point, and starts Mirror Chamber intro.
- Fade returns to transparent and controlled fall begins after the state has already switched to `MirrorChamber`.
- The player resolves four deep-memory conversations. Each resolved mirror auto-shatters. When all four are done, the giant dissolves and the player returns to the Memory Room.

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

From current inspection of `scene_WebGL.unity`, `ClownController` is bound roughly as follows:

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

- Memory placement completeness is based on the identity of required memory objects, not a floating-point counter.
- The EZPZ Confirm Button remains clickable before completion so it can provide explicit incomplete-placement feedback.
- Initial and final placement deliberately use different Confirm behavior: initial Confirm does nothing and all three surface objects automatically start the crisis; final Confirm requires all seven objects and shows explicit incomplete feedback.
- One phased `MemoryPlacementController` handles both the preliminary three-object layout and final seven-object redistribution. It preserves initial assignments/transforms and exposes the final placement data to the report.
- The final report is compositional: seven independent ternary placements select seven of 21 authored fragments. It reports attention allocation without a success score or single correct ending.
- Each object has three distinct possible report outcomes, but only the outcome matching its confirmed final zone is shown in a playthrough.
- The PC remains on page 1 after returning to Office. `View Report` appears only after report preparation and directly opens the dynamically registered final report page.
- The report's former Back button is now `Close Session`. It plays one neutral ending for every distribution, then fades to black and enters `SessionComplete`; it does not return to page 1.
- The player's VR camera should not be forcibly rotated during grab/mouth movement.
- Player follows the hand by position only, via `xrOrigin.position`; while grabbed, the tracked head is kept aligned to `grabAnchor` without parenting or forced rotation.
- Roof and player share the same `grabAnchor` concept to keep the hand/roof/player relationship consistent.
- Grab completion is determined by the grab reference reaching the player, not by `handIKTarget` alone.
- Grab-to-player has no timeout now; the giant hand must reach the player before the sequence continues.
- Mouth arrival waits on `WaitForHandNear(...)`; if the hand never reaches the mouth, `mouthArrivalTimeout` lets the sequence continue with a warning instead of getting stuck forever.
- Assistant dialogue is independent and should not block the clown crisis coroutine unless explicitly requested.
- Footsteps and rumble are sequential: footsteps finish first, rumble starts afterward.
- `SwallowTransition` starts before the hand-to-mouth movement is complete, allowing the fade to black to overlap with the final mouth movement for a smoother visual transition.
- Memory content display is now handled via pre-configured Image/RawImage/VideoPlayer components on the Canvas, toggled by Holdable UnityEvents. The old `MemoryContentDisplay` script has been removed.
- The `BreakGlass` game state is removed and `HandleFinalChamberGlassShattered()` in `GameStateController` is **deprecated** (2026-08-10). The empty method must remain because the imported `StylizedBreakableGlass` script still calls it directly for source compatibility. The player no longer manually breaks a glass wall. Instead:
  - Each mirror auto-shatters when its `MemoryDialogueController` conversation resolves successfully (via the `onComplete` UnityEvent).
  - After all four mirrors are resolved, the giant space dissolves automatically.
  - The player, assistant, and four freed memory objects return to the Memory Room (HippoRoot).
  - The `BackToOffice` state is now reserved for after Stage 4 (final seven-object redistribution), not for the mirror chamber exit.
  - `StylizedBreakableGlass` assets are still usable for the auto-shatter visual effect on each mirror — bind their `ForceShatter()` to the `MemoryDialogueController.onComplete` event.

## Known Risks / Things to Check in Unity

- Verify in WebGL Play Mode that each required memory is detected when entering and leaving every placement zone, especially if the object has multiple colliders.
- Verify that moving a memory directly from one zone to another leaves it assigned only to the new zone.
- The three placement Trigger Colliders must not overlap. If an object overlaps two zones, the current dictionary uses whichever `OnTriggerEnter` ran last, which may not match the apparent visual placement.
- Verify that the second uniquely placed object triggers the missing-memory dialogue exactly once.
- Verify that the third uniquely placed object starts `GiantCrisis` exactly once without a Confirm interaction.
- Verify that each surface memory remains at its assigned position after leaving and returning to `HippoRoot`.
- Verify that the four restored deep memories appear at the correct return points, use gravity, and cannot be ray-picked in the Giant before their Holdable components are enabled on return.
- Verify that incomplete final Confirm shows the existing English feedback and complete final Confirm plays the response exactly once before returning to Office.
- Test report generation with all seven items in Focus, all in Context, and all in Background. These are the longest single-category stress cases for `OutcomeText` layout and confirm that all 21 branches are reachable.
- Verify that the `View Report` button is hidden during the first Office visit and appears on page 1 only after the final return. Its saved persistent `On Click` list is currently empty as required.
- Verify `OutcomeText` readability at the target WebGL resolution. It uses one rich-text block and may need final font-size/spacing adjustment on the in-world computer screen.
- Assign audio clips to both `AssistantController.sessionClosingLines`, change the existing button label to `Close Session`, and verify the transition Canvas remains visible after all scene roots are disabled.
- Verify Close Session cannot be triggered twice, the report stays visible while the assistant speaks, subtitles clear before the fade, and the final frame remains fully black with no hidden Office interaction.
- If `footstepsAudio` is looping, the crisis coroutine will wait forever before rumble/animation starts.
- If `handIKTarget` or `playerHead` is missing, the grab stage now stops and logs an error instead of continuing.
- If `grabAnchor` is placed poorly on the hand skeleton, the player/roof may appear offset even if script logic is correct.
- If `mouthPoint`, `handIKTarget`, `handTipReference`, or `grabAnchor` are poorly placed, the mouth arrival wait may last until `mouthArrivalTimeout` and then continue with a warning.
- During `SwallowTransition`, `SwallowController` releases the clown's player-follow control once the screen is fully black and before moving the XR Origin to the pipe start point.
- If the roof seems to rotate unnaturally, keep `alignRoofRotationToGrabAnchor` off.
- If the roof seems not to align with the hand enough, tune `roofAttachDuration` and the local position of `grabAnchor`.
- If Rig weight appears not to change, verify that the Inspector is showing the outer `Rig` component weight, not only the `TwoBoneIKConstraint` weight.
- `MemoryDialogueController` uses a data-driven approach: all dialogue branching, text, and audio are configured in the Inspector via serialized fields — no per-mirror subclassing needed. Four instances with different field values cover the four planned deep-memory conversations.
- The Stage 3 dialogue now includes harmful shortcuts in Round 2 as well as Round 1 (updated 2026-08-09 from the earlier "all Round 2 choices are constructive" design). This creates more gameplay tension: the player must navigate carefully through both rounds.

## Stage 3 Implementation Notes

### MemoryDialogueController Scene Setup

The four `MemoryDialogueController` instances are connected in the WebGL scene. The following Inspector checks are still required after importing or changing scene content:

1. Confirm each mirror's `MemoryDialogueController` has the correct UI, audio, and dialogue references.
2. Configure the serialized fields:
   - **UI**: assign `startButtonRoot`, `dialogueRoot`, `round1ChoicesRoot`, `dialogueText` (TMP).
   - **Audio**: assign `dialogueAudioSource`.
   - **Opening**: set `openingText` and `openingClip` (the initial internal voice line for that memory).
   - **Round 1**: configure three `Round1ChoiceData` entries — two constructive (set `goesToRound2 = true` and pick `nextBranchIndex = 0` or `1`), one harmful (set `goesToRound2 = false`). Fill in `responseText` and `responseClip` for each.
   - **Round 2**: configure two `Round2BranchData` entries — each has a `choicesRoot` (the GameObject parent for that branch's two buttons), `promptText`, `promptClip`, and two `Round2ChoiceData` choices (one with `completesMemory = true`, one with `false`).
   - **Completion**: bind `onComplete` to deactivate the mirror and activate the restored memory object (e.g., medal, clock, phone, or pen).
3. Create a trigger collider around each mirror. Bind `OnTriggerEnter` → `MemoryDialogueController.SetPlayerInArea(true)` and `OnTriggerExit` → `SetPlayerInArea(false)`.
4. Create button GameObjects for Round 1 (3 buttons) and Round 2 (2 buttons per branch, each branch under its own `choicesRoot`). Attach `MemoryDialogueChoiceRelay` to each button and set `choiceIndex`.
5. Configure Round 1 choice index to 0, 1, 2 (lower-left, lower-right, top-center — order in Inspector matches order in gameplay).
6. Configure Round 2 choice index to 0 (constructive/success) and 1 (harmful/fail) per branch.

### Stage 3 Next Steps

- Test the full flow: enter area → start conversation → round 1 → harmful exit vs. round 2 → success exit vs. harmful exit.
- Ensure all four mirrors can be completed in any order.
- Verify each failed conversation plays its failure response and then resumes the opening audio loop; starting a conversation should leave the opening loop running until the player makes a choice, as currently designed.
- Verify the first resolved mirror triggers `HandleFirstMemoryReleased()` exactly once even though all four mirror completion events call it.
- Verify the DungeonRoot counter reaches four once, calls `HandleAllMemoriesResolved()`, fades Nightmare Volume to 0, and returns the player, assistant, and four freed objects to the Memory Room.
- The four explicit `memoryRoomReturnPoints` are now bound in Medal / Clock / Phone / Pen order. Confirm their physical placement visually.
- Verify the assigned `swallowTransitionLines` subtitle/audio timing during the full black-screen transition.

## Verification Pattern

For script-only changes, use:

```powershell
dotnet build Assembly-CSharp.csproj --no-restore
```

The 2026-08-16 Editor build after the flow cleanup passed with 0 errors. A clean/incremental Runtime build may still report existing warnings from obsolete EZPZ `FindObjectsSortMode` calls; these are inside the imported toolkit rather than the custom flow scripts.

## Future Update Rule

When making meaningful changes to game flow, scene bindings, or major script behavior, update this document before finishing the task. Future Codex sessions should read this file before doing broad project analysis.
