# Damage Tracker [1.1.0] 
Public Release 1-1-0, build #23

Keep better tracks on all the damage everyone dealt, taken and healed! A "DPS Meter" made by Pudassassin.

## **Presets**

#### \[Damage Indicator+]
- a QoL-improved style based on the good old Damage Indicator mod.

#### \[Default]*
- text color of the number matches the associating player, as well as a +/- sign and outline color to signify damage or healing.
- default setting at fresh installation

#### \[BEEG NUMBERS]
- bigger text size and color-coded to damage/heal as well as shortening it down for high-value numbers.

## **Setting**

#### \[Number Formatting]
- **Plain**: just raw number, no shortening or separators.
    - (eg. "0.123" | "1234" | "1234567" | "1234567890")
- **Spacing**: using space as separators, show full number.
    - (eg. "0.123" | "1 234" | "1 234 567" | "1 234 567 890")
- **Commas**: the normal written number, show full number.
    - (eg. "0.123" | "1,234" | "1,234,567" | "1,234,567,890")
- **Shorten**: chop down the **stronk** number with multi-million suffixes.
    - (eg. "0.123" | "1.23 ki" | "1.23 mil" | "1.23 bil")
- **Metric**: the standard metric-style number.
    - (eg. "0.123" | "1.234 ki" | "1.235 Me" | "1.235" Gi)
- **Science**: the sciencific-notation number, only apply to number above a million.
    - (eg. "0.123" | "1 234" | "1.23E+006" | "1.23E+009")

#### \[Use Player's Color]
- **when enabled**: the number texts are color-matched with their associating players. Number texts are also come with +/-/* signs and color-coded outline.
- **when disabled**: the number texts are instead color-coded to whether it is damage (red), heal (green) and negative-heal (magenta) regardless of the source players, and with black outline.


#### \[Max Number to Show Decimals]
- the maximum value of the non-shortened number texts that will retain tailing decimal details.

#### \[Decimal Text Scale]
- the relative scale to shrink down decimal part of the number texts.

#### \[Hide ALL Decimals]
- the override to hide **ALL** decimal part of the number texts, will not apply to scientific notation numbers.


#### \[Number Base Size]
- the base font size for the value-100 number texts

#### \[Value-to-Size Scaling]
- the scaling factor for number texts going above or below 100. Resize with logarithmic scaling (aka. order of magnitude, number of digits).


#### \[Number Display Time]
- the upper duration limit for the number texts to persist on screen.
* \* the timing is scaled alongside the game speed; it will last longer during point/round transistion, rematch menu, etc.

#### \[Number Opaque Time]
- the duration for number texts to be displayed fully opaque and then fading out afterward

#### \[Number Sum Time Window]
- the duration window for multiple instances of damage/heal to consolidate into one bigger number. (aka. the merging of  damage-over-time or regeneration)
- \* a 1-second window is a proper DPS number grouping

#### \[New Number Sum Delay]
- the time gap for the new instances of damage/heal to be displayed and grouped up in a new number text. (aka. discrete bursts of damage or healing)


## **Note from the modder**
Hopefully this mod will help clarifying all the damages being taken, given and healed away. All numbers are rendered on screen-space.

## Patch Notes
#### Public Release 1-1 \[v1.2.0]
- Fixed major issue with damage number not disappearing after a set duration and **eventually slow the game down or worse**.

#### Public Release 1-1 \[v1.1.0]
- Damage numbers now guaranteed to disappear based on real-time duration if the game's slow-mo effect lasts too long for some reason.
- Damage numbers will stop tracking when the battle ends and resume at battle starts.
- Fixed the bug preventing anything players from taking / resolving damage in Sandbox Mode.

#### Public Release 1-0 \[v1.0.0]
- It all begins. Core functionality with in-game options and approximate preview.
