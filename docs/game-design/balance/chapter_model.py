"""Planning arithmetic only. No Unity imports or game-state writes."""
import csv
import json
import math
import importlib.util
from copy import deepcopy
from pathlib import Path

import boss_model
CONFIG = json.loads(Path(__file__).with_name("config.json").read_text(encoding="utf-8"))
RECOMMENDED = [1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5]
LEVELS = [
    (['E01','E01'], ['WA','WB']),
    (['E01','E01'], ['WA','WB']),
    (['E02','E01'], ['WA','WA','WB']),
    (['E01','E02','E01'], ['WA','WB','WB','WC']),
    (['E01','E01','E02'], ['WA','WB','WC','WC']),
    (['E01','E03','E01'], ['WA','WB','WB','WC']),
    (['E01','E03','E01'], ['WA','WB','WB','WB','WC']),
    (['E01','E03','E01'], ['WA','WB','WB','WB','WB','WC']),
    (['E01','E03','E01'], ['MB','WA','WC']),
    (['E01','E05','E01'], ['MB','WA','WC']),
    (['E01','E03','E05'], ['WA','MC','MB']),
    (['E01','H','E02'], ['WA','MB','WC','WC']),
    (['E03','H','E01'], ['MB','MA','WC']),
    (['E02','H','E01'], ['MB','WA','WA','WC']),
    (['E01','E03','E05'], ['WA','MC','MB']),
]

def floor(x):
    return math.floor(x + 1e-9)

def need(level):
    return CONFIG["progression"]["xp_level_base"] + CONFIG["progression"]["xp_level_linear"]*(level-1) + CONFIG["progression"]["xp_level_quadratic"]*(level-1)**2

def stats(role, level):
    data = next(r for r in CONFIG['roles'] if r['id']==role)
    base_hp, base_atk, base_pdef = data['hp'], data['attack'], data['pdef']
    return dict(max_hp=round(base_hp*(1+CONFIG["progression"]["hp_growth"]*(level-1)), 4),
                attack=round(base_atk*(1+CONFIG["progression"]["attack_growth"]*(level-1)), 4),
                pdef=base_pdef+CONFIG["progression"]["defense_growth"]*(level-1))

def grant(hero, amount):
    hero['xp'] += amount
    while hero['xp'] >= need(hero['level']):
        hero['xp'] -= need(hero['level'])
        hero['level'] += 1

def enemy(stage, ident, kind):
    recommended = RECOMMENDED[stage-1]
    w_attack = stats('W', recommended)['attack']
    e = 8+2*recommended
    hp_factor, pdef, mdef = {
        'E01':(.75,0,0), 'E02':(1.0,20,0), 'E03':(.90,50,5),
        'E05':(.70,0,0), 'H':(.70,0,0)
    }[kind]
    hp = floor(hp_factor*w_attack)
    if stage == 12 and kind == 'E02':
        hp = 32
    if kind == 'E03':
        hp, pdef, mdef = {6:(18,100,5), 7:(19,200,5)}.get(stage,(20,300,5))
    return dict(id=ident, kind=kind, hp=hp, max_hp=hp, pdef=pdef, mdef=mdef,
                e=e, winding_left=2 if kind == 'H' else 0,
                link=('C' if stage == 12 else 'A') if kind == 'H' else '')

def resolve_winding(healer, linked):
    # Ordinary encounters use the same exact recovery rule as the Boss.
    mechanic = {'charges': healer['winding_left']}
    heal = boss_model.resolve_winding(mechanic, linked)
    healer['winding_left'] = mechanic['charges']
    return heal


def run_battle(stage, heroes, plan=None, mage_heavy=False):
    kinds, actions = LEVELS[stage-1]
    units = {chr(65+i):enemy(stage,chr(65+i),kind) for i,kind in enumerate(kinds)}
    active = {role:dict(stats(role, h['level']), hp=stats(role,h['level'])['max_hp'])
              for role,h in heroes.items()}
    damage = {role:0 for role in heroes}
    trace = []
    total_healed = 0
    for token in plan or actions:
        role, target = token
        if mage_heavy and 'M' in heroes:
            role = 'M'
        if units[target]['hp'] <= 0:
            continue  # a stronger attack may remove a planned repeat
        alive_order = [key for key,u in units.items() if u['hp']>0]
        assert alive_order.index(target) < (1 if role == 'W' else 2), (stage,role,target,alive_order)
        assert active[role]['hp'] > 0, (stage, 'selected dead hero')
        phase = len(trace)+1
        unit = units[target]
        defense = unit['pdef'] if role == 'W' else unit['mdef']
        raw = floor(active[role]['attack']*100/(100+defense))
        dealt = min(unit['hp'],raw)
        unit['hp'] -= dealt
        damage[role] += dealt
        healed_now = 0
        incoming = {r:0 for r in active}
        intents = []
        for u in units.values():
            if u['hp'] <= 0:
                continue
            kind = u['kind']
            if kind == 'H' and phase%2 == 0 and u['winding_left'] > 0:
                linked = units[u['link']]
                heal = resolve_winding(u,linked)
                healed_now += heal
                intents.append(f"{u['id']}:wind:{heal}")
                continue
            if kind == 'E02' and phase%2:
                intents.append(f"{u['id']}:charge")
                continue
            if kind == 'E05' and phase%2:
                intents.append(f"{u['id']}:aim")
                continue
            legal = [r for r,p in active.items() if p['hp']>0]
            if not legal:
                break
            victim = legal[-1] if kind == 'E05' else legal[0]
            coeff = {'E01':.6,'E02':1.3,'E03':.65,'E05':.9,'H':.25}[kind]
            loss = min(active[victim]['hp'],floor(u['e']*coeff*100/(100+active[victim]['pdef'])))
            active[victim]['hp'] -= loss
            incoming[victim] += loss
            intents.append(f"{u['id']}:{victim}:{loss}")
        total_healed += healed_now
        trace.append(dict(stage=stage, action=phase, role=role, target=target,
                          raw_damage=raw, effective_damage=dealt, restored_enemy_hp=healed_now,
                          incoming_w=incoming.get('W',0), incoming_m=incoming.get('M',0),
                          w_hp=active['W']['hp'],m_hp=active.get('M',{}).get('hp',''),
                          enemy_hp=';'.join(f"{k}:{u['hp']}" for k,u in units.items()),
                          enemy_actions=';'.join(intents)))
    victory = all(u['hp'] <= 0 for u in units.values()) and any(p['hp']>0 for p in active.values())
    return dict(victory=victory, survivors=all(p['hp']>0 for p in active.values()),
                damage=damage, initial_enemy_hp=sum(u['max_hp'] for u in units.values()),
                healed=total_healed, trace=trace, active=active, units=units)

def simulate(scenario, base_offset=None, ad_multipliers=None):
    if base_offset is None:
        base_offset = CONFIG["progression"]["base_offset"]
    heroes = {'W':dict(level=1,xp=0)}
    cards = proofs = proofs_spent = 0
    taunt_learned = False
    rows, traces = [], []
    for stage in range(1,17):
        on_entry = deepcopy(heroes)
        entry_proof = int(stage == CONFIG['progression']['first_entry_proof_stage'])
        proofs += entry_proof
        cards_used = int(stage == 16 and scenario == 'mage_heavy_manual_one_card')
        if cards_used:
            assert cards >= cards_used
            cards -= cards_used
            grant(heroes['W'], cards_used*CONFIG['progression']['xp_card'])
        counterfactual = scenario in ['mage_heavy_no_cards','guided_proof_saved']
        # Scenario input represents explicit player confirmation, not automatic spending.
        # Each simulation is a first-clear itinerary; it does not implement persistence.
        confirmed_learning = stage == 16 and not counterfactual
        if confirmed_learning:
            assert heroes['W']['level'] >= CONFIG['progression']['skill_learning_level'] and proofs-proofs_spent >= 1
            proofs_spent += 1
            taunt_learned = True
        entry = dict(model_version='content_v4_entry16_tutorial',
                     scenario_scope='mathematical_counterexample_not_approved_entry' if counterfactual else 'mainline_tutorial_completed',
                     first_entry_proofs_awarded_before_battle=entry_proof,
                     manual_cards_used_before_battle=cards_used,
                     manual_card_xp_to_warrior_before_battle=cards_used*CONFIG['progression']['xp_card'],
                     manual_proofs_spent_before_battle=int(confirmed_learning),
                     cards_before_battle=cards,proofs_awarded_cumulative_before_battle=proofs,
                     proofs_unspent_before_battle=proofs-proofs_spent,
                     entry_tutorial_status=('completed_after_player_confirmation' if taunt_learned else 'not_completed_counterfactual_only') if stage==16 else 'not_triggered',
                     battle_entry_approved_by_current_tutorial_rule=int(stage!=16 or taunt_learned))
        for role,label in [('W','warrior'),('M','mage')]:
            entry[label+'_level_on_stage_entry'] = on_entry[role]['level'] if role in on_entry else ''
            entry[label+'_xp_into_level_on_stage_entry'] = on_entry[role]['xp'] if role in on_entry else ''
        if stage <= 15:
            battle = run_battle(stage,heroes,mage_heavy=scenario.startswith('mage_heavy'))
            battle['assist'] = dict.fromkeys(heroes,0)
        else:
            boss_plan = 'clear_first_correct_taunt' if taunt_learned else 'no_taunt_clear_first'
            boss_trace, result = boss_model.run(boss_plan, heroes['M']['level'], heroes['W']['level'])
            battle = dict(victory=True,survivors=True,initial_enemy_hp=result['total_enemy_initial_hp'],
                          healed=result['actual_shell_healing'],trace=boss_trace,
                          damage={r:result[l+'_effective_damage'] for r,l in [('W','warrior'),('M','mage')]},
                          assist={r:result[l+'_assist_contribution_candidate'] for r,l in [('W','warrior'),('M','mage')]},
                          active={r:dict(hp=result[l+'_hp_remaining']) for r,l in [('W','warrior'),('M','mage')]})
        if not battle['victory']:
            raise AssertionError((scenario,stage,'unresolved plan',battle['units']))
        assert battle['survivors'], (scenario,stage,'dead participant')
        multiplier = ad_multipliers[stage-1] if ad_multipliers is not None else (2.5 if scenario=='every_win_reward_2_5_no_cards' else 1)
        base = (base_offset + CONFIG["progression"]["base_per_stage"]*stage)*(CONFIG["progression"]["boss_xp_multiplier"] if stage==16 else 1)
        row = dict(scenario=scenario,stage=stage,recommended_level=RECOMMENDED[stage-1],**entry,
                   base_xp=base,settlement_multiplier=multiplier,initial_enemy_hp=battle['initial_enemy_hp'],
                   enemy_healed=battle['healed'],actions=len(battle['trace']),
                   estimated_seconds=round(9+4*len(battle['trace']) if stage==16 else CONFIG['normal_time']['fixed_seconds']+CONFIG['normal_time']['seconds_per_action']*len(battle['trace']),2),
                   action_plan=' '.join((r['selected_class'] if stage==16 else r['role'])+r['target'] for r in battle['trace']),
                   boss_configuration='candidate_r26_m12' if stage==16 else '',
                   boss_scenario=boss_plan if stage==16 else '',taunt_learned_before=int(taunt_learned))
        for role,label in [('W','warrior'),('M','mage')]:
            h = heroes.get(role)
            row[label+'_level_before'] = h['level'] if h else ''
            row[label+'_xp_into_level_before'] = h['xp'] if h else ''
            row[label+'_damage'] = battle['damage'].get(role,'')
            row[label+'_assist_contribution_candidate'] = battle['assist'].get(role,'')
            if h:
                c = (battle['damage'][role]+battle['assist'][role])/(battle['initial_enemy_hp']/CONFIG['progression']['contribution_divisor'])
                s = CONFIG["progression"]["score_floor"] + CONFIG["progression"]["score_log_scale"]*math.log2(1+c)
                g = CONFIG['progression']['overlevel_factor']**max(0,h['level']-RECOMMENDED[stage-1]-CONFIG['progression']['overlevel_grace'])
                xp = floor(floor(base*g*s)*multiplier)
                row[label+'_score_ratio'] = round(c,6)
                row[label+'_score_multiplier'] = round(s,6)
                row[label+'_level_factor'] = round(g,6)
                row[label+'_battle_xp'] = xp
                row[label+'_end_battle_hp'] = battle['active'][role]['hp']
                grant(h,xp)
            else:
                for key in ['score_ratio','score_multiplier','level_factor','battle_xp','end_battle_hp']:
                    row[label+'_'+key] = ''
        raw_cards = 2 if stage==16 else (1 if stage in [4,8,12] else 0)
        cards += math.ceil(raw_cards*multiplier)
        row['first_clear_cards_base'] = raw_cards
        row['cards_inventory'] = cards
        row['ordinary_material_base_candidate'] = 6 if stage==16 else (3 if stage in [4,8,12] else 2)
        row['ordinary_material_awarded_candidate'] = math.ceil(row['ordinary_material_base_candidate']*multiplier)
        row['first_clear_proofs_awarded_after_battle'] = int(stage == CONFIG['progression']['first_clear_proof_stage'])
        proofs += row['first_clear_proofs_awarded_after_battle']
        row['proofs_awarded_cumulative'] = proofs
        row['proofs_unspent'] = proofs-proofs_spent
        row['taunt_learned_after'] = int(taunt_learned)
        if stage == 8:
            heroes['M'] = dict(level=RECOMMENDED[stage-1],xp=0)
        row['character_unlocked'] = 'mage' if stage==8 else ''
        for role,label in [('W','warrior'),('M','mage')]:
            row[label+'_level_after'] = heroes[role]['level'] if role in heroes else ''
            row[label+'_xp_into_level_after'] = heroes[role]['xp'] if role in heroes else ''
        rows.append(row)
        if stage <= 15:
            traces.extend(dict(scenario=scenario,**r) for r in battle['trace'])
    return dict(rows=rows,traces=traces,heroes=heroes,cards=cards,
                taunt_level_ready=heroes['W']['level']>=5)
