"""Deterministic candidate arithmetic only: no board, Unity, PRD, items, or timing test."""
import csv
import json
from copy import deepcopy
from fractions import Fraction as F
from pathlib import Path

CONFIG = json.loads(Path(__file__).with_name("config.json").read_text(encoding="utf-8"))
PARAMS = CONFIG["boss"]
PLANS = CONFIG["boss_plans"]
CONFIGS = {"candidate_r26_m12": {}}



def num(x):
    return round(float(x), 2)


def damage(attack, defense, multiplier=F(1)):
    return int(F(attack) * 100 / (100 + F(defense)) * multiplier)


def player_stats(role, level):
    data = next(r for r in CONFIG['roles'] if r['id']==role)
    growth = CONFIG['progression']
    return {'hp': F(data['hp']) * (1 + F(str(growth['hp_growth'])) * (level - 1)),
            'attack': F(data['attack']) * (1 + F(str(growth['attack_growth'])) * (level - 1)),
            'defense': F(data['pdef']) + F(str(growth['defense_growth'])) * (level - 1)}


def enemy_from(name, kind):
    data = deepcopy(PARAMS[kind])
    data.update({'name': name, 'kind': kind, 'max_hp': data['hp'], 'hp': F(data['hp'])})
    data['charges'] = data.get('heal_charges', 0)
    return data


def resolve_winding(mechanic, shell):
    healed = min(F(shell['max_hp']) * F(PARAMS['mechanic']['heal_fraction']), shell['max_hp'] - shell['hp']) if shell['hp'] > 0 and mechanic['charges'] > 0 else F(0)
    if healed > 0:
        mechanic['charges'] -= 1
        shell['hp'] += healed
    return healed


def winding_boundary_evidence():
    shell = enemy_from('shell','shell')
    a, b = enemy_from('A','mechanic'), enemy_from('B','mechanic')
    full = resolve_winding(a,shell)
    evidence = {'full_health': {'healed':num(full),'hp':num(shell['hp']),'charges':a['charges']}}
    shell['hp'] = F(54)
    a_heal = resolve_winding(a,shell)
    b_heal = resolve_winding(b,shell)
    evidence['two_mechanics_first_fills'] = {'initial_hp':54,'a_healed':num(a_heal),'b_healed':num(b_heal),'final_hp':num(shell['hp']),'a_charges':a['charges'],'b_charges':b['charges']}
    shell['hp'] = F(0)
    dead_heal = resolve_winding(b,shell)
    evidence['dead_target'] = {'healed':num(dead_heal),'hp':num(shell['hp']),'charges':b['charges']}
    return evidence


