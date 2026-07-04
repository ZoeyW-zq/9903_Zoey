# DDES9903 A1 项目分析（基于 brief、rubric 和当前 Unity 项目）

## 评分要求摘要

Assessment brief 要求你提交：500-2000 字设计/过程报告 PDF、500MB 内的 Unity GitHub 仓库、可玩的 WebGL 链接。作业目标不是高精成品，而是能表达设计意图的 functional interactive storyboard。Rubric 权重是：Story Structure 30%、Immersive Design Elements 50%、Collaborative Practice 10%、Report Clarity 10%。

## 当前做得好的地方

1. Story structure 已经比较完整。项目状态机覆盖 OfficeIntro -> Crystal Ball -> Hippocampus -> GiantCrisis -> SwallowTransition -> MirrorChamber -> BreakGlass -> BackToOffice，能对应 exposition、rise、climax、falling action/resolution。

2. 叙事空间转换清晰。OfficeRoot、HippoRoot、DungeonRoot 不只是技术分组，也能解释为安全现实、记忆内部、梦魇/反思空间，对 rubric 的 Use of Space 很有帮助。

3. Immersive design evidence 很强。项目用了 XR Interaction Toolkit、自由观看/导航、3D 环境、世界内物件、音频、post-processing volume、屏幕 fade、video memory、fall motion、giant hand IK 等多模态元素，比较贴合 b1-b15。

4. embodied action 不是摆设。玩家用手靠近 crystal ball 进入记忆，被巨人抓住，经历吞咽/坠落，最后打破玻璃返回办公室。这些动作都对应故事阶段，而不是单纯小游戏交互。

5. 你对 VR comfort 有意识。ClownController 让 XR Origin 跟随 grabAnchor 的位置，但避免继承手部旋转，这能支持 b1/b2：不使用容易晕的屏幕式相机，不冻结玩家视角。

6. 技术实现能支撑报告论证。GameStateController、AssistantController、ClownController、SwallowController、MemoryContentDisplay 都可以作为“design intent became functional prototype”的证据。

## 当前弱点 / 可能扣分点

1. Collaborative Practice 目前证据不足。仓库里没有 Teams/class channel 截图、反馈记录、测试记录等。这个部分占 10%，如果不补，可能直接接近 0 分。不要只写“我和同学交流了”，rubric 要证据。

2. WebGL link 未知。Brief 明确要求 playable WebGL link。仓库里有 `scene_WebGL.unity`，但我没有看到已发布链接。没有链接会影响提交完整性，也会影响 marker 判断功能性。

3. 报告还需要真实截图/表格。Rubric 的 Report Clarity 会奖励 thoughtful use of tables and images。当前生成的 Word 草稿有结构表，但没有项目截图。建议加入：office/crystal ball、Hippocampus、giant clown roof grab、swallow/fall、mirror chamber/glass break。

4. 有一个明显代码风险：`ClownController.CrisisRoutine()` 中 `WaitForHandNear(mouthPoint, mouthArrivalDistance, mouthArrivalTimeout);` 没有 `yield return`。这意味着手到嘴的等待逻辑不会真正阻塞，timeout warning 也不会按预期工作。作为报告可不必展开，但作为项目质量建议应修。

5. SwallowTransition 和 clown grab 有潜在时序冲突。项目上下文已记录：玩家可能仍在跟随 grabAnchor，同时 SwallowController 也在移动 XR Origin。若演示中出现位置跳变，需要调整 detach 时机或在黑屏后停止 hand-follow。

6. 音频 loop 设置有风险。PROJECT_CONTEXT 已指出如果 `footstepsAudio` 被设为 Loop，crisis coroutine 会一直等待，导致 rumble/animation 不开始。提交前必须在 Unity Inspector 检查。

7. Asset / AI acknowledgements 需要更精确。README 有很多第三方链接，但最终报告最好只保留实际使用的资产，并补充许可证/来源。若 sleep.png、video.mp4、dialogue/audio 或图片由 AI 生成，也要写清楚服务、prompt 概述和编辑方式。

8. Git work history 当前有大量 unstaged/untracked changes。Brief 要 “credible work history”。提交前应整理、commit，并确认仓库大小在 500MB 内。不要把 Library/Temp 之类目录提交上去。

## 可能的 rubric 档位判断

Story Structure：当前大概可争取 Credit 到 Distinction。结构完整，而且 climax/resolution 有清楚动作；若想冲 HD，需要在报告里更明确解释每个阶段为什么 unique/interesting，并证明没有明显 pacing 或 bug 问题。

Immersive Design Elements：当前可争取 Credit 到 Distinction，潜力比 Story 更高。优势是 space、sensory、semiotic、embodied action 都有具体实现。想冲更高，需要把这些元素之间的 synergy 写清楚，而不是分开列功能。

Collaborative Practice：目前风险最高。没有截图证据时可能 Fail。补 4-8 条带日期、上下文、反馈内容的 Teams 证据，是性价比最高的提分动作。

Report Clarity：Word 草稿结构已经接近可用，但要补截图、统一 citation、删掉/替换占位符，最后导出 PDF。否则 clarity 会被 placeholders 拉低。

## 优先改进清单

1. 发布并填写 WebGL link。

2. 补 Teams/class channel 协作证据截图，并按 c1-c12 标注。

3. 给报告插入 4-6 张项目截图，每张配一句说明它对应哪个 rubric 点。

4. 修 `WaitForHandNear` 缺少 `yield return` 的问题，并检查 SwallowTransition 的 XR Origin 时序。

5. 在 Unity 里确认 footstepsAudio 不 loop、所有 scene references 都绑定、final glass break 能触发 BackToOffice。

6. 整理 git 状态并提交，保证 GitHub 仓库能展示可信工作历史。

7. 清理 Asset Citations 和 AI Acknowledgements，避免漏报或泛泛而谈。
