import urllib.request
import urllib.error
import json

data = {"model": "qwen3-vl:8b", "prompt": "return {\"status\": \"ok\"}", "stream": False}
req = urllib.request.Request("http://localhost:11434/api/generate", data=json.dumps(data).encode('utf-8'), headers={'Content-Type': 'application/json'})

try:
    with urllib.request.urlopen(req, timeout=30) as response:
        print(response.read().decode('utf-8'))
except Exception as e:
    print(f"Error: {e}")
