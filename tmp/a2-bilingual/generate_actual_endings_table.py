from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(r"E:\GitHub\9903_Zoey")
SOURCE = ROOT / "Assets" / "__My Project" / "scripts" / "FinalReportController.cs"
OUTPUT = ROOT / "tmp" / "a2-bilingual" / "actual_endings_table.png"

OUTCOMES = [
    (
        "Water Bottle",
        "Recent memory / 近期记忆",
        (
            "Work-while-ill distress recedes, but physical warning signs may be overlooked.",
            "带病工作的痛苦感减弱，但身体发出的警告信号可能被忽视。",
        ),
        (
            "Illness and overwork are recognized earlier, supporting rest, medication, and help-seeking.",
            "能够更早识别疾病与过度工作，从而支持休息、服药和寻求帮助。",
        ),
        (
            "Pain and work pressure remain highly active, reinforcing fear and resentment.",
            "疼痛与工作压力持续高度活跃，进一步强化恐惧和怨愤。",
        ),
    ),
    (
        "Sunset Photograph",
        "Recent memory / 近期记忆",
        (
            "The peaceful memory remains available, but work may continue to overshadow restorative moments.",
            "这段平静记忆仍可被唤起，但工作仍可能遮蔽具有修复作用的时刻。",
        ),
        (
            "The memory supports occasional pauses without abandoning responsibilities.",
            "这段记忆支持偶尔停下来，同时不放弃应承担的责任。",
        ),
        (
            "Restorative moments become an active part of daily life beyond work and productivity.",
            "修复性的时刻成为日常生活的主动部分，使生活不再只有工作与产出。",
        ),
    ),
    (
        "LEGO Bricks",
        "Recent memory / 近期记忆",
        (
            "The achievement remains accessible, but external judgment may still dominate self-worth.",
            "这份成就感仍可被唤起，但外部评价可能继续主导自我价值。",
        ),
        (
            "Private achievement balances criticism without turning the hobby into another performance test.",
            "个人成就感能够平衡批评，同时不会把爱好变成另一场表现测试。",
        ),
        (
            "Patient creation becomes a stable source of satisfaction and self-worth.",
            "耐心创造成为满足感与自我价值的稳定来源。",
        ),
    ),
    (
        "Second-Place Medal",
        "Deep memory / 深层记忆",
        (
            "Parental disappointment loses influence, but the achievement may also feel less personally meaningful.",
            "父母的失望影响减弱，但这项成就对本人而言也可能变得不那么重要。",
        ),
        (
            "Effort can be valued without needing first place to prove personal worth.",
            "努力本身可以被肯定，不必用第一名来证明个人价值。",
        ),
        (
            "Ranking and disappointment remain active, keeping achievement closely tied to self-worth.",
            "排名与失望仍然活跃，使成就继续与自我价值紧密绑定。",
        ),
    ),
    (
        "Old Alarm Clock",
        "Deep memory / 深层记忆",
        (
            "Guilt about rest fades, but signs of exhaustion may also receive less attention.",
            "休息带来的内疚减弱，但疲惫信号也可能得到更少关注。",
        ),
        (
            "Physical limits are recognized, and rest becomes part of responsible self-care.",
            "身体界限得到承认，休息成为负责任自我照顾的一部分。",
        ),
        (
            "Rest must still feel earned, turning self-care into another standard to perform correctly.",
            "休息仍必须先被赚取，使自我照顾变成另一项必须正确完成的标准。",
        ),
    ),
    (
        "Old Phone",
        "Deep memory / 深层记忆",
        (
            "Rejection has less control over help-seeking, making trusted support easier to approach.",
            "拒绝对求助行为的控制减弱，使可靠的支持更容易被接近。",
        ),
        (
            "Support feels legitimate, though trust is chosen carefully and some hesitation remains.",
            "寻求支持被视为合理，但信任对象仍会被谨慎选择，也保留一些犹豫。",
        ),
        (
            "Anticipated rejection encourages overexplaining, withdrawal, and carrying pressure alone.",
            "对拒绝的预期促使过度解释、退缩，并独自承担压力。",
        ),
    ),
    (
        "Red Correction Pen",
        "Deep memory / 深层记忆",
        (
            "Past humiliation no longer defines ability, though public criticism may still feel destabilizing.",
            "过去的羞辱不再定义能力，但公开批评仍可能带来动摇。",
        ),
        (
            "Mistakes can be reviewed and corrected without becoming proof of total incompetence.",
            "错误可以被检查和修正，而不会成为完全无能的证明。",
        ),
        (
            "Mistakes and authority judgment remain highly active, encouraging perfectionism and repeated checking.",
            "错误与权威评价持续高度活跃，促使完美主义和反复检查。",
        ),
    ),
]

WIDTH = 3000
MARGIN_X = 54
TITLE_TOP = 38
TABLE_TOP = 212
COLUMN_WIDTHS = [430, 820, 820, 820]
HEADER_HEIGHT = 118
CELL_PAD_X = 24
CELL_PAD_Y = 18

NAVY = "#22384A"
TEXT = "#17212B"
MUTED = "#59697A"
GRID = "#BCC7D1"
BACKGROUND = (86, 122, 142)
CONTEXT = (73, 127, 105)
FOCUS = (166, 97, 76)
RECENT = "#2D789E"
DEEP = "#745194"


def load_font(filename, size):
    return ImageFont.truetype(str(Path(r"C:\Windows\Fonts") / filename), size)


FONT_TITLE = load_font("msyhbd.ttc", 54)
FONT_SUBTITLE = load_font("msyh.ttc", 27)
FONT_HEADER = load_font("msyhbd.ttc", 31)
FONT_OBJECT = load_font("arialbd.ttf", 31)
FONT_TYPE = load_font("msyh.ttc", 24)
FONT_EN = load_font("arial.ttf", 29)
FONT_CN = load_font("msyh.ttc", 28)


