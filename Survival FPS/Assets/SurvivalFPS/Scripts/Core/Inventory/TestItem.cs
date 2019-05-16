using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Inventory
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Inventory Item Template/Test Item")]
    public class TestItem : InventoryItemTemplate
    {
        public override void Use(PlayerManager player)
        {
            player.TakeDamage(100f);
        }
    }
}
