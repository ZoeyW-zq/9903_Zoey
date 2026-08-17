from pathlib import Path

from docx import Document
from docx.enum.style import WD_STYLE_TYPE
from docx.oxml.ns import qn
from docx.shared import Pt


SOURCE = Path(
    r"E:\GitHub\9903_Zoey\output\docx"
) / "DDES9903_A2_Report_Draft_Memory_Organizer_Bilingual_Actual_Outcomes_Corrected.docx"
OUTPUT = Path(
    r"E:\GitHub\9903_Zoey\output\docx"
) / "DDES9903_A2_Report_Bilingual_Revised_Under_3000_Words.docx"


REPLACEMENTS = {
    5: (
        "The Memory Organizer is a short VR and WebGL story about how anxiety changes attention. "
        "The player starts in an office, enters a memory room, and places three recent memories in "
        "Focus, Context, or Background. A Giant Clown then breaks into the room, captures the player, "
        "and swallows them. Inside its body, four containers hold painful memories. The player approaches "
        "them in any order and answers two-round conversations. Constructive responses break a container "
        "and release its object; harmful responses end the attempt and allow a retry. After all four "
        "objects are free, the player returns to the memory room and redistributes all seven memories. "
        "The confirmed layout selects one outcome for each object in the office report, followed by a "
        "neutral Close Session ending.",
        "《记忆整理师》是一段关于焦虑如何改变注意力的短篇 VR 与 WebGL 故事。玩家从办公室出发，进入记忆空间，"
        "并把三段近期记忆放入“焦点”“语境”或“背景”。随后，巨型小丑闯入房间，抓住并吞下玩家。它的体内有四个"
        "封存痛苦记忆的容器。玩家可以按任意顺序接近容器，并完成两轮对话。建设性回应会打破容器并释放物件；有害"
        "回应会结束本次尝试，但允许重试。四个物件全部释放后，玩家回到记忆空间，重新分配七段记忆。确认后的布局会"
        "为办公室报告中的每个物件选择一条结果，最后以中性的“Close Session”结束。",
    ),
    9: (
        "Figure 1 maps the implemented story onto Freytag's structure. OfficeDialogue and the first "
        "placement build context and tension. GiantCrisis and SwallowTransition form the climax because "
        "safety and movement are taken away. The body-interior conversations are the falling action: the "
        "player works through four beliefs after the physical threat has peaked. FinalMemoryPlacement "
        "resolves the conflict, and the office report is the denouement. The path is mostly linear, but "
        "mirror order, dialogue attempts, and the final layout vary within it.",
        "图 1 把已实现的故事映射到 Freytag 结构。OfficeDialogue 与第一次放置建立背景并累积张力。GiantCrisis 和 "
        "SwallowTransition 构成高潮，因为安全感和移动能力在此被夺走。身体内部的对话属于下降行动：物理威胁达到顶点后，"
        "玩家开始处理四个核心信念。FinalMemoryPlacement 解决冲突，办公室报告则构成结局。整体路径以线性为主，但镜子"
        "顺序、对话尝试和最终布局可以变化。",
    ),
    12: (
        "Agency is layered rather than constant. In the office, the player can hear the job explanation, "
        "explore repeatedly, or begin work. These choices set the pace but lead to the same assignment. "
        "The Giant's arrival and swallow are fixed because they are the central conflict, so the project "
        "does not claim to offer an unrestricted branching world.",
        "玩家能动性分层出现，而不是始终保持相同强度。在办公室中，玩家可以听取工作说明、反复探索或开始任务。"
        "这些选择控制节奏，但都会进入同一委托。巨型小丑的出现和吞咽是固定事件，因为它们构成核心冲突。因此，项目"
        "并不声称提供一个不受限制的分支世界。",
    ),
    13: (
        "Inside the Giant, the player chooses the order of four containers. Each conversation has two "
        "rounds. Two first-round responses continue to different second-round prompts; a harmful response "
        "at either round plays an alternate reply and closes the attempt. The unresolved container can then "
        "be tried again. This creates local variation without producing broken or permanently blocked story states.",
        "在巨型小丑体内，玩家可以决定四个容器的处理顺序。每段对话有两轮。第一轮中的两个回应会进入不同的第二轮"
        "问题；任一轮中的有害回应都会播放另一条答复并结束本次尝试。尚未解决的容器随后可以重试。这种设计产生局部"
        "变化，同时避免故事状态损坏或永久卡住。",
    ),
    14: (
        "Feedback is direct and non-numeric. A harmful response leaves the container intact and restarts "
        "its idle voice. A constructive second-round response triggers the completion event, breaks the "
        "container, and releases the original object. The player can read progress from sound, dialogue, "
        "and the physical state of the chamber without scores or permanent punishment.",
        "反馈直接且不使用数值。有害回应会让容器保持完整，并重新开始其待机语音。建设性的第二轮回应会触发完成事件，"
        "打破容器并释放原始物件。玩家可以通过声音、对话和空间中的物理状态判断进度，而不需要分数或永久惩罚。",
    ),
    15: (
        "The strongest agency comes from the final layout. The player can move seven memories among Focus, "
        "Context, and Background and revise every position before confirmation. Seven independent choices "
        "produce 3^7, or 2,187, layouts. The implementation does not write 2,187 full endings; it selects "
        "one of three object-specific outcomes for each memory, using 21 authored fragments in total. These "
        "results change the report, not the main plot.",
        "最强的能动性来自最终布局。玩家可以在“焦点”“语境”和“背景”之间移动七段记忆，并在确认前修改每个位置。"
        "七个独立选择会产生 3^7，即 2,187 种布局。实现中没有编写 2,187 个完整结局，而是为每段记忆从三条物件专属"
        "结果中选择一条，总共只需 21 条已写好的片段。这些结果会改变报告，但不会改变主线剧情。",
    ),
    16: (
        "Control follows this hierarchy. Movement is locked during the grab, but head tracking or mouse look "
        "remains active. It returns for the container stage and expands to free revision of the final layout. "
        "The experience moves from temporary restraint to a decision with visible results.",
        "控制方式遵循同一层级。抓取期间位移被锁定，但头部追踪或鼠标观察仍然可用。容器阶段恢复移动，最终布局则可"
        "自由修改。体验由暂时受限逐步过渡到会产生可见结果的决定。",
    ),
    22: (
        "Coherence depends on one rule: a Memory Organizer can redistribute attention but cannot delete, "
        "rewrite, or invent memories. Each deep object expresses part of the client's anxiety: the medal "
        "links worth to first place; the clock turns rest into guilt; the phone links rejection to fear of "
        "asking for help; and the red pen turns one mistake into a judgment of ability. These connected "
        "beliefs explain why the three recent memories alone are incomplete.",
        "叙事连贯性依赖一条规则：记忆整理师可以重新分配注意力，但不能删除、改写或创造记忆。每个深层物件都表达来访者"
        "焦虑的一部分：奖牌把价值与第一名相连；闹钟把休息变成内疚；手机把被拒绝与害怕求助相连；红笔则把一次错误"
        "变成对能力的判断。这些彼此相关的信念解释了为什么仅有三段近期记忆并不完整。",
    ),
    23: (
        "The Freytag phases also follow clear cause and effect. Three recent placements reveal that painful "
        "memories are missing, which triggers the Giant crisis. The swallow moves the player from threat into "
        "reflection. Four constructive conversations release the memories, and the final redistribution applies "
        "that new understanding. The office report closes the story by showing the consequence of the confirmed layout.",
        "Freytag 各阶段也遵循清楚的因果关系。三段近期记忆的放置揭示痛苦记忆仍然缺失，从而触发巨型小丑危机。"
        "吞咽让玩家从外部威胁进入内部反思。四段建设性对话释放记忆，最终重新分配则应用新的理解。办公室报告通过展示"
        "确认布局的后果来结束故事。",
    ),
    24: (
        "State gates prevent events from appearing out of order. The crisis waits for three distinct recent "
        "objects and the missing-memory cue. A container completes only once, after a constructive second-round "
        "response; a harmful response keeps it available. After four completions, the player and released objects "
        "return to the memory room. Final confirmation requires all seven objects, and the report reads only these stored positions.",
        "状态门控防止事件顺序错乱。危机会等待三个不同的近期物件和缺失记忆提示完成。容器只会在建设性的第二轮回应后"
        "完成一次；有害回应会让它保持可用。四个容器完成后，玩家与释放的物件回到记忆空间。最终确认要求七个物件"
        "全部放置，报告也只读取这些已保存的位置。",
    ),
    25: (
        "Shared systems keep this structure practical. The four conversations use one data-driven "
        "MemoryDialogueController with different text, audio, branches, and completion settings. Initial and "
        "final placement use phases of the same MemoryPlacementController. FinalReportController reads seven "
        "stored zones and selects seven of 21 outcome fragments. This supports 2,187 coherent layouts without "
        "claiming that the main narrative has 2,187 branches.",
        "共享系统让该结构保持可实现。四段对话使用同一个数据驱动的 MemoryDialogueController，只配置不同的文本、音频、"
        "分支和完成条件。初次与最终放置使用同一个 MemoryPlacementController 的不同阶段。FinalReportController 读取七个"
        "已保存区域，并从 21 条结果中选择七条。这样可以支持 2,187 种连贯布局，而不声称主线剧情拥有 2,187 个分支。",
    ),
    27: (
        "Each space has a specific narrative purpose. The warm office frames memory work as professional "
        "care. The crystal ball marks the threshold to the client's subconscious. The hippocampus-inspired "
        "room turns attention into three physical zones around a central table. The Giant destroys this safety "
        "and moves the player into a compact body-interior chamber. That chamber is designed for focused "
        "encounters, not open-world exploration.",
        "每个空间都有明确的叙事作用。温暖的办公室把记忆工作呈现为专业照护。水晶球标记进入来访者潜意识的门槛。"
        "以海马体为灵感的房间把注意力转化为中央桌周围的三个物理区域。巨型小丑破坏这种安全感，并把玩家带入紧凑的"
        "身体内部空间。该空间服务于集中互动，而不是开放世界探索。",
    ),
    30: (
        "The memory room supports repeated picking up, listening, comparing, and placing. Focus, Context, "
        "and Background make an abstract judgment visible as location. When the player returns, the four released "
        "objects appear beside the three recent memories. Reusing the room changes its meaning: a place for an "
        "incomplete first arrangement becomes the site of the final seven-object decision.",
        "记忆空间支持反复拿起、聆听、比较和放置。“焦点”“语境”和“背景”把抽象判断变成可见的位置。玩家返回时，"
        "四个释放物件会与三段近期记忆一起出现。空间的重复使用改变了它的意义：原本承载不完整首次布局的地方，最终"
        "成为七物件决策的场所。",
    ),
    33: (
        "The body-interior chamber works as a small hub with four clear destinations. Movement returns after "
        "the fall, and Start Conversation appears only within the relevant interaction area. The player chooses "
        "the visit order, but each position remains tied to one memory. After the fourth completion, the chamber "
        "breaks down and the state controller returns the player to the memory room. Final Play Mode checks are "
        "still needed for trigger overlap and return-point alignment.",
        "身体内部空间是一个包含四个明确目的地的小型枢纽。坠落结束后，玩家恢复移动；只有进入对应交互区域时才会出现 "
        "Start Conversation。玩家可以选择访问顺序，但每个位置始终对应一段记忆。第四个容器完成后，空间开始崩解，"
        "状态控制器把玩家送回记忆空间。目前仍需在 Play Mode 中最终检查触发器重叠和返回点对齐。",
    ),
    37: (
        "Lighting, post-processing, dialogue, and sound distinguish the four main stages. The assistant links "
        "them with a continuous voice: it briefs the player, warns of the Giant, reacts during the black-screen "
        "swallow, and reports progress inside the body. This spoken guidance keeps state changes readable without "
        "depending only on interface text.",
        "灯光、后期处理、对话和声音区分四个主要阶段。助手用连续的声音连接它们：进行任务说明、警告巨型小丑出现、"
        "在黑屏吞咽期间作出反应，并报告身体内部的进度。这种口头引导让状态变化清晰可读，而不只依赖界面文字。",
    ),
    38: (
        "The crisis audio follows a causal order. The warning leads to footsteps; after they finish, roof "
        "rumble and Giant animation begin. Groans, heartbeat, and Nightmare processing build pressure during "
        "the grab. Near the mouth, a black fade hides the teleport while swallow sounds and the assistant's "
        "panicked line continue. The controlled fall completes the transition. Audio therefore joins several "
        "technical scene changes into one understandable event.",
        "危机音频遵循因果顺序。警告之后出现脚步声；脚步结束后，屋顶震动和巨型小丑动画开始。抓取期间的低吼、心跳和"
        "Nightmare 后期处理共同增加压力。接近嘴部时，黑色淡出隐藏传送，而吞咽声和助手的惊慌台词继续播放。受控坠落"
        "完成转场。因此，音频把多个技术场景变化连接成一个容易理解的事件。",
    ),
    41: (
        "Reverb and echo make the compact body chamber sound much larger. Each unresolved memory repeats an "
        "idle line with a short gap, so its belief feels persistent. Dialogue gives different replies to "
        "constructive and harmful choices, while the breaking sound marks completion. As containers are resolved, "
        "their loops stop and the chamber becomes less crowded with voices.",
        "混响和回声让紧凑的身体内部空间听起来更大。每段未解决记忆会隔一小段时间重复待机台词，使其信念显得持续存在。"
        "对话会对建设性和有害选择给出不同回应，而破碎声标记完成。随着容器被解决，对应循环停止，空间中的声音也逐渐减少。",
    ),
    44: (
        "The soundscapes also separate dramatic functions. The crisis uses sharp movement and body sounds; "
        "the later chamber uses a long, reflective acoustic tail. Container breaks and assistant responses mark "
        "progress, and the final spatial failure prepares the return. The systems are implemented, but full WebGL "
        "and XR playthrough timing, subtitle sync, and report readability still need final validation.",
        "不同声景也承担不同的戏剧功能。危机使用强烈的移动声和身体声；之后的空间则使用较长、具有反思感的声音尾部。"
        "容器破碎与助手回应标记进度，最终空间失效为返回做准备。这些系统已经实现，但完整 WebGL 与 XR 流程的时序、"
        "字幕同步和报告可读性仍需最终验证。",
    ),
    48: (
        "Ordinary objects make hidden beliefs tangible. The medal represents rank, the alarm clock guilt about "
        "rest, the old phone fear of asking for help, and the red pen shame about mistakes. Sealing them in "
        "containers shows how the client has kept these memories locked away. Their later return to the memory "
        "room supports the rule that the past remains present even after its meaning changes.",
        "日常物件让隐藏信念变得可触摸。奖牌代表排名，闹钟代表对休息的内疚，旧手机代表害怕求助，红笔代表对错误的羞耻。"
        "把它们封在容器中，表现来访者如何压住这些记忆。物件后来回到记忆空间，也支持项目的规则：即使意义改变，过去"
        "仍然存在。",
    ),
    49: (
        "A container breaks only after a constructive interpretation. The break does not mean that the event "
        "disappears or that a belief is instantly cured. It marks a change from a fixed interpretation to a more "
        "flexible one. The memory object survives and can be handled again, so release comes from changing its "
        "frame rather than erasing the experience.",
        "容器只会在记忆获得建设性解释后破碎。破碎并不表示事件消失，也不表示某个信念被立刻治愈。它标记的是从固定解释"
        "转向更灵活理解的变化。记忆物件仍然存在并可以再次拿取，因此释放来自解释框架的改变，而不是抹去经历。",
    ),
    50: (
        "Other signs extend this idea. The swallow, black screen, and fall mark entry into the Giant's inner "
        "space; cavern-like sound enlarges that psychological space. Focus, Context, and Background are not labels "
        "for good, neutral, and bad. They show how much present attention a memory receives. The final report "
        "translates those positions into specific possible effects on the client.",
        "其他符号延伸了这一概念。吞咽、黑屏和坠落标记进入巨型小丑的内部空间；洞穴般的声音则放大这个心理空间。"
        "“焦点”“语境”和“背景”不是好、中、坏的标签，而是表示一段记忆目前获得多少注意力。最终报告把这些位置转化为"
        "对来访者可能产生的具体影响。",
    ),
    54: (
        "The player learns the system through action. They activate the crystal ball, pick up memories, listen "
        "while handling them, and place them in attention zones. Inside the Giant, they walk to a container and "
        "choose dialogue responses; the container breaks automatically after a constructive resolution, so the "
        "design avoids presenting reflection as physical attack. Final redistribution returns to object handling "
        "with the widest freedom.",
        "玩家通过行动理解系统。他们启动水晶球、拿起记忆、在操作物件时聆听，并把物件放入注意力区域。在巨型小丑体内，"
        "玩家走向容器并选择对话回应；建设性解决后容器自动破碎，因此设计没有把反思表现成物理攻击。最终重新分配回到"
        "物件操作，并提供最大的自由度。",
    ),
    55: (
        "The capture creates a temporary, embodied loss of control without switching to an external camera. "
        "The Giant removes the roof, reaches into the room, and carries the player toward its mouth. Locomotion "
        "and jumping stop, but head direction or mouse look remains active. During the black fade, audio continues "
        "while the player moves to the fall start point; ControlledFall then reaches the body chamber before "
        "movement is restored.",
        "抓取在不切换到外部镜头的情况下，制造暂时且具身的失控体验。巨型小丑移开屋顶，把手伸入房间，并把玩家带向嘴部。"
        "位移和跳跃停止，但头部方向或鼠标观察仍然可用。黑色淡出期间，音频继续播放，玩家被移到坠落起点；随后 "
        "ControlledFall 把玩家送到身体内部空间，再恢复移动。",
    ),
    56: (
        "Other characters and objects also enact the story. The assistant warns the player, reacts to the "
        "swallow, responds to released memories, and starts the return after all four are complete. Containers "
        "stay closed while unresolved and break on their completion events. Back in the memory room, the four "
        "objects regain holdability and gravity at set return points, enabling the final placement.",
        "其他角色与物件也在表演故事。助手警告玩家、对吞咽作出反应、回应已释放的记忆，并在四段记忆全部完成后开始返回。"
        "未解决时，容器保持关闭；完成事件触发后，容器破碎。回到记忆空间时，四个物件在设定的返回点恢复可拿取状态和"
        "重力，从而支持最终放置。",
    ),
    87: (
        "Google AI Studio Nano Banana 2 Lite generated two consistent 3D cartoon images: a tired office "
        "worker at night with medicine, and the same person asleep.",
        "Google AI Studio Nano Banana 2 Lite 用于生成两张角色一致的 3D 卡通图像：一张是疲惫的办公室职员在夜间与"
        "药物一起工作，另一张是同一人物入睡。",
    ),
    88: (
        "Deevid.ai generated a single-shot video of the same worker taking medicine, drinking water, and "
        "returning to work at a night desk.",
        "Deevid.ai 用于生成单镜头视频，内容为同一人物在夜间办公桌前服药、喝水并继续工作。",
    ),
    89: (
        "ElevenLabs generated the robot assistant voice from the written script to keep narration clear and "
        "consistent across scene transitions.",
        "ElevenLabs 根据书面对话脚本生成机器人助手语音，使叙述在场景转场中保持清晰和一致。",
    ),
    90: (
        "OpenAI Codex supported implementation analysis, debugging, rubric comparison, and report editing. "
        "Final design, asset, and submission decisions remain my own.",
        "OpenAI Codex 辅助实现分析、调试、评分标准对照和报告编辑。最终设计、资产和提交决定由我本人负责。",
    ),
}


