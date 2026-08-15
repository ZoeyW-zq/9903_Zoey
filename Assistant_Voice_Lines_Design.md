# 🤖 机器人助手语音台词设计 · Robot Assistant Voice Line Design

> **Bilingual Edition — 中英对照版**
>
> 设计日期：2026-08-10
>
> 角色定位：记忆整理师的专业AI助手，性格温暖、幽默、可爱，偶尔冒出小小的吐槽。它像一个热心的实习护士，带着一点笨拙的认真劲儿，在关键时刻又能给予坚定的支持。
>
> **Personality**: A professional yet warm AI assistant for the Memory Organizer. Cute, slightly witty, like a devoted nurse-intern who's earnest to a fault — clumsy in small ways, steady when it matters.

---

## 🏢 第一阶段：办公室 · Stage 1: Office

### 1. 问候 · Greeting
> `greetingLines` — after `BeginOfficeDialogue()`

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Good morning — well, it's always morning somewhere in someone's memory, right? I'm your assistant. You can call me… actually, I don't have a name yet. Maybe you can give me one later?" | "早上好——唔，反正在谁的记忆里总有个地方是早上对吧？我是你的助手。你可以叫我……呃，我好像还没有名字。要不待会儿你给我起一个？" |
| 2 | "Anyway! Welcome to the Memory Organizer workspace. Your client is currently in REM sleep and waiting for us. No pressure — they're literally unconscious." | "总之！欢迎来到记忆整理师工作站。你的来访者正处于REM睡眠中，正在等我们。别紧张——人家现在完全无意识。" |
| 3 | "Before we dive in, would you like to learn more about what a Memory Organizer does, take a look around the office, or just jump right in? I promise all three options are equally valid. Well, two of them buy me more time to finish my system check." | "在我们开始之前，你想了解记忆整理师的工作内容，在办公室四处看看，还是直接开始？我保证三个选项都一样合理。好吧，其中两个能让我多争取点时间完成系统自检。" |

---

### 2. 职业说明 · Job Explanation
> `jobExplanationLines` — after player selects "Learn about the role"

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Great question! So, a Memory Organizer is kind of like… an interior designer for the mind. Except you can't knock down walls, and the furniture has feelings." | "好问题！记忆整理师呢，有点像……心灵室内设计师。不过你不能敲墙，而且所有家具都有情绪。" |
| 2 | "Here's the important part — and I always emphasize this to new Organizers: you cannot delete, alter, or create memories. That's not how brains work, and honestly, it would be terrifying if it were." | "重点来了——我对每位新整理师都会强调这个：你不能删除、修改或创造记忆。大脑不是这么运作的，说实话，要是真能这样的话也太恐怖了。" |
| 3 | "What you CAN do is help the client decide how much attention each memory gets." | "你能做的，是帮来访者决定此时此刻每段记忆获得多少「关注度」。" |
| 4 | "Does that make sense? I practiced that explanation seventeen times in the mirror. The mirror was not impressed, but I think it went okay." | "讲清楚了吗？我在镜子前练习了十七遍这段说明。镜子没什么反应，但我觉得效果还行。" |

---

### 3. 探索送别 · Exploration Departure
> `explorationDepartureLines` — after player selects "Explore the office"

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Ooh, exploring! Take your time — I'll just be here… floating. Very still. Radiating patience." | "哦哦，要逛逛！慢慢来——我就在这儿……漂浮着。一动不动。散发着耐心光芒。" |
| 2 | "When you're ready to continue, just come find me. I'm not going anywhere. Literally — I don't have legs." | "准备好继续了就来找我。我哪儿也去不了。字面意思——我没有腿。" |

---

### 4. 探索归来 · Exploration Return
> `explorationReturnLines` — after player returns from exploration

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "You're back! Did you find anything interesting? I spent the time reorganizing my internal file structure. Alphabetically. It was very satisfying." | "你回来了！发现什么有趣的东西了吗？我利用这段时间把我的内部文件结构重新整理了一遍。按字母排序的。非常满足。" |
| 2 | "Ready to get started for real this time? The client's been in REM for a while now — they're probably having the most interesting dreams while we chat." | "这次准备好正式开始了吗？来访者已经进入REM睡眠一段时间了——我们聊天的工夫人家说不定正做着最精彩的梦呢。" |

