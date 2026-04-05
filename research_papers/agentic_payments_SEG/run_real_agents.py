import urllib.request
import json
import time
import statistics

OLLAMA_URL = "http://localhost:11434/api/generate"
MODEL_NAME = "qwen3-vl:8b"

# Simulation parameters
NUM_CHAINS = 20
CHAIN_LENGTH = 10     # Max depth of the pipeline
ALPHA = 0.20          # Liquidity Coupling parameter
BASE_CAPITAL = 1.0    # Capital per agent
PAYMENT_AMT = 0.5     # Payment size per hop

empirical_failures = []

def call_agent(prompt_text):
    """Hits the local Ollama API to simulate an agent performing a task."""
    data = {"model": MODEL_NAME, "prompt": prompt_text, "stream": False, "options": {"temperature": 0.8, "num_predict": 20}}
    req = urllib.request.Request(OLLAMA_URL, data=json.dumps(data).encode('utf-8'), headers={'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=10) as response:
            res = json.loads(response.read().decode('utf-8'))
            return res.get('response', '')
    except Exception as e:
        return ""

def process_chain():
    """Runs a pipeline of agents until completion or an LLM failure."""
    for step in range(CHAIN_LENGTH):
        # We ask the LLM to perform a simple task. We intentionally add ambiguity to induce natural failures.
        prompt = f"Agent {step}, output ONLY valid JSON containing a key 'status' with value 'ok'. Nothing else. No markdown."
        output = call_agent(prompt)
        
        # Did it fail?
        failed = False
        try:
            parsed = json.loads(output.strip('` \n'))
            if parsed.get('status') != 'ok':
                failed = True
        except:
            failed = True
            
        if failed:
            return step # Failed at this hop
    return CHAIN_LENGTH # Success

print(f"Running {NUM_CHAINS} agent pipelines using local {MODEL_NAME}...")
for i in range(NUM_CHAINS):
    sys_fail_hop = process_chain()
    empirical_failures.append(sys_fail_hop)
    print(f"Chain {i+1}/{NUM_CHAINS} -> Reached hop {sys_fail_hop}")

# Analyze the cascade depth
failure_rates = [1 if hop < CHAIN_LENGTH else 0 for hop in empirical_failures]
tcr = 1.0 - (sum(failure_rates) / NUM_CHAINS)

def simulate_cascade(chain_failure_hop, alpha):
    """Computes cascade depth based on the protocol rules."""
    if chain_failure_hop == CHAIN_LENGTH:
        return 0 # No cascade
    
    # Financial simulation of the cascade going backwards
    unresolved_debt = PAYMENT_AMT
    hops_propagated = 0
    # Cascade propagates upstream
    for i in range(chain_failure_hop, 0, -1):
        hops_propagated += 1
        escrow_protection = PAYMENT_AMT * alpha
        unresolved_debt -= escrow_protection
        if unresolved_debt <= 0.05: # Minimum insolvability threshold epsilon
            break
            
    return hops_propagated

bs_depths = [simulate_cascade(h, 0.0) for h in empirical_failures if h < CHAIN_LENGTH]
seg_depths = [simulate_cascade(h, ALPHA) for h in empirical_failures if h < CHAIN_LENGTH]

avg_bs = statistics.mean(bs_depths) if bs_depths else 0
avg_seg = statistics.mean(seg_depths) if seg_depths else 0

print("\n=== EMPIRICAL RESULTS (Local LLMs) ===")
print(f"Task Completion Rate (TCR): {tcr*100:.1f}%")
print(f"Avg Cascade Depth (Bilateral, α=0): {avg_bs:.1f} hops")
print(f"Avg Cascade Depth (SEG, α={ALPHA}): {avg_seg:.1f} hops")
print(f"Cascade Reduction: {((avg_bs - avg_seg)/avg_bs*100) if avg_bs > 0 else 0:.1f}%")
