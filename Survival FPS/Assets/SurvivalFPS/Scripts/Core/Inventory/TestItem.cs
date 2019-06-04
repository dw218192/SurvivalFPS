using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Messaging;

namespace SurvivalFPS.Core.Inventory
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Inventory Item Template/Test Item")]
    public class TestItem : InventoryItemTemplate
    {
        protected override void OnItemBeingUsed(PlayerManager player, ItemInstance itemInstance)
        {
            player.TakeDamage(100f);
        }

        protected override void OnItemUsed(PlayerManager player, ItemInstance itemInstance)
        {
        }
    }
}
