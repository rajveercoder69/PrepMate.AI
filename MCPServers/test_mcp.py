import requests
import json

# Test the hello tool
data = {
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
        "name": "hello",
        "arguments": {
            "name": "Alice"
        }
    }
}

response = requests.post("http://localhost:8080", json=data)
print("Status Code:", response.status_code)
print("Response:", response.text)