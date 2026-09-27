"""Read-only planning audit: fixed-entry scoring, not a new battle engine."""
import csv
import hashlib
import json
import re
from collections import defaultdict
from fractions import Fraction as F
from pathlib import Path

import boss_model as boss
import chapter_model as chapter

HERE = Path(__file__).resolve().parent
WEIGHTS = {"damage": F(1), "shield": F(1), "heal": F(1),
           "block": F(1, 2), "taken": F(1, 4), "bind": F(1, 2), "taunt": F(1, 2)}


def event(source, kind, role, amount, hit=None):
    assert source and role and kind in WEIGHTS
    amount = F(str(amount))
    assert amount >= 0
    assert kind not in ("taken", "taunt") or hit
    return dict(source=source, kind=kind, role=role, amount=amount, hit=hit)


def score(events):
    seen, total, alternatives = set(), defaultdict(F), defaultdict(dict)
    for e in events:
        key = (e["source"], e["kind"], e["role"])
        assert key not in seen, ("duplicate source", key)
        seen.add(key)
        value = e["amount"] * WEIGHTS[e["kind"]]
        if e["kind"] in ("taunt", "taken"):
            group = alternatives[(e["hit"], e["role"])]
            assert e["kind"] not in group, ("duplicate hit component", e)
            group[e["kind"]] = value
        else:
            total[e["role"]] += value
    for (_, role), group in alternatives.items():
        total[role] += max(group.values())
    return dict(total)


def log2_bounds(x, terms):
    assert x >= 1
    k = 0
    while x >= 2:
        x /= 2
        k += 1
    if x == 1:
        return F(k), F(k)

    def ln_bounds(t):
        lower = 2 * sum((t ** (2 * j + 1) / (2 * j + 1) for j in range(terms)), F(0))
        tail = 2 * t ** (2 * terms + 1) / ((2 * terms + 1) * (1 - t * t))
        return lower, lower + tail

    lo, hi = ln_bounds((x - 1) / (x + 1))
    ln2_lo, ln2_hi = ln_bounds(F(1, 3))
    return k + lo / ln2_hi, k + hi / ln2_lo


def integer_xp(base, level, recommended, contribution, reference, multiplier=F(1)):
    assert base >= 0 and contribution >= 0 and reference > 0
    growth = chapter.CONFIG["progression"]
    factor = F(str(growth["overlevel_factor"])) ** max(0, level - recommended - growth["overlevel_grace"])
    for terms in (12, 24, 48):
        lo, hi = log2_bounds(1 + contribution / reference, terms)
        scale, offset = F(str(growth["score_log_scale"])), F(str(growth["score_floor"]))
        lower, upper = base * factor * (offset + scale * lo), base * factor * (offset + scale * hi)
        if int(lower) == int(upper):
            return int(int(lower) * multiplier), terms
    raise AssertionError("Sample reward interval straddles an integer; no epsilon fallback")


def dot_vector(hp, shield, amounts):
    """Explicit proposed single-shield input; no status/ownership simulation."""
    assert hp >= 0 and shield >= 0 and all(v >= 0 for v in amounts)
    q = sum(amounts, F(0))
    absorbed = min(shield, q)
    shares = [absorbed * v / q if q else F(0) for v in amounts]
    life = [v - s for v, s in zip(amounts, shares)]
    d = sum(life, F(0))
    actual = min(hp, d)
    return shares, [actual * v / d if d else F(0) for v in life]


