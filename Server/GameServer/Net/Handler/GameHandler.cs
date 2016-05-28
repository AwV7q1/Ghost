﻿using Server.Common.Constants;
using Server.Common.Data;
using Server.Common.IO;
using Server.Common.IO.Packet;
using Server.Ghost;
using Server.Ghost.Accounts;
using Server.Ghost.Characters;
using Server.Net;
using Server.Packet;
using System;
using System.Collections.Generic;
using System.Net;

namespace Server.Handler
{
    public static class GameHandler
    {
        public static void Game_Log_Req(InPacket lea, Client gc)
        {
            //int re = SearchBytes(lea.Content, new byte[] { 0x0 });
            string[] data = lea.ReadString(0x100/*re*/).Split(new[] { (char)0x20 }, StringSplitOptions.None);
            int encryptKey = int.Parse(data[1]);
            string username = data[2];
            string password = data[4];
            //lea.Skip(206);
            int selectCharacter = lea.ReadByte();
            IPAddress hostid = lea.ReadIPAddress();

            gc.SetAccount(new Account(gc));

            try
            {
                gc.Account.Load(username);
                //var pe = new PasswordEncrypt(encryptKey);
                //string encryptPassword = pe.encrypt(gc.Account.Password, password.ToCharArray());
                if (!password.Equals(gc.Account.Password) || gc.Account.Banned > 0)
                {
                    gc.Dispose();
                    Log.Error("Login Fail!");
                }
                else
                {
                    gc.Account.Characters = new List<Character>();
                    foreach (dynamic datum in new Datums("Characters").PopulateWith("id", "accountId = '{0}' ORDER BY position ASC", gc.Account.ID))
                    {
                        Character character = new Character(datum.id, gc);
                        character.Load(false);
                        character.IP = hostid;
                        gc.Account.Characters.Add(character);
                    }
                    gc.SetCharacter(gc.Account.Characters[selectCharacter]);
                }
                Log.Inform("Password = {0}", password);
                //Log.Inform("encryptKey = {0}", encryptKey);
                //Log.Inform("encryptPassword = {0}", encryptPassword);
            }
            catch (NoAccountException)
            {
                gc.Dispose();
                Log.Error("Login Fail!");
            }

            Character chr = gc.Character;
            chr.CharacterID = gc.CharacterID;
            Maps.AllCharacters.Add(chr);

            StatusPacket.updateHpMp(gc, 0, 0, 0);
            GamePacket.FW_DISCOUNTFACTION(gc);
            QuestPacket.getQuestInfo(gc, chr.Quests.getQuests());
            StatusPacket.getStatusInfo(gc);
            InventoryPacket.getCharacterEquip(gc);
            //GamePacket.getCharacterInvenAll(gc);
            SkillPacket.getSkillInfo(gc, chr.Skills.getSkills());
            QuestPacket.getQuickSlot(gc);
            InventoryPacket.getStoreInfo(gc);
            InventoryPacket.getStoreInfo(gc);
            InventoryPacket.getStoreMoney(gc);
            MapPacket.enterMapStart(gc);
            InventoryPacket.getInvenEquip(gc);
            InventoryPacket.getInvenEquip1(gc);
            InventoryPacket.getInvenEquip2(gc);
            InventoryPacket.getInvenSpend3(gc);
            InventoryPacket.getInvenOther4(gc);
            InventoryPacket.getInvenPet5(gc);
            InventoryPacket.getInvenCash(gc);
        }

        public static void Command_Req(InPacket lea, Client gc)
        {
            string[] cmd = lea.ReadString(60).Split(new[] { (char)0x20 }, StringSplitOptions.None);

            if (gc.Account.Master == 0 || cmd.Length < 1)
                return;
            var chr = gc.Character;

            switch (cmd[0])
            {
                case "//notice":
                    if (cmd.Length != 2)
                        break;
                    foreach (Character all in Maps.AllCharacters)
                    {
                        GamePacket.getNotice(all.Client, 3, cmd[1]);
                    }
                    break;
                case "//item":
                    if (cmd.Length != 2)
                        break;
                    chr.Items.Add(new Item(int.Parse(cmd[1]), chr.Items.GetNextFreeSlot((InventoryType.ItemType)InventoryType.getItemType(int.Parse(cmd[1]))), InventoryType.getItemType(int.Parse(cmd[1]))));
                    InventoryPacket.getInvenEquip1(gc);
                    InventoryPacket.getInvenEquip2(gc);
                    InventoryPacket.getInvenSpend3(gc);
                    InventoryPacket.getInvenOther4(gc);
                    InventoryPacket.getInvenPet5(gc);
                    break;
                case "//money":
                    if (cmd.Length != 2)
                        break;
                    chr.Money = int.Parse(cmd[1]);
                    InventoryPacket.getCharacterEquip(gc);
                    break;
                case "//levelup":
                    chr.LevelUp();
                    break;
                case "//gogo":
                    if (cmd.Length != 3)
                        break;
                    MapPacket.warpToMapAuth(gc, true, short.Parse(cmd[1]), short.Parse(cmd[2]), -1, -1);
                    break;
                case "//test":
                    StatusPacket.updateHpMp(gc, 0, 0, 0);
                    break;
                default:
                    break;
            }
        }

        //private static int SearchBytes(byte[] haystack, byte[] needle)
        //{
        //    var len = needle.Length;
        //    var limit = haystack.Length - len;
        //    for (var i = 0; i <= limit; i++)
        //    {
        //        var k = 0;
        //        for (; k < len; k++)
        //        {
        //            if (needle[k] != haystack[i + k]) break;
        //        }
        //        if (k == len) return i;
        //    }
        //    return -1;
        //}
    }
}
