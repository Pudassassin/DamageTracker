using HarmonyLib;
using UnityEngine;
using UnboundLib;
using DamageTracker.MonoBehaviors;

namespace DamageTracker.Patches
{
    [HarmonyPatch(typeof(HealthHandler))]
    class HealthHandler_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("Heal")]
        static void RecordHeal(Player ___player, ref float healAmount)
        {
            if (___player.data.health >= ___player.data.maxHealth && healAmount > 0)
            {
                // skip / untrack condition
                return;
            }

            PlayerDamageTracker tracker = ___player.gameObject.GetComponent<PlayerDamageTracker>();
            // positive healing
            if (healAmount > 0.0f)
            {
                tracker.TrackHeal(healAmount);
            }
            
            // negative 'healing' -- magick damage, life drains, etc.
            else if (healAmount < 0.0f)
            {
                tracker.TrackNegHeal(-healAmount);
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("DoDamage")]
        static void RecordDamage(HealthHandler __instance, ref Vector2 damage, Player ___player)
        {
            // bullets, and all other sort of damaging capabilities
            PlayerDamageTracker tracker = ___player.gameObject.GetComponent<PlayerDamageTracker>();
            tracker.TrackDamage(damage.magnitude);
        }

        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.First)]
        // [HarmonyPatch("CallTakeDamage")]
        // static void BleedEffect(HealthHandler __instance, Vector2 damage, Vector2 position, Player damagingPlayer, Player ___player)
        // {
        //     var bleed = ___player.data.stats.GetAdditionalData().Bleed;
        //     if (bleed > 0f)
        //     {
        //         __instance.TakeDamageOverTime(damage * bleed, position, 3f - 0.5f / 4f + bleed / 4f, 0.25f, Color.red, null, damagingPlayer, true);
        //     }
        // }

        //[HarmonyPrefix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}

        //[HarmonyPostfix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}
    }
}