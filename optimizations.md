# Overview

This document keep tracks of measurments of different runs with different parameters, across different commits.

# References

Using Athena's ASS version 0.50b data.

Skill loadouts:

```JSON
{
    "Attack Defense": [
        45,
        48,
        27,
        80,
        146
    ],
    "Resistances": [
        16,
        19,
        79,
        76,
        296,
        64,
        209,
        70,
        13,
        6,
        3,
        9,
        12,
        73,
        35,
        67,
        32
    ],
    "Versus Behemoth Support 1": [
        27,
        55,
        186,
        314,
        60,
        58,
        12,
        35,
        230
    ],
    "Versus Behemoth Support 2": [
        27,
        55,
        186,
        60,
        58,
        237,
        12,
        35,
        230
    ]
}
```

# Results

- Machine A is a desktop.
- Machine B is a laptop.
- Memory consumption is checked with task manager, very rough.

## Phase 1

### Machine A

#### Attack Defense skill loadout

- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 86 MB

```
Heads count:  16
Chests count: 14
Gloves count: 8
Waists count: 13
Legs count:   7
Charms count:   6
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 978'432
-----
Matching result: 243
Took: 2'706 ms
```

#### Resistances

- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 1117 MB

```
Heads count:  21
Chests count: 22
Gloves count: 12
Waists count: 19
Legs count:   15
Charms count:   17
-----
Min sLot size: 1
Max sLot size: 2
-----
Combination count: 26'860'680
-----
Matching result: 0
Took: 89'157 ms
```

#### Versus Behemoth Support 1

- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 140 MB

```
Heads count:  17
Chests count: 16
Gloves count: 10
Waists count: 13
Legs count:   8
Charms count:   8
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 2'263'040
-----
Matching result: 102
Took: 4'122 ms
```

#### Versus Behemoth Support 2

- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 190 MB

```
Heads count:  19
Chests count: 17
Gloves count: 10
Waists count: 13
Legs count:   9
Charms count:   9
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 3'401'190
-----
Matching result: 4
Took: 6'360 ms
```

### Machine B

#### Attack Defense skill loadout

- Power cord unplugged.
- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 112 MB

```
Heads count:  16
Chests count: 14
Gloves count: 8
Waists count: 13
Legs count:   7
Charms count:   6
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 978'432
-----
Matching result: 243
Took: 6'605 ms
```

#### Resistances

- Power cord unplugged.
- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 1130 MB

```
Heads count:  21
Chests count: 22
Gloves count: 12
Waists count: 19
Legs count:   15
Charms count:   17
-----
Min sLot size: 1
Max sLot size: 2
-----
Combination count: 26'860'680
-----
Matching result: 0
Took: 239'767 ms
```

#### Versus Behemoth Support 1

- Power cord unplugged.
- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 160 MB

```
Heads count:  17
Chests count: 16
Gloves count: 10
Waists count: 13
Legs count:   8
Charms count:   8
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 2'263'040
-----
Matching result: 102
Took: 11'700 ms
```

#### Versus Behemoth Support 2

- Power cord unplugged.
- No decoration override.
- Weapon slots 22-.
- Memory consumption up to 217 MB

```
Heads count:  19
Chests count: 17
Gloves count: 10
Waists count: 13
Legs count:   9
Charms count:   9
-----
Min sLot size: 1
Max sLot size: 3
-----
Combination count: 3'401'190
-----
Matching result: 4
Took: 16'370 ms
```
