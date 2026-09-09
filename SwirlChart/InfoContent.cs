namespace SwirlChart;

/// <summary>
/// The reference text behind the original ActSwirl2 "Info 1" and "Info 2" screens,
/// excerpted from "Exhaust Spreads and Troubleshooting" (Roointon Pavri, 7 June 2000).
///
/// Simple line markup, rendered by <see cref="InfoPage"/>:
///   #  section heading      ##  sub heading
///   +  bullet               -   sub bullet      --  sub-sub bullet
///   |  table row (first row of a run is the header)
///   &gt;  muted note
/// </summary>
public static class InfoContent
{
    public const string Info1Title = "Exhaust Spreads and Troubleshooting";

    public const string Info1 = @"# EXHAUST TEMPERATURE VARIATION
|Item|Resp. Component|Range|Exh. Temp **
|Can to Can Fuel Flow|Flow Divider|± 2.5%|± 25° F
|Exhaust Temperature|Exhaust Thermocouples|± 0.75% of act. temp|± 7° F
|Combustion Liner Air Flow|Combustion Liner|± 1.5%|± 15° F
|Dilution & Leakage From Cooling Flow|All Stationary Cooled Comp.|± 3%|± 30° F
> ** Variation at base load
## Statistical Average of All Above:  ± 41° F

# HARDWARE DESIGN MODIFICATIONS AFFECTING SPREADS
+ COMBUSTOR DESIGN
- More peaked profile from combustor will produce higher spread
+ EXHAUST FRAME STRUT
- Straight
- Rotated
- Rotated & Chambered

# TROUBLESHOOTING EXHAUST TEMPERATURE SPREADS
## Data needed
+ Hot Gas Swirl vs. Load
+ Machine data needed
- Exhaust temperature thermocouple readings (TTX's)
- Load
+ Machine data useful
- TTX's, MW on more than one fuel
- TTX's, MW with and without water / steam
- Fuel line pressure readings (oil only)
- TTX's, MW at 5-10 MW intervals
## Clues to look for
- Hot spot or cold spot
- Does hot / cold spot rotate with load
- Has the spread suddenly appeared or has been there always

# ** TROUBLESHOOTING **
## Cold Spot
+ Most likely problem is a blockage in the fuel passages
- Debris in gas passages
- Coked up oil passages, check valves, distribution valves
+ Fuel bypassing the nozzle - happens if oil purge check valve(s) are leaking
+ Air leak in liner or T/P
- T/P seal disengagement
## Hot Spot
+ Most likely problem is more fuel from one can
- Loose gas tip, eroded air cone, burned oil pilot
+ Blocked liner holes
- Broken compressor blades

# ** COMPONENT PROBLEMS **
## Combustion System Components
- Fuel Nozzles
- Liners
- T/P's (include floating & side seals)
- Distribution valves (multiple nozzles)
- Can covers (multiple nozzles)
## Fuel System Components
- Check valves (oil and purge)
- Flow divider
- Gaskets in pigtails
- Selector valve

# COMMENTS
- Majority of spread problems occur during installation. These are usually not serious.
- If a spread suddenly occurs during operation - could be serious.
- If FSNL spread reaches 50-80° F, one or more liner does not have flame.
- Note that the machine will not crossfire at FSNL. If possible, increase load, the machine may crossfire at 30-50% load; or transfer to other fuel (if available) and transfer back.
- Some unnatural causes of spreads. Most of these occur due to human error:
-- Pigtails connected wrong
-- Gas flange taped and the tape not removed
-- Two check valves in one line
-- One nozzle of a different design

# CONCLUSIONS
- Any unit running below allowable (125° F on MS6B / 7EA / 9E; 175° F on F class machines) is acceptable to run indefinitely.
- If a unit starts from the beginning at a ""perceived"" high spread (90-100° F), and does not increase, there is no cause for concern.
- If more than one units are at a site, the spreads on the units will be different. Also, if one unit runs at 60° F spread and the next at 85° F, does not mean there is something ""wrong"" with the unit with
- Time to get concerned:
-- if there is a sudden jump in spread
-- if spread continues to increase with time

> Excerpt from: EXHAUST SPREADS AND TROUBLESHOOTING — Roointon Pavri, June 7, 2000";

    public const string Info2Title = "Tracing Exhaust Temperature Spreads";

    public const string Info2 = @"# TRACING EXHAUST TEMPERATURE SPREADS
## Steps For Trouble Shooting
+ The Source of A High Temperature Spread Can Often Be Traced Back To A Specific Location In The Combustion Chamber Locations.
+ There Are Three Basic Steps To Be Followed:
- Correctly Identify The High and Low Spots In The Exhaust Temperature Profile
- Back-Trace The Exhaust Temperature Anomaly Through The Gas Swirl Angle To Chamber Location
- Identify The Hardware Which Is Capable of Producing A Variation In The Combustion Pattern.

# WHAT IS THE SWIRL ANGLE?
- The Swirl Angle Is The Angle Between The Measured Representative Exhaust Gas Temperature, At Varying Loads, And The Known Combustor Source-Location.
- The Swirl Angle Should Be Similar For Similar Model Series Units Operating Under The Same Conditions.
- However, The Swirl Angle Is Not A Rigidly Controlled Parameter And Could Be Expected To Vary Between Units.
-- It Should Be Treated Only As A Tool.
- Trouble Shooting Via Exhaust Temperature Spreads Has Been Very Helpful In Identifying The Location Of Malfunctioning Combustion Hardware.

# USING THE MAP & GRAPH
- Locate The Cold Region By Looking At The Exhaust Temperature Data.
- Select The Cold Thermocouple And Its Corresponding Location On The Map.
- Using The Graph, Find The Swirl Angle Which Corresponds To The Load At Which The Data Was Taken.
- From The Location Of The Low Thermocouple, Back-Trace (Clockwise On The Map) The Amount Of The Swirl Angle To Identify The Location Of The Probable Cause.
- Use Common Trouble Shooting Techniques To Isolate The Defective Hardware.

# TROUBLE SHOOTING HINTS
+ HOT STREAK (signifies excess fuel / not enough air)
- Inspect Liners for plugged holes
- Check Fuel nozzle assembly
+ COLD STREAK (signifies excess air / not enough fuel)
- Inspect fuel nozzles for plugged orifices
- Inspect check valves for proper operation
- Inspect cross-fire tubes for leaks
- Inspect transition piece seals for proper installation and leaks";
}