def tint(rgb, amount=0.91):
    return tuple(round(channel + (255 - channel) * amount) for channel in rgb)


def wrap(draw, text, font, max_width):
    if not text:
        return []

    if " " not in text:
        lines = []
        current = ""
        for char in text:
            candidate = current + char
            if current and draw.textlength(candidate, font=font) > max_width:
                lines.append(current)
                current = char
            else:
                current = candidate
        if current:
            lines.append(current)
        return lines

    lines = []
    current = ""
    for word in text.split():
        candidate = word if not current else f"{current} {word}"
        if current and draw.textlength(candidate, font=font) > max_width:
            lines.append(current)
            current = word
        else:
            current = candidate
    if current:
        lines.append(current)
    return lines


def measure_outcome(draw, english, chinese, width):
    available = width - 2 * CELL_PAD_X
    en_lines = wrap(draw, english, FONT_EN, available)
    cn_lines = wrap(draw, chinese, FONT_CN, available)
    height = len(en_lines) * 37 + 10 + len(cn_lines) * 38
    return en_lines, cn_lines, height


def verify_source_text():
    source_text = SOURCE.read_text(encoding="utf-8")
    missing = []
    for row in OUTCOMES:
        for english, _ in row[2:]:
            if english not in source_text:
                missing.append(english)
    if missing:
        raise RuntimeError("Outcome text no longer matches FinalReportController.cs: " + repr(missing))


def main():
    verify_source_text()

    scratch = Image.new("RGB", (WIDTH, 100), "white")
    scratch_draw = ImageDraw.Draw(scratch)
    measured_rows = []
    for row in OUTCOMES:
        cell_measurements = [
            measure_outcome(scratch_draw, english, chinese, COLUMN_WIDTHS[index + 1])
            for index, (english, chinese) in enumerate(row[2:])
        ]
        object_height = 76
        content_height = max(object_height, *(measurement[2] for measurement in cell_measurements))
        row_height = max(174, content_height + 2 * CELL_PAD_Y)
        measured_rows.append((row, cell_measurements, row_height))

    table_height = HEADER_HEIGHT + sum(row[2] for row in measured_rows)
    height = TABLE_TOP + table_height + 48
    image = Image.new("RGB", (WIDTH, height), "#F4F6F8")
    draw = ImageDraw.Draw(image)

    draw.text(
        (MARGIN_X, TITLE_TOP),
        "Implemented Final Report Outcomes / 实际实现的最终报告结局",
        font=FONT_TITLE,
        fill=TEXT,
    )
    draw.text(
        (MARGIN_X, TITLE_TOP + 73),
        "English sentences match the current in-game UI; Chinese text is a report translation. / 英文为当前游戏内实际文本，中文为报告对照翻译。",
        font=FONT_SUBTITLE,
        fill=MUTED,
    )

    x_positions = [MARGIN_X]
    for width in COLUMN_WIDTHS:
        x_positions.append(x_positions[-1] + width)

    headers = [
        ("Memory object\n记忆物体", NAVY),
        ("BACKGROUND\n背景注意", BACKGROUND),
        ("CONTEXT\n语境注意", CONTEXT),
        ("FOCUS\n焦点注意", FOCUS),
    ]
    for index, (label, colour) in enumerate(headers):
        x0, x1 = x_positions[index], x_positions[index + 1]
        draw.rectangle((x0, TABLE_TOP, x1, TABLE_TOP + HEADER_HEIGHT), fill=colour, outline=GRID, width=2)
        line1, line2 = label.split("\n")
        draw.text((x0 + CELL_PAD_X, TABLE_TOP + 23), line1, font=FONT_HEADER, fill="white")
        draw.text((x0 + CELL_PAD_X, TABLE_TOP + 65), line2, font=FONT_HEADER, fill="white")

    y = TABLE_TOP + HEADER_HEIGHT
    for row_index, (row, measurements, row_height) in enumerate(measured_rows):
        name, kind = row[:2]
        is_recent = kind.startswith("Recent")
        object_fill = "#EDF5F9" if is_recent else "#F4EFF8"
        stripe = RECENT if is_recent else DEEP
        body_fills = [tint(BACKGROUND), tint(CONTEXT), tint(FOCUS)]

        draw.rectangle(
            (x_positions[0], y, x_positions[1], y + row_height),
            fill=object_fill,
            outline=GRID,
            width=2,
        )
        draw.rectangle((x_positions[0], y, x_positions[0] + 10, y + row_height), fill=stripe)
        draw.text((x_positions[0] + CELL_PAD_X, y + CELL_PAD_Y), name, font=FONT_OBJECT, fill=TEXT)
        draw.text((x_positions[0] + CELL_PAD_X, y + CELL_PAD_Y + 48), kind, font=FONT_TYPE, fill=stripe)

        for column_index, ((english, chinese), measurement) in enumerate(zip(row[2:], measurements), start=1):
            x0, x1 = x_positions[column_index], x_positions[column_index + 1]
            draw.rectangle((x0, y, x1, y + row_height), fill=body_fills[column_index - 1], outline=GRID, width=2)
            en_lines, cn_lines, _ = measurement
            text_y = y + CELL_PAD_Y
            for line in en_lines:
                draw.text((x0 + CELL_PAD_X, text_y), line, font=FONT_EN, fill=TEXT)
                text_y += 37
            text_y += 10
            for line in cn_lines:
                draw.text((x0 + CELL_PAD_X, text_y), line, font=FONT_CN, fill=MUTED)
                text_y += 38

        y += row_height

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image.save(OUTPUT, optimize=True)
    print(OUTPUT)


if __name__ == "__main__":
    main()
