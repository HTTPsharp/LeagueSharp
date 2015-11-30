using System;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using HttpDinger;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace HttpDinger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Welcome Message upon loading assembly.
            Game.PrintChat(
                "<font color=\"#00BFFF\">HttpDinger -<font color=\"#FFFFFF\"> Recommended Version Successfully Loaded.</font>");
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        public const string ChampName = "heimerdinger";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        private static Items.Item zhonyas;
        public static Spell Q;
        public static Spell Q1;
        public static Spell W;
        public static Spell E;
        public static Spell E1;
        public static Spell E2;
        public static Spell E3;
        public static Spell QR;
        public static Spell WR;
        public static Spell ER;
        public static Spell R;
        private static Obj_AI_Hero player = ObjectManager.Player;
        public static SpellSlot Ignite;

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            zhonyas = new Items.Item(3157, 1f);
            E = new Spell(SpellSlot.E, 925);
            E.SetSkillshot(0.5f, 120f, 1200f, false, SkillshotType.SkillshotCircle);

            Q = new Spell(SpellSlot.Q, 325);
            Q.SetSkillshot(0.5f, 40f, 1100f, true, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 100);
            WR = new Spell(SpellSlot.W, 1100);

            E1 = new Spell(SpellSlot.E, 925);
            E2 = new Spell(SpellSlot.E, 1125);
            E3 = new Spell(SpellSlot.E, 1325);

            W = new Spell(SpellSlot.W, 1150);

            W.SetSkillshot(0.5f, 40f, 3000f, true, SkillshotType.SkillshotLine);
            WR.SetSkillshot(0.5f, 40f, 3000f, true, SkillshotType.SkillshotLine);
            E1.SetSkillshot(0.5f, 120f, 1200f, false, SkillshotType.SkillshotLine);
            E2.SetSkillshot(0.25f + E1.Delay, 120f, 1200f, false, SkillshotType.SkillshotLine);
            E3.SetSkillshot(0.3f + E2.Delay, 120f, 1200f, false, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 100);



            Config = new Menu("HttpDinger", "heimer", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            //COMBOMENU

            var combo = Config.AddSubMenu(new Menu("Combo Settings", "Combo Settings"));
            combo.AddItem(new MenuItem("UseR", "Use R -").SetValue(true));
            combo.AddItem(new MenuItem("UseQ", "Use Q -").SetValue(true));
            combo.AddItem(new MenuItem("UseQR", "Use QR").SetValue(true));
            combo.AddItem(new MenuItem("QRcount", "Want use Enemy").SetValue(new Slider(2, 5, 1)));
            combo.AddItem(new MenuItem("Blank", "                                         "));
            combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("UseWR", "Use WR").SetValue(true));
            combo.AddItem(new MenuItem("eBlank", "                                         "));
            combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("UseER", "Use ER").SetValue(true));
            combo.AddItem(new MenuItem("ERcount", "use Stun count").SetValue(new Slider(3, 5, 1)));

            combo.AddItem(new MenuItem("dBlank", "                                         "));
            combo.AddItem(new MenuItem("zhonyas", "Use Zhonyas").SetValue(true));
            combo.AddItem(new MenuItem("zhonyashp", "HP Percent").SetValue(new Slider(30, 100, 0)));

            //HARASSMENU

            Config.SubMenu("Harass Settings")
                .AddItem(new MenuItem("harassW", "Use W").SetValue(true));
            Config.SubMenu("Harass Settings")
                .AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            Config.SubMenu("Harass Settings")
                .AddItem(new MenuItem("harassmana", "Mana Percent").SetValue(new Slider(30, 100, 0)));

            //LANECLEARMENU
            Config.SubMenu("Laneclear Settings")
            .AddItem(new MenuItem("laneW", "Use W").SetValue(true));
            Config.SubMenu("Laneclear Settings")
                .AddItem(new MenuItem("laneE", "Use E").SetValue(true));
            Config.SubMenu("Laneclear Settings")
                .AddItem(new MenuItem("laneclearmana", "Mana Percent").SetValue(new Slider(30, 100, 0)));

            //DRAWINGMENU

            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("draw.Q", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.Gray)));
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("draw.W", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.DodgerBlue)));
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("draw.E", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.LightBlue)));
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("draw.RE", "Draw RE Range").SetValue(new Circle(true, System.Drawing.Color.CornflowerBlue)));

            //MISCMENU

            Config.SubMenu("Misc").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("blockAA", "Block AA harass in Laneclear under enemy turret").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("AntiGap", "Anti Gapcloser - E").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupt Spells - E").SetValue(false));
            Config.AddToMainMenu();

            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;

        }
        private static void CastER(Obj_AI_Base target)
        {

            PredictionOutput prediction;

            if (ObjectManager.Player.Distance(target) < E1.Range)
            {
                var oldrange = E1.Range;
                E1.Range = E2.Range;
                prediction = E1.GetPrediction(target, true);
                E1.Range = oldrange;
            }
            else if (ObjectManager.Player.Distance(target) < E2.Range)
            {
                var oldrange = E2.Range;
                E2.Range = E3.Range;
                prediction = E2.GetPrediction(target, true);
                E2.Range = oldrange;
            }
            else if (ObjectManager.Player.Distance(target) < E3.Range)
            {
                prediction = E3.GetPrediction(target, true);
            }
            else
            {
                return;
            }

            if (prediction.Hitchance >= HitChance.High)
            {
                if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) <= E1.Range + E1.Width)
                {
                    Vector3 p;
                    if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) > 300)
                    {
                        p = prediction.CastPosition -
                            100 *
                            (prediction.CastPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized()
                                .To3D();
                    }
                    else
                    {
                        p = prediction.CastPosition;
                    }
                    R.Cast();
                    E1.Cast(p);
                }
                else if (ObjectManager.Player.ServerPosition.Distance(prediction.CastPosition) <=
                         ((E1.Range + E1.Range) / 2))
                {
                    var p = ObjectManager.Player.ServerPosition.To2D()
                        .Extend(prediction.CastPosition.To2D(), E1.Range - 100);
                    {
                        R.Cast();
                        E1.Cast(p.To3D());
                    }
                }
                else
                {
                    var p = ObjectManager.Player.ServerPosition.To2D() +
                            E1.Range *
                            (prediction.CastPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized
                                ();

                    {
                        R.Cast();
                        E1.Cast(p.To3D());
                    }
                }
            }
        }
            protected  void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("blockAA", true).GetValue<bool>() && player.Position.UnderTurret(true) && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)

                return;
                    
                }


        private static void Game_OnGameUpdate(EventArgs args)
            {
            if (player.IsDead)
                return;

            if (player.HealthPercent <= Config.Item("zhonyashp").GetValue<Slider>().Value)
                zhonyas.Cast();

            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var laneclear = (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width);

            var Wfarmpos = W.GetLineFarmLocation(allMinionsW, W.Width);
            var Efarmpos = E.GetCircularFarmLocation(allMinionsE, E.Width);
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Wfarmpos.MinionsHit >= 3 && Config.Item("laneW").GetValue<bool>()
                && player.ManaPercent >= lanemana)
            {
                W.Cast(Wfarmpos.Position);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Efarmpos.MinionsHit >= 3 && allMinionsE.Count >= 1 && Config.Item("laneE").GetValue<bool>()
                && player.ManaPercent >= lanemana)
            {
                E.Cast(Efarmpos.Position);
            }

            var mixed = (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed);
            var htarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && mixed && Config.Item("harassE").GetValue<bool>() && htarget.IsValidTarget(E.Range)
                && player.ManaPercent >= harassmana)
            {
                E.CastIfHitchanceEquals(htarget, HitChance.High, true);
            }
            if (W.IsReady() && mixed && Config.Item("harassW").GetValue<bool>() && htarget.IsValidTarget(W.Range)
                && player.ManaPercent >= harassmana)
            {
                W.CastIfHitchanceEquals(htarget, HitChance.High, true);
            }

            //Combo
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var qtarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
            var wpred = W.GetPrediction(target);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Q.IsReady() && R.IsReady() && Config.Item("UseQR").GetValue<bool>() &&
                    Config.Item("UseQ").GetValue<bool>() && qtarget.IsValidTarget(650) &&
                    player.Position.CountEnemiesInRange(650) >=
                    Config.Item("QRcount").GetValue<Slider>().Value)
                {
                    R.Cast();
                    Q.Cast(player.Position.Extend(target.Position, + 300));
                }
                else
                {
                    if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && qtarget.IsValidTarget(650) &&
                    
                        player.Position.CountEnemiesInRange(650) >= 1)
                    {
                        Q.Cast(player.Position.Extend(target.Position, + 300));
                    }
                }
                if (E3.IsReady() && R.IsReady() && Config.Item("UseER").GetValue<bool>() &&
                    Config.Item("UseR").GetValue<bool>() &&
                    target.Position.CountEnemiesInRange(450 - 250) >=
                    Config.Item("ERcount").GetValue<Slider>().Value)
                {
                    CastER(target);
                }
                else
                {
                    if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                    {
                        E.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                    if (W.IsReady() && Config.Item("UseWR").GetValue<bool>() && Config.Item("UseR").GetValue<bool>() &&
                        R.IsReady() && target.IsValidTarget(W.Range) &&
                        wpred.Hitchance >= HitChance.High && CalcDamage(target) > target.Health)
                    {
                        R.Cast();

                        Utility.DelayAction.Add(1010,
                            () => W.CastIfHitchanceEquals(target, HitChance.High, true));
                    }
                    else
                    {
                        if (W.IsReady() && Config.Item("UseW").GetValue<bool>() && target.IsValidTarget(W.Range))
                        {
                            W.CastIfHitchanceEquals(target, HitChance.High, true);
                        }
                    }
                }
            }
        }



        private static
            void OnDraw(EventArgs args)
        {
            //Draw Skill Cooldown on Champ
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            {

            }
            if (R.IsReady() && Config.Item("Rrdy").GetValue<bool>())
            {
                Drawing.DrawText(pos.X, pos.Y, Color.Gold, "R is Ready!");
            }

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            foreach (var tar in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
            {
            }

            if (Config.Item("draw.Q").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range,
                        Q.IsReady() ? Config.Item("draw.Q").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.W").GetValue<Circle>().Active)
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range,
                        W.IsReady() ? Config.Item("draw.W").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.E").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("draw.E").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.RE").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E3.Range - 2,
                        E3.IsReady() ? Config.Item("draw.R").GetValue<Circle>().Color : System.Drawing.Color.Red);
        }



        private static void OnEndScene(EventArgs args)
        {
            //Damage Indicator
            if (Config.SubMenu("Misc").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.DarkGreen);
                }
            }
        }

        private static int CalcDamage(Obj_AI_Base target)
        {



            //Calculate Combo Damage

            var aa = player.GetAutoAttackDamage(target, true);
            var damage = aa;

            if (Ignite != SpellSlot.Unknown &&
                player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);


            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }
            }

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>()) // rdamage
            {

                damage += E.GetDamage(target);
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<bool>())
            {
                damage += W.GetDamage(target);
            }
            if (W.IsReady() && Config.Item("UseW").GetValue<bool>())
            {
                if (R.IsReady() && Config.Item("UseW").GetValue<bool>() && Config.Item("UseR").GetValue<bool>())
                    damage += W.GetDamage(target) * 2.2;
            }
            return (int)damage;






        }

        private static
            void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && Config.Item("AntiGap").GetValue<bool>())
                E.Cast(gapcloser.End);
        }


        private static
            void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (E.IsReady() && unit.IsValidTarget(E.Range) && Config.Item("Interrupt").GetValue<bool>())
                E.Cast(unit.Position);
        }

    }
}
