using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace KayleHu3
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Targeted W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        public static Menu Menu, SkillMenu, MiscMenu, DrawMenu;
        public static string Version = "2.1";


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.ChampionName != "Kayle")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Targeted(SpellSlot.W, 900);
            E = new Spell.Active(SpellSlot.E, 525);
            R = new Spell.Targeted(SpellSlot.R, 900);

            #region Menu
            Menu = MainMenu.AddMenu("Kayle Hu3", "kaylehu3");
            Menu.AddGroupLabel("Kayle Hu3 V" + Version);
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SkillMenu = Menu.AddSubMenu("Skills", "Skills");
            SkillMenu.AddGroupLabel("Skills");
            SkillMenu.AddLabel("Combo");
            SkillMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SkillMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SkillMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SkillMenu.AddLabel("Harass");
            SkillMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SkillMenu.Add("Eharass", new CheckBox("Use E on Harass"));
            SkillMenu.AddLabel("LastHit");
            SkillMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SkillMenu.Add("Elast", new CheckBox("Use E on LastHit"));
            SkillMenu.Add("manaLast", new Slider("Mana % To Use Q", 30));
            SkillMenu.AddLabel("LaneClear");
            SkillMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SkillMenu.Add("Elane", new CheckBox("Use E on LaneClear"));
            SkillMenu.Add("manaLane", new Slider("Mana % To Use Q", 30));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            MiscMenu.AddLabel("Ult Manager");
            MiscMenu.Add("Rme", new CheckBox("Use R in Yourself"));
            MiscMenu.Add("HPme", new Slider("Health % To Use R", 20));
            MiscMenu.Add("Rally", new CheckBox("Use R in Ally"));
            MiscMenu.Add("HPally", new Slider("Health % To Use R on Ally", 20));
            MiscMenu.AddLabel("Health Manager");
            MiscMenu.Add("Wme", new CheckBox("Use W in Yourself"));
            MiscMenu.Add("HPWme", new Slider("Health % To Use W", 80));
            MiscMenu.Add("Wally", new CheckBox("Use W in Ally"));
            MiscMenu.Add("HPWally", new Slider("Health % To Use W on Ally", 40));

            DrawMenu = Menu.AddSubMenu("Draw", "Draw");
            DrawMenu.AddGroupLabel("Draw");
            DrawMenu.AddLabel("Draw");
            DrawMenu.Add("Qdraw", new CheckBox("Draw Q"));
            DrawMenu.Add("Wdraw", new CheckBox("Draw W"));
            DrawMenu.Add("Edraw", new CheckBox("Draw E"));
            DrawMenu.Add("Rdraw", new CheckBox("Draw R"));
            DrawMenu.Add("drawWhenReady", new CheckBox("Draw When SKills Are Ready"));
            #endregion Menu

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling()) return;

            KillSteal();

            AutoUlt();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
        }

        private static void KillSteal()
        {
            var useQ = MiscMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= Player.GetSpellDamage(target ,SpellSlot.Q))
            {
                Q.Cast(target);
            }

        }

        private static void Combo()
        {
            var useQ = SkillMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }   
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast();
            }
            if (useW && W.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && Player.HealthPercent < 95)
            {
                W.Cast(Player);
            }
        }

        private static void Harass()
        {
            var useQ = SkillMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["Eharass"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void LaneClear()
        {
            var useQ = SkillMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["Elane"].Cast<CheckBox>().CurrentValue;
            var mana = SkillMenu["manaLane"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy);
            if (useQ && Q.IsReady() && Player.ManaPercent >= mana)
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget(Q.Range) && minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast(minion);
                    }
                }
            }
            if (useE && E.IsReady() && Player.ManaPercent >= mana)
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void LastHit()
        {
            var useQ = SkillMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["Elast"].Cast<CheckBox>().CurrentValue;
            var mana = SkillMenu["manaLast"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (useQ && Q.IsReady() && Player.ManaPercent >= mana)
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget(Q.Range) && minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast(minion);
                    }
                }
            }

            if (useE && E.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget(650) && minion.Health <= Player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.Cast();
                    }
                }

            }
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)
                || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                var minion = target as Obj_AI_Base;
                var useE = SkillMenu["Elast"].Cast<CheckBox>().CurrentValue;
                var mana = SkillMenu["manaLast"].Cast<Slider>().CurrentValue;

                if (minion != null)
                {
                    if (useE && E.IsReady() && Player.ManaPercent >= mana &&
                        Player.GetSpellDamage(minion, SpellSlot.E) >= minion.Health && minion.IsValidTarget())
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void AutoUlt()
        {
            var Rme = MiscMenu["Rme"].Cast<CheckBox>().CurrentValue;
            var Rally = MiscMenu["Rally"].Cast<CheckBox>().CurrentValue;
            var HPme = MiscMenu["HPme"].Cast<Slider>().CurrentValue;
            var HPally = MiscMenu["HPally"].Cast<Slider>().CurrentValue;
            var Wme = MiscMenu["Wme"].Cast<CheckBox>().CurrentValue;
            var Wally = MiscMenu["Wally"].Cast<CheckBox>().CurrentValue;
            var HPWme = MiscMenu["HPWme"].Cast<Slider>().CurrentValue;
            var HPWally = MiscMenu["HPWally"].Cast<Slider>().CurrentValue;
            var allies = EntityManager.Heroes.Allies.OrderBy(a => a.Health).Where(a => !a.IsZombie);

            if (Rme && R.IsReady() && Player.HealthPercent < HPme && R.IsReady() && Player.CountEnemiesInRange(R.Range) >= 1)
            {
                R.Cast(Player);
            }
            if (Wme && W.IsReady() && Player.HealthPercent < HPWme)
            {
                W.Cast(Player);
            }
            if (Wally && W.IsReady())
            {
                foreach (var ally in allies)
                    if (ally.Health < HPWally)
                    {
                        R.Cast(ally);
                    }
            }

            if (Rally && R.IsReady())
            {
                foreach (var ally in allies)
                    if (ally.Health < HPally && ally.CountEnemiesInRange(1000) >= 1)
                    {
                        R.Cast(ally);
                    }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;

            var drawQ = DrawMenu["Qdraw"].Cast<CheckBox>().CurrentValue;
            var drawW = DrawMenu["Wdraw"].Cast<CheckBox>().CurrentValue;
            var drawE = DrawMenu["Edraw"].Cast<CheckBox>().CurrentValue;
            var drawR = DrawMenu["Rdraw"].Cast<CheckBox>().CurrentValue;
            var drawReady = DrawMenu["drawWhenReady"].Cast<CheckBox>().CurrentValue;

            if (drawReady)
            {
                if (drawQ && Q.IsReady())
                {
                    new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Position);
                }
                if (drawW && W.IsReady())
                {
                    new Circle() { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(Player.Position);
                }
                if (drawE && E.IsReady())
                {
                    new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = E.Range }.Draw(Player.Position);
                }
                if (drawR && R.IsReady())
                {
                    new Circle() { Color = Color.DeepPink, BorderWidth = 1, Radius = R.Range }.Draw(Player.Position);
                }
            }
            else
            {
                if (drawQ)
                {
                    new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Position);
                }
                if (drawW)
                {
                    new Circle() { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(Player.Position);
                }
                if (drawE)
                {
                    new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = E.Range }.Draw(Player.Position);
                }
                if (drawR)
                {
                    new Circle() { Color = Color.DeepPink, BorderWidth = 1, Radius = R.Range }.Draw(Player.Position);
                }
            }
        }
    }
}
