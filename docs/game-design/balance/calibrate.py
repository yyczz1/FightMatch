"""Run the first-chapter planning model. Standard library only; no Unity files touched."""
import csv
import hashlib
import json
import math
import random
from copy import deepcopy
from pathlib import Path
from statistics import mean

import boss_model as boss
import chapter_model as chapter

HERE = Path(__file__).resolve().parent
OUT = HERE / 'results'
CONFIG = chapter.CONFIG
CHECKS = []

def check(name, condition, detail):
    CHECKS.append(dict(check=name,passed=bool(condition),detail=detail))
    if not condition:
        raise AssertionError((name,detail))

def csv_out(name, rows):
    with (OUT/name).open('w',encoding='utf-8-sig',newline='') as stream:
        writer=csv.DictWriter(stream,fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)

def json_out(name, data):
    (OUT/name).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

def prd_rate(c):
    survival=1.0
    expectation=0.0
    for attempt in range(1,10001):
        expectation+=survival
        survival*=1-min(1,attempt*c)
        if survival<1e-15:
            break
    return 1/expectation

def prd_constant(p):
    lo,hi=0.0,p
    for _ in range(60):
        mid=(lo+hi)/2
        if prd_rate(mid)>p:
            hi=mid
        else:
            lo=mid
    return (lo+hi)/2

def short_prd(c, opportunities):
    states={0:1.0}
    successes=0.0
    for _ in range(opportunities):
        after={}
        for failures,mass in states.items():
            p=min(1,(failures+1)*c)
            successes+=mass*p
            after[0]=after.get(0,0)+mass*p
            after[failures+1]=after.get(failures+1,0)+mass*(1-p)
        states=after
    return successes/opportunities

def boards():
    small={'A':[(1,1),(1,2),(2,2)],'B':[(1,4),(2,4),(3,4)],'C':[(3,1),(4,1),(4,2),(4,3)]}
    large={'A':[(1,1),(1,2),(2,2),(3,2)],'B':[(1,4),(2,4),(3,4),(4,4)],'C':[(3,5),(4,5),(5,5),(5,4),(5,3),(5,2)]}
    result=[]
    for stage in range(1,16):
        n=4 if stage<6 else 5
        source=small if n==4 else large
        paths={k:list(v) for k,v in list(source.items())[:len(chapter.LEVELS[stage-1][0])]}
        # Geometric reflections preserve adjacency, identity, and the combat target order.
        if stage!=9:
            paths={k:[(n+1-x if stage%2==0 else x,n+1-y if stage%3==0 else y) for x,y in v] for k,v in paths.items()}
        result.append(dict(stage=stage,phase=1,size=n,paths=paths,source='content reference' if stage==9 else 'first-pass reflected template'))
    result.extend([
        dict(stage=16,phase=1,size=6,paths={'A':[(1,1),(1,2),(2,2),(3,2)],'B':[(1,5),(2,5),(2,4),(3,4),(4,4)],'shell':[(3,6),(4,6),(5,6),(6,6),(6,5),(6,4),(6,3),(6,2)]},source='content reference'),
        dict(stage=16,phase=2,size=5,paths={'soldier':[(2,1),(2,2),(3,2),(3,3)],'core':[(1,4),(2,4),(3,4),(4,4),(5,4),(5,3),(5,2)]},source='content reference')])
    for board in result:
        occupied=set()
        for key,path in board['paths'].items():
            check(f"board_{board['stage']}_{board['phase']}_{key}",
                  len(path)>=2 and len(set(path))==len(path)
                  and all(1<=x<=board['size'] and 1<=y<=board['size'] for x,y in path)
                  and all(abs(a[0]-b[0])+abs(a[1]-b[1])==1 for a,b in zip(path,path[1:]))
                  and not occupied.intersection(path), 'Inside grid, adjacent, simple, disjoint default route')
            occupied.update(path)
    return result

def run():
    OUT.mkdir(exist_ok=True)
    snapshots={s:chapter.simulate(s) for s in ['guided_no_ads_no_cards','mage_heavy_no_cards','mage_heavy_manual_one_card','guided_proof_saved','every_win_reward_2_5_no_cards']}
    standard=snapshots['guided_no_ads_no_cards']['rows']
    check('mage_after_stage8',standard[7]['mage_level_before']=='' and standard[7]['mage_level_after']==3,'No stage-8 retroactive XP')
    check('warrior_level5_by_proof',standard[15]['warrior_level_on_stage_entry']>=5,'Standard first entry16, no ads/cards/replay farming')
    check('proofs_only_at_entry16_and_first_clear16',all(r['proofs_awarded_cumulative']==0 for r in standard[:15]) and standard[15]['first_entry_proofs_awarded_before_battle']==1 and standard[15]['first_clear_proofs_awarded_after_battle']==1 and standard[15]['proofs_awarded_cumulative']==2,'Two generic proofs; entry and whole-encounter victory are distinct events')
    check('learn_before_battle',standard[15]['manual_proofs_spent_before_battle']==1 and standard[15]['proofs_unspent_before_battle']==0 and standard[15]['taunt_learned_before']==1 and standard[15]['proofs_unspent']==1,'Explicit confirmation and explanation represented as completed; persistence not implemented')
    card_rows=snapshots['mage_heavy_manual_one_card']['rows']
    check('manual_card_at_entry16',card_rows[14]['warrior_level_after']==4 and card_rows[14]['warrior_xp_into_level_after']==157 and card_rows[14]['cards_inventory']==3 and card_rows[15]['manual_cards_used_before_battle']==1 and card_rows[15]['warrior_level_before']==5 and card_rows[15]['warrior_xp_into_level_before']==42 and card_rows[15]['cards_before_battle']==2,'W4+157 / 165 -> explicit 50 XP card -> W5+42 before learning')
    check('counterfactual_not_approved_entry',all(snapshots[s]['rows'][15]['battle_entry_approved_by_current_tutorial_rule']==0 and snapshots[s]['rows'][15]['manual_proofs_spent_before_battle']==0 for s in ['mage_heavy_no_cards','guided_proof_saved']),'Numerical no-taunt comparisons do not authorize skipping the tutorial')
    all_normal=[r for v in snapshots.values() for r in v['traces']]
    all_growth=[r for v in snapshots.values() for r in v['rows']]
    boss_summaries=[]
    boss_traces=[]
    for scenario in boss.PLANS:
        mage_level=3 if scenario=='full_health_retained_then_two_heals' else 4
        trace,summary=boss.run(scenario,mage_level,5)
        boss_summaries.append(summary)
        boss_traces.extend(trace)
    t,b=boss.run('clear_first_correct_taunt',4,5)
    check('boss_standard',b['total_actions']==11 and b['warrior_hp_remaining']==28 and b['mage_hp_remaining']==89.28,str(b['total_actions'])+' actions; W28; M89.28')
    a=[r for r in t if r['phase']==1][-1]
    z=[r for r in t if r['phase']==2][0]
    check('phase_continuity',a['warrior_hp_after']==z['warrior_hp_before'] and a['mage_hp_after']==z['mage_hp_before'] and a['taunt_cd_after']==z['taunt_cd_before'],'Flip preserves HP and CD')
    check('cast_round_cd',all(r['taunt_cd_after']==2 for r in t if r['taunt_cast']),'No decrement on cast action')
    check('kill_saves_cd',any(r['taunt_skipped_by_kill'] and r['taunt_cd_after']==0 for r in t),'Killing soldier does not spend ready taunt')
    edge=boss.winding_boundary_evidence()
    check('full_health_keeps_charge',edge['full_health']['charges']==2 and edge['full_health']['healed']==0,str(edge['full_health']))
    check('dead_stays_dead',edge['dead_target']['hp']==0 and edge['dead_target']['charges']==2,str(edge['dead_target']))
    check('two_healers_no_waste',edge['two_mechanics_first_fills']['b_charges']==2,str(edge['two_mechanics_first_fills']))
    grid=boards()
    by_board={(v['stage'],v['phase']):v for v in grid}
    for row in all_normal:
        check(f"normal_route_{row['scenario']}_{row['stage']}_{row['action']}",row['target'] in by_board[(row['stage'],1)]['paths'],'Reference combat action has a disjoint default route')
    for row in boss_traces:
        check(f"boss_route_{row['scenario']}_{row['global_action']}",row['target'] in by_board[(16,row['phase'])]['paths'],'Reference combat action has a disjoint default route')
    sweep=[]
    original_ranged=boss.PARAMS['boss']['ranged_attack']
    for ranged in [22,26,30]:
        boss.PARAMS['boss']['ranged_attack']=ranged
        for wl in [4,5,6]:
            for ml in [3,4,5]:
                for scenario in ['clear_first_correct_taunt','no_taunt_clear_first']:
                    try:
                        trace,result=boss.run(scenario,ml,wl)
                        sweep.append(dict(ranged_attack=ranged,warrior_level=wl,mage_level=ml,plan=scenario,result='both_alive',actions=result['total_actions'],w_hp=result['warrior_hp_remaining'],m_hp=result['mage_hp_remaining'],reason=''))
                    except AssertionError as exc:
                        sweep.append(dict(ranged_attack=ranged,warrior_level=wl,mage_level=ml,plan=scenario,result='preset_plan_failed',actions='',w_hp='',m_hp='',reason=str(exc)))
    boss.PARAMS['boss']['ranged_attack']=original_ranged
    ad=CONFIG['ads']
    check('ad_distribution',abs(sum(ad['weights'])-1)<1e-12 and min(ad['multipliers'])>=2,'Sum=1; minimum total multiplier=2')
    reward=[]
    for count in range(1,7):
        expected=sum(p*math.ceil(count*m-1e-9) for m,p in zip(ad['multipliers'],ad['weights']))
        reward.append(dict(base_quantity=count,expected_quantity=round(expected,6),effective_multiplier=round(expected/count,6),min_quantity=math.ceil(count*min(ad['multipliers'])),max_quantity=math.ceil(count*max(ad['multipliers']))))
    rng=random.Random(915)
    ad_runs=[]
    for i in range(200):
        multipliers=rng.choices(ad['multipliers'],weights=ad['weights'],k=16)
        result=chapter.simulate('guided_no_ads_no_cards',ad_multipliers=multipliers)
        before=result['rows'][14]
        ad_runs.append(dict(sample=i+1,warrior_before_boss=before['warrior_level_after'],mage_before_boss=before['mage_level_after'],cards_before_boss=before['cards_inventory'],actions_total=sum(r['actions'] for r in result['rows']),multipliers=','.join(map(str,multipliers))))
    prd=[]
    for role in CONFIG['roles']:
        for level in [1,5,10,20]:
            p=min(role['p_cap'],role['p0']+(level-1)*role['p_per_level'])
            c=prd_constant(p)
            check(f"prd_{role['id']}_{level}",abs(prd_rate(c)-p)<1e-10,'Long-run rate matches target; battle reset measured separately')
            prd.append(dict(role=role['name'],level=level,target_probability=p,prd_c=round(c,10),first_opportunity=c,first_3_average=short_prd(c,3),first_6_average=short_prd(c,6),guaranteed_by_opportunity=math.ceil(1/c)))
    enemies=[]
    stages=[]
    for stage in range(1,16):
        for ident,kind in enumerate(chapter.LEVELS[stage-1][0]):
            enemies.append(dict(stage=stage,**chapter.enemy(stage,chr(65+ident),kind)))
        row=standard[stage-1]
        materials=row['ordinary_material_base_candidate']
        wood=1 if stage%2==0 else 0
        stages.append(dict(stage=stage,recommended_level=row['recommended_level'],base_xp=row['base_xp'],enemy_order=','.join(chapter.LEVELS[stage-1][0]),reference_plan=row['action_plan'],actions=row['actions'],estimated_seconds=row['estimated_seconds'],card_first_clear=row['first_clear_cards_base'],tin_per_win=materials-wood,wood_per_win=wood,unlock='mage,axe_recipe,resource_ad' if stage==8 else '',proof_first_entry='',proof_first_entry_quantity=0,proof_first_clear='',proof_first_clear_quantity=0,entry_tutorial='',board=f'L{stage:02d}-1'))
    stages.append(dict(stage=16,recommended_level=5,base_xp=150,enemy_order='A,B,shell / soldier,core',reference_plan='see boss_actions.csv',actions=11,estimated_seconds=53,card_first_clear=2,tin_per_win=4,wood_per_win=2,unlock='',proof_first_entry='general',proof_first_entry_quantity=1,proof_first_clear='general',proof_first_clear_quantity=1,entry_tutorial='gift_once,player_confirm_level5_taunt,explain_mage_priority_and_redirect,complete_tutorial,normal_ready_then_boss_intro',board='L16-1 / L16-2'))
    tin=sum(r['tin_per_win'] for r in stages[:8]);wood=sum(r['wood_per_win'] for r in stages[:8])
    check('axe_without_ads',min(tin//3,wood)>=1,f'Before stage 9: {tin} tin, {wood} wood; can craft {min(tin//3,wood)*6} axes')
    summary=dict(version=CONFIG['version'],revision=CONFIG['revision'],calibration_revision='2026-09-18-exact-winding',checks=len(CHECKS),checks_passed=sum(x['passed'] for x in CHECKS),normal_scenarios=len(snapshots),growth_rows=len(all_growth),tutorial_persistence_verified=False,boss_scenarios=len(boss_summaries),boss_sweep_cases=len(sweep),boss_sweep_preset_failures=sum(r['result']!='both_alive' for r in sweep),ad_samples=len(ad_runs),ad_seed=915,
                 ordinary_average_budget_seconds=round(mean(r['estimated_seconds'] for r in standard[:15]),2),stage8_budget_seconds=standard[7]['estimated_seconds'],boss_standard_budget_seconds=b['estimated_seconds_not_measured'],boss_standard_w_hp=b['warrior_hp_remaining'],boss_standard_m_hp=b['mage_hp_remaining'],advertised_multiplier_mean=sum(x*y for x,y in zip(ad['multipliers'],ad['weights'])),ad_before_boss_ranges={k:[min(r[k] for r in ad_runs),max(r[k] for r in ad_runs)] for k in ['warrior_before_boss','mage_before_boss','cards_before_boss']},w_level5_first_stage=next(r['stage'] for r in standard if r['warrior_level_after']>=5))
    for name,rows in [('stages.csv',stages),('enemies.csv',enemies),('progression.csv',all_growth),('normal_actions.csv',all_normal),('boss_scenarios.csv',boss_summaries),('boss_actions.csv',boss_traces),('boss_sweep.csv',sweep),('ad_rounding.csv',reward),('ad_progression_samples.csv',ad_runs),('prd.csv',prd),('checks.csv',CHECKS)]:
        csv_out(name,rows)
    json_out('boards.json',grid)
    json_out('summary.json',summary)
    json_out('workbook_data.json',dict(config=CONFIG,stages=stages,enemies=enemies,growth=standard,boss=boss_summaries,prd=prd,rounding=reward,summary=summary))
    sources={name:hashlib.sha256((HERE/name).read_bytes()).hexdigest() for name in ['config.json','chapter_model.py','boss_model.py','calibrate.py']}
    json_out('manifest.json',dict(version=CONFIG['version'],revision=CONFIG['revision'],calibration_revision='2026-09-18-exact-winding',sha256=sources,ad_seed=915))
    print(json.dumps(summary,ensure_ascii=False,indent=2))

if __name__=='__main__':
    run()