---

### 5. 开始工作 · Start Work
> `startWorkLines` — after player selects "Start work"

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Alright! Here we go. Professional mode: engaged." | "好的！来吧。专业模式：已启动。" |
| 2 | "Let me pull up the client's file. One moment… and… there we are. The computer screen should be showing the case details now. Take a moment to review them — I'll wait. Again. I'm very good at waiting." | "让我调出来访者档案。稍等……好了。电脑屏幕上应该显示了案例详情。请花点时间查看——我再等等。又等。我真的很擅长等待。" |

---

### 6. 水晶球引导 · Crystal Ball Instruction
> `crystalBallInstructionLines` — in `OnStartWorkDialogueComplete()`, after `AwaitCrystalBall`

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Client report reviewed? Great. Now — do you see that crystal ball that just appeared? That's our entry point into the client's subconscious." | "来访者报告看完了？很好。现在——看到刚刚出现的水晶球了吗？那是我们进入来访者潜意识的入口。" |
| 2 | "Place your hand on the crystal ball and keep it there. It needs about two seconds to establish a stable connection." | "把手放在水晶球上，保持不动。建立稳定连接大约需要两秒。” |
| 3 | "Well. See you on the other side! … That sounded more dramatic than I intended. But also, I meant it." | "另一边见！……说得比我预想的更戏剧化。不过，我是认真的。" |

---

## 🧠 第二阶段：海马体记忆室 · Stage 2: Hippocampus Memory Room

### 7. 海马体介绍 · Hippocampus Intro
> `hippocampusIntroLines` — PlayHippocampusIntro()

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "And we're here! Welcome to the memory space. Everything you see around you? These are the client's established memories, the ones that have been here for years." | "到了！欢迎来到记忆空间。你周围看到的一切，都是来访者多年来建立的稳固记忆。" |
| 2 | "Let me explain how this works. Some of the client's recent memories are about to surface — they'll appear as physical objects you can pick up and examine. Your job is to decide how much current attention each one deserves." | "我来解释一下游戏规则——我是说，工作流程。来访者的一些近期记忆即将浮现——它们会以实物的形式出现，你可以拿起查看。你的任务是决定每段记忆目前值得多少关注度。" |
| 3 | "There are three attention zones on the wall. 'Focus' — for memories that need immediate, active attention. 'Context' — for memories that should stay available as useful background. And 'Background' — for memories that are still present and accessible, but don't need to dominate awareness right now." | "房间里有三个关注度区域。'聚焦区'——需要立刻获得主动关注的记忆。'情境区'——作为有用的背景信息持续可用的记忆。还有'背景区'——依然存在、可获取，但不需要占据当前意识的记忆。" |
| 4 | "The recent memories are right there on the table. Go ahead and pick them up — each one will play a memory clip when you grab it. I'll be right here if you need me." | "近期记忆就在桌子上。你可以把它们拿起来——每拿起一个就会播放一段记忆片段。有需要的话我就在这里。" |

---

### 8. 水瓶记忆 · Water Bottle Memory
> `waterBottleMemoryLines` — PlayWaterBottleMemory()

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Ah, this one… The client has a fever. Everyone else has gone home. They're drinking water, taking medicine, and just… keeping going." | "啊，这一段……来访者发烧了，但独自留在办公室完成项目。其他人都已经回家了。他们喝着水、吃着药，就那么……继续撑着。" |

---

### 9. 日落照片记忆 · Sunset Photograph Memory
> `sunsetPhotoMemoryLines` — PlaySunsetPhotoMemory()

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Oh, this is a beautiful one. They took this photo. Not for social media, not to show anyone. Just… because the sky looked beautiful, and that felt worth remembering." | "他们拍了这张照片。不是为了发社交媒体，不是为了给别人看。就只是因为……天空很美，而这件事值得被记住。" |

---

### 10. 乐高积木记忆 · LEGO Bricks Memory
> `legoBricksMemoryLines` — PlayLegoBricksMemory()

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Look at this! The client spent a whole evening assembling this model — brick by brick. They fell into that deep, quiet focus where time just… disappears. There's only pure, private satisfaction. The kind that doesn't need an audience." | "快看这个！来访者花了一整个晚上拼这个模型——一块一块地。他们沉浸在那深沉安静的专注状态里，时间就那么……消失了。就是纯粹、私密的满足感。不需要观众的那种。" |

