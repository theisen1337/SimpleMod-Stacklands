


using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace TheisenExampleNS
{
     public class TestCard : CardData
    {
        public RectTransform Modal;
        public bool ModalIsOpen;

         public void OpenModal()
        {
            Modal.gameObject.SetActive(value: true);
            ModalIsOpen = true;
        }

        public void CloseModal()
        {
            Modal.gameObject.SetActive(value: false);
            ModalIsOpen = false;
        }

        public void TestModal(CardData cb, Action onDone)
        {
            ModalScreen.instance.Clear();
            ModalScreen.instance.SetTexts(SokLoc.Translate("label_name_villager_title"), SokLoc.Translate("label_name_villager_text"));
            TMP_InputField input = ModalScreen.instance.AddInputNoButton();
            if (!string.IsNullOrEmpty(cb.CustomName))
            {
                input.text = cb.CustomName;
            }
            else
            {
                input.text = cb.Name;
            }
            input.characterLimit = 12;
            ModalScreen.instance.AddOption(SokLoc.Translate("label_random_name"), delegate
            {
                input.text = GetRandomName();
            });
            ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
            {
                ProfanityChecker profanityChecker = WorldManager.instance.GameDataLoader.ProfanityChecker;
                string text = input.text;
                if (profanityChecker.IsProfanityInLanguage(SokLoc.instance.CurrentLanguage, text))
                {
                    text = "Bobba";
                }
                cb.CustomName = text;
                CloseModal();
                onDone();
            });
            OpenModal();
        }

        public string GetRandomName()
        {
            return WorldManager.instance.GameDataLoader.VillagerNames.Choose();
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (!(otherCard is BaseVillager) && !(otherCard is Animal))
            {
                return otherCard is Kid;
            }
            return true;
        }

        public override void UpdateCard()
        {
            CardData card = null;
            CardData card2 = null;
            if ((HasCardOnTop(out card) || IsOnCard<CardData>(out card2)) && !ModalIsOpen)
            {
                CardData bs = ((card != null) ? card : card2);
                if (CanHaveCard(bs))
                {
                    TestModal(bs, delegate
                    {
                        if (bs is BaseVillager)
                        {
                            QuestManager.instance.SpecialActionComplete("name_villager");
                        }
                        bs.MyGameCard.RemoveFromStack();
                        bs.MyGameCard.SendIt();
                    });
                }
                else
                {
                    bs.MyGameCard.RemoveFromStack();
                }
            }
            base.UpdateCard();
        }

    }
}