# Paper 03: Dynamic α as a Markov Decision Process

**Status**: Planned (Future Work)  
**Emerged from**: Paper 01 — Liquidity Coupling in Autonomous Agent Networks  
**Target Venue**: NeurIPS MLSys Workshop / ICML  

---

## The Core Problem with Paper 01
Paper 01 uses a **fixed α = 0.20** for all agent pairings. This is safe and easy to prove,
but it's suboptimal. Some pairings need more protection (high-risk counterparties), others
need less (established reliable agents). Locking 20% of capital everywhere wastes resources.

## The New Contribution: Adaptive Coupling Policy
Define the **Adaptive Coupling Policy** as a Markov Decision Process where:
- **State**: Agent A_i's posterior belief about counterparty A_j's solvency type
- **Action**: The coupling parameter α_ij to deploy for this transaction
- **Reward**: Task payment p minus locked capital cost c_S(α_ij · w_ij)
- **Constraint**: Network must remain subcritical — ρ(M(1-α)) < 1

The optimal policy `π*(A_i, A_j) = α_ij*` minimizes total locked capital while
staying on the subcritical side of the stability frontier:
```
min_α  Σ_{ij} α_ij · w_ij
subject to  ρ(M(1-α)) < 1
```
This is a **convex optimization problem** that can be solved analytically using the
Perron-Frobenius dual.

## The Pareto Frontier
Paper 01's Table 1 empirically traces this frontier without naming it. Paper 03 derives
it theoretically and proves that the optimal adaptive policy dominates the fixed-α strategy
by 15-30% in capital efficiency at equal cascade safety.

## What Needs to be Done
- [ ] Formalize the MDP state/action/reward/transition structure
- [ ] Prove convexity of the optimization over the coupling matrix
- [ ] Derive the closed-form adaptive policy using Perron-Frobenius duality
- [ ] Implement a Bayesian belief updater for agent solvency estimation
- [ ] Run a comparison experiment: fixed α=0.20 vs. adaptive policy on 10,000-node sim
- [ ] Show capital efficiency improvement on the stability-efficiency frontier