def boundary_checks():
    checks = []

    def check(name, condition):
        assert condition, name
        checks.append(name)

    ledger = [event("hit1/block", "block", "W", 4), event("hit1/shield", "shield", "K", 8),
              event("hit1/life", "taken", "W", 5, "hit1"), event("heal1", "heal", "M", 7),
              event("bind1", "bind", "A", 16)]
    check("separate_block_shield_hp_heal_bind", score(ledger) == {"W": F(13, 4), "K": 8, "M": 7, "A": 8})
    redirected = [event("t1", "taunt", "W", 26, "h1"), event("d1", "taken", "W", 20, "h1")]
    check("same_hit_max_not_sum", score(redirected)["W"] == 13)
    other = [event("d2", "taken", "W", 8, "h2")]
    check("different_hit_still_scores", score(redirected + other)["W"] == 15)
    check("same_hit_other_recipient_not_suppressed", score(redirected + [event("d3", "taken", "M", 8, "h1")]) == {"W": 13, "M": 2})
    check("permutation_invariant", score(list(reversed(ledger + redirected))) == score(ledger + redirected))
    try:
        score(ledger + ledger[:1])
    except AssertionError:
        checks.append("duplicate_source_rejected")
    else:
        raise AssertionError("Duplicate source accepted")
    for name, args in [("missing_source_rejected", ("", "damage", "W", 5)),
                       ("missing_hit_rejected", ("h1", "taken", "W", 5)),
                       ("negative_amount_rejected", ("h1", "damage", "W", -1))]:
        try:
            event(*args)
        except AssertionError:
            checks.append(name)
        else:
            raise AssertionError(name)
    for name, hp, shield, raw, expected in [
        ("approved_dot_2_3", 5, 0, [4, 6], [F(2), F(3)]),
        ("approved_dot_exact_thirds", 1, 0, [1, 2], [F(1, 3), F(2, 3)]),
        ("proposed_shield3", 5, 3, [4, 6], [F(2), F(3)]),
        ("proposed_shield8", 5, 8, [4, 6], [F(4, 5), F(6, 5)]),
        ("proposed_shield10", 5, 10, [4, 6], [F(0), F(0)]),
        ("zero_dot", 5, 3, [0, 0], [F(0), F(0)]),
    ]:
        absorbed, actual = dot_vector(F(hp), F(shield), list(map(F, raw)))
        check(name, actual == expected and sum(absorbed) == min(shield, sum(raw)))
        assert dot_vector(F(hp), F(shield), list(map(F, reversed(raw)))) == (list(reversed(absorbed)), list(reversed(actual)))
    check("empty_events_no_score", score([]) == {})
    check("exact_log_power_of_two", log2_bounds(F(8), 12) == (F(3), F(3)))
    return checks


def fixed_entry_events(row):
    stage = int(row["stage"])
    heroes = {r: {"level": int(row[f"{label}_level_before"])}
              for r, label in (("W", "warrior"), ("M", "mage")) if row[f"{label}_level_before"] != ""}
    prefix = f'{row["scenario"]}/{stage}'
    events = []
    if stage < 16:
        battle = chapter.run_battle(stage, heroes, mage_heavy=row["scenario"].startswith("mage_heavy"))
        assert battle["victory"] and battle["survivors"]
        assert sum(battle["damage"].values()) == battle["initial_enemy_hp"] + battle["healed"]
        trace = battle["trace"]
        for r in trace:
            source = f'{prefix}/1/{r["action"]}'
            events.append(event(source + "/player", "damage", r["role"], r["effective_damage"]))
            incoming = defaultdict(F)
            for token in r["enemy_actions"].split(";"):
                if not token:
                    continue
                hit = re.fullmatch(r"([A-Z]):([WM]):([0-9]+)", token)
                if hit:
                    enemy, role, amount = hit.groups()
                    hit_id = f"{source}/{enemy}"
                    events.append(event(hit_id, "taken", role, amount, hit_id))
                    incoming[role] += F(amount)
                else:
                    assert re.fullmatch(r"[A-Z]:(charge|aim|wind:[0-9./]+)", token), token
            assert incoming["W"] == F(str(r["incoming_w"])) and incoming["M"] == F(str(r["incoming_m"]))
        for role in heroes:
            lost = sum((e["amount"] for e in events if e["kind"] == "taken" and e["role"] == role), F(0))
            assert lost == F(str(battle["active"][role]["max_hp"])) - F(str(battle["active"][role]["hp"]))
    else:
        trace, result = boss.run(row["boss_scenario"], heroes["M"]["level"], heroes["W"]["level"])
        for r in trace:
            source = f'{prefix}/{r["phase"]}/{r["phase_action"]}'
            events.append(event(source + "/player", "damage", r["selected_class"], r["normal_damage_effective"]))
            incoming = defaultdict(F)
            redirect_seen = False
            for token in r["enemy_events"].split(" / "):
                if not token:
                    continue
                hit = re.fullmatch(r"([A-Za-z]+):hit_([WM])=([0-9.]+)", token)
                if hit:
                    enemy, role, amount = hit.groups()
                    assert F(amount).denominator == 1, "Rounded trace HP damage needs exact model source"
                    hit_id = f"{source}/{enemy}"
                    events.append(event(hit_id, "taken", role, amount, hit_id))
                    incoming[role] += F(amount)
                    if r["taunt_redirected_boss_ranged"] and enemy in ("shell", "core"):
                        assert r["boss_intent"] == "ranged" and role == "W" and r["taunt_cast"]
                        events.append(event(hit_id, "taunt", "W", r["boss_ranged_attack"], hit_id))
                        redirect_seen = True
                else:
                    assert re.fullmatch(r"[A-Za-z]+:(charge|rest|heal_shell=[0-9.]+;charges=[0-9]+)", token), token
            assert redirect_seen == bool(r["taunt_redirected_boss_ranged"])
            for role, label in (("W", "warrior"), ("M", "mage")):
                assert incoming[role] == F(str(r[f"{label}_hp_before"])) - F(str(r[f"{label}_hp_after"]))
        assert int(row["actions"]) == result["total_actions"]
    assert len(trace) == int(row["actions"])
    for role, label in (("W", "warrior"), ("M", "mage")):
        if role not in heroes:
            continue
        actual = sum((e["amount"] for e in events if e["kind"] == "damage" and e["role"] == role), F(0))
        assert actual == F(row[f"{label}_damage"]), (prefix, role, "damage source mismatch")
        assist = sum((e["amount"] / 2 for e in events if e["kind"] == "taunt" and e["role"] == role), F(0))
        assert assist == F(row[f"{label}_assist_contribution_candidate"])
    return events, heroes, len(trace)


