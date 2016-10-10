using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;
using SharpDX;
using Ensage.Common.Menu;


namespace InvokerNinja
{
    class SunStrike
    {
        private static readonly Menu Menu = new Menu("Invoker", "Invoker", true, "npc_dota_hero_Invoker", true);
        private static Hero me, target;
        private static uint nextskillvalue, combonumber, nextskillflee = 0;
        private static Ability quas, wex, exort, invoke, coldsnap, meteor, alacrity, tornado, forgespirit, blast, sunstrike, emp, icewall, ghostwalk;
        private static Item eul, medallion, solar_crest, malevolence, bloodthorn, urn;
        private static bool comboing = false, target_magic_imune, target_isinvul, target_meteor_ontiming, target_emp_ontiming, target_sunstrike_ontiming, target_blast_ontiming, quas_level, exort_level, wex_level, forge_in_my_side, ice_wall_distance;
        private static float distance_me_target;
        private static ParticleEffect targetParticle;
        private static List<Unit> myunits;
        static void Main(string[] args)
        {
            Menu.AddItem(new MenuItem("Combo Mode", "Combo Mode").SetValue(new KeyBind('T', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Flee Mode", "Flee Mode").SetValue(new KeyBind('U', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Target Select", "Target Select").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnWndProc += Exploding;
            Drawing.OnDraw += Target_esp;
            Game.OnUpdate += orb_checker;
            Orbwalking.Load();
        }
        public static void orb_checker(EventArgs args)
        {
            //if (!Game.IsInGame || Game.IsWatchingGame)
            //    return;
            //me = ObjectMgr.LocalHero;
            //if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
            //    return;
            ////quas, wex, exort checker
            //if (me.Level <= 6)
            //{
            //    if (quas.Level >= 2)
            //        quas_level = true;
            //    else
            //        quas_level = false;
            //    if (wex.Level >= 2)
            //        wex_level = true;
            //    else
            //        wex_level = false;
            //    if (exort.Level >= 2)
            //        exort_level = true;
            //    else
            //        exort_level = false;
            //}
            //else if (me.Level <= 25)
            //{
            //    if (quas.Level >= 4)
            //        quas_level = true;
            //    else
            //        quas_level = false;
            //    if (wex.Level >= 4)
            //        wex_level = true;
            //    else
            //        wex_level = false;
            //    if (exort.Level >= 4)
            //        exort_level = true;
            //    else
            //        exort_level = false;
            //}
            //if(me.NetworkActivity == NetworkActivity.Attack)
            //{
            //    if (!me.Modifiers.Any(x => x.Name.Contains("exort")))
            //    {
            //        exort.UseAbility(false);
            //        exort.UseAbility(false);
            //        exort.UseAbility(false);
            //        Utils.Sleep(1500, "attack1_time");
            //    }
            //}
            //else if( me.Health < me.MaximumHealth *0.5)
            //{
            //    if (!me.Modifiers.Any(x => x.Name.Contains("quas")))
            //    {
            //        quas.UseAbility(false);
            //        quas.UseAbility(false);
            //        quas.UseAbility(false);
            //    }
            //}
            //else if(me.Health > me.MaximumHealth *0.5)
            //{
            //        if (!me.Modifiers.Any(x => x.Name.Contains("wex")))
            //        {
            //            wex.UseAbility(false);
            //            wex.UseAbility(false);
            //            wex.UseAbility(false);
            //        }
            //}
        }
        public static void Target_esp(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
                return;
            if (targetParticle == null && target != null)
            {
                targetParticle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);
            }
            if(target == null || !target.IsVisible || !target.IsAlive)
            {
                targetParticle.Dispose();
                targetParticle = null;
            }
            if (target != null)
            {
                targetParticle.SetControlPoint(2, me.Position);
                targetParticle.SetControlPoint(6, new Vector3(1, 0, 0));
                targetParticle.SetControlPoint(7, target.Position);
            }
        }
        public static void Exploding(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
                return;
            //adicionar modifiers(mirana arrow...), adiocionar low hp < sunstrike_damage detector, adicionar target selector e moving separado

            if (Game.IsKeyDown(Menu.Item("Flee Mode").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
            {
                if (Utils.SleepCheck("ORBSFIND"))
                {
                    quas = me.Spellbook.SpellQ;
                    wex = me.Spellbook.SpellW;
                    exort = me.Spellbook.SpellE;
                    invoke = me.Spellbook.SpellR;
                    coldsnap = me.FindSpell("invoker_cold_snap");
                    tornado = me.FindSpell("invoker_tornado");
                    blast = me.FindSpell("invoker_deafening_blast");
                    icewall = me.FindSpell("invoker_ice_wall");
                    ghostwalk = me.FindSpell("invoker_ghost_walk");
                    eul = me.FindItem("item_cyclone");
                    Utils.Sleep(250, "ORBSFIND");
                }
                if (me.CanMove() || me.CanCast())
                {
                    nextskillflee = NextSkillFlee();
                    if (nextskillflee == 1 && Utils.SleepCheck("ghostwalk"))
                    {
                        if (!Iscasted(ghostwalk) && invoke.CanBeCasted())
                        {
                            quas.UseAbility(false);
                            quas.UseAbility(false);
                            wex.UseAbility(false);
                            invoke.UseAbility(false);
                        }
                        if ((me.Health / (float)me.MaximumHealth) <= 0.5)
                        {
                            quas.UseAbility(false);
                            quas.UseAbility(false);
                            quas.UseAbility(false);
                        }
                        else
                        {
                            wex.UseAbility(false);
                            wex.UseAbility(false);
                            wex.UseAbility(false);
                        }
                        ghostwalk.UseAbility(false);
                        Utils.Sleep(500, "ghostwalk");
                    }
                    if (Utils.SleepCheck("movingnow"))
                    {
                        me.Move(Game.MousePosition, false);
                        Utils.Sleep(300, "movingnow");
                    }
                }
            }
            if (Game.IsKeyDown(Menu.Item("Target Select").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
                target = me.ClosestToMouseTarget(1000);
            if (Game.IsKeyDown(Menu.Item("Combo Mode").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
                {
                if (Utils.SleepCheck("ORBSFIND"))
                {
                    quas = me.Spellbook.SpellQ;
                    wex = me.Spellbook.SpellW;
                    exort = me.Spellbook.SpellE;
                    invoke = me.Spellbook.SpellR;
                    coldsnap = me.FindSpell("invoker_cold_snap");
                    forgespirit = me.FindSpell("invoker_forge_spirit");
                    meteor = me.FindSpell("invoker_chaos_meteor");
                    alacrity = me.FindSpell("invoker_alacrity");
                    tornado = me.FindSpell("invoker_tornado");
                    blast = me.FindSpell("invoker_deafening_blast");
                    sunstrike = me.FindSpell("invoker_sun_strike");
                    emp = me.FindSpell("invoker_emp");
                    icewall = me.FindSpell("invoker_ice_wall");
                    ghostwalk = me.FindSpell("invoker_ghost_walk");
                    eul = me.FindItem("item_cyclone");
                    medallion = me.FindItem("item_medallion_of_courage");
                    solar_crest = me.FindItem("item_solar_crest");
                    malevolence = me.FindItem("item_orchid");
                    bloodthorn = me.FindItem("item_bloodthorn");
                    urn = me.FindItem("item_urn_of_shadows");
                    Utils.Sleep(250, "ORBSFIND");
                }
                if (target != null && (!target.IsAlive || target.IsIllusion || distance_me_target > 3000 || !target.IsVisible))
                    target = null;
                if(target == null)
                    target = me.BestAATarget(1000);
                if (target != null && target.IsValid && !target.IsIllusion)
                {
                    distance_me_target = target.NetworkPosition.Distance2D(me.NetworkPosition);
                    myunits = ObjectMgr.GetEntities<Unit>().Where(x => x.Team == me.Team && x.IsControllable && x != null && x.IsAlive && x.Distance2D(target) <= 2000 && x.IsControllable && x.IsValid).ToList();
                    forge_in_my_side = ObjectMgr.GetEntities<Unit>().Where(x => x.Team == me.Team && x.IsControllable && x != null && x.IsAlive && x.Distance2D(target) <= 700 && x.Name.Contains("npc_dota_invoker_forged_spirit") && x.IsValid).Any();
                    ice_wall_distance = me.InFront(120).Distance2D(target.Position) <= 300;
                    target_magic_imune = target.IsMagicImmune();
                    target_isinvul = target.IsInvul();
                    target_blast_ontiming = IsOnTiming(blast);
                    target_meteor_ontiming = IsOnTiming(meteor);
                    target_emp_ontiming = IsOnTiming(emp);
                    target_sunstrike_ontiming = IsOnTiming(sunstrike);
                    if ((quas.Level > 0 || wex.Level > 0 || exort.Level > 0) && invoke.Level >= 1)
                    {
                        if (IsComboPrepared() != 0 || comboing)
                        {
                            // combo 1 - > euls -> meteor -> sunstrike -> defineblast
                            // //combo 2 -> emp -> tornado
                            // //combo 3 -> meteor -> tornado
                            if (!comboing)
                            {
                                comboing = true;
                                combonumber = IsComboPrepared();
                                Utils.Sleep(3000, "combotime");
                            }
                            if (Utils.SleepCheck("combotime"))
                                comboing = false;
                            if (combonumber == 1)
                            {
                                if (Utils.SleepCheck("orbwalker"))
                                {
                                    Orbwalking.Orbwalk(target);
                                    // me.Attack(target, false);
                                    Utils.Sleep(200, "orbwalker");
                                }
                                if (eul.CanBeCasted() && Utils.SleepCheck("eul"))
                                {
                                    eul.UseAbility(target,false);
                                    Utils.Sleep(800, "eul");
                                }
                                if(meteor.Cooldown == 0)
                                {
                                    InvokeSkill(meteor);
                                    if (meteor.CanBeCasted() && Utils.SleepCheck("cd_meteor") && target_meteor_ontiming)
                                    {
                                        meteor.UseAbility(target.Position, false);
                                        Utils.Sleep(250, "cd_meteor");
                                        Utils.Sleep(250, "cd_meteor_a");
                                    }
                                }
                                if (sunstrike.Cooldown == 0)
                                {
                                    InvokeSkill(sunstrike);
                                    if (sunstrike.CanBeCasted() && Utils.SleepCheck("cd_sunstrike") && target_sunstrike_ontiming)
                                    {
                                        sunstrike.UseAbility(target.Position, false);
                                        Utils.Sleep(250, "cd_sunstrike");
                                        Utils.Sleep(700, "cd_sunstrike_a");
                                    }
                                }
                                if (blast.Cooldown == 0 && sunstrike.Cooldown > 0 && meteor.Cooldown > 0)
                                {
                                    InvokeSkill(blast);
                                    if (blast.CanBeCasted() && Utils.SleepCheck("cd_blast") && target_blast_ontiming)
                                    {
                                        blast.UseAbility(target.Position, false);
                                        comboing = false;
                                        Utils.Sleep(250, "cd_blast");
                                        Utils.Sleep(800, "cd_blast_a");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (NextSkill() != nextskillvalue)
                        {
                            nextskillvalue = NextSkill();
                        }
                        if (nextskillvalue != 0)
                        {
                            // 1 - coldsnap, 2- meteor, 3 - alacrity, 4 - tornado, 5 -forgespirit, 6 -blast, 7 - sunstrike, 8 - emp, 9 - icewall, 10 - sunstrike
                            if (nextskillvalue == 1 && Utils.SleepCheck("cd_coldsnap"))
                            {
                                InvokeSkill(coldsnap);
                                    if (coldsnap.CanBeCasted())
                                    {
                                        coldsnap.UseAbility(target, false);
                                        Utils.Sleep(250, "cd_coldsnap");
                                    }
                            }
                            if (nextskillvalue == 2 && Utils.SleepCheck("cd_meteor"))
                            {
                                InvokeSkill(meteor);
                                    if (meteor.CanBeCasted())
                                    {
                                        meteor.UseAbility(Prediction.PredictedXYZ(target, 1300 / 4 + target.MovementSpeed), false);
                                        Utils.Sleep(250, "cd_meteor");
                                        Utils.Sleep(250, "cd_meteor_a");
                                    }
                            }
                            if (nextskillvalue == 3 && Utils.SleepCheck("cd_alacrity"))
                            {
                                InvokeSkill(alacrity);
                                    if (alacrity.CanBeCasted())
                                    {
                                        alacrity.UseAbility(me, false);
                                        Utils.Sleep(250, "cd_alacrity");
                                    }
                            }
                            if (nextskillvalue == 4 && Utils.SleepCheck("cd_tornado"))
                            {
                                InvokeSkill(tornado);
                                    if (tornado.CanBeCasted())
                                    {
                                        tornado.UseAbility(Prediction.PredictedXYZ(target, (distance_me_target/1100 * 1000) + target.MovementSpeed), false);
                                        Utils.Sleep(250, "cd_tornado");
                                        Utils.Sleep(850, "cd_tornado_a");
                                    }
                            }
                            if (nextskillvalue == 5 && Utils.SleepCheck("cd_forgespirit"))
                            {
                                InvokeSkill(forgespirit);
                                    if (forgespirit.CanBeCasted())
                                    {
                                        forgespirit.UseAbility(false);
                                        Utils.Sleep(250, "cd_forgespirit");
                                    }
                            }
                            if (nextskillvalue == 6 && Utils.SleepCheck("cd_blast"))
                            {
                                InvokeSkill(blast);
                                    if (blast.CanBeCasted())
                                    {
                                        blast.UseAbility(Prediction.PredictedXYZ(target, (distance_me_target / 1100 * 1000) + target.MovementSpeed), false);
                                        Utils.Sleep(250, "cd_blast");
                                        Utils.Sleep(800, "cd_blast_a");
                                    }
                            }
                            //if (nextskillvalue == 7)
                            //{
                            //    InvokeSkill(sunstrike);
                            //    sunstrike.UseAbility(target.Position, false);
                            //}
                            if (nextskillvalue == 8 && Utils.SleepCheck("cd_emp"))
                            {
                                InvokeSkill(emp);
                                    if (emp.CanBeCasted())
                                    {
                                        emp.UseAbility(Prediction.PredictedXYZ(target, 1700 / 3 + target.MovementSpeed), false);
                                        Utils.Sleep(250, "cd_emp");
                                        Utils.Sleep(1000, "cd_emp_a");
                                    }
                            }
                            if (nextskillvalue == 9 && Utils.SleepCheck("cd_icewall"))
                            {
                                InvokeSkill(icewall);
                                    if (icewall.CanBeCasted())
                                    {
                                        icewall.UseAbility(false);
                                        Utils.Sleep(250, "cd_icewall");
                                    }
                            }
                            if (nextskillvalue == 10 && Utils.SleepCheck("cd_sunstrike"))
                            {
                                InvokeSkill(sunstrike);
                                    if (sunstrike.CanBeCasted())
                                    {
                                        sunstrike.UseAbility(Prediction.PredictedXYZ(target, 1700/4 + target.MovementSpeed), false);
                                        Utils.Sleep(250, "cd_sunstrike");
                                        Utils.Sleep(700, "cd_sunstrike_a");
                                    }
                            }
                            if(nextskillvalue == 0 && Utils.SleepCheck("moving_idle"))
                                {
                                    me.Move(Game.MousePosition, false);
                                    Utils.Sleep(600, "moving_idle");
                                }
                            //// modifier_invoker_deafining_blast_knockback
                            //// modifier_invoker_tornado
                            //// modifier_eul_cyclone
                            //// modifier_obsidian_destroyer_astral_imprisonment_prison
                            //// modifier_shadow_demon_disruption
                        }
                        }
                    }
                    //itens
                    if (medallion.CanBeCasted() && !target_magic_imune && !target_isinvul && Utils.SleepCheck("medallion"))
                    {
                        medallion.UseAbility(target, false);
                        Utils.Sleep(500, "medallion");
                    }
                    if (solar_crest.CanBeCasted() && !target_magic_imune && !target_isinvul && Utils.SleepCheck("crest"))
                    {
                        solar_crest.UseAbility(target, false);
                        Utils.Sleep(500, "crest");
                    }
                    if (malevolence.CanBeCasted() && !target_magic_imune && !target_isinvul && Utils.SleepCheck("male") && !(IsComboPrepared() != 0 || comboing))
                    {
                        malevolence.UseAbility(target, false);
                        Utils.Sleep(500, "male");
                        Utils.Sleep(5000, "malepop");
                    }
                    if (bloodthorn.CanBeCasted() && !target_magic_imune && !target_isinvul && Utils.SleepCheck("blood") && !(IsComboPrepared() != 0 || comboing))
                    {
                        bloodthorn.UseAbility(target, false);
                        Utils.Sleep(500, "blood");
                        Utils.Sleep(5000, "bloodpop");
                    }
                    if (urn.CanBeCasted() && !target_magic_imune && !target_isinvul && Utils.SleepCheck("urn") && !(IsComboPrepared() != 0 || comboing))
                    {
                        urn.UseAbility(target, false);
                        Utils.Sleep(800, "urn");
                    }
                    if (Utils.SleepCheck("orbwalker"))
                    {
                        Orbwalking.Orbwalk(target);
                       // me.Attack(target, false);
                        Utils.Sleep(200, "orbwalker");
                    }
                    if (myunits != null)
                    {
                        foreach (Unit unit in myunits)
                        {
                            if (unit == null && unit.IsAlive) continue;
                            if (unit.Name.Contains("necronomicon") || unit.Name.Contains("npc_dota_invoker_forged_spirit"))
                            {
                                Ability spell;
                                if (Utils.SleepCheck("attack" + unit.Handle))
                                {
                                    unit.Attack(target);
                                    if (unit.Name.Contains("npc_dota_necronomicon_archer"))
                                    {
                                        spell = unit.Spellbook.Spell1;
                                        if (spell != null && spell.CanBeCasted(target))
                                            spell.UseAbility(target);
                                    }
                                    Utils.Sleep(1000, "attack" + unit.Handle);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Utils.SleepCheck("moving_idle"))
                    {
                        me.Move(Game.MousePosition, false);
                        Utils.Sleep(600, "moving_idle");
                    }
                }
            }
        }
        private static uint IsComboPrepared()
        {
            // combo 1 - > euls -> meteor -> sunstrike -> defineblast
            // combo 2 -> emp -> tornado
            // //combo 3 -> meteor -> tornado
            if (Iscasted(meteor) && Iscasted(sunstrike) && sunstrike.Cooldown == 0 && meteor.Cooldown == 0 && me.FindItem("item_cyclone") != null)
                return 1;
            //if (Iscasted(emp) && Iscasted(tornado) && emp.Cooldown == 0 && tornado.Cooldown == 0)
            //    return 2;
            //if (Iscasted(meteor) && Iscasted(tornado) && meteor.Cooldown == 0 && tornado.Cooldown == 0)
            //    return 3;
            //if ((me.Spellbook.SpellD.Name == "invoker_emp" || me.Spellbook.SpellD.Name == "invoker_tornado") && (me.Spellbook.SpellF.Name == "invoker_emp" || me.Spellbook.SpellF.Name == "invoker_tornado"))
            //    return 2;
            return 0;
        }
        private static bool IsOnTiming(Ability skill)
        {
            try
            {
                double timing = 10, timing_a = 0.4;
                if (skill.Name == sunstrike.Name)
                {
                    timing = 1.7 + (Game.Ping / 1000);
                    timing_a = 0.7;
                }
                else if (skill.Name == meteor.Name)
                {
                    timing = 1.5 + (Game.Ping / 1000);
                    timing_a = 0.5;
                }
                else if (skill.Name == emp.Name)
                {
                    timing = 2.9 + (Game.Ping / 1000);
                    timing_a = 1.5;
                }
                else if (skill.Name == blast.Name)
                {
                    timing = distance_me_target / 1100;
                    timing_a = 0;
                }
                if (target.HasModifier("modifier_invoker_deafining_blast_knockback") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_deafining_blast_knockback").RemainingTime <= timing) && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_deafining_blast_knockback").RemainingTime > timing_a))
                    return true;
                else if (target.HasModifier("modifier_invoker_tornado") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_tornado").RemainingTime <= timing) && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_tornado").RemainingTime > timing_a))
                    return true;
                else if (target.HasModifier("modifier_eul_cyclone") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone").RemainingTime <= timing) && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_eul_cyclone").RemainingTime > timing_a))
                    return true;
                else if (target.HasModifier("modifier_obsidian_destroyer_astral_imprisonment_prison") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison").RemainingTime <= timing) && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison").RemainingTime > timing_a))
                    return true;
                else if (target.HasModifier("modifier_invoker_cold_snap") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_invoker_cold_snap").RemainingTime > timing_a))
                    return true;
                else if (target.HasModifier("modifier_shadow_demon_disruption") && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_shadow_demon_disruption").RemainingTime <= timing) && (target.Modifiers.FirstOrDefault(x => x.Name == "modifier_shadow_demon_disruption").RemainingTime > timing_a))
                    return true;
                else
                    return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
        private static bool InvokerCanCast(Ability skill)
        {
            if (skill == null)
                return false;
            if (skill.Cooldown == 0 && me.Mana >= skill.ManaCost && skill.Level > 0 && me.CanCast())
            {
                if (skill.Name == coldsnap.Name && quas.Level > 0)
                    return true;
                else if (skill.Name == meteor.Name && wex.Level > 0 && exort.Level > 0)
                    return true;
                else if (skill.Name == alacrity.Name && wex.Level > 0 && exort.Level > 0)
                    return true;
                else if (skill.Name == tornado.Name && quas.Level > 0 && wex.Level > 0)
                    return true;
                else if (skill.Name == forgespirit.Name && quas.Level > 0 && exort.Level > 0)
                    return true;
                else if (skill.Name == blast.Name && quas.Level > 0 && wex.Level > 0 && exort.Level > 0)
                    return true;
                else if (skill.Name == sunstrike.Name && exort.Level > 0)
                    return true;
                else if (skill.Name == ghostwalk.Name && quas.Level > 0 && wex.Level > 0)
                    return true;
                else if (skill.Name == icewall.Name && exort.Level > 0 && exort.Level > 0)
                    return true;
                else if (skill.Name == emp.Name && wex.Level > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        private static uint NextSkillFlee()
        {
            try
            {
                // 1 - ghostwalk, 2 icewall, 3 - coldsnap, 4 - blast
                if (Iscasted(ghostwalk) && ghostwalk.CanBeCasted() && !me.IsInvisible())
                    return 1;
                if (InvokerCanCast(ghostwalk) && !me.IsInvisible())
                    return 1;
                return 0;
            }
            catch (NullReferenceException)
            {
                return 0;
            }
        }
        private static uint NextSkill()
        {
            // 1 - coldsnap, 2- meteor, 3 - alacrity, 4 - tornado, 5 -forgespirit, 6 -blast, 7 - sunstrike(off), 8 - emp, 9 - icewall, 10 - sunstrike
            try
            {
                if (Iscasted(coldsnap) && coldsnap.CanBeCasted() && distance_me_target <= 750 && !target_isinvul && !target_magic_imune)
                    return 1;
                if (Iscasted(forgespirit) && forgespirit.CanBeCasted() && distance_me_target <= 600 && !target_isinvul && !forge_in_my_side)
                    return 5;
                if (Iscasted(alacrity) && alacrity.CanBeCasted() && distance_me_target <= 700 && !target_isinvul)
                    return 3;
                if (Iscasted(icewall) && icewall.CanBeCasted() && distance_me_target <= 200 && !target_magic_imune)
                    return 9;
                if (Iscasted(blast) && blast.CanBeCasted() && distance_me_target <= 600 && (!target_isinvul || target_blast_ontiming))
                    return 6;
                if (Iscasted(tornado) && tornado.CanBeCasted() && !target_isinvul && !target_magic_imune && distance_me_target <= 2800 && Utils.SleepCheck("bloodpop") && Utils.SleepCheck("malepop"))
                    return 4;
                if (Iscasted(meteor) && meteor.CanBeCasted() && !target_magic_imune && (target.MovementSpeed <= 250 || target_meteor_ontiming) && distance_me_target <= 700 && Utils.SleepCheck("cd_tornado_a"))
                    return 2;
                if (Iscasted(sunstrike) && sunstrike.CanBeCasted() && (target.MovementSpeed < 200 || target_sunstrike_ontiming))
                    return 10;
                if (Iscasted(emp) && emp.CanBeCasted() && (target.MovementSpeed <= 190 || target_emp_ontiming) && distance_me_target <= 700)
                    return 8;
                //quas, wex, exort checker
                if(me.Level <= 6)
                {
                    if (quas.Level >= 2)
                        quas_level = true;
                    else
                        quas_level = false;
                    if (wex.Level >= 2)
                        wex_level = true;
                    else
                        wex_level = false;
                    if (exort.Level >= 2)
                        exort_level = true;
                    else
                        exort_level = false;
                }
                else if (me.Level <= 10)
                {
                    if (quas.Level >= 3)
                        quas_level = true;
                    else
                        quas_level = false;
                    if (wex.Level >= 3)
                        wex_level = true;
                    else
                        wex_level = false;
                    if (exort.Level >= 3)
                        exort_level = true;
                    else
                        exort_level = false;
                }
                else if (me.Level <= 15)
                {
                    if (quas.Level >= 4)
                        quas_level = true;
                    else
                        quas_level = false;
                    if (wex.Level >= 4)
                        wex_level = true;
                    else
                        wex_level = false;
                    if (exort.Level >= 4)
                        exort_level = true;
                    else
                        exort_level = false;
                }
                else if(me.Level <= 25)
                {
                    if (quas.Level >= 5)
                        quas_level = true;
                    else
                        quas_level = false;
                    if (wex.Level >= 5)
                        wex_level = true;
                    else
                        wex_level = false;
                    if (exort.Level >= 5)
                        exort_level = true;
                    else
                        exort_level = false;
                }
                //skills sequence
                if (quas_level && coldsnap.Cooldown == 0 && distance_me_target <= 750 && !target_isinvul && !target_magic_imune)
                    return 1;
                if (exort_level && forgespirit.Cooldown == 0 && distance_me_target <= 600 && !target_isinvul && !forge_in_my_side)
                    return 5;
                if ((exort_level || wex_level) && alacrity.Cooldown == 0 && distance_me_target <= 600 && !target_isinvul)
                    return 3;
                if (quas_level && icewall.Cooldown == 0 && !target_magic_imune && ice_wall_distance)
                    return 9;
                if (wex_level && tornado.Cooldown == 0 && !target_isinvul && !target_magic_imune && distance_me_target <= 2800 && Utils.SleepCheck("cd_meteor_a") && Utils.SleepCheck("cd_blast_a") && Utils.SleepCheck("cd_emp_a") && Utils.SleepCheck("bloodpop") && Utils.SleepCheck("malepop"))
                    return 4;
                if (exort_level && meteor.Cooldown == 0 && !target_magic_imune && (target.MovementSpeed <= 250 || target_meteor_ontiming) && distance_me_target <= 700 && Utils.SleepCheck("cd_tornado_a"))
                    return 2;
                if (exort_level && sunstrike.Cooldown == 0 && (target.MovementSpeed < 200 || target_sunstrike_ontiming) && Utils.SleepCheck("cd_tornado_a") && me.AttackSpeedValue >= 150)
                    return 10;
                if ((exort_level || quas_level || wex_level) && blast.Cooldown == 0 && !target_magic_imune && distance_me_target <= 950 && !target_isinvul && Utils.SleepCheck("cd_tornado_a"))
                    return 6;
                if (wex_level && emp.Cooldown == 0 && (target.MovementSpeed <= 190 || target_emp_ontiming) && Utils.SleepCheck("cd_tornado_a") && distance_me_target <= 700 && (target.Mana > target.MaximumMana * 0.35))
                    return 8;

                return 0;
            }
            catch (NullReferenceException)
            {
                return 0;
            }
        }
        private static void orb_type(Ability skill)
        {
            if (skill == null) return;
            if (!Utils.SleepCheck("PINGCANCEL")) return;
            if (skill.Name == quas.Name)
            {
                quas.UseAbility(false);
                quas.UseAbility(false);
                quas.UseAbility(false);
                Utils.Sleep(Game.Ping, "PINGCANCEL");
            }
            else if (skill.Name == wex.Name)
            {
                wex.UseAbility(false);
                wex.UseAbility(false);
                wex.UseAbility(false);
                Utils.Sleep(Game.Ping, "PINGCANCEL");
            }
            else if (skill.Name == exort.Name)
            {
                exort.UseAbility(false);
                exort.UseAbility(false);
                exort.UseAbility(false);
                Utils.Sleep(Game.Ping,"PINGCANCEL");
            }
            else
                return;
        }
        private static void InvokeSkill(Ability skill)
        {
            if (skill == null) return;
            if (!Utils.SleepCheck("PINGCANCEL")) return;
            //quas, wex, exort, invoke, coldsnap, meteor, alacrity, tornado, forgespirit, blast, sunstrike, emp, icewall, ghostwalk;
            if (skill.Name == coldsnap.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    quas.UseAbility(false);
                    quas.UseAbility(false);
                    quas.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == meteor.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    exort.UseAbility(false);
                    exort.UseAbility(false);
                    wex.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == alacrity.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    wex.UseAbility(false);
                    wex.UseAbility(false);
                    exort.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == tornado.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    wex.UseAbility(false);
                    wex.UseAbility(false);
                    quas.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == forgespirit.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    exort.UseAbility(false);
                    exort.UseAbility(false);
                    quas.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == blast.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    quas.UseAbility(false);
                    wex.UseAbility(false);
                    exort.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == sunstrike.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    exort.UseAbility(false);
                    exort.UseAbility(false);
                    exort.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == emp.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    wex.UseAbility(false);
                    wex.UseAbility(false);
                    wex.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == icewall.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    quas.UseAbility(false);
                    quas.UseAbility(false);
                    exort.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else if (skill.Name == ghostwalk.Name)
            {
                if (!Iscasted(skill) && invoke.CanBeCasted())
                {
                    quas.UseAbility(false);
                    quas.UseAbility(false);
                    wex.UseAbility(false);
                    invoke.UseAbility(false);
                    Utils.Sleep(Game.Ping, "PINGCANCEL");
                    return;
                }
            }
            else
                return;

        }
        private static bool Iscasted(Ability skill)
        {
            if (skill.AbilitySlot == AbilitySlot.Slot_4 || skill.AbilitySlot == AbilitySlot.Slot_5)
                return true;
            else
                return false;
        }
    }
}
