from pathlib import Path
from zipfile import ZIP_DEFLATED, ZipFile

from lxml import etree
from PIL import Image


ROOT = Path(r"E:\GitHub\9903_Zoey")
SOURCE = ROOT / "output" / "docx" / "DDES9903_A2_Report_Draft_Memory_Organizer_Bilingual.docx"
OUTPUT = (
    ROOT
    / "output"
    / "docx"
    / "DDES9903_A2_Report_Draft_Memory_Organizer_Bilingual_Actual_Outcomes.docx"
)
FIGURE = ROOT / "tmp" / "a2-bilingual" / "actual_endings_table.png"

CAPTION = (
    "Figure 2. The 21 one-sentence outcomes implemented in the final report UI. / "
    "图 2：最终报告 UI 中实际实现的 21 条单句结局。"
)

NS = {
    "w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main",
    "wp": "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing",
    "a": "http://schemas.openxmlformats.org/drawingml/2006/main",
    "r": "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
    "pr": "http://schemas.openxmlformats.org/package/2006/relationships",
}


def paragraph_text(paragraph):
    return "".join(paragraph.xpath(".//w:t/text()", namespaces=NS))


def main():
    if OUTPUT.exists():
        OUTPUT.unlink()

    with ZipFile(SOURCE, "r") as source_package:
        document_xml = source_package.read("word/document.xml")
        rels_xml = source_package.read("word/_rels/document.xml.rels")
        document = etree.fromstring(document_xml)
        relationships = etree.fromstring(rels_xml)

        caption_paragraph = next(
            paragraph
            for paragraph in document.xpath(".//w:p", namespaces=NS)
            if paragraph_text(paragraph).startswith("Figure 2.")
        )

        image_paragraph = caption_paragraph.getprevious()
        while image_paragraph is not None and not image_paragraph.xpath(".//a:blip", namespaces=NS):
            image_paragraph = image_paragraph.getprevious()
        if image_paragraph is None:
            raise RuntimeError("Could not locate the Figure 2 image paragraph.")

        blip = image_paragraph.xpath(".//a:blip", namespaces=NS)[0]
        relationship_id = blip.get(f"{{{NS['r']}}}embed")
        relationship = relationships.xpath(
            f"./pr:Relationship[@Id='{relationship_id}']",
            namespaces=NS,
        )[0]
        media_path = "word/" + relationship.get("Target").lstrip("/")

        width_emu = 24 * 360000
        with Image.open(FIGURE) as image:
            height_emu = round(width_emu * image.height / image.width)

        for extent in image_paragraph.xpath(".//wp:extent | .//a:xfrm/a:ext", namespaces=NS):
            extent.set("cx", str(width_emu))
            extent.set("cy", str(height_emu))

        doc_properties = image_paragraph.xpath(".//wp:docPr", namespaces=NS)
        if doc_properties:
            doc_properties[0].set("title", "Implemented Final Report Outcomes")
            doc_properties[0].set(
                "descr",
                "A bilingual table of the 21 one-sentence outcomes implemented for seven memory objects across Background, Context, and Focus attention.",
            )

        caption_text_nodes = caption_paragraph.xpath(".//w:t", namespaces=NS)
        if not caption_text_nodes:
            raise RuntimeError("Figure 2 caption has no text node.")
        caption_text_nodes[0].text = CAPTION
        for node in caption_text_nodes[1:]:
            node.text = ""

        updated_document_xml = etree.tostring(
            document,
            xml_declaration=True,
            encoding="UTF-8",
            standalone="yes",
        )
        figure_bytes = FIGURE.read_bytes()

        with ZipFile(OUTPUT, "w", compression=ZIP_DEFLATED) as output_package:
            for entry in source_package.infolist():
                if entry.filename == "word/document.xml":
                    data = updated_document_xml
                elif entry.filename == media_path:
                    data = figure_bytes
                else:
                    data = source_package.read(entry.filename)
                output_package.writestr(entry, data)

    print(OUTPUT)
    print(f"Replaced media part: {media_path}")


if __name__ == "__main__":
    main()
