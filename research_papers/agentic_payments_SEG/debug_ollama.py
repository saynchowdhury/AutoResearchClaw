#!/usr/bin/env python3
"""Debug script to test Ollama API"""

import urllib.request
import json

OLLAMA_URL = "http://localhost:11434/api/generate"

payload = {
    "model": "qwen3-vl:8b",
    "prompt": 'Classify the sentiment of this text: "The product works exactly as described."\nOutput ONLY valid JSON: {"sentiment": "POS"} or {"sentiment": "NEG"} or {"sentiment": "NEU"}\nNo markdown, no explanation, no trailing text. Only the JSON object.',
    "stream": False,
    "options": {
        "temperature": 0.2,
        "num_predict": 60,
    }
}

try:
    req = urllib.request.Request(
        OLLAMA_URL,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"}
    )
    with urllib.request.urlopen(req, timeout=30) as resp:
        response_data = json.loads(resp.read().decode("utf-8"))
        raw_response = response_data.get("response", "").strip()
        print(f"Full response data: {response_data}")
        print(f"\n--- Raw response string ---")
        print(repr(raw_response))
        print(f"\n--- Attempting to parse as JSON ---")
        try:
            parsed = json.loads(raw_response)
            print(f"Parsed successfully: {parsed}")
        except json.JSONDecodeError as e:
            print(f"JSON parse error: {e}")

except Exception as e:
    print(f"Error: {e}")
