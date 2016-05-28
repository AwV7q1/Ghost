﻿using Server.Common.Constants;
using Server.Common.Data;
using Server.Common.IO.Packet;
using Server.Ghost;
using Server.Handler;
using Server.Packet;

namespace Server.Net.Handler
{
    public static class CouponHandler
    {
        public static void Use_Coupon_Req(InPacket lea, Client gc)
        {
            var chr = gc.Character;
            string Code = lea.ReadString(20);
            foreach (dynamic datum in new Datums("Coupon").Populate())
            {
                if (Code.Equals(datum.code) && datum.valid == 1)
                {
                    byte Type = InventoryType.getItemType(datum.itemID);
                    chr.Items.Add(new Item(datum.itemID, chr.Items.GetNextFreeSlot((InventoryType.ItemType)Type), InventoryType.getItemType(datum.itemID), (short)datum.quantity));
                    InventoryHandler.UpdateEquip(gc, Type);
                    InventoryPacket.clearDropItem(gc, chr.CharacterID, -1, datum.itemID, 1);
                    datum.valid = 0;
                    datum.Update("code = '{0}'", Code);
                    break;
                }
            }
        }
    }
}
