using HarmonyLib;
using UnityEngine;
using UnboundLib;

namespace DamageTracker.Patches
{
    [HarmonyPatch(typeof(HealthHandler))]
    class HealthHandler_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("Heal")]
        static void ApplyHealMultiplier(Player ___player, ref float healAmount)
        {
            // positive healing
            
            // negative 'healing' -- magick damage, life drains, etc.
            
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("DoDamage")]
        static void ApplyDamageMultiplier(HealthHandler __instance, ref Vector2 damage, Player ___player)
        {
            // bullets, and all other sort of damaging capabilities

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