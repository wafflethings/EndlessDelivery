using System;
using System.Collections.Generic;
using System.Linq;
using EndlessDelivery.Components;
using EndlessDelivery.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EndlessDelivery.Gameplay
{
    public class Room : MonoBehaviour
    {
        public List<Chimney> Chimneys = new();
        [Space(15)]
        public List<Present> Presents = new();
        public List<int> PresentColourAmounts = new();

        public Dictionary<WeaponVariant, Chimney> AllChimneys = new()
        {
            { WeaponVariant.BlueVariant, null },
            { WeaponVariant.GreenVariant, null },
            { WeaponVariant.RedVariant, null },
            { WeaponVariant.GoldVariant, null }
        };
        
        public Dictionary<WeaponVariant, int> AmountDelivered = new()
        {
            { WeaponVariant.BlueVariant, 0 },
            { WeaponVariant.GreenVariant, 0 },
            { WeaponVariant.RedVariant, 0 },
            { WeaponVariant.GoldVariant, 0 }
        };

        public bool Done(WeaponVariant colour)
        {
            return AmountDelivered[colour] == PresentColourAmounts[(int)colour];
        }

        public bool ChimneysDone => Chimneys.All(c => Done(c.VariantColour));

        public void Start()
        {
            DecideChimneyColours();
            DecidePresentColours();
            
            for (int i = 0; i < PresentColourAmounts.Count; i++)
            {
                Chimney chimney = AllChimneys[(WeaponVariant)i];

                if (chimney != null)
                {
                    chimney.AmountToDeliver = PresentColourAmounts[i];
                }
            }
        }

        public void DecideChimneyColours()
        {
            WeaponVariant colour = 0;
            
            foreach (Chimney chimney in Chimneys.Shuffle())
            {
                AllChimneys[colour] = chimney;
                chimney.SetColour(colour);
                chimney.Room = this;
                colour++;
            }
        }
        
        public void DecidePresentColours()
        {
            Dictionary<WeaponVariant, int> amounts = new();
            for (int i = 0; i < PresentColourAmounts.Count; i++)
            {
                if (PresentColourAmounts[i] != 0)
                {
                    amounts.Add((WeaponVariant)i, PresentColourAmounts[i]);
                }
            }

            foreach (Present present in Presents)
            {
                if (amounts.Count != 0)
                {
                    KeyValuePair<WeaponVariant, int> pair = amounts.ToList().Pick();
                    present.SetColour(pair.Key);
                    amounts[pair.Key]--;

                    if (amounts[pair.Key] == 0)
                    {
                        amounts.Remove(pair.Key);
                    } 
                }
                else
                {
                    present.gameObject.SetActive(false);
                }
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.GetComponent<NewMovement>() != null)
            {
                GameManager.Instance.SetRoom(this);
            }
        }
    }
}