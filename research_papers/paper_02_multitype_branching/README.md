# Paper 02: Multi-Type Branching Process for Heterogeneous Agent Networks

**Status**: Planned (Future Work)  
**Emerged from**: Paper 01 — Liquidity Coupling in Autonomous Agent Networks  
**Target Venue**: ACM Economics & Computation (EC) / AAMAS  

---

## The Core Problem with Paper 01
Paper 01 (Liquidity Coupling) proves stability using a **scalar λ** — a single mean
downstream obligation rate. This is valid for homogeneous networks where every agent
class fans out similarly. It breaks for real deployments.

Real agent pipelines are **heterogeneous**: orchestrator agents route to 5-10 sub-agents,
while leaf worker agents branch to zero. The aggregate λ can *look* safe (λ_avg ≈ 1.3)
while the cascade still explodes through orchestrator hubs.

## The New Contribution: Perron-Frobenius Branching
Let M be the **mean offspring matrix** where M[i,j] = expected number of type-j offspring
from a type-i agent. Replace Theorem 4.1 of Paper 01 with:

**Theorem (Multi-Type Stability)**: The insolvency cascade halts almost surely if and only if
the spectral radius of the coupled matrix satisfies `ρ(M(1-α)) < 1`.

This gives a **type-specific threshold**:
```
α > 1 - 1/ρ(M)
```
This collapses to Paper 01's result when M = λI, but for real hub-spoke networks the
spectral radius can be 3-5x larger than the naive λ average, meaning the "safe" α
deployed by Paper 01 can be dangerously insufficient.

## What Needs to be Done
- [ ] Formalize the multi-type agent class taxonomy (router, worker, leaf, oracle)
- [ ] Derive the M matrix from real Swarms.ai deployment graphs
- [ ] Prove the Perron-Frobenius stability theorem with Liquidity Coupling
- [ ] Run experiments with a heterogeneous pipeline (mix of hub and leaf agents)
- [ ] Compare empirical cascade behaviors with the scalar vs spectral bound
