# Spellrunner (Temp Name)




## Description

This is a first-person shooter where you play as a 3D pixel-art mage fighting through waves of goblins, orcs, and wizards inside a massive tower. Each floor of the castle focuses on a specific enemy type and ends with a unique boss. To reach each boss room, you must explore the floor and find the lever that unlocks it. Once the boss is defeated, the staircase to the next level becomes accessible. You begin outside the castle in a dark, foggy forest with no enemies. Armed with a basic wand, you approach the tower at the center and enter the first floor: the goblin level. After defeating the goblin boss, you advance to the orc level, then the wizard level, and finally a chaotic final floor that mixes all enemy types. Throughout the tower, you can discover new wands with different effects. Your UI shows your health, mana, and kill bar. Left-click uses your wand’s normal spell, which consumes mana. Right-click unleashes your special ability, a powerful attack that drains the kill bar (filled by defeating enemies). You also have a dash ability on Shift for dodging attacks or repositioning quickly. If you die at any point, you are sent back to the first floor to try again.

## Current Goal

Finish and polish the initial staffs and weapons, finalize and refine player movement, and design and polish the first level along with the outside forest area.

## Contributions


### Yons (Weapons (Staffs), Camera, and Player)

Initial commit including a set of core gameplay and player-related implementations:

Player Prefab – Base player object with components and setup

Movement Controller – Initial movement logic and input integration

Weapons & Staffs – Added staff/weapon assets and pickup prefabs

Combat Foundations – Beginning framework for combat scripts

Camera Setup – Basic camera rig configured for the player

===========================================

### Jonathan (UI and Artwork)

Created fire wand and normal wand(gun wand)

Added textures to all wands

Implemented goblin enemy with 4 animations (idle, walk, attack, and die)

Added trees, torches, and other miscellaneous additions within level

===========================================

### Antonio (Enemies and AI)

Created goblin, wizard, and orc untextured prefabs

enemy navigation and stats

additional combat foundations, ult bar and projectiles

===========================================

### Jorge (Level Design

Created starting area, first level, and first final boss room

added script to change scene

