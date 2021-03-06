﻿#region

#endregion

namespace Irelia_Reloaded
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        ///     The gatotsu tick
        /// </summary>
        private static int gatotsuTick;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the botrk.
        /// </summary>
        /// <value>
        ///     The botrk.
        /// </value>
        private static Items.Item Botrk { get; set; }

        /// <summary>
        ///     Gets or sets the cutlass.
        /// </summary>
        /// <value>
        ///     The cutlass.
        /// </value>
        private static Items.Item Cutlass { get; set; }

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance has sheen buff.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has sheen buff; otherwise, <c>false</c>.
        /// </value>
        private static bool HasSheenBuff
        {
            get
            {
                return Player.HasBuff("sheen");
            }
        }

        /// <summary>
        ///     Gets or sets the ignite slot.
        /// </summary>
        /// <value>
        ///     The ignite slot.
        /// </value>
        private static SpellSlot IgniteSlot { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the omen.
        /// </summary>
        /// <value>
        ///     The omen.
        /// </value>
        private static Items.Item Omen { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        private static Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Program" /> is packets.
        /// </summary>
        /// <value>
        ///     <c>true</c> if packets; otherwise, <c>false</c>.
        /// </value>
        private static bool Packets
        {
            get
            {
                return Menu.Item("packets").GetValue<bool>();
            }
        }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the ult is activated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the ult is activated; otherwise, <c>false</c>.
        /// </value>
        private static bool UltActivated
        {
            get
            {
                return Player.HasBuff("ireliatranscendentbladesspell");
            }
        }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private static Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Fired when the OnGameLoad event is fired.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void GameOnOnGameLoad(EventArgs args)
        {
            if (Player.CharData.BaseSkinName != "Irelia")
            {
                return;
            }

            // Setup Spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 1000);

            // Setup Ignite
            IgniteSlot = Player.GetSpellSlot("summonerdot");

            // Add skillshots
            Q.SetTargetted(0f, 2200);
            R.SetSkillshot(0.5f, 120, 1600, false, SkillshotType.SkillshotLine);

            // Create Items
            Botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            Cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            Omen = ItemData.Randuins_Omen.GetItem();

            // Create Menu
            SetupMenu();

            // sexy af color, ty kawaii Kurisu :3
            Game.PrintChat("<font color=\"#7CFC00\"><b>Irelia Reloaded:</b></font> Loaded");

            // Setup Dmg Indicator
            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            // Subscribe to needed events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;

            // to get Q tickcount in least amount of lines.
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when there is an incoming capcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget() && Menu.Item("gapcloserE").GetValue<bool>() && E.IsReady())
            {
                E.Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void Combo()
        {
            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useW = Menu.Item("useW").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var useR = Menu.Item("useR").GetValue<bool>();
            var minQRange = Menu.Item("minQRange").GetValue<Slider>().Value;
            var useEStun = Menu.Item("useEStun").GetValue<bool>();
            var useQGapclose = Menu.Item("useQGapclose").GetValue<bool>();
            var useWBeforeQ = Menu.Item("useWBeforeQ").GetValue<bool>();
            var procSheen = Menu.Item("procSheen").GetValue<bool>();
            var useIgnite = Menu.Item("useIgnite").GetValue<bool>();
            var useRGapclose = Menu.Item("useRGapclose").GetValue<bool>();

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target == null && useQGapclose)
            {
                /** var minionQ =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget())
                        .Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .FirstOrDefault(
                            x =>
                                x.Distance(TargetSelector.GetTarget(Q.Range * 5, TargetSelector.DamageType.Physical)) <
                                Q.Range);*/

                var minionQ =
                    MinionManager.GetMinions(Q.Range)
                        .Where(
                            x =>
                            x.IsValidTarget(Q.Range) && Player.GetSpellDamage(x, SpellSlot.Q) > x.Health
                            && Player.Distance(HeroManager.Enemies.OrderBy(y => y.Distance(Player)).FirstOrDefault())
                            < x.Distance(HeroManager.Enemies.OrderBy(y => y.Distance(Player)).FirstOrDefault()))
                        .FirstOrDefault(
                            x =>
                            x.Distance(HeroManager.Enemies.OrderBy(y => y.Distance(Player)).FirstOrDefault()) < Q.Range);

                if (minionQ != null)
                {
                    Q.CastOnUnit(minionQ, Packets);
                    return;
                }

                if (useRGapclose)
                {
                    var minionR = ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget())
                        .Where(x => x.Distance(Player) < Q.Range) // Use Q.Range so we follow up with a Q
                        .Where(x => x.CountEnemiesInRange(Q.Range) >= 1)
                        .FirstOrDefault(
                            x =>
                            x.Health - Player.GetSpellDamage(x, SpellSlot.R) < Player.GetSpellDamage(x, SpellSlot.Q));

                    if (minionR != null)
                    {
                        R.Cast(minionR, Packets);
                    }
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Botrk.IsReady())
            {
                Botrk.Cast(target);
            }

            if (Cutlass.IsReady())
            {
                Cutlass.Cast(target);
            }

            if (Omen.IsReady() && Omen.IsInRange(target)
                && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player))
            {
                Omen.Cast();
            }

            if (useIgnite && target != null && target.IsValidTarget(600)
                && (IgniteSlot.IsReady()
                    && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health))
            {
                Player.Spellbook.CastSpell(IgniteSlot, target);
            }

            if (useWBeforeQ)
            {
                if (useW && W.IsReady())
                {
                    W.Cast(Packets);
                }

                if (useQ && Q.IsReady() && target.Distance(Player.ServerPosition) > minQRange)
                {
                    Q.CastOnUnit(target, Packets);
                }
            }
            else
            {
                if (useQ && Q.IsReady() && target.Distance(Player.ServerPosition) > minQRange)
                {
                    Q.CastOnUnit(target, Packets);
                }

                if (useW && W.IsReady())
                {
                    W.Cast(Packets);
                }
            }

            if (useEStun)
            {
                if (target.CanStunTarget() && useE && E.IsReady())
                {
                    E.Cast(target, Packets);
                }
            }
            else
            {
                if (useE && E.IsReady())
                {
                    E.Cast(target, Packets);
                }
            }

            if (useR && R.IsReady() && !UltActivated)
            {
                R.Cast(target, Packets);
            }

            // Get target that is in the R range
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (useR && UltActivated)
            {
                if (!procSheen)
                {
                    return;
                }

                // Fire Ult if player is out of AA range, with Q not up or not in range
                if (target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player))
                {
                    R.Cast(rTarget, Packets);
                }
                else
                {
                    if (!HasSheenBuff)
                    {
                        R.Cast(rTarget, Packets);
                    }
                }
            }
            else
            {
                R.Cast(rTarget, Packets);
            }
        }

        /// <summary>
        ///     Get the damage to a hero.
        /// </summary>
        /// <param name="hero">The hero.</param>
        /// <returns>The damage done to the hero.</returns>
        private static float DamageToUnit(Obj_AI_Hero hero)
        {
            float dmg = 0;

            var spells = new List<Spell> { Q, W, E, R };
            foreach (var spell in spells.Where(x => x.IsReady()))
            {
                // Account for each blade
                if (spell.Slot == SpellSlot.R)
                {
                    dmg += spell.GetDamage(hero) * 4;
                }
                else
                {
                    dmg += spell.GetDamage(hero);
                }
            }

            if (Botrk.IsReady())
            {
                dmg += (float)Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }

            if (Cutlass.IsReady())
            {
                dmg += (float)Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }

            return dmg;
        }

        /// <summary>
        ///     Fired when the game redraws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();
            var drawR = Menu.Item("drawR").GetValue<bool>();
            var drawStunnable = Menu.Item("drawStunnable").GetValue<bool>();
            var p = Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            foreach (var minion in
                MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
            {
                Render.Circle.DrawCircle(minion.Position, 65, Color.FromArgb(124, 252, 0), 3);
            }

            if (!drawStunnable)
            {
                return;
            }

            foreach (var unit in
                ObjectManager.Get<Obj_AI_Hero>().Where(x => x.CanStunTarget() && x.IsValidTarget()))
            {
                var drawPos = Drawing.WorldToScreen(unit.Position);
                var textSize = Drawing.GetTextExtent("Stunnable");
                Drawing.DrawText(drawPos.X - textSize.Width / 2f, drawPos.Y, Color.Aqua, "Stunnable");
            }
        }

        /// <summary>
        ///     Fired when teh game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameUpdate(EventArgs args)
        {
            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }

        /// <summary>
        ///     Fired when there is an interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private static void InterrupterOnOnPossibleToInterrupt(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var spell = args;
            var unit = sender;

            if (spell.DangerLevel != Interrupter2.DangerLevel.High || !unit.CanStunTarget())
            {
                return;
            }

            var interruptE = Menu.Item("interruptE").GetValue<bool>();
            var interruptQe = Menu.Item("interruptQE").GetValue<bool>();

            if (E.IsReady() && E.IsInRange(unit, E.Range) && interruptE)
            {
                E.Cast(unit, Packets);
            }

            if (Q.IsReady() && E.IsReady() && Q.IsInRange(unit, Q.Range) && interruptQe)
            {
                Q.Cast(unit, Packets);

                var timeToArrive = (int)(1000 * Player.Distance(unit) / Q.Speed + Q.Delay);
                Utility.DelayAction.Add(timeToArrive, () => E.Cast(unit, Packets));
            }
        }

        /// <summary>
        ///     Steals kills.
        /// </summary>
        private static void KillSteal()
        {
            var useQ = Menu.Item("useQKS").GetValue<bool>();
            var useR = Menu.Item("useRKS").GetValue<bool>();
            var useIgnite = Menu.Item("useIgniteKS").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(Q.Range) && Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .OrderBy(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Q.Cast(bestTarget, Packets);
                }
            }

            if (useR && (R.IsReady() || UltActivated))
            {
                //TODO: Account for all 4 Charges
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(R.Range))
                        .Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .OrderBy(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    R.Cast(bestTarget, Packets);
                }
            }

            if (useIgnite && IgniteSlot.IsReady())
            {
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(600))
                        .Where(x => Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) / 5 > x.Health)
                        .OrderBy(x => x.ChampionsKilled)
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, bestTarget);
                }
            }
        }

        /// <summary>
        ///     Last hits minions.
        /// </summary>
        private static void LastHit()
        {
            var useQ = Menu.Item("lastHitQ").GetValue<bool>();
            var waitTime = Menu.Item("gatotsuTime").GetValue<Slider>().Value;
            var manaNeeded = Menu.Item("manaNeededQ").GetValue<Slider>().Value;
            var dontQUnderTower = Menu.Item("noQMinionTower").GetValue<bool>();

            if (useQ && Player.Mana / Player.MaxMana * 100 > manaNeeded
                && Environment.TickCount - gatotsuTick >= waitTime * 10)
            {
                foreach (var minion in
                    MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                {
                    if (dontQUnderTower && !minion.UnderTurret())
                    {
                        Q.Cast(minion, Packets);
                    }
                    else
                    {
                        Q.Cast(minion, Packets);
                    }
                }
            }
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        /// <summary>
        ///     Fired when the OnProcessSpellCast event is fired.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "IreliaGatotsu" && sender.IsMe)
            {
                gatotsuTick = Environment.TickCount;
            }
        }

        /// <summary>
        ///     Setups the menu.
        /// </summary>
        private static void SetupMenu()
        {
            Menu = new Menu("Irelia Reloaded", "cmIreliaReloaded", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "cmTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalking", "cmOrbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "cmCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQGapclose", "Gapclose with Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("minQRange", "Minimum Q Range")).SetValue(new Slider(250, 20, 400));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useEStun", "Only Use E to Stun").SetValue(false));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWBeforeQ", "W before Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("procSheen", "Proc Sheen Before Firing R").SetValue(true));
            comboMenu.AddItem(new MenuItem("useRGapclose", "Use R to Weaken Minion to Gapclose").SetValue(true));
            comboMenu.AddItem(new MenuItem("useIgnite", "Use Ignite").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // KS
            var ksMenu = new Menu("KS", "cmKS");
            ksMenu.AddItem(new MenuItem("useQKS", "KS With Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("useRKS", "KS With R").SetValue(false));
            ksMenu.AddItem(new MenuItem("useIgniteKS", "KS with Ignite").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Farming
            var farmingMenu = new Menu("Farming", "cmFarming");
            farmingMenu.AddItem(new MenuItem("lastHitQ", "Last Hit w/ Q").SetValue(false));
            farmingMenu.AddItem(new MenuItem("manaNeededQ", "Last Hit Mana %")).SetValue(new Slider(35));
            farmingMenu.AddItem(new MenuItem("noQMinionTower", "Don't Q Minion Undertower").SetValue(true));
            farmingMenu.AddItem(new MenuItem("gatotsuTime", "Delay between Q")).SetValue(new Slider(35));

            // Wave Clear SubMenu
            var waveClearMenu = new Menu("Wave Clear", "cmWaveClear");
            waveClearMenu.AddItem(new MenuItem("waveclearQ", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearQKillable", "Only Q Killable Minion").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearW", "Use W").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearR", "Use R").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("waveClearMana", "Wave Clear Mana %").SetValue(new Slider(20)));
            farmingMenu.AddSubMenu(waveClearMenu);
            Menu.AddSubMenu(farmingMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "cmDraw");
            drawMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawDmg", "Draw Combo Damage").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawStunnable", "Draw Stunnable").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawKillableQ", "Draw Minions Killable with Q").SetValue(false));
            Menu.AddSubMenu(drawMenu);

            // Misc
            var miscMenu = new Menu("Misc", "cmMisc");
            miscMenu.AddItem(new MenuItem("packets", "Use Packets").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptE", "E to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "Use Q+E to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("gapcloserE", "Use E on Gapcloser").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Does the wave clear.
        /// </summary>
        private static void WaveClear()
        {
            var useQ = Menu.Item("waveclearQ").GetValue<bool>();
            var useQKillable = Menu.Item("waveclearQKillable").GetValue<bool>();
            var useW = Menu.Item("waveclearW").GetValue<bool>();
            var useR = Menu.Item("waveclearR").GetValue<bool>();
            var reqMana = Menu.Item("waveClearMana").GetValue<Slider>().Value;
            var waitTime = Menu.Item("gatotsuTime").GetValue<Slider>().Value;
            var dontQUnderTower = Menu.Item("noQMinionTower").GetValue<bool>();

            if (Player.ManaPercent < reqMana)
            {
                return;
            }

            if (useQ && Q.IsReady() && Environment.TickCount - gatotsuTick >= waitTime * 10)
            {
                if (useQKillable)
                {
                    foreach (var minion in
                        MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                    {
                        if (dontQUnderTower)
                        {
                            if (!minion.UnderTurret())
                            {
                                Q.Cast(minion, Packets);
                            }
                        }
                        else
                        {
                            Q.Cast(minion, Packets);
                        }
                    }
                }
                else
                {
                    Q.Cast(MinionManager.GetMinions(Q.Range).FirstOrDefault(), Packets);
                }
            }

            if (useW && W.IsReady())
            {
                if (Orbwalker.GetTarget() is Obj_AI_Minion && W.IsInRange(Orbwalker.GetTarget().Position, W.Range))
                {
                    W.Cast(Packets);
                }
            }

            if (!useR || !R.IsReady())
            {
                return;
            }

            // Get best position for ult
            var pos = R.GetLineFarmLocation(MinionManager.GetMinions(R.Range));
            R.Cast(pos.Position, Packets);
        }

        #endregion
    }

    /// <summary>
    ///     Provides helpful extensions
    /// </summary>
    public static class Extension
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether this instance can stun the target.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns><c>true</c> if the Player can stun the unit.</returns>
        public static bool CanStunTarget(this AttackableUnit unit)
        {
            return unit.Health / unit.MaxHealth * 100
                   > ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;
        }

        #endregion
    }
}