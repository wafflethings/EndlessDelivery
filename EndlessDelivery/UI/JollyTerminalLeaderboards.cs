using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EndlessDelivery.Scores;
using EndlessDelivery.Scores.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalLeaderboards : MonoBehaviour
{
    public Button[] PageButtons;
    public LeaderboardEntry[] Entries;
    public TMP_Text PageText;
    private List<ScoreResult> _pageScores;
    private bool _hasLoadedScores;
    private int _page = 0;
    private int _pageAmount;
        
    public void Start()
    {
            SetStuff();
        }

    private async void SetStuff()
    {
            _pageAmount = Mathf.CeilToInt(await Endpoints.GetScoreAmount() / 5f);
        }
        
    public void OnEnable()
    {
            if (!_hasLoadedScores)
            {
                foreach (LeaderboardEntry entry in Entries)
                {
                    entry.gameObject.SetActive(false);
                }
                
                RefreshPage();
            }
        }

    public void ScrollPage(int amount)
    {
            if ((_page + amount) >= 0 && (_page + amount) <= _pageAmount - 1)
            {
                _page += amount;
            }

            PageText.text = (_page + 1).ToString();

            RefreshPage();
        }

    public void Refresh()
    {
            //unity events cant call asnyc funcs
            RefreshAsync();
        }

    private async void RefreshAsync()
    {
            transform.parent.gameObject.SetActive(false);
            await Score.GetServerScoreAndSetIfHigher();
            GetComponentInParent<JollyTerminal>().AssignScoreText();
            transform.parent.gameObject.SetActive(true);
        }
        
    public async Task RefreshPage()
    {
            if (!await Endpoints.IsServerOnline())
            {
                HudMessageReceiver.Instance.SendHudMessage("Server offline!");
                gameObject.SetActive(false);
                return;
            }
            
            foreach (Button button in PageButtons)
            {
                button.interactable = false;
            }

            try
            {
                _pageScores = await Endpoints.GetScoreRange((_page * 5), 5);

                for (int i = 0; i < 5; i++)
                {
                    if (i < _pageScores.Count)
                    {
                        Entries[i].SetValuesAndEnable(_pageScores[i]);
                    }
                    else
                    {
                        Entries[i].gameObject.SetActive(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Endpoints.DisplayError("Failed to load scores!!" + ex);
            }
            
            foreach (Button button in PageButtons)
            {
                button.interactable = true;
            }
        }
}