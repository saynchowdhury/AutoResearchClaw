"""PDF parsing integration using opendataloader-pdf.

This module provides high-fidelity conversion of research PDFs into Markdown
formats suitable for LLM analysis, preserving tables, reading order, and math.
"""

import logging
from pathlib import Path
from typing import Optional

try:
    from opendataloader import convert
except ImportError:
    convert = None

logger = logging.getLogger(__name__)

def parse_pdf_to_markdown(pdf_path: str | Path, output_dir: Optional[str | Path] = None) -> str:
    """Convert a PDF file to a high-quality Markdown string.
    
    Parameters
    ----------
    pdf_path : str | Path
        Path to the source PDF file.
    output_dir : Optional[str | Path], optional
        Directory to save the converted files. If None, uses a temporary or sibling dir.
        
    Returns
    -------
    str
        The content of the converted Markdown file.
    """
    if convert is None:
        logger.error("opendataloader-pdf is not installed. Run 'pip install opendataloader-pdf'.")
        return ""
    
    pdf_path = Path(pdf_path)
    if not pdf_path.exists():
        logger.error(f"PDF file not found: {pdf_path}")
        return ""
    
    # If no output_dir, use same directory as PDF
    if output_dir is None:
        output_dir = pdf_path.parent / "parsed_content"
    
    output_dir = Path(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    
    logger.info(f"Parsing PDF: {pdf_path} -> {output_dir}")
    
    try:
        # opendataloader-pdf CLI/API tool
        # Note: Depending on the specific version, this call might vary.
        # This implementation assumes the documented 'convert' function.
        convert(
            input_path=str(pdf_path),
            output_path=str(output_dir),
            formats=["markdown"]
        )
        
        # OpenDataLoader typically creates a folder or file with the same name as the PDF
        md_file = output_dir / f"{pdf_path.stem}.md"
        if md_file.exists():
            return md_file.read_text(encoding="utf-8")
        
        # Search for any .md file in the output dir if the naming differs
        md_files = list(output_dir.glob("*.md"))
        if md_files:
            return md_files[0].read_text(encoding="utf-8")
            
        return ""
        
    except Exception as e:
        logger.error(f"Failed to parse PDF {pdf_path}: {e}")
        return ""

if __name__ == "__main__":
    # Test script
    import sys
    if len(sys.argv) > 1:
        content = parse_pdf_to_markdown(sys.argv[1])
        print(content[:500] + "...")
    else:
        print("Usage: python pdf_parser.py <path_to_pdf>")