def replace_bilingual(paragraph, english: str, chinese: str) -> None:
    if len(paragraph.runs) < 3:
        raise ValueError(
            f"Paragraph {paragraph._p.getroottree().getpath(paragraph._p)} "
            "does not have the expected bilingual run structure"
        )

    paragraph.runs[0].text = english
    paragraph.runs[1].text = "\n"
    paragraph.runs[2].text = chinese
    for run in paragraph.runs[3:]:
        run.text = ""

    chinese_run = paragraph.runs[2]
    chinese_run.font.name = "Microsoft YaHei"
    chinese_run.font.size = Pt(9.5)
    rfonts = chinese_run._element.get_or_add_rPr().get_or_add_rFonts()
    rfonts.set(qn("w:eastAsia"), "Microsoft YaHei")
    rfonts.set(qn("w:ascii"), "Microsoft YaHei")
    rfonts.set(qn("w:hAnsi"), "Microsoft YaHei")


def main() -> None:
    if not SOURCE.exists():
        raise FileNotFoundError(SOURCE)

    document = Document(SOURCE)
    for index, (english, chinese) in REPLACEMENTS.items():
        replace_bilingual(document.paragraphs[index], english, chinese)

    document.core_properties.title = "DDES9903 Assessment 2 Report - The Memory Organizer"
    document.core_properties.subject = "Bilingual revision with English manuscript under 3,000 words"

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    document.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    main()
