//using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace TheisenExampleNS
{

     public abstract class ConfigEntryHelper : ConfigEntryBase
    {
        public abstract void SetDefaults();

        protected static ModalScreen popup;

        public void CloseMenu()
        {
            GameCanvas.instance.CloseModal();
            popup = null;
        }

        public static string RightAlign(string txt)
        {
            return "<align=right>" + txt + "</align>";
        }

        public static string ColorText(string color, string txt)
        {
            return $"<color={color}>" + txt + "</color>";
        }
    }

    internal class ConfigFreeText : ConfigEntryBase
    {
        private string Text;
        public override object BoxedValue { get => new object(); set => _ = value; }

        public ConfigFreeText(string name, ConfigFile config, string text)
        {
            Name = name;
            Config = config;
            ValueType = typeof(object);
            SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm(text);
            if (term == null) term = SokLoc.FallbackSet.GetTerm(text);
            Text = term == null ? text : term.GetText();
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    btn.transform.localScale = Vector3.one;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TextMeshPro.text = Text;
                }
            };
            config.Entries.Add(this);
        }
    }

     public class ConfigEnum<T> : ConfigEntryHelper where T : Enum
    {
        private T content;
        private CustomButton anchor;

        public delegate string OnDisplayText();
        public delegate string OnDisplayEnum(T t);
        public delegate string OnDisplayTooltip(T t);
        public OnDisplayText onDisplayText;
        public OnDisplayEnum onDisplayEnum;
        public OnDisplayTooltip onDisplayTooltip;

        public delegate bool OnChange(T c);
        public OnChange onChange;

        public string popupText;
        public string popupTooltip;

        public T DefaultValue { get { T[] t = (T[])Enum.GetValues(typeof(T)); return t[0]; } }

        public override object BoxedValue { get => content;
            set
            {
                if (value is int)
                {
                    T[] a = (T[])Enum.GetValues(typeof(T));
                    content = a[(int)value];
                }
                else if (value is T)
                {
                    content = (T)value;
                }
            }
        }

        public ConfigEnum(string name, ConfigFile configFile, T defaultValue, ConfigUI ui = null)
        {
            Name = name;
            ValueType = typeof(System.Object);
            Config = configFile;
            if (Config.Data.TryGetValue(name, out _))
            {
                BoxedValue = Config.GetEntry<int>(name);
            }
            else
            {
                BoxedValue = defaultValue;
            }
            UI = new ConfigUI()
            {
                Hidden = true,
                Name = ui?.Name,
                NameTerm = ui?.NameTerm ?? name,
                Tooltip = ui?.Tooltip,
                TooltipTerm = ui?.TooltipTerm,
                PlaceholderText = ui?.PlaceholderText,
                RestartAfterChange = ui?.RestartAfterChange ?? false,
                ExtraData = ui?.ExtraData,
                OnUI = delegate (ConfigEntryBase c)
                {
                    //DifficultyMod.Log($"UI.Hidden = {UI.Hidden}");
                    anchor = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    anchor.transform.localScale = Vector3.one;
                    anchor.transform.localPosition = Vector3.zero;
                    anchor.transform.localRotation = Quaternion.identity;
                    anchor.TextMeshPro.text = onDisplayText != null? onDisplayText() : c.UI.GetName();
                    anchor.TooltipText = c.UI.GetTooltip();
                    anchor.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            configFile.Entries.Add(this);
        }

        private void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(popupText, popupTooltip);
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                T thisEntry = t;
                string text = onDisplayText != null ? onDisplayEnum(thisEntry) : Enum.GetName(typeof(T), thisEntry);
                if (thisEntry.Equals(content)) text = ColorText("blue", text);
                CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab);
                btn.transform.SetParent(ModalScreen.instance.ButtonParent);
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localScale = Vector3.one;
                btn.transform.localRotation = Quaternion.identity;
                btn.TextMeshPro.text = text;
                btn.TooltipText = onDisplayTooltip != null ? onDisplayTooltip(thisEntry) : null;
                btn.Clicked += delegate ()
                {
                    if (onChange != null && onChange(thisEntry) || onChange == null)
                    {
                        Config.Data[Name] = (int)(object)thisEntry;
                        content = thisEntry;
                        anchor.TextMeshPro.text = onDisplayText != null ? onDisplayText() : UI.GetName();
                        CloseMenu();
                    }
                };
            }
            GameCanvas.instance.OpenModal();
        }

        public override void SetDefaults()
        {
            BoxedValue = DefaultValue;
            if (popup != null)
            {

            }
        }
    }

    // [HarmonyPatch]
    // internal static class DrawingPatch
    // {

    //     private static Vector3 _startView;
    //     private static GameObject _object;
    //     private static Image _image;
    //     private static RectTransform _rect;


    //     [HarmonyPatch(typeof(GameScreen), "Update")]
    //     [HarmonyPostfix]
    //     public static void DrawCustomGui(GameScreen __instance)
    //     {
    //         if (WeaponStandInfo.DrawRectangle) {

    //             _object = new GameObject();
    //             _object.transform.SetParent(__instance.transform, false);

    //             _startView = WeaponStandInfo.StartingPoint;
    //             _object.transform.position = _startView;

    //             _image = _object.AddComponent<Image>();
    //             _rect = _image.rectTransform;
    //             _image.color = new Color(1f, 1f, 1f, .6f);
    //             _rect.pivot = Vector2.zero;
    //             _rect.position = new Vector3(_startView.x * 16f,_startView.y,_startView.z * 16f);
    //             _rect.sizeDelta = new Vector2(30, 30);

    //         }else{
    //             UnityEngine.Object.Destroy(_object.gameObject);
    //         }
    //     }

    // }

    public class WeaponStandInfo {
        private static bool drawRectangle = false;
        public static bool DrawRectangle {
            get {return drawRectangle;}
            set {drawRectangle = value;}
        }

        private static Vector3 startingPoint = Vector3.one;
        public static Vector3 StartingPoint {
            get {return startingPoint;}
            set {startingPoint = value;}
        }
    }

    // create a class called WeaponStand which extends the card class
    public class WeaponStand : CardData
    {
        // [Header("Food")]
	    // [ExtraData("food_value")]
	    public int WeaponCount = 0;
        public int MaxWeaponCount = 30;
        public string StandTerm = "TheisenExample_weapon_stand_description";
        [ExtraData("health")]
        public int HealthPoints = 3;

        public float ViewSize = 2.0f;

        public bool DrawRectangle = false;

        private bool CheckInit;

        private Vector3 currentPosition;
        private Vector2 currentSize;
        private float viewTime;
        //private Harmony? harmony;
        public static ModLogger? _Logger;
        public RectTransform Modal;
        public bool ModalIsOpen;

        // private void Awake()
        // {
        //     harmony = new Harmony("TheisenExampleNS.WeaponStand");
        //     harmony.PatchAll();
        // }

        // private void OnDestroy()
        // {
        //     harmony.UnpatchSelf();
        // }

    
        // public void OpenModal()
        // {
        //     Modal.gameObject.SetActive(value: true);
        //     ModalIsOpen = true;
        // }

        // public void CloseModal()
        // {
        //     Modal.gameObject.SetActive(value: false);
        //     ModalIsOpen = false;
        // }

        // public void TestModal(CardData cb, Action onDone)
        // {
        //     ModalScreen.instance.Clear();
        //     ModalScreen.instance.SetTexts(SokLoc.Translate("label_name_villager_title"), SokLoc.Translate("label_name_villager_text"));
        //     TMP_InputField input = ModalScreen.instance.AddInputNoButton();
        //     if (!string.IsNullOrEmpty(cb.CustomName))
        //     {
        //         input.text = cb.CustomName;
        //     }
        //     else
        //     {
        //         input.text = cb.Name;
        //     }
        //     input.characterLimit = 12;
        //     ModalScreen.instance.AddOption(SokLoc.Translate("label_random_name"), delegate
        //     {
        //         input.text = GetRandomName();
        //     });
        //     ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
        //     {
        //         ProfanityChecker profanityChecker = WorldManager.instance.GameDataLoader.ProfanityChecker;
        //         string text = input.text;
        //         if (profanityChecker.IsProfanityInLanguage(SokLoc.instance.CurrentLanguage, text))
        //         {
        //             text = "Bobba";
        //         }
        //         cb.CustomName = text;
        //         CloseModal();
        //         onDone();
        //     });
        //     OpenModal();
        // }

        // public string GetRandomName()
        // {
        //     return WorldManager.instance.GameDataLoader.VillagerNames.Choose();
        // }







        // public override void UpdateCard()
        // {
        //     // Update view time.
        //     viewTime += Time.deltaTime;

        //     MyGameCard.SpecialIcon.sprite = SpriteManager.instance.HealthIcon;
        //     // MyGameCard.SpecialValue = WeaponCount;
        //     // MyGameCard.SpecialIcon.sprite = SpriteManager.instance.TorsoIconFilled;
        //     descriptionOverride = SokLoc.Translate(StandTerm);
		//     descriptionOverride += "\n\n";
        //     //descriptionOverride = SokLoc.Translate(StandTerm, LocParam.Create("count", WeaponCount.ToString()));
        //     descriptionOverride = descriptionOverride + "<i>" + WeaponCount.ToString() + "</i>";
        //     // descriptionOverride = SokLoc.Translate(
        //     //     StandTerm, LocParam.Create("count", WeaponCount.ToString()), 
        //     //     LocParam.Create("max_count", MaxWeaponCount.ToString()), 
        //     //     LocParam.Create("goldicon", Icons.Gold), 
        //     //     LocParam.Create("shellicon", Icons.Shell));


        //     if (WeaponStandInfo.DrawRectangle){
        //         Vector3 position = MyGameCard.transform.position;
        //         Vector3 start_position = new Vector3(position.x, (0f - position.z) * 0.001f, position.z);

        //         Bounds bounds = new Bounds(ClampStartPosition(start_position), ConflictZone());
        //         _ = Extensions.Perlin(viewTime * 10f) * 0.01f;
        //         Vector2 vector = new Vector2(bounds.size.x, bounds.size.z);
        //         Vector3 center = bounds.center;
        //         if (!CheckInit)
        //         {
        //             CheckInit = true;
        //             currentPosition = center;
        //         }
        //         currentPosition = Vector3.Lerp(currentPosition, center, Time.deltaTime * 16f);
        //         currentSize = Vector3.Lerp(currentSize, vector, 1f); // Time.deltaTime * 16f

        //         descriptionOverride += "\n";
        //         descriptionOverride = descriptionOverride + "<i>" + center.ToString() + "</i>";
        //         //ConflictRectangle
        //         // DrawManager.instance.DrawShape(new ConflictRectangle
        //         // {
        //         //     Size = currentSize,
        //         //     Center = currentPosition
        //         // });

        //         // set up static parameters. these are used for all following Draw.Line calls

        //         // Gizmos.color = Color.blue;
        //         // Gizmos.DrawWireSphere(currentPosition, 1f);
        //         // Gizmos.DrawWireSphere(currentPosition, 2f);
        //         // Gizmos.DrawWireSphere(currentPosition, 4f);
        //         // Gizmos.DrawWireSphere(currentPosition, 8f);
        //         // Gizmos.DrawWireSphere(currentPosition, 16f);
        //         // Debug.Log("OnCardUpdate");
        //         // Debug.Log(currentPosition);


        //     }

        //     base.UpdateCard();
        // }

        // private void OnDrawGizmos()
	    // {
        //     Debug.Log("Draw Gizmo");
        //     if (WeaponStandInfo.DrawRectangle){
        //         Vector3 position = MyGameCard.transform.position;
        //         Vector3 start_position = new Vector3(position.x, (0f - position.z) * 0.001f, position.z);
        //         Bounds bounds = new Bounds(ClampStartPosition(start_position), ConflictZone());

        //         Gizmos.color = Color.green;
        //         Gizmos.DrawWireSphere(bounds.center, 1f);
        //     }
        // }

    public bool NothingSelected
        {
            get
            {
                if (!(EventSystem.current.currentSelectedGameObject == null))
                {
                    return !EventSystem.current.currentSelectedGameObject.activeInHierarchy;
                }
                return true;
            }
        }

	public GameObject SelectedObject => EventSystem.current.currentSelectedGameObject;

        private Vector3 ConflictZone(){
            float height = MyGameCard.GetHeight();
            float width = MyGameCard.GetWidth();

            return new Vector3(z: height, 
                x: width * 4 + WorldManager.instance.ConflictWidthIncrease, 
                y: 0.03f);
        }

        private Vector3 ClampStartPosition(Vector3 p)
        {
            Vector3 conflictSize = ConflictZone();

            float num = conflictSize.x * 0.5f;
            float num2 = conflictSize.z * 0.5f;
            Bounds tightWorldBounds = MyGameCard.MyBoard.TightWorldBounds;
            float num3 = 0.1f;
            p.x = Mathf.Clamp(p.x, tightWorldBounds.min.x + num + num3, tightWorldBounds.max.x - num - num3);
            p.z = Mathf.Clamp(p.z, tightWorldBounds.min.z + num2 * 0.5f + num3 + WorldManager.instance.CombatOffset, tightWorldBounds.max.z + num2 * 0.5f - num3);
            return p;
        }

    	// protected override bool CanHaveCard(CardData otherCard)
        // {
        //     return otherCard.MyCardType == CardType.Equipable;
        // }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (!(otherCard is BaseVillager) && !(otherCard is Animal))
            {
                return otherCard is Kid;
            }
            return true;
        }

        // public override void UpdateCard()
        // {
        //     CardData card = null;
        //     CardData card2 = null;
        //     if ((HasCardOnTop(out card) || IsOnCard<CardData>(out card2)) && !ModalIsOpen)
        //     {
        //         CardData bs = ((card != null) ? card : card2);
        //         if (CanHaveCard(bs))
        //         {
        //             TestModal(bs, delegate
        //             {
        //                 if (bs is BaseVillager)
        //                 {
        //                     QuestManager.instance.SpecialActionComplete("name_villager");
        //                 }
        //                 bs.MyGameCard.RemoveFromStack();
        //                 bs.MyGameCard.SendIt();
        //             });
        //         }
        //         else
        //         {
        //             bs.MyGameCard.RemoveFromStack();
        //         }
        //     }
        //     base.UpdateCard();
        // }
    

        public override void Clicked()
        {

            Vector3 position = MyGameCard.transform.position;
            Vector3 start_position = new Vector3(position.x, (0f + position.z) * 0.001f, position.z);
            Bounds bounds = new Bounds(ClampStartPosition(start_position), ConflictZone());
            WeaponStandInfo.StartingPoint = bounds.center;
            WeaponStandInfo.DrawRectangle = !WeaponStandInfo.DrawRectangle;

            //DrawRectangle = !DrawRectangle;
            Value += 1;
            WeaponCount += 3;

            // ShowWeaponStandModal(delegate
            // {
            //     Debug.Log("Clicked.");
            // });

            base.Clicked();
        }
    }
}