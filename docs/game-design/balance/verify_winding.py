"""Read-only checks for the exact winding migration; no result exports."""
import csv
import json
import unittest
from decimal import Decimal, localcontext
from fractions import Fraction as F
from pathlib import Path

import chapter_model as chapter

HERE = Path(__file__).resolve().parent


def read_rows(name):
    with (HERE / 'results' / name).open(encoding='utf-8-sig', newline='') as stream:
        return list(csv.DictReader(stream))


def csv_value(value):
    text = str(value)
    try:
        return Decimal(text)
    except ArithmeticError:
        return text


class WindingChecks(unittest.TestCase):
    def test_recovery_boundaries(self):
        cases = [
            ('stage12', 32, 8, 2, F(16, 5), 1),
            ('stage13', 20, 5, 2, F(2), 1),
            ('stage14', 24, 4, 2, F(12, 5), 1),
            ('shell', 60, 37, 2, F(6), 1),
            ('missing_one', 32, 31, 2, F(1), 1),
            ('full', 32, 32, 2, F(0), 2),
            ('dead', 32, 0, 2, F(0), 2),
            ('exhausted', 32, 8, 0, F(0), 0),
            ('constructed_19', 19, 17, 2, F(19, 10), 1),
        ]
        for name, maximum, hp, charges, expected, remaining in cases:
            with self.subTest(name=name):
                target = {'max_hp': maximum, 'hp': F(hp)}
                healer = {'winding_left': charges}
                self.assertEqual(chapter.resolve_winding(healer, target), expected)
                self.assertEqual(target['hp'], F(hp) + expected)
                self.assertEqual(healer['winding_left'], remaining)

    def test_second_healer_preserves_charge_after_first_fills(self):
        target = {'max_hp': 32, 'hp': F(31)}
        first, second = {'winding_left': 2}, {'winding_left': 2}
        self.assertEqual(chapter.resolve_winding(first, target), 1)
        self.assertEqual(chapter.resolve_winding(second, target), 0)
        self.assertEqual((first['winding_left'], second['winding_left']), (1, 2))

    def test_stage12_reachable_positive_recovery(self):
        entry = next(r for r in read_rows('progression.csv')
                     if r['scenario'] == 'guided_no_ads_no_cards' and r['stage'] == '12')
        heroes = {role: {'level': int(entry[label + '_level_before'])}
                  for role, label in [('W', 'warrior'), ('M', 'mage')]}
        self.assertEqual(heroes, {'W': {'level': 4}, 'M': {'level': 3}})
        result = chapter.run_battle(12, heroes, plan=['WA', 'MC', 'MC', 'WB'])
        self.assertTrue(result['victory'] and result['survivors'])
        self.assertEqual(len(result['trace']), 4)
        self.assertEqual(result['trace'][1]['restored_enemy_hp'], F(16, 5))
        self.assertEqual(result['trace'][1]['enemy_hp'], 'A:0;B:16;C:56/5')
        self.assertEqual(result['damage'], {'W': 33, 'M': F(176, 5)})
        self.assertEqual(result['active']['W']['hp'], 100)
        self.assertEqual(result['active']['M']['hp'], 83.52)
        with localcontext() as context:
            context.prec = 60
            score = Decimal('.5') + Decimal('.5') * (
                1 + Decimal('35.2') / Decimal('32.5')).ln() / Decimal(2).ln()
            self.assertEqual(int(Decimal(42) * score), 43)

    def test_original_growth_and_normal_actions_unchanged(self):
        expected = read_rows('progression.csv')
        actions = read_rows('normal_actions.csv')
        self.assertEqual((len(expected), len(actions)), (80, 263))
        actual_rows, actual_actions = [], []
        for scenario in dict.fromkeys(row['scenario'] for row in expected):
            result = chapter.simulate(scenario)
            actual_rows.extend(result['rows'])
            actual_actions.extend(result['traces'])
        for label, before, after in [('growth', expected, actual_rows),
                                     ('actions', actions, actual_actions)]:
            self.assertEqual(len(before), len(after), label)
            for row_index, (old, new) in enumerate(zip(before, after)):
                self.assertEqual(set(old), set(new), (label, row_index))
                for key in old:
                    self.assertEqual(csv_value(old[key]), csv_value(new[key]),
                                     (label, row_index, key, old[key], new[key]))

    def test_g01_integer_experience_without_epsilon(self):
        result = chapter.run_battle(1, {'W': {'level': 4}})
        self.assertTrue(result['victory'])
        self.assertEqual(len(result['trace']), 2)
        self.assertEqual(result['damage']['W'], 30)
        self.assertEqual(result['active']['W']['hp'], 119)
        with localcontext() as context:
            context.prec = 60
            raw = Decimal(20) * Decimal('.5625') * (
                Decimal('.5') + Decimal('.5') * Decimal(3).ln() / Decimal(2).ln())
            self.assertEqual(int(raw), 14)
            self.assertEqual(157 + int(raw) - 165, 6)

    def test_reference_paths_for_stage12(self):
        boards = json.loads((HERE / 'results' / 'boards.json').read_text(encoding='utf-8'))
        board = next(b for b in boards if b['stage'] == 12 and b['phase'] == 1)
        occupied = set()
        for name in ['A', 'C', 'B']:
            points = [tuple(p) for p in board['paths'][name]]
            self.assertEqual(len(set(points)), len(points))
            self.assertFalse(occupied.intersection(points))
            self.assertTrue(all(1 <= v <= board['size'] for p in points for v in p))
            self.assertTrue(all(abs(a[0] - b[0]) + abs(a[1] - b[1]) == 1
                                for a, b in zip(points, points[1:])))
            occupied.update(points)


if __name__ == '__main__':
    unittest.main(verbosity=2)
