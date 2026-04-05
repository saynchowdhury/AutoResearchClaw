# Paper 01: Liquidity Coupling in Autonomous Agent Networks

**Status**: ✅ Production Ready — v5.2  
**Author**: Sayan Mallick Chowdhury (Independent Researcher)  
**Venue**: arXiv cs.GT + cs.MA (ready to submit). Workshop at NeurIPS/ICML 2026.

---

## What This Paper Is

This paper introduces **Liquidity Coupling** — the first formal mechanism for preventing insolvency cascades in autonomous agent economies. When AI agents hire each other to perform tasks and pay each other automatically, a single agent failure can collapse an entire multi-hop pipeline. This paper proves the exact conditions under which that collapse is mathematically impossible.

## Core Contribution: The Stability Threshold

$$\alpha > 1 - \frac{1}{\lambda}$$

If every agent stakes at least this fraction of capital when extending credit, insolvency cascades halt *almost surely* (proven via Galton-Watson branching processes).

Empirically tested on real 10-node LLM pipelines (qwen3-vl:8b via Ollama): **64% reduction in cascade propagation depth**.

## File Structure

```
paper.tex          — arXiv-ready LaTeX source (v5.2)
paper.pdf          — Compiled 8-page PDF (final)
references.bib     — 22 verified citations with correct authors
paper.md           — Markdown version of the paper
abstract.md        — Standalone abstract
run_real_agents.py — Empirical experiment script (10-node LLM pipeline)
apply_fixes.py     — Batch LaTeX fix utility (used during revision)
compile.bat        — One-click LaTeX compile script
```

## Version History

| Version | Change |
|---------|--------|
| v1 | Initial draft — generic agentic payment protocol |
| v2 | Repositioned around Liquidity Coupling mechanism |
| v3 | Added PBE game theory, Galton-Watson branching, Sybil cartel analysis |
| v4 | Added empirical LLM test logs, benchmark table vs Swarms.ai / Conway |
| v5 | Fixed PBE one-shot deviation (reputation slashing), decoupled empirical numbers, restored α sweep, formalized ψ(C), added APS Triad table, corrected all reference authors |
| **v5.2** | **Added Cho-Kreps pooling equilibrium elimination, Corollary 4.2 (scale-free stability break)** |

## Mathematical Contributions

1. **Theorem 4.1** (Cascade Halting): `α > 1 - 1/λ` → cascade halts almost surely  
2. **Corollary 4.2** (Scale-Free): Power-law topologies require hub-priority coupling  
3. **Theorem 6.1** (PBE Signal): Liquidity Coupling is a credible solvency signal  
4. **Equilibrium Uniqueness**: Cho-Kreps Intuitive Criterion eliminates pooling equilibria  
5. **Theorem 8.1** (Collusion): Sybil cartel MEV attacks are unprofitable via ψ(C)

## Related Future Papers

| Paper | Topic | Folder |
|-------|-------|--------|
| Paper 02 | Multi-type branching process (Perron-Frobenius) | `../paper_02_multitype_branching/` |
| Paper 03 | Dynamic α as an MDP control problem | `../paper_03_dynamic_alpha_mdp/` |
| Paper 04 | Cascade extinction as optimal stopping | `../paper_04_optimal_stopping/` |

## How to Compile

```powershell
.\compile.bat
```

Requires MiKTeX (installed). Output: `paper.pdf`.

## How to Submit to arXiv

1. Create account at arXiv.org
2. Submit to categories: `cs.GT` (Game Theory), `cs.MA` (Multi-Agent Systems)
3. Seek endorsement from a faculty member in these areas first
4. Upload `paper.tex` + `references.bib` together (do NOT upload compiled PDF)