def main():
    checks = boundary_checks()
    with (HERE / "results/progression.csv").open(encoding="utf-8-sig", newline="") as stream:
        rows = list(csv.DictReader(stream))
    assert len(rows) == 80
    projections, event_count, action_count = [], 0, 0
    for row in rows:
        events, heroes, actions = fixed_entry_events(row)
        event_count += len(events)
        action_count += actions
        old = score([e for e in events if e["kind"] != "taken"])
        candidate = score(events)
        out = {k: row[k] for k in ("scenario", "stage", "scenario_scope")}
        out.update(events=len(events), actions=actions, characters={},
                   source_events=[dict(e, amount=str(e["amount"])) for e in events])
        for role, label in (("W", "warrior"), ("M", "mage")):
            if role not in heroes:
                continue
            args = (F(row["base_xp"]), heroes[role]["level"], int(row["recommended_level"]))
            ref, multiplier = F(row["initial_enemy_hp"]) / 2, F(row["settlement_multiplier"])
            before, terms_old = integer_xp(*args, old.get(role, F(0)), ref, multiplier)
            after, terms_new = integer_xp(*args, candidate.get(role, F(0)), ref, multiplier)
            assert before == int(row[f"{label}_battle_xp"]), (row["scenario"], row["stage"], role, before)
            out["characters"][role] = dict(level=heroes[role]["level"], old_contribution=str(old.get(role, F(0))),
                candidate_contribution=str(candidate.get(role, F(0))), old_xp=before, candidate_xp=after,
                xp_delta=after-before, interval_terms=max(terms_old, terms_new))
        projections.append(out)
    g = chapter.run_battle(1, {"W": {"level": 4}})
    assert g["victory"] and len(g["trace"]) == 2 and g["damage"]["W"] == 30 and g["active"]["W"]["hp"] == 119
    g01_before, _ = integer_xp(F(20), 4, 1, F(30), F(15))
    g01_after, _ = integer_xp(F(20), 4, 1, F(125, 4), F(15))
    assert g01_before == g01_after == 14 and 157 + g01_after - 165 == 6
    assert int(F(14) * F(11, 5)) == 30 and int(F(20) * F(11, 5)) == 44
    checks.extend(["80_fixed_entry_replays", "original_integer_rewards_match", "g01_14_and_level5_plus6", "normal_vs_ad_base_then_bonus"])
    files = ["config.json", "chapter_model.py", "boss_model.py", "results/progression.csv",
             "2026-09-19-calibration-inputs.md", "verify_contribution_handoff.py"]
    report = dict(revision="CP-N2-I1-2026-09-19", scope="fixed_entry_scoring_projection_not_progression_resimulation",
        enabled_real_events=["damage", "taken", "taunt"], constructed_only=["shield", "heal", "block", "bind", "DOT"],
        boundary_checks=checks, fixed_entry_rows=len(rows), actions=action_count, events=event_count,
        character_comparisons=sum(len(p["characters"]) for p in projections),
        changed_character_rewards=sum(c["xp_delta"] != 0 for p in projections for c in p["characters"].values()),
        g01=dict(old_xp=g01_before, candidate_xp=g01_after, level=5, xp_into_level=6),
        source_sha256={f: hashlib.sha256((HERE / f).read_bytes()).hexdigest() for f in files}, rows=projections)
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