def run(scenario, mage_level, warrior_level=5, configuration='candidate_r26_m12'):
    config = deepcopy(PARAMS)
    config['boss'].update(CONFIGS[configuration])
    plan = PLANS[scenario]
    players = {'W': player_stats('W', warrior_level), 'M': player_stats('M', mage_level)}
    max_hp = {role: player['hp'] for role, player in players.items()}
    cd = 0
    rows = []
    total_damage = {'W': 0, 'M': 0}
    taken = {'W': F(0), 'M': F(0)}
    healing = F(0)
    casts = 0
    redirections = [0, 0]
    wasted_casts = 0
    heal_charges_spent = 0
    phase_actions = []
    for phase in [1, 2]:
        enemies = ([enemy_from('A', 'mechanic'), enemy_from('B', 'mechanic'), enemy_from('shell', 'shell')]
                   if phase == 1 else [enemy_from('soldier', 'soldier'), enemy_from('core', 'core')])
        phase_plan = plan[f'phase_{phase}']
        vulnerable = False
        local_action = 0
        for role, target_name in phase_plan:
            assert players[role]['hp'] > 0, (scenario, phase, local_action, 'dead actor')
            target = next(e for e in enemies if e['name'] == target_name)
            if target['hp'] <= 0:
                continue
            local_action += 1
            alive = [e for e in enemies if e['hp'] > 0]
            rank = alive.index(target) + 1
            intent = config[f'phase_{phase}_cycle'][(local_action - 1) % 4]
            hp_before = {k: num(p['hp']) for k, p in players.items()}
            cd_before = cd
            multiplier = F(config['boss']['core_vulnerability']) if phase == 2 and target_name == 'core' and vulnerable else F(1)
            raw = 0
            actual = F(0)
            in_range = rank <= (1 if role == 'W' else 2)
            if in_range:
                defense = target['physical_defense' if role == 'W' else 'magic_defense']
                raw = damage(players[role]['attack'], defense, multiplier)
                actual = min(F(raw), target['hp'])
                target['hp'] -= actual
                total_damage[role] += actual
            taunted = None
            cast = False
            if role == 'W' and plan['taunt_enabled'] and cd == 0 and target['hp'] > 0:
                taunted = target_name
                cd = config['taunt_cd']
                casts += 1
                cast = True
            assert raw > 0 or cast, (scenario, phase, local_action, 'no effective attack or skill')
            vulnerable = False
            event_heal = F(0)
            events = []
            redirect = False
            charged_this_phase = 0
            for enemy in enemies:
                if enemy['hp'] <= 0:
                    continue
                kind = enemy['kind']
                if kind == 'mechanic' and local_action % 2 == 0 and enemy['charges'] > 0:
                    shell = next(e for e in enemies if e['name'] == 'shell')
                    healed = resolve_winding(enemy, shell)
                    heal_charges_spent += int(healed > 0)
                    charged_this_phase += int(healed > 0)
                    healing += healed
                    event_heal += healed
                    events.append(f"{enemy['name']}:heal_shell={num(healed)};charges={enemy['charges']}")
                    continue
                if kind in ['shell', 'core']:
                    if intent in ['charge', 'rest']:
                        events.append(f"{enemy['name']}:{intent}")
                        if cast and taunted == enemy['name']:
                            wasted_casts += 1
                        continue
                    alive_players = [k for k, p in players.items() if p['hp'] > 0]
                    if not alive_players:
                        continue
                    normal_target = ('M' if 'M' in alive_players else alive_players[-1]) if intent == 'ranged' else alive_players[0]
                    legal = alive_players[:config['boss']['ranged_range' if intent == 'ranged' else 'melee_range']]
                    victim = 'W' if taunted == enemy['name'] and 'W' in legal else normal_target
                    if intent == 'ranged' and victim != normal_target:
                        redirect = True
                        redirections[phase - 1] += 1
                    attack = config['boss']['ranged_attack' if intent == 'ranged' else 'melee_attack']
                    coeff = F(1) if intent == 'ranged' else F(config['boss']['melee_coefficient'])
                    if phase == 2 and intent == 'melee':
                        vulnerable = True
                elif kind == 'soldier':
                    if local_action % 2 == 1:
                        events.append('soldier:charge')
                        continue
                    victim = next((k for k, p in players.items() if p['hp'] > 0), None)
                    attack = config['soldier']['attack']
                    coeff = F(config['soldier']['heavy_coefficient'])
                else:
                    victim = next((k for k, p in players.items() if p['hp'] > 0), None)
                    attack = config['mechanic']['attack']
                    coeff = F(1)
                if victim is None:
                    continue
                hit = damage(attack, players[victim]['defense'], coeff)
                effective_hit = min(F(hit), players[victim]['hp'])
                players[victim]['hp'] -= effective_hit
                taken[victim] += effective_hit
                events.append(f"{enemy['name']}:hit_{victim}={num(effective_hit)}")
            if not cast and cd > 0:
                cd -= 1
            done = all(e['hp'] == 0 for e in enemies)
            rows.append({
                'configuration': configuration, 'boss_ranged_attack': config['boss']['ranged_attack'], 'boss_melee_base_attack': config['boss']['melee_attack'],
                'scenario': scenario, 'warrior_level': warrior_level, 'mage_level': mage_level,
                'phase': phase, 'global_action': len(rows) + 1, 'phase_action': local_action,
                'selected_class': role, 'target': target_name, 'target_alive_rank_before': rank,
                'normal_in_range': int(in_range), 'direct_multiplier': num(multiplier),
                'normal_damage_rolled': raw, 'normal_damage_effective': num(actual),
                'taunt_cast': int(cast), 'taunt_skipped_by_kill': int(role == 'W' and plan['taunt_enabled'] and cd_before == 0 and target['hp'] <= 0),
                'taunt_redirected_boss_ranged': int(redirect), 'taunt_cd_before': cd_before, 'taunt_cd_after': cd,
                'warrior_effective_assist_events_this_action': int(redirect), 'mage_effective_assist_events_this_action': 0,
                'boss_intent': intent, 'actual_shell_heal': num(event_heal), 'heal_charges_spent_this_phase': charged_this_phase,
                'enemy_events': ' / '.join(events),
                'warrior_hp_before': hp_before['W'], 'warrior_hp_after': num(players['W']['hp']),
                'mage_hp_before': hp_before['M'], 'mage_hp_after': num(players['M']['hp']),
                'enemy_hp_after': json.dumps({e['name']: num(e['hp']) for e in enemies}, separators=(',', ':')),
                'mechanic_charges_after': json.dumps({e['name']: e['charges'] for e in enemies if e['kind'] == 'mechanic'}, separators=(',', ':')),
                'phase_cleared_assuming_drawable_lines': int(done),
            })
            assert all(p['hp'] > 0 for p in players.values()), (scenario, phase, local_action, 'party member died')
            if done:
                break
        assert all(e['hp'] == 0 for e in enemies), (scenario, phase, 'incomplete phase')
        phase_actions.append(local_action)
    total_hp = 2 * config['mechanic']['hp'] + config['shell']['hp'] + config['soldier']['hp'] + config['core']['hp']
    assert sum(total_damage.values()) == total_hp + healing
    for role in players:
        assert max_hp[role] - players[role]['hp'] == taken[role]
    # Action budgets are reported by the calibration runner, not enforced as combat rules.
    budget = config['time_budget']
    estimated_seconds = len(rows) * budget['seconds_per_action'] + sum(v for k, v in budget.items() if k != 'seconds_per_action')
    summary = {
        'configuration': configuration, 'boss_ranged_attack': config['boss']['ranged_attack'], 'boss_melee_base_attack': config['boss']['melee_attack'],
        'scenario': scenario, 'warrior_level': warrior_level, 'mage_level': mage_level,
        'taunt_enabled': int(plan['taunt_enabled']), 'phase_1_actions': phase_actions[0], 'phase_2_actions': phase_actions[1],
        'total_actions': len(rows), 'warrior_max_hp': num(max_hp['W']), 'mage_max_hp': num(max_hp['M']),
        'warrior_hp_remaining': num(players['W']['hp']), 'mage_hp_remaining': num(players['M']['hp']),
        'warrior_damage_taken': num(taken['W']), 'mage_damage_taken': num(taken['M']),
        'warrior_effective_damage': num(total_damage['W']), 'mage_effective_damage': num(total_damage['M']),
        'warrior_effective_assist_events': sum(redirections), 'mage_effective_assist_events': 0,
        'assist_score_per_effective_event_candidate': config['boss']['ranged_attack'] / 2,
        'warrior_assist_contribution_candidate': sum(redirections) * config['boss']['ranged_attack'] / 2,
        'mage_assist_contribution_candidate': 0, 'reference_contribution_candidate': total_hp / 2,
        'total_enemy_initial_hp': total_hp, 'actual_shell_healing': num(healing),
        'healer_charges_spent': heal_charges_spent, 'taunt_casts': casts,
        'phase_1_ranged_redirects': redirections[0], 'phase_2_ranged_redirects': redirections[1],
        'taunts_wasted_during_charge_or_rest': wasted_casts,
        'warrior_hp_remaining_fraction': num(players['W']['hp'] / max_hp['W']),
        'mage_hp_remaining_fraction': num(players['M']['hp'] / max_hp['M']),
        'estimated_seconds_not_measured': round(estimated_seconds, 2),
        'outcome': 'both_alive_arithmetic_only', 'scenario_meaning': plan['meaning'],
    }
    return rows, summary

