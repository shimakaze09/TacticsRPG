# Delivery roadmap

**Updated:** 2026-08-10  
**Goal:** finish one coherent phase and feature group at a time instead of
pulling unrelated work from a flat backlog.

GitHub Issues is the only operational backlog. This document defines delivery
gates and how to read the labels; it does not duplicate implementable task
lists. Product intent belongs in [GDD.md](GDD.md), current observations in
[PROJECT_REVIEW.md](PROJECT_REVIEW.md), and engineering rules in
[ARCHITECTURE.md](ARCHITECTURE.md).

## Work-selection rule

1. Select the lowest numbered phase that has unfinished blockers or critical
   work.
2. Within that phase, finish one `group:*` cluster before changing groups.
3. Within the group, take `priority: blocker` / `critical` / `high` first.
4. Use `weight:*` to keep a pull request reviewable; split 13-point epics into
   child issues before implementation.
5. Do not implement an issue that is missing any classification label.

[View all open issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen)

## Phase gates

### Phase 0 — Stabilize

[Open phase-0 issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+0-stabilize%22)

Exit when combat/progression invariants are trustworthy, critical data and
lifecycle defects are resolved, generated content can be validated from a
clean checkout, and the balance report has stable input formulas.

### Phase 1 — Core loop

[Open phase-1 issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+1-core-loop%22)

Exit when a fresh build completes Title → Hub → Battle → Results → Hub twice,
then saves, quits, and continues with the same roster, rewards, jobs, and
equipment. Scene/UI/persistence ownership must survive the second loop.

### Phase 2 — Vertical slice

[Open phase-2 issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+2-vertical-slice%22)

Exit when Chapter 1's six battles, story delivery, side-content seed, meta
screens, input, art/audio first pass, save slots, and quantitative balance gate
form a shippable demo. The slice should establish the revised story identity
before later campaign content is authored.

### Phase 3 — Tactical identity

[Open phase-3 issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+3-tactical-identity%22)

Exit when the existing jobs and gear have setting-native decision loops and
the battle layer supports the selected shared mechanics—timeline, reactions,
terrain/coverage, deployables, evidence, and contract clauses—without
content-specific engine branches.

### Phase 4 — Campaign

[Open phase-4 issues](https://github.com/shimakaze09/TacticsRPG/issues?q=is%3Aissue+is%3Aopen+label%3A%22phase%3A+4-campaign%22)

Exit when Acts 2–3, endings, remaining quests/relics, post-slice presentation,
and any approved new jobs are implemented on proven systems.

## Label contract

Every unfinished issue carries:

| Dimension | Meaning |
|---|---|
| `priority:*` | Player/project importance: blocker, critical, high, medium, or low |
| `group:*` | Primary feature family; stay in this cluster until its phase goal is coherent |
| `phase:*` | Earliest delivery gate in which the work should land |
| `weight:*` | Relative review/implementation size: 1, 2, 3, 5, 8, or 13 |
| `bug` / `enhancement` | Issue type; this does not replace the four planning labels |

Weights are estimates, not time promises. A 13 means “epic: split before
coding,” not “more important.” Dependencies written in an issue override a
tempting label sort.

## Primary groups

The current tracker uses `core-flow`, `combat-rules`, `status`, `balance`,
`jobs`, `progression`, `equipment-economy`, `persistence`, `ai`, `ui-ux`,
`content-chapter1`, `story-world`, `art-audio`, `input-accessibility`, and
`tooling-architecture`.

When work genuinely spans several groups, choose the group that owns the
acceptance result and link dependencies. Do not add multiple group labels just
to make an issue appear in every view.
