# Terminology

The five armor pieces (helm, chest, arms, waist, legs) are named `armor pieces`. The charm is not an armor piece.
The charm is named `charm` (surprised?)
Armor pieces and charm are named `equipment`.

A simpler hierarchical view is as follow:

- Equipment
    - Armor pieces
        - Helm
        - Chest
        - Arms
        - Waist
        - Legs
    - Charm

Decoration and jewel terms are used interchangeably.

# Understanding the default algorithm

Before implementing a solver, it is important to understand how a classic solver works.

Note that all solvers do not necessarily work the same way, the implementation is entirely up to you.

The explanations in this document are given based on the default implementation, which is a naive brute-force solving, consisting of a function that tells whether a given combination or parameters is valid or not, and then testing all the combination against that function.

In such a solver, two aspects are important.
1. The test function must be as fast as possible.
2. The least combinations possible must be tested.

The point 2 is actually the most important, because the amount of combination grows exponentially with the amount of equipment involved.

To give an example, with the average of 15 armor pieces per category (15 helms, 15 chests, etc...) and 5 charms, it makes 3'796'875 combinations to test.

So because all combinations have to be tested, it is very important to minimize the amount of equipment that take place in the solving, in order to minimize the amount of combination to test.

Classically, a solver works in 3 phases:

## Phase 1 - Reduction

This phase removes all equipment and decorations that do not match any desired skills. This is a trivial step, but very important nonetheless, because the next phase is the most complicated, so it is useful to start the next on a clean base.

## Phase 2 - Election

This phase is actually the most complicated one. Its role is to mark which equipment will really be involved in the search.

The difference with the phase 1 is that phase 1 removes what is absolutely useless. Phase 2 does not remove anything, it simply marks the equipment that will be used in the solving. This let's the user tweak which equipment he or she want to still use, or remove anyway, through the `Advanced search` window.

Phases 1 and 2 occur during input selection, which are:
- Skill selection
- Weapon slots selection
- Decorations override
- Equipment override
- Rarity selection
- Gender selection

Phases 1 and 2 constitute what is called the **solver data**, which will be the working input of the phase 3.

## Phase 3 - Resolution

This phase will construct a collection of combinations of equipment to test, based on election done in phase 2, and naively test all combinations, keeping the ones that satisfy the user's skills selection.

The test function is complicated to implement as well, but very mechanical, so one just has to follow many logical rules in order to get a complex test function.

# Fundamental rules

It is important to understand that the 3 phases of the default algorithm are not necessarily what must be done.

For example, the phase 1 removes what matches absolutely no desired skills, but you could still keep some matching nothing for a cosmetic purpose.

Your phase 2 could keep all equipment if you have a crazy blazing ultra fast test function, why bother electing.

The only rules you have to respect are:
1. Creation of the solver data *(happens before the search and let the user tweak stuffs)*
2. Finding solution to desired skills *(happens during the search)*

The rule 1 just described above contains the phases 1 and 2 of the default algorithm.
The rule 2 is the phase 3.

# Implementing a solver

Here the term solver is used to describe both the solver data and the solver.

*Underway...*
