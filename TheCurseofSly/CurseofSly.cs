using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using GlobalEnums;
namespace TheCurseofSly
{
    public class CurseofSly:Mod,IGlobalSettings<Setting>,IMenuMod
    {
        public GameObject SmallGeo => UnityEngine.Object.Instantiate(_smallGeo);
        public override string GetVersion()
        {
            return "2.1(FOR1.5)";
        }
        public override void Initialize()
        {
            ModHooks.NewGameHook += NewStart;
            ModHooks.AfterSavegameLoadHook += Load;
            ModHooks.BeforePlayerDeadHook += Instance_BeforePlayerDeadHook;
            On.GeoControl.OnEnable += GeoControl_OnEnable;
            On.GeoCounter.AddGeo += Death;
            On.GeoControl.OnTriggerEnter2D += Col;
            GetPrefab();
            On.HealthManager.TakeDamage += Count;
        }
        public bool ToggleButtonInsideMenu => false;
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? wrappedToggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menuEntries = new List<IMenuMod.MenuEntry>();
            menuEntries.Add(
                new IMenuMod.MenuEntry
                {
                    Name="Mode",
                    Description="Choose the mode(easy/hard)",
                    Values=Enum.GetNames(typeof(Setting.Mode)).ToArray(),
                    Saver=i=>GS.mode=(Setting.Mode)i,
                    Loader=()=>(int)GS.mode
                }
                );
            menuEntries.Add(
               new IMenuMod.MenuEntry
               {
                   Name = "Punishment",
                   Description = "Choose the punishment when you get geo",
                   Values = Enum.GetNames(typeof(Setting.PunishmentType)).ToArray(),
                   Saver = i => GS.punishment = (Setting.PunishmentType)i,
                   Loader = () => (int)GS.punishment
               }
               );
            return menuEntries;
        }

        private void Count(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
             bool IsBoss = bossNames.Contains(self.gameObject.name);
            if(GS.mode!=Setting.Mode.None)
            {
                if (IsBoss)
                {
                    if (hitInstance.AttackType == AttackTypes.Nail)
                    {

                        hitCount++;

                        if (hitCount % maxhit == 0)
                        {
                            SpawnGeo(self.gameObject.GetComponent<Collider2D>());
                            hitCount = 0;
                        }
                    }
                }
            }
            orig(self, hitInstance);
        }

        private void GeoControl_OnEnable(On.GeoControl.orig_OnEnable orig, GeoControl self)
        {
            BossSceneController oi = BossSceneController.Instance;
            if(GS.mode!=Setting.Mode.None)
            {
                BossSceneController.Instance = null;
            }
            try
            {
                orig(self);
            }
            finally
            {
                BossSceneController.Instance = oi;
            }
        }


        public void GetPrefab()
        {
            // this is very bad but it will work
            Resources.LoadAll<GameObject>("");
            foreach (GameObject i in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (i.name == "Geo Small")
                {
                    _smallGeo = i;
                }
            }
        }
        private void Instance_BeforePlayerDeadHook()
        {
           if(GS.mode!=Setting.Mode.None)
            {
                HeroController.instance.TakeGeo(PlayerData.instance.geo);
            }
        }

        private void Load(SaveGameData data)
        {
            NewStart();
        }

        private void NewStart()
        {
           if(GS.mode!=Setting.Mode.None)
            {
                hitCount = 0;
                if (GS.mode == Setting.Mode.Hard)
                {
                    maxhit = 3;
                    Log("Hard");
                }
                else
                {
                    if (GS.mode == Setting.Mode.Easy)
                    {
                        maxhit = 6;
                        Log("Easy");
                    }
                }
            }
        }
        private void SpawnGeo(Collider2D collider2D)
        {
            GameObject smallPrefab = _smallGeo;
            UnityEngine.Object.Destroy(smallPrefab.Spawn());
            smallPrefab.SetActive(true);
            FlingUtils.Config config = new FlingUtils.Config
            {
                Prefab = smallPrefab,
                AmountMax=1,
                AmountMin=1,
                AngleMax=80f,
                AngleMin=115f,
                SpeedMax=45f,
                SpeedMin=15f
            };
            FlingUtils.SpawnAndFling(config, collider2D.gameObject.transform, new Vector3(0f, 0f, 0f));
            smallPrefab.SetActive(false);
        }
        private void Col(On.GeoControl.orig_OnTriggerEnter2D orig, GeoControl self, UnityEngine.Collider2D collision)
        {
            if (collision.gameObject.name == "HeroBox")//Knight contact with geo
            {
                if(GS.mode!=Setting.Mode.None)
                {
                    PlayerData.instance.geo = 0;
                }
                if(GS.punishment==Setting.PunishmentType.Die)
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 9999, 0);
                }
                else
                {
                    if(GS.punishment==Setting.PunishmentType.Damage)
                    {
                        HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 2, 0);
                    }
                }
                
            }
           /* if(collision.gameObject.name=="Slash"&&!HeroController.instance.cState.onGround)
            {
                Log("Recoil");
                Vector2 speed = new Vector2(HeroController.instance.GetComponent<Rigidbody2D>().velocity.x,15.7f);
                HeroController.instance.GetComponent<Rigidbody2D>().velocity = speed;
            }*/
            orig(self, collision);
        }

        private void Death(On.GeoCounter.orig_AddGeo orig, GeoCounter self, int geo)
        {
            
            if(GS.mode==Setting.Mode.Hard)
            {
                PlayerData.instance.geo = 0;
                if (geo > 50)
                {
                    if (GS.punishment == Setting.PunishmentType.Die)
                    {
                        HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 9999, 0);
                    }
                    if(GS.punishment == Setting.PunishmentType.Damage)
                    {
                        HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 2, 0);
                    }

                }
            }
            if (GS.punishment != Setting.PunishmentType.Die)
            {
                orig(self, geo);
            }
            else
            {
                orig(self, 0);
            }


        }

       public static Setting GS { get; set; } = new Setting();
        public void OnLoadGlobal(Setting s) => GS = s;
        public Setting OnSaveGlobal() => GS;
        GameObject _smallGeo;
        int hitCount;
        int maxhit;
        private readonly List<String> bossNames = new List<String> { "Dream Mage Lord", "Dung Defender", "Fluke Mother", "Ghost Warrior Galien", "Ghost Warrior Hu", "Ghost Warrior Markoth", "Ghost Warrior Marmu", "Ghost Warrior No Eyes", "Ghost Warrior Slug", "Ghost Warrior Xero", "Giant Buzzer Col", "Giant Fly", "Grey Prince", "Grimm Boss", "Head", "Hive Knight", "Hornet Boss 1", "Hornet Boss 2", "Infected Knight", "Jar Collector", "Jellyfish GG(Clone)", "Lancer", "Lobster", "Lost Kin", "Mage Knight", "Mage Lord", "Mantis Lord", "Mantis Lord S1", "Mantis Lord S2", "Mantis Traitor Lord", "Mawlek Body", "Mega Fat Bee", "Mega Fat Bee (1)", "Mega Zombie Beam Miner (1)", "Mimic Spider", "Nightmare Grimm Boss", "Radiance", "White Defender", "Zombie Beam Miner Rematch", "Hollow Knight Boss", "Mega Jellyfish","HK Prime" ,"Absolute Radiance","Sly Boss","Mato","Oro","Sheo Boss","Hornet Nosk","Hollow Knight Boss"};
    }
}
