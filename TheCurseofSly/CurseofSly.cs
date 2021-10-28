using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using GlobalEnums;
namespace TheCurseofSly
{
    public class CurseofSly:Mod,ITogglableMod,IGlobalSettings<Setting>
    {
        public GameObject SmallGeo => UnityEngine.Object.Instantiate(_smallGeo);
        public override string GetVersion()
        {
            return "2.0(FOR1.5)";
        }
        public override void Initialize()
        {
            ModHooks.NewGameHook += NewStart;
            ModHooks.AfterSavegameLoadHook += Load;
            ModHooks.BeforePlayerDeadHook += Instance_BeforePlayerDeadHook;
            On.GeoControl.OnEnable += GeoControl_OnEnable;
            GetPrefab();
            On.HealthManager.TakeDamage += Count;
        }

        private void Count(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
             bool IsBoss = bossNames.Contains(self.gameObject.name);
            if (IsBoss)
            {
                if(hitInstance.AttackType==AttackTypes.Nail)
                {

                    hitCount++;

                    if (hitCount % maxhit == 0)
                    {
                        SpawnGeo(self.gameObject.GetComponent<Collider2D>());
                        hitCount = 0;
                    }
                }
            }
            orig(self, hitInstance);
        }

        private void GeoControl_OnEnable(On.GeoControl.orig_OnEnable orig, GeoControl self)
        {
            BossSceneController oi = BossSceneController.Instance;
            BossSceneController.Instance = null;
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
            HeroController.instance.TakeGeo(PlayerData.instance.geo);
        }

        private void Load(SaveGameData data)
        {
            NewStart();
        }

        private void NewStart()
        {
            hitCount = 0;
            if(mysetting.IsHard)
            {
                maxhit = 3;
                On.GeoCounter.AddGeo += Death;
                On.GeoControl.OnTriggerEnter2D += Col;
                Log("Hard");
            }
            else
            {
                maxhit = 6;
                On.GeoControl.OnTriggerEnter2D += Col;
                Log("Easy");
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
                PlayerData.instance.geo = 0;
                if(mysetting.immediateltDie)
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 9999, 0);
                }
                else
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 2, 0);
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
            if(geo!=0)
            {
                if (mysetting.immediateltDie)
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 9999, 0);
                }
                else
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 2, 0);
                }
            }
            orig(self, 0);
        }

       public static Setting mysetting { get; set; } = new Setting();
        public void OnLoadGlobal(Setting s) => mysetting = s;
        public Setting OnSaveGlobal() => mysetting;
        public void Unload()
        {
            ModHooks.NewGameHook -= NewStart;
            ModHooks.AfterSavegameLoadHook -= Load;
            ModHooks.BeforePlayerDeadHook -= Instance_BeforePlayerDeadHook;
            On.GeoControl.OnTriggerEnter2D -= Col;
            On.GeoCounter.AddGeo -= Death;
            On.GeoControl.OnEnable -= GeoControl_OnEnable;
            On.HealthManager.TakeDamage -= Count;
        }
        GameObject _smallGeo;
        int hitCount;
        int maxhit;
        private readonly List<String> bossNames = new List<String> { "Dream Mage Lord", "Dung Defender", "Fluke Mother", "Ghost Warrior Galien", "Ghost Warrior Hu", "Ghost Warrior Markoth", "Ghost Warrior Marmu", "Ghost Warrior No Eyes", "Ghost Warrior Slug", "Ghost Warrior Xero", "Giant Buzzer Col", "Giant Fly", "Grey Prince", "Grimm Boss", "Head", "Hive Knight", "Hornet Boss 1", "Hornet Boss 2", "Infected Knight", "Jar Collector", "Jellyfish GG(Clone)", "Lancer", "Lobster", "Lost Kin", "Mage Knight", "Mage Lord", "Mantis Lord", "Mantis Lord S1", "Mantis Lord S2", "Mantis Traitor Lord", "Mawlek Body", "Mega Fat Bee", "Mega Fat Bee (1)", "Mega Zombie Beam Miner (1)", "Mimic Spider", "Nightmare Grimm Boss", "Radiance", "White Defender", "Zombie Beam Miner Rematch", "Hollow Knight Boss", "Mega Jellyfish","HK Prime" ,"Absolute Radiance","Sly Boss","Mato","Oro","Sheo Boss","Hornet Nosk","Hollow Knight Boss"};
    }
}