---

### 11. 缺失的痛苦记忆 · Assistant Notices Missing Memories
> *(Narrative cue — deferred to later polish, but here's the line design)*
> Triggered after the second surface-memory object is placed (per PROJECT_CONTEXT.md design)

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Hmm. Wait a moment. Something's not right here…" | "嗯。等一下。这儿有些不对劲……" |
| 2 | "Those established memories — they're all… pleasant. Or at least, not deeply painful." | "这些稳定的记忆——它们全都是……愉快的。或者说，至少不是深度痛苦的。" |
| 3 | "But that can't be right. A normal memory space doesn't look like this — it's never just the happy stuff. The painful ones should be here too. Where are they?" | "但这不对啊。一个正常的记忆空间不应该是这样的——从来不会只有快乐的部分。那些痛苦的也应该在啊。它们去哪儿了？" |
| 4 | "……Let's finish placing these three first. But I'm going to keep an eye on things. Metaphorically. I don't actually have eyelids. Also, my left sensor array has been glitching since Tuesday, so if I miss something, that's my excuse." | "……我们先放好这三个再说。不过我会盯着点的。比喻意义上的盯着。我其实没有眼皮。另外，我的左侧传感器阵列从周二开始就有点小故障，所以如果我漏掉了什么，那正好是我的借口。" |

---

### 12. 噩梦预警 · Nightmare Warning
> `nightmareWarningLines` — PlayNightmareWarning() — concurrent with ClownController.CrisisRoutine

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Wait — what was that? I heard something." | "等等——那是什么？我听到了什么。" |
| 2 | "Um. Okay. I'm picking up a… presence. A very large presence. Approaching. Rapidly. I don't know what it is yet, but it is NOT in the client's file and I really wish it were because at least then I'd have a footnote to read." | "呃。好。我检测到一个……存在。一个非常大的存在。正在快速靠近。我还不知道那是什么，但它不在来访者档案里。我多希望它在啊，那样至少我还能翻个脚注看看。" |
| 3 | "WHAT IS THAT. WHAT — is that a GIANT CLOWN?! Why is there a giant clown?! Nobody mentioned a giant clown! I read the entire case file! Twice! There was no clown appendix!" | "那是什么！！什么——那是个巨型小丑吗？！为什么会有巨型小丑？！没人提过巨型小丑啊！我把整个案例档案读了两遍！根本没有小丑附录！" |
| 4 | "Okay. Okay. No need to panic. I can just teleport us back to the office — one button, clean exit. Just give me a second to… wait. Why is the signal getting weaker? Oh no. Oh no no no — the connection is dropping! I can't — I can't get a lock on the office coordinates!" | "没事。没事。用不着慌。我可以直接把咱俩传送回办公室——一个按钮，干净退出。给我一秒……等等。信号怎么越来越弱了？！哦不。不不不——连接在断开！我锁定不了办公室的坐标！" |

---

## 🤡 第三阶段：巨人体内 / 吞咽过渡 · Stage 3: Swallow & Giant Interior

### 13. 吞咽过渡 · Swallow Transition
> `swallowTransitionLines` — PlaySwallowTransition() — during black hold

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "……… Okay. So. We're inside a giant clown now. Those are words I just said, and they are accurate. I'm processing that." | "………好的。那么。我们现在在一个巨大的小丑体内。我刚才说了这些话，而且它们是正确的描述。我正在消化这个事实。" |

---

### 14. 镜子室介绍 · Mirror Chamber Intro
> `mirrorChamberIntroLines` — PlayMirrorChamberIntro()

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Well, it's so… intimate. In a terrifying way. Closed in. And so loud." | "这么……私密。以一种令人恐惧的方式。这么封闭。这么吵。" |
| 2 | "Wait. Those voices… I recognize these. These are the client's painful memories. The ones that were missing from the memory room. They've been trapped here — trapped inside those… those containers." | "等等。那些声音……我认得。这些就是来访者的痛苦记忆。那些在记忆室里找不到的。它们一直被困在这里——困在那些……容器里。" |
| 3 | "They've been echoing in here for so long that the client probably doesn't even hear them as memories anymore. Just as noise. Just as… 'this is who I am.' But it's not. It's just old pain on a loop." | "它们在这里回响了太久了，来访者大概已经听不出那是记忆了。只听到噪音。只觉得……'我就是这样的人'。但不是的。那只是旧伤痛在循环播放。" |
| 4 | "I think… I think you need to talk to them. Each one. The voices are loud because nobody's ever really answered them — just let them talk. Maybe if someone responds differently, something changes? I'm guessing here. This is definitely not in my manual." | "我觉得……我觉得你需要跟它们对话。每一个。那些声音之所以这么吵，是因为从来没有人真正回应过它们——只是任由它们说。也许如果有人用不同的方式回应，有些东西会改变？我在猜。这绝对不在我的手册里。" |
| 5 | "I'll stay close. Or as close as I can without getting in your way, keep trying to reconnect to the outside. And if anything goes wrong, I'll… well, I'll think of something. Probably." | "我会待在附近。或者说尽量待在不挡路的地方。我也会继续尝试重新连接外界。如果出了什么问题，我会……嗯，我会想到办法的。大概吧。" |

---

### 15. 记忆释放 · Memory Released
> *New dialogue array needed: `memoryReleasedLines`* — triggered after each mirror's conversation resolves successfully and the mirror auto-shatters

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "It broke! On its own — you didn't even touch it! And the echo disappeared!" | "它碎掉了！自己碎的——你碰都没碰！回声也消失了。" |
| 2 | "And look — the real memory was still there. Intact. Waiting. It just needed to be understood differently." | "你看——真实的记忆一直都在。完整的。等着我们。它只是需要被以不同的方式理解。" |

---

### 16. 全部记忆解放 · All Memories Released
> *New dialogue array needed: `allMemoriesReleasedLines`* — triggered after the fourth and final mirror resolves

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "That's all four! Every single painful echo has been quieted. The real memories — the medal, the clock, the phone, the pen — they're all free now." | "四面全完成了！每一道痛苦的回声都安静下来了。真正的记忆——奖牌、闹钟、手机、红笔——它们现在全都自由了。" |
| 2 | "Can you feel that? The whole space is… shifting. Without the echoes feeding it, this place can't hold itself together. It's coming apart!" | "你感觉到了吗？整个空间在……变形。没了回声供养它，这个地方撑不住了。在分崩离析！" |
| 3 | "Wait — the signal! It's coming back! The connection is re-establishing — I can lock onto the memory room coordinates now!" | "等等——信号！信号回来了！连接在重建——我现在能锁定记忆室的坐标了！" |
| 4 | "I'm initiating the transfer! Everyone hold on — well, the memories don't have hands, but you know what I mean. We are LEAVING!" | "我在启动传送！所有人抓紧——好吧，记忆物件没有手，但你懂我意思。我们要走了！！" |

### 17. 返回记忆空间 · Return to Memory Space
> *Rewrites `returnToOfficeLines` — this now fires after the giant dissolves; player and four memory objects return to the Memory Room (NOT the office)*

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "……And we're back. The memory room. Solid ground. Familiar walls. The sky is back to normal — look, you can actually see it now. No giant clowns in sight. I have never been so relieved to see this place." | "……我们回来了。记忆室。坚实的地面。熟悉的墙壁。天空也恢复正常了——你看，现在能看到了。视线范围内没有巨型小丑。我从来没有这么高兴看到这个地方。" |
| 2 | "……Although. That ceiling is definitely going to need some work. I'll dispatch a repair crew later. Structural damage is NOT in our job description." | "……不过。那天花板绝对是得修了，之后我会派遣维修员的。建筑结构损坏不在我们的职责范围内。" |
| 3 | "So. That was the giant clown. Those painful memories the client had been feeding for years — they grew into something that could literally swallow us. And the only way out was to help the client hear those memories differently. You did that. Four times." | "所以。那就是巨型小丑。来访者喂养了多年的那些痛苦记忆——它们长成了一个能真的吞掉我们的东西。而唯一的出路是帮来访者以不同的方式倾听那些记忆。你做到了。四次。" |
| 4 | "Now there's one more big task ahead — the final attention redistribution with all seven memories. The three you placed earlier, plus the four we just freed. They're all here now. But first… let me take a moment to NOT be inside a giant anything. This is really underrated. When you're ready, just hit the confirm button — take all the time you need." | "现在前面还有一项大任务——用全部七段记忆做最终关注度重新分配。你之前放置的三段，加上我们刚刚解放的四段。它们现在全在这里了。但首先……让我享受一下不在任何巨型东西里面的感觉。不呆在巨型东西里面这件事真的太被低估了。准备好了就直接按确认按钮——慢慢来，不急。" |

---

## 📊 第四阶段：最终记忆重分配 → 返回办公室 · Stage 4: Final Redistribution → Return to Office
> *(Not yet implemented — dialogue is pre-designed)*

### 18. 确认分配结果 · Confirmation Response
> Triggered after the player confirms the final seven-object placement

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "Final distribution confirmed! Alright — let's head back to the office and see what the report says. The crystal ball should still be active… give me a second to lock onto the return coordinates." | "最终分配已确认！好——我们回办公室看看报告怎么说。水晶球应该还在运行中……给我一秒锁定返回坐标。" |
| 2 | "Here we go. Smooth ride this time, I promise. No giant clowns. No digestive tracts. Just a normal, boring, wonderful teleport. See you on the other side!" | "走吧。这次保证是平稳旅程。没有巨型小丑。没有消化道。就是一个正常、无聊、美妙的传送。另一边见！" |

---

### 19. 返回办公室 & 查看报告 · Return to Office & Report Review
> After teleport back to office; merges old Sections 19 + 20

| # | 英文 English | 中文 Chinese |
|---|-------------|-------------|
| 1 | "And we're back in the office! " | "回到办公室了！" |
| 2 | "The computer should already be processing the session data… yep, the report is ready. Go ahead and select 'View Report' — let's see what your final distribution looks like on paper." | "电脑应该已经在处理会话数据了……嗯，报告已经就绪。去选择'查看报告'吧——看看你的最终分配在纸面上是什么样的。" |

---

## 📋 第五阶段：办公室报告与结束 · Stage 5: Office Report & Closing
> ⚠️ **以下部分待定 — Pending outcome system design**

### 20. 播放来访者留言 · Play Client Message
> ⚠️ **TBD — 待定**
> After player selects "Play Client Message." The client's message content depends on the final attention distribution outcome (positive, mixed, or negative). Assistant's reaction line should be designed after the outcome variants are finalized.

### 21. 结束语 · Closing
> ⚠️ **TBD — 待定**
> After report review and client message. The closing dialogue needs variants: a positive-leaning outcome can use a warm, encouraging tone; a negative-leaning outcome (e.g., painful memories placed in Focus) needs a more measured, reflective tone that acknowledges the difficulty without false cheerfulness. Design these variants after the outcome model is finalized.

---

## 🔧 系统/边缘情况台词 · System / Edge Case Lines
> *(Not in current code — for future use during error states, idle moments, etc.)*

### 闲置提示 · Idle Prompt
> If player stands still for ~30 seconds without interacting

| 英文 English | 中文 Chinese |
|-------------|-------------|
| "Still there? Just checking. Take all the time you need — memory work isn't a speedrun. Unless it is, in which case I have no idea what the world record is." | "还在吗？就问问。慢慢来——记忆工作不是速通游戏。如果是的话，我也不知道世界纪录是多少。" |

### 重复访问同一镜子 · Revisiting Completed Mirror
> If player approaches an already-resolved mirror

| 英文 English | 中文 Chinese |
|-------------|-------------|
| "This one's already at peace. Look — it's back to being a [medal/clock/phone/pen]. Much quieter now, isn't it? The real thing always is." | "这个已经平静下来了。看——它现在恢复到[奖牌/闹钟/手机/红笔]的样子了。安静多了对吧？真实的东西总是这样。" |

### 所有镜子完成 · All Mirrors Resolved
> After the fourth and final mirror auto-shatters and the memory is released

| 英文 English | 中文 Chinese |
|-------------|-------------|
| "That's all four! The echoes have stopped. Can you feel the difference? The whole space is… quieter. Like a room after the rain stops. The giant can't sustain itself anymore — it's already starting to come apart." | "四面全完成了！回声停了。你能感觉到不同吗？整个空间都……更安静了。像雨停之后的房间。巨人撑不住了——它已经开始分崩离析了。" |

---

## 📝 设计备注 · Design Notes

### 角色一致性 · Character Consistency
- 小助手说话有点啰嗦（"我说太多了对不对"）+ 立即自我纠正的倾向
- 习惯在严肃说明后插入一句调侃来缓解气氛
- 对自己的AI身份有认知并经常自嘲（"我没有腿"、"我是桌面助手"、"我没有情感模块"）
- 对来访者怀有真正的关心，但用幽默包装起来
- 偶尔在对话中加入"统计学"来假装很科学，然后立刻承认自己编的

### 语气指南 · Tone Guide
| 场景 | 情绪调色板 |
|------|-----------|
| Office greeting | ☀️ 温暖、轻快、欢迎 |
| Job explanation | 📚 专业但仍然轻松 |
| Hippocampus intro | 🌌 略带敬畏、好奇 |
| Memory reveal | 🫀 共情、温和 |
| Nightmare warning | 😱 惊慌但不失控（喜剧式恐慌） |
| Swallow transition | 🤢 厌恶 + 诡异的乐观 |
| Mirror chamber | 🎯 坚定、支持性 |
| Memory released (per-mirror) | 🫧 释放感、安静、敬畏 |
| All memories released / Giant dissolves | 🌊 震撼、释然、信号恢复 |
| Return to memory space | 🌅 满足、温暖、幽默（天花板梗） |
| Confirmation response (18) | ✈️ 轻快、准备好收尾 |
| Return to office & report (19) | 🏠 归家感、满足、期待 |
| Client message & Closing (20-21) | ⚠️ 待定 |

### 当前实现状态 · Implementation Status
| 对话数组 | Inspector填写 | 代码调用 |
|---------|-------------|---------|
| `greetingLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `jobExplanationLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `explorationDepartureLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `explorationReturnLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `startWorkLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `crystalBallInstructionLines` | ✅ 已填写 | ✅ OfficeDialogueController |
| `hippocampusIntroLines` | ✅ 已填写 | ✅ AssistantController |
| `waterBottleMemoryLines` | ✅ 已填写 | ✅ AssistantController |
| `sunsetPhotoMemoryLines` | ✅ 已填写 | ✅ AssistantController |
| `legoBricksMemoryLines` | ✅ 已填写 | ✅ AssistantController |
| `nightmareWarningLines` | ✅ 已填写 | ✅ AssistantController |
| `swallowTransitionLines` | ✅ 已填写 | ✅ AssistantController |
| `mirrorChamberIntroLines` | ✅ 已填写 | ✅ AssistantController |
| `memoryReleasedLines` | 🆕 新数组 | ⏳ 需创建 |
| `allMemoriesReleasedLines` | 🆕 新数组 | ⏳ 需创建 |
| `returnToOfficeLines` | ♻️ 需改写为返回记忆空间 | ✅ AssistantController (需修改) |
| `confirmationResponseLines` (18) | 🆕 新数组 | ⏳ 需创建 |
| `officeReturnAndReportLines` (19) | 🆕 新数组 | ⏳ 需创建 |
| `breakGlassLines` | ❌ 已废弃 | 删除 |
| `glassBrokenPraiseLines` | ❌ 已废弃 | 删除 |
| Client message (20) | ⚠️ 待定 | 等待结局系统设计 |
| Closing (21) | ⚠️ 待定 | 等待结局系统设计 |
| Edge case / idle / completed-mirror lines | ⏳ 未实现 | N/A |

---

> 📄 此文档可直接作为Inspector中DialogueLine数组的填写参考。每段台词对应一个DialogueLine元素，`text`字段填英文/中文（根据目标受众选择），`audioClip`字段填对应的录音文件引用。
>
> This document serves as a direct reference for filling the DialogueLine arrays in the Inspector. Each line corresponds to one DialogueLine element — use the `text` field for the line content, and the `audioClip` field for the corresponding voice recording reference.
