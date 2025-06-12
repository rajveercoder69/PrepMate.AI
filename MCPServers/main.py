import io
import fitz  # PyMuPDF
import pytesseract
import httpx
from PIL import Image
import requests
from mcp.server.fastmcp import FastMCP

# Initialize FastMCP server
mcp = FastMCP("MCPServer_Pdf")
CHUNK_SIZE = 4000  # Number of characters per chunk

def extract_text_from_page(page) -> str:
    """Attempt to extract text from page, fallback to OCR if empty."""
    text = page.get_text().strip()
    if text:
        return text

    # Fallback: Render image and run OCR
    pix = page.get_pixmap(dpi=300)
    image = Image.open(io.BytesIO(pix.tobytes("png")))
    ocr_text = pytesseract.image_to_string(image)
    return ocr_text.strip()

@mcp.tool()
def hello(name: str) -> str:
    return f"Hello, {name}!"

@mcp.tool()
def fetch_list_pdf(token: str, person_id: int, category: str) -> dict:
    """
    Fetch list of PDFs for a given person ID and category.
    Calls your .NET backend and returns PDF metadata (id, title, category, created date).
    """

    api_url = f"https://localhost:7032/PersonDetails/PdfListforMcp?personId={person_id}&category={category}"

    headers = {
        "Authorization": f"Bearer {token}"  # Optional — only if your API checks it
    }

    try:
        response = requests.get(api_url, headers=headers, verify=False)  # verify=False only if localhost SSL is self-signed
        response.raise_for_status()
        data = response.json()

        if not data.get("isSuccess", False):
            return {"error": data.get("error", "Unknown error occurred."), "result": []}

        # Optionally format output if needed
        return {
            "pdfs": data["result"],
            "count": len(data["result"]),
            "category": category
        }

    except requests.exceptions.HTTPError as e:
        return {"error": f"HTTP error: {e.response.status_code} - {e.response.text}"}
    except Exception as ex:
        return {"error": f"Failed to fetch PDF list: {str(ex)}"}

@mcp.tool()
def fetch_and_extract_pdf_text(token: str, person_id: int, file_id: int, chunk_index: int = 0) -> dict:
    """
    Fetch a PDF via API and return the extracted text in a specific chunk.
    Supports OCR for image-based PDFs.
    """
    api_url = f"https://localhost:7032/PersonDetails/fileForMCP?personId={person_id}&fileId={file_id}"
    headers = {"Authorization": f"Bearer {token}"}

    try:
        response = requests.get(api_url, headers=headers, verify=False)
        response.raise_for_status()

        pdf_data = io.BytesIO(response.content)
        doc = fitz.open(stream=pdf_data, filetype="pdf")

        full_text = ""
        for page in doc:
            extracted = extract_text_from_page(page)
            full_text += extracted + "\n"

        doc.close()

        if not full_text.strip():
            return {
                "chunk": "",
                "done": True,
                "next_chunk": None,
                "message": "No text could be extracted from the PDF."
            }

        chunks = [full_text[i:i+CHUNK_SIZE] for i in range(0, len(full_text), CHUNK_SIZE)]

        if chunk_index >= len(chunks):
            return {
                "chunk": "",
                "done": True,
                "next_chunk": None,
                "message": "All chunks sent."
            }

        return {
            "chunk": chunks[chunk_index],
            "done": chunk_index + 1 >= len(chunks),
            "next_chunk": chunk_index + 1 if chunk_index + 1 < len(chunks) else None,
            "message": f"Chunk {chunk_index + 1} of {len(chunks)}"
        }

    except requests.exceptions.HTTPError as e:
        return {
            "chunk": "",
            "done": True,
            "next_chunk": None,
            "message": f"HTTP error: {e.response.status_code} - {e.response.text}"
        }
    except Exception as ex:
        return {
            "chunk": "",
            "done": True,
            "next_chunk": None,
            "message": f"Error extracting PDF: {str(ex)}"
        }

@mcp.tool()
def stream_all_pdf_chunks(token: str, person_id: int, file_id: int) -> str:
    """
    Fetch all chunks from a PDF and return full text.
    """
    full_text = ""
    chunk_index = 0

    while True:
        result = fetch_and_extract_pdf_text(token, person_id, file_id, chunk_index)

        if "chunk" not in result:
            return f"Error: {result.get('message', 'Unexpected error')}"

        full_text += result["chunk"]

        if result.get("done", True):
            break

        chunk_index = result.get("next_chunk", chunk_index + 1)

    return full_text
    
if __name__ == "__main__":
    print("Starting MCP Server...")
    try:
        print("Server initializing...")
        mcp.run()  # This blocks here while server runs
    except KeyboardInterrupt:
        print("Server stopped by user")
    except Exception as e:
        print(f"Server error: {e}")
    finally:
        print("Server shutdown complete")
