using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HistoricalEvent
{
    public string eventName;
    public string description;
    public System.Action<GameManager> applyEffect;
    public System.Action<GameManager> clearEffect;
    public int earliestTurn;
    public int latestTurn;
    public bool hasTriggered = false;

    public HistoricalEvent(string name, string desc, System.Action<GameManager> apply, System.Action<GameManager> clear = null)
    {
        eventName = name;
        description = desc;
        applyEffect = apply;
        clearEffect = clear;
    }
}

public static class HistoricalEventManager
{
    static List<HistoricalEvent> eventPool;
    static HistoricalEvent activeEvent = null;

    public static HistoricalEvent ActiveEvent => activeEvent;

    static void EnsurePool()
    {
        if (eventPool != null) return;
        eventPool = new List<HistoricalEvent>();

        eventPool.Add(new HistoricalEvent("French Revolution",
            "The streets of Paris run with the blood of the old order. The Bastille has fallen, " +
            "and the ancien régime crumbles under the weight of popular fury. Secret societies scramble " +
            "to fill the power vacuum \u2014 but the chaos cuts both ways. Every faction loses 5 power as " +
            "governments collapse, yet the Illuminati, architects of Enlightenment ideology, seize the " +
            "moment and gain 10 influence over the bewildered masses.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.power = Mathf.Max(0, f.power - 5);
                var illum = gm.factions.Find(f => f.factionId == 0);
                if (illum != null && !illum.isEliminated)
                    illum.influence = Mathf.Min(100, illum.influence + 10);
            }) { earliestTurn = 1,   latestTurn = 15  });

        eventPool.Add(new HistoricalEvent("Napoleonic Wars",
            "Napoleon Bonaparte has shattered the European order with cannon and genius. His Grande " +
            "Armée marches from Madrid to Moscow, rewriting borders with every battle. In the shadows, " +
            "secret networks ride the coattails of conquest \u2014 moving agents, smuggling gold, and " +
            "assassinating rivals under the cover of war. All ATTACK cards deal +3 additional damage " +
            "this turn, as the entire continent is mobilised for total war.",
            gm => { foreach(var f in gm.factions) if(!f.isEliminated) gm.LogMessage("Napoleonic Wars: attack cards deal +3 damage this turn!"); },
            gm => { gm.LogMessage("Napoleonic Wars fades \u2014 combat returns to normal."); }) { earliestTurn = 10,  latestTurn = 35  });

        eventPool.Add(new HistoricalEvent("Congress of Vienna",
            "The great powers of Europe gather in Vienna to redraw the map and restore the balance " +
            "shattered by Napoleon. Behind the gilded diplomacy, secret societies manoeuvre frantically \u2014 " +
            "bribing delegates, forging treaties, and planting loyalists in every new government. " +
            "The salons buzz with intrigue. INFLUENCE cards gain +2 power this turn, as the art of " +
            "persuasion has never been more valuable.",
            gm => { gm.LogMessage("Congress of Vienna: INFLUENCE cards gain +2 power this turn."); },
            gm => { gm.LogMessage("Congress of Vienna concludes."); }) { earliestTurn = 20,  latestTurn = 30  });

        eventPool.Add(new HistoricalEvent("Industrial Revolution",
            "Factories belch smoke across Britain and the Continent. Steam power, railways, and mass " +
            "production transform economies overnight. Fortunes are made and lost in a season. " +
            "Secret societies race to control the new industrial barons \u2014 funding factories, " +
            "infiltrating unions, and securing the coal seams that power empires. FARMING cards gain " +
            "+3 power this turn, and all factions earn +1 gold income as industrialisation takes hold.",
            gm => { foreach(var f in gm.factions) if(!f.isEliminated) { f.goldIncome += 1; } gm.LogMessage("Industrial Revolution: FARMING cards gain +3 power. All factions gain +1 gold income."); },
            gm => { foreach(var f in gm.factions) f.goldIncome = Mathf.Max(0, f.goldIncome - 1); gm.LogMessage("Industrial Revolution settles."); }) { earliestTurn = 30,   latestTurn = 70 });

