from pathlib import Path
import csv
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "artifacts"
OUT_DIR.mkdir(exist_ok=True)

FONT_REGULAR = "C:/Windows/Fonts/msyh.ttc"
FONT_BOLD = "C:/Windows/Fonts/msyhbd.ttc"

rows = [
    {
        "object": "Water Bottle",
        "object_zh": "水瓶",
        "Background": "Work-while-ill distress recedes, but physical warning signs may be overlooked.",
        "Background_zh": "带病工作造成的痛苦减轻，但身体发出的警告信号也可能被忽略。",
        "Context": "Illness and overwork are recognized earlier, supporting rest, medication, and help-seeking.",
        "Context_zh": "更早识别疾病与过度工作，支持休息、服药和寻求帮助。",
        "Focus": "Pain and work pressure remain highly active, reinforcing fear and resentment.",
        "Focus_zh": "疼痛与工作压力仍然高度活跃，强化恐惧与怨恨。",
    },
    {
        "object": "Sunset Photograph",
        "object_zh": "日落照片",
        "Background": "The peaceful memory remains available, but work may continue to overshadow restorative moments.",
        "Background_zh": "平静的记忆仍可被调取，但工作可能继续遮蔽恢复性的时刻。",
        "Context": "The memory supports occasional pauses without abandoning responsibilities.",
        "Context_zh": "这段记忆支持偶尔停下来，同时不必放弃责任。",
        "Focus": "Restorative moments become an active part of daily life beyond work and productivity.",
        "Focus_zh": "恢复性的时刻成为日常生活的主动组成部分，不再只围绕工作和生产力。",
    },
    {
        "object": "LEGO Bricks",
        "object_zh": "乐高积木",
        "Background": "The achievement remains accessible, but external judgment may still dominate self-worth.",
        "Background_zh": "成就仍可被想起，但外界评价仍可能主导自我价值。",
        "Context": "Private achievement balances criticism without turning the hobby into another performance test.",
        "Context_zh": "个人成就在不把爱好变成另一场表现考验的情况下，平衡外界批评。",
        "Focus": "Patient creation becomes a stable source of satisfaction and self-worth.",
        "Focus_zh": "耐心创造成为稳定的满足感与自我价值来源。",
    },
    {
        "object": "Second-Place Medal",
        "object_zh": "第二名奖牌",
        "Background": "Parental disappointment loses influence, but the achievement may also feel less personally meaningful.",
        "Background_zh": "父母的失望失去影响，但这项成就也可能变得不再具有个人意义。",
        "Context": "Effort can be valued without needing first place to prove personal worth.",
        "Context_zh": "无需用第一名证明个人价值，也可以认可自己的努力。",
        "Focus": "Ranking and disappointment remain active, keeping achievement closely tied to self-worth.",
        "Focus_zh": "排名与失望仍然活跃，使成就感继续紧密绑定自我价值。",
    },
    {
        "object": "Old Alarm Clock",
        "object_zh": "旧闹钟",
        "Background": "Guilt about rest fades, but signs of exhaustion may also receive less attention.",
        "Background_zh": "对休息的内疚减轻，但也可能较少关注疲惫的迹象。",
        "Context": "Physical limits are recognized, and rest becomes part of responsible self-care.",
        "Context_zh": "能够识别身体界限，休息成为负责任的自我照顾。",
        "Focus": "Rest must still feel earned, turning self-care into another standard to perform correctly.",
        "Focus_zh": "休息仍必须先被证明值得，使自我照顾变成另一项需要正确完成的标准。",
    },
    {
        "object": "Old Phone",
        "object_zh": "旧手机",
        "Background": "Rejection has less control over help-seeking, making trusted support easier to approach.",
        "Background_zh": "拒绝不再那么能控制求助行为，更容易接近值得信任的支持。",
        "Context": "Support feels legitimate, though trust is chosen carefully and some hesitation remains.",
        "Context_zh": "求助感到合理，但会谨慎选择信任对象，仍保留一些犹豫。",
        "Focus": "Anticipated rejection encourages overexplaining, withdrawal, and carrying pressure alone.",
        "Focus_zh": "预期中的拒绝促使过度解释、退缩，并独自承受压力。",
    },
    {
        "object": "Red Correction Pen",
        "object_zh": "红色批改笔",
        "Background": "Past humiliation no longer defines ability, though public criticism may still feel destabilizing.",
        "Background_zh": "过去的羞辱不再定义能力，但公开批评仍可能造成动摇。",
        "Context": "Mistakes can be reviewed and corrected without becoming proof of total incompetence.",
        "Context_zh": "可以复盘和改正错误，而不把错误视为完全无能的证明。",
        "Focus": "Mistakes and authority judgment remain highly active, encouraging perfectionism and repeated checking.",
        "Focus_zh": "错误与权威评价仍高度活跃，促使完美主义和反复检查。",
    },
]


def font(size, bold=False):
    path = FONT_BOLD if bold and Path(FONT_BOLD).exists() else FONT_REGULAR
    return ImageFont.truetype(path, size)


def wrap(draw, text, fnt, max_width):
    words = text.split()
    lines = []
    line = ""
    for word in words:
        trial = word if not line else line + " " + word
        if draw.textbbox((0, 0), trial, font=fnt)[2] <= max_width:
            line = trial
        else:
            if line:
                lines.append(line)
            line = word
    if line:
        lines.append(line)
    return lines


def wrap_cjk(draw, text, fnt, max_width):
    lines = []
    line = ""
    for char in text:
        trial = line + char
        if draw.textbbox((0, 0), trial, font=fnt)[2] <= max_width:
            line = trial
        else:
            if line:
                lines.append(line)
            line = char
    if line:
        lines.append(line)
    return lines


def draw_cell(draw, xy, en, zh, width, fill, border, padding=22):
    x0, y0, x1, y1 = xy
    draw.rectangle(xy, fill=fill, outline=border, width=2)
    en_font = font(24)
    zh_font = font(23)
    en_lines = wrap(draw, en, en_font, width - padding * 2)
    zh_lines = wrap_cjk(draw, zh, zh_font, width - padding * 2)
    line_gap = 8
    en_h = sum(draw.textbbox((0, 0), s, font=en_font)[3] for s in en_lines) + line_gap * max(0, len(en_lines) - 1)
    zh_h = sum(draw.textbbox((0, 0), s, font=zh_font)[3] for s in zh_lines) + line_gap * max(0, len(zh_lines) - 1)
    total_h = en_h + 18 + zh_h
    y = y0 + max(padding, (y1 - y0 - total_h) // 2)
    for line in en_lines:
        draw.text((x0 + padding, y), line, font=en_font, fill="#17212b")
        y += draw.textbbox((0, 0), line, font=en_font)[3] + line_gap
    y += 10
    for line in zh_lines:
        draw.text((x0 + padding, y), line, font=zh_font, fill="#44515c")
        y += draw.textbbox((0, 0), line, font=zh_font)[3] + line_gap


def main():
    W = 3600
    title_h = 180
    legend_h = 130
    header_h = 110
    row_h = 300
    left_w = 430
    col_w = (W - left_w) // 3
    H = title_h + legend_h + header_h + row_h * len(rows) + 50
    img = Image.new("RGB", (W, H), "#f7f8fa")
    draw = ImageDraw.Draw(img)
    draw.text((60, 38), "Figure 2. Actual Memory Outcomes by Attention Allocation", font=font(44, True), fill="#15202b")
    draw.text((60, 102), "图 2：不同注意力分配下的实际记忆结局", font=font(32), fill="#52616d")

    legend_y = title_h + 18
    legend_items = [("Background", "背景注意"), ("Context", "情境注意"), ("Focus", "焦点注意")]
    legend_colors = ["#e8eef4", "#e6f0ec", "#f5e9e7"]
    x = 60
    for (en, zh), color in zip(legend_items, legend_colors):
        draw.rounded_rectangle((x, legend_y, x + 380, legend_y + 72), radius=12, fill=color, outline="#c4cdd5", width=2)
        draw.text((x + 22, legend_y + 12), en, font=font(25, True), fill="#20303c")
        draw.text((x + 22, legend_y + 42), zh, font=font(21), fill="#52616d")
        x += 410

    table_y = title_h + legend_h
    x_positions = [0, left_w, left_w + col_w, left_w + col_w * 2, W]
    headers = [("Memory object", "记忆物体"), ("Background", "背景注意"), ("Context", "情境注意"), ("Focus", "焦点注意")]
    header_fills = ["#263746", "#456075", "#3d6b5b", "#8a5148"]
    for i, ((en, zh), fill) in enumerate(zip(headers, header_fills)):
        x0, x1 = x_positions[i], x_positions[i + 1]
        draw.rectangle((x0, table_y, x1, table_y + header_h), fill=fill, outline="#ffffff", width=2)
        bbox = draw.textbbox((0, 0), en, font=font(27, True))
        draw.text((x0 + (x1 - x0 - (bbox[2] - bbox[0])) // 2, table_y + 15), en, font=font(27, True), fill="#ffffff")
        bbox = draw.textbbox((0, 0), zh, font=font(22))
        draw.text((x0 + (x1 - x0 - (bbox[2] - bbox[0])) // 2, table_y + 58), zh, font=font(22), fill="#e8edf1")

    for r, row in enumerate(rows):
        y0 = table_y + header_h + r * row_h
        y1 = y0 + row_h
        fill = "#ffffff" if r % 2 == 0 else "#f1f4f6"
        draw.rectangle((0, y0, left_w, y1), fill=fill, outline="#c8d0d7", width=2)
        draw.text((28, y0 + 88), row["object"], font=font(27, True), fill="#1c2d39")
        draw.text((28, y0 + 134), row["object_zh"], font=font(24), fill="#52616d")
        for c, key in enumerate(("Background", "Context", "Focus")):
            x0, x1 = x_positions[c + 1], x_positions[c + 2]
            draw_cell(draw, (x0, y0, x1, y1), row[key], row[key + "_zh"], x1 - x0, legend_colors[c], "#c8d0d7")

    draw.text((60, H - 38), "Source: FinalReportController.cs (implemented game outcomes)", font=font(20), fill="#6a7782")
    png_path = OUT_DIR / "Figure_2_Actual_Memory_Outcomes_Bilingual.png"
    img.save(png_path, dpi=(200, 200))

    csv_path = OUT_DIR / "Figure_2_Actual_Memory_Outcomes_Bilingual.csv"
    with csv_path.open("w", newline="", encoding="utf-8-sig") as f:
        writer = csv.writer(f)
        writer.writerow(["Memory object", "中文名称", "Background (EN)", "背景注意（中）", "Context (EN)", "情境注意（中）", "Focus (EN)", "焦点注意（中）"])
        for row in rows:
            writer.writerow([row["object"], row["object_zh"], row["Background"], row["Background_zh"], row["Context"], row["Context_zh"], row["Focus"], row["Focus_zh"]])
    print(png_path)
    print(csv_path)


if __name__ == "__main__":
    main()
