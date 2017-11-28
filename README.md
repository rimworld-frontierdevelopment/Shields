# Frontier Developments Shields

Build shields. Stop bullets, rockets, drop pods, meteorites, and more!

## Features

#### Shields do:
- Stop projectiles (bullets, rockets, grenades, mortars, drop pods) from entering the shield
- Allows projectiles from inside to exit the shield
- Shields use a small amount of idle power
- Shield strength is the stored energy on your power net
- Shields generators will heat up as the shields absorb damage and will shut off if they overheat
- Shields come in three sizes:
- small (3m)
- medium (3m - 10m)
- large (5m - 25m)
- The small shield is minifiable and portable and contains an internal battery (1000 Wd, 50% efficient)
- Larger shields can soak more heat before the shield collapses
- Shields exceeding their thermal threshold can suffer from three stages of breakdowns:
  - minor: Zzzt event with no fire or explosion
  - major: minor and a normal breakdown of the shield
  - critical: major, minor, and a small explosion

#### Shields Don't:
- Stop movement of pawns in or out of the shields
- Stop fires
- Heal people
- Stop fires from explosions from propagating into the shield (Tip: don't stand near the edge if the bad guy has an incendiary launcher)

#### Options:
- Energy per damage
- Heat
  - Enable heat
  - Heat per damage
  - Enable/disable any of the heat failures

#### Languages
- English

#### Compatability:
- Save file compatible
- No known mod conflicts

## Addons

#### Optionally Supported Mods:
- Combat Extended (CE): Stops CE projectiles
- RedistHeat: Cool shields with ducts
- Centralized Climate Control: Cool shields with pipes
## Contributions

Contributions will be credited to the authors. Merged contributions will be owned by the mod itself (important for future license changes, but we anticpate that never happening).

### Localization

We want to support as many languages as possible. If you would like to submit translations please submit them as pull request.

### Extending

Please do! We designed this to be as modular as possible. New shields buildings can be added by extending Building_ShieldBase. New shield types are best added as comps similar to Comp_ShieldRadial. New handlers should be added by registering a Harmony patch, use ProjectileHandler as a sample. 