        eventPool.Add(new HistoricalEvent("Revolutions of 1848",
            "The Spring of Nations erupts across Europe \u2014 barricades in Paris, Vienna, Berlin, and Rome. " +
            "Liberals, nationalists, and radicals pour into the streets demanding constitutions and liberty. " +
            "But revolutions are unpredictable beasts. Secret society networks are disrupted, informers " +
            "disappear, and agents go underground. A random faction loses 10 influence as their carefully " +
            "cultivated contacts flee the chaos or change allegiances overnight.",
            gm => {
                var alive = gm.factions.FindAll(f => !f.isEliminated);
                if (alive.Count > 0)
                {
                    var target = alive[Random.Range(0, alive.Count)];
                    target.influence = Mathf.Max(0, target.influence - 10);
                }
            }) { earliestTurn = 55, latestTurn = 65 });

        eventPool.Add(new HistoricalEvent("Unification of Germany",
            "Bismarck's iron-and-blood diplomacy has forged a German empire from the patchwork of " +
            "principalities. The new Reich sends shockwaves through every chancellery in Europe. " +
            "The Carbonari, long champions of national liberation, find their ideology weaponised by " +
            "this Prussian colossus and gain 10 power from the nationalist tide. But the atmosphere of " +
            "suspicion is suffocating \u2014 all factions lose 3 secrets as their informant networks are " +
            "rolled up by newly aggressive state security services.",
            gm => {
                var carbonari = gm.factions.Find(f => f.factionId == 3);
                if (carbonari != null && !carbonari.isEliminated)
                    carbonari.power = Mathf.Min(100, carbonari.power + 10);
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.secrets = Mathf.Max(0, f.secrets - 3);
            }) { earliestTurn = 75, latestTurn = 90 });

        eventPool.Add(new HistoricalEvent("Scramble for Africa",
            "European powers carve up the African continent in a frenzy of colonial acquisition, " +
            "drawing borders with rulers and rifles. The Berlin Conference legitimises the plunder, " +
            "and secret societies are right behind the colonial armies \u2014 looting ancient knowledge, " +
            "recruiting local networks, and siphoning resources into their coffers. All factions gain " +
            "5 secrets from the exploitation of colonial intelligence networks spanning three continents.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.secrets = Mathf.Min(50, f.secrets + 5);
            }) { earliestTurn = 90, latestTurn = 110 });

        eventPool.Add(new HistoricalEvent("Belle Époque",
            "Paris shimmers with gaslight and decadence. The arts flourish, technology amazes, and the " +
            "bourgeoisie dance on the edge of catastrophe they cannot yet see. In the underground salons " +
            "and private clubs, secret society dealings reach a fever pitch \u2014 wealth flows freely and " +
            "discretion can be purchased. BLACK_MARKET card effects are doubled this turn, as luxury, " +
            "corruption, and opportunity reach their gilded peak.",
            gm => { gm.LogMessage("Belle Époque: BLACK_MARKET card effects are doubled this turn."); },
            gm => { gm.LogMessage("Belle Époque ends."); }) { earliestTurn = 105, latestTurn = 120 });

        eventPool.Add(new HistoricalEvent("Assassination at Sarajevo",
            "A single pistol shot on a Sarajevo street corner detonates the powder keg of Europe. " +
            "Archduke Franz Ferdinand lies dead and the alliance system drags every major power into " +
            "war within weeks. Mobilisation orders supersede everything. Secret societies scramble to " +
            "adapt \u2014 their agents are conscripted, their safehouses requisitioned, their plans shredded " +
            "by the machinery of total war. No ATTACK cards can be played this turn by anyone, and all " +
            "factions lose 3 power as their organisations are disrupted by the sudden mobilisation.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.power = Mathf.Max(0, f.power - 3);
            },
            gm => { }) { earliestTurn = 118, latestTurn = 126 });

        eventPool.Add(new HistoricalEvent("Russian Revolution",
            "The Romanov dynasty collapses under the weight of war, famine, and revolutionary fury. " +
            "Lenin arrives at Finland Station and the Bolsheviks seize power in October. The Russian " +
            "secret police \u2014 the Okhrana \u2014 is dissolved, and its files are opened, exposing informant " +
            "networks across Europe. The Freemasons lose 8 influence as their carefully cultivated " +
            "Russian contacts are liquidated or driven underground by the new revolutionary government.",
            gm => {
                var masons = gm.factions.Find(f => f.factionId == 2);
                if (masons != null && !masons.isEliminated)
                    masons.influence = Mathf.Max(0, masons.influence - 8);
            },
            gm => { }) { earliestTurn = 123, latestTurn = 130 });

        eventPool.Add(new HistoricalEvent("Great Depression",
            "Wall Street collapses in October 1929 and the shockwave tears through every economy on " +
            "earth. Banks fail. Factories close. Unemployment reaches a third of the workforce in some " +
            "nations. Secret society treasuries, invested in the markets and in the businesses they " +
            "controlled, are devastated. All factions lose 10 power as their financial networks " +
            "implode, and the secrets pipeline slows to a trickle as desperate agents sell information " +
            "to the highest bidder.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.power = Mathf.Max(0, f.power - 10);
            }) { earliestTurn = 128, latestTurn = 138 });

        eventPool.Add(new HistoricalEvent("Rise of Fascism",
            "Mussolini has marched on Rome. Hitler's brownshirts rule the German streets. Franco's " +
            "Nationalists wage a brutal civil war in Spain. A new political religion of violence and " +
            "national mysticism sweeps across Europe, and the Knights Templar \u2014 masters of martial " +
            "discipline and hierarchical power \u2014 find their moment. They gain 15 power as the continent " +
            "embraces authoritarian order. SABOTAGE cards deal +2 extra damage this turn, as political " +
            "terror becomes a legitimate instrument of statecraft.",
            gm => {
                var templar = gm.factions.Find(f => f.factionId == 1);
                if (templar != null && !templar.isEliminated)
                    templar.power = Mathf.Min(100, templar.power + 15);
            },
            gm => { }) { earliestTurn = 132, latestTurn = 142 });

        eventPool.Add(new HistoricalEvent("World War II",
            "The most destructive conflict in human history engulfs the globe. From Stalingrad to the " +
            "Pacific, from the deserts of North Africa to the forests of Burma, empires and ideologies " +
            "clash in an orgy of industrial slaughter. Secret societies find their centuries-old " +
            "networks shredded by bombs, occupation, and betrayal. All factions lose 8 power as their " +
            "organisations are decimated. The Rosicrucians, whose occult European networks depended on " +
            "peacetime freedom, suffer an additional -2 power penalty on their cards.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.power = Mathf.Max(0, f.power - 8);
            },
            gm => { }) { earliestTurn = 138, latestTurn = 148 });

        eventPool.Add(new HistoricalEvent("Cold War Begins",
            "The guns fall silent but the shadow war intensifies. The world cleaves into two armed " +
            "camps \u2014 the American sphere and the Soviet bloc \u2014 and every intelligence service on earth " +
            "goes into overdrive. Spies, double agents, and disinformation campaigns become the primary " +
            "currency of power. Secret societies find themselves embedded in both CIA and KGB networks, " +
            "playing both sides. All factions gain 5 secrets as the espionage infrastructure reaches " +
            "unprecedented scale, and the black market thrives in the grey zones of a divided continent.",
            gm => {
                foreach (var f in gm.factions)
                    if (!f.isEliminated) f.secrets = Mathf.Min(50, f.secrets + 5);
            },
            gm => { }) { earliestTurn = 144, latestTurn = 150 });

        eventPool.Add(new HistoricalEvent("Nuclear Age",
            "The atomic bomb has irrevocably changed what power means. A single weapon can erase a " +
            "city in a flash of light \u2014 and every major power races to build more. Secret societies " +
            "that have operated for centuries in the shadows now face annihilation from weapons that " +
            "respect no allegiance and no tradition. In this terrifying new order, one faction has " +
            "positioned itself closest to the nuclear arsenals: a random faction gains 20 power from " +
            "the influence they have purchased inside the weapons programmes of the superpowers.",
            gm => {
                var alive = gm.factions.FindAll(f => !f.isEliminated);
                if (alive.Count > 0)
                {
                    var target = alive[Random.Range(0, alive.Count)];
                    target.power = Mathf.Min(100, target.power + 20);
                }
            }) { earliestTurn = 148, latestTurn = 150 });
    }

    public static bool TryTriggerEvent(GameManager gm)
    {
        EnsurePool();
        if (activeEvent != null) return false;

        int turn = gm.currentTurn;

        // Collect events whose window is currently open and haven't fired yet
        List<HistoricalEvent> eligible = eventPool.FindAll(e =>
            !e.hasTriggered &&
            turn >= e.earliestTurn &&
            turn <= e.latestTurn);

        if (eligible.Count == 0) return false;

        // BUG FIX 5: Always select the chronologically earliest eligible event.
        // Sort by earliestTurn ascending so events always fire in historical order.
        eligible.Sort((a, b) => a.earliestTurn.CompareTo(b.earliestTurn));
        HistoricalEvent selected = eligible[0];

        selected.hasTriggered = true;
        activeEvent = selected;
        activeEvent.applyEffect?.Invoke(gm);
        gm.LogMessage($"\u26A1 HISTORICAL EVENT [{gm.CurrentYear} AD]: {activeEvent.eventName} \u2014 {activeEvent.description}");
        return true;
    }

    public static void Reset()
    {
        if (eventPool != null)
            foreach (var e in eventPool) e.hasTriggered = false;
        activeEvent = null;
    }

    public static void ClearActiveEvent(GameManager gm)
    {
        if (activeEvent != null)
        {
            activeEvent.clearEffect?.Invoke(gm);
            activeEvent = null;
        }
    }

    public static bool IsEventActive(string name)
    {
        return activeEvent != null && activeEvent.eventName == name;
    }

    public static int GetInfluenceBoost()
    {
        if (activeEvent != null && activeEvent.eventName == "Congress of Vienna")
            return 2;
        return 0;
    }

    public static int GetAttackBoostForFaction(int factionId)
    {
        if (activeEvent != null && activeEvent.eventName == "Napoleonic Wars")
            return 3;
        return 0;
    }

    public static int GetInfluenceBoostForFaction(int factionId)
    {
        if (activeEvent != null && activeEvent.eventName == "Russian Revolution" && factionId == 3)
            return 5;
        return 0;
    }

    public static int GetFarmingBoost()
    {
        if (activeEvent != null && activeEvent.eventName == "Industrial Revolution")
            return 3;
        return 0;
    }

    public static int GetSabotageBoost()
    {
        if (activeEvent != null && activeEvent.eventName == "Rise of Fascism")
            return 2;
        return 0;
    }

    public static float GetBlackMarketMultiplier()
    {
        if (activeEvent != null && activeEvent.eventName == "Belle Époque")
            return 2f;
        if (activeEvent != null && activeEvent.eventName == "Cold War Begins")
            return 1.5f;
        return 1f;
    }

    public static int GetPenaltyForFaction(int factionId)
    {
        if (activeEvent != null && activeEvent.eventName == "World War II" && factionId == 3)
            return -2;
        return 0;
    }

    public static bool IsAttacksBlocked()
    {
        return activeEvent != null && activeEvent.eventName == "Assassination at Sarajevo";
    }
}
