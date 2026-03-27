using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;

    public Text pointsMessage;
    public Text creditMessage;
    public Dropdown betDropdown;

    public int[] values = new int[52];
    int cardIndex = 0;

    private int credit = 1000;
    private int currentBet = 0;

    private void Awake()
    {
        InitCardValues();
    }

    private void Start()
    {
        InitBetDropdown();
        UpdateCreditUI();

        if (pointsMessage != null)
            pointsMessage.text = "Puntos: 0";
        if (probMessage != null)
            probMessage.text = "";
        if (finalMessage != null)
            finalMessage.text = "Elige tu apuesta y pulsa Play Again";

        hitButton.interactable = false;
        stickButton.interactable = false;

        playAgainButton.onClick.RemoveAllListeners();
        playAgainButton.onClick.AddListener(Deal);
    }

    // ========== SISTEMA DE BANCA ==========

    private void InitBetDropdown()
    {
        if (betDropdown == null) return;

        betDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        for (int i = 10; i <= credit; i += 10)
        {
            options.Add(i + " Créditos");
        }
        if (options.Count == 0) options.Add("0 Créditos");
        betDropdown.AddOptions(options);
    }

    private int GetSelectedBet()
    {
        if (betDropdown == null) return 0;
        return (betDropdown.value + 1) * 10;
    }

    private void UpdateCreditUI()
    {
        if (creditMessage != null)
            creditMessage.text = "Credito: " + credit.ToString();
    }

    private void UpdatePointsUI()
    {
        if (pointsMessage != null)
            pointsMessage.text = "Puntos: " + player.GetComponent<CardHand>().points.ToString();
    }

    private void ApplyBetResult(bool playerWins, bool isDraw)
    {
        if (isDraw)
        {
            credit += currentBet;
        }
        else if (playerWins)
        {
            credit += currentBet * 2;
        }

        UpdateCreditUI();

        if (creditMessage != null)
            creditMessage.text = credit + " (Apuesta: " + currentBet + ")";
    }

    // ========== BARAJA ==========

    private void InitCardValues()
    {
        for (int i = 0; i < 52; i++)
        {
            int cardRank = i % 13;

            if (cardRank == 0)
                values[i] = 11;
            else if (cardRank >= 10)
                values[i] = 10;
            else
                values[i] = cardRank + 1;
        }
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < faces.Length; i++)
        {
            int randomIndex = Random.Range(0, faces.Length);

            Sprite tempFace = faces[i];
            int tempValue = values[i];

            faces[i] = faces[randomIndex];
            values[i] = values[randomIndex];

            faces[randomIndex] = tempFace;
            values[randomIndex] = tempValue;
        }
    }

    // ========== DINÁMICA DEL JUEGO ==========

    void StartGame()
    {
        currentBet = GetSelectedBet();
        if (currentBet > credit) currentBet = credit;
        credit -= currentBet;
        UpdateCreditUI();

        if (betDropdown != null)
            betDropdown.interactable = false;

        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(false);
        UpdatePointsUI();

        bool playerBJ = player.GetComponent<CardHand>().points == 21;
        bool dealerBJ = dealer.GetComponent<CardHand>().points == 21;

        if (playerBJ || dealerBJ)
        {
            dealer.GetComponent<CardHand>().InitialToggle();

            if (playerBJ && dealerBJ)
            {
                finalMessage.text = "EMPATE - Ambos tienen Blackjack!";
                ApplyBetResult(false, true);
            }
            else if (playerBJ)
            {
                finalMessage.text = "BLACKJACK! Ganas!";
                ApplyBetResult(true, false);
            }
            else
            {
                finalMessage.text = "El dealer tiene Blackjack! Pierdes!";
                ApplyBetResult(false, false);
            }

            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    // ========== PROBABILIDADES ==========

    private void CalculateProbabilities()
    {
        int remaining = faces.Length - cardIndex;

        if (remaining <= 0)
        {
            probMessage.text = "No quedan cartas en la baraja.";
            return;
        }

        int playerPoints = player.GetComponent<CardHand>().points;

        int dealerBeats = 0;
        int safePicks = 0;
        int bustPicks = 0;

        for (int i = cardIndex; i < faces.Length; i++)
        {
            int simulatedCardValue = values[i];

            int dTotal = CalculateDealerSimulatedPoints(simulatedCardValue);
            if (dTotal > playerPoints && dTotal <= 21)
                dealerBeats++;

            int pTotal = CalculatePlayerSimulatedPoints(simulatedCardValue);
            if (pTotal >= 17 && pTotal <= 21) safePicks++;
            if (pTotal > 21) bustPicks++;
        }

        float probA = (float)dealerBeats / remaining * 100f;
        float probB = (float)safePicks / remaining * 100f;
        float probC = (float)bustPicks / remaining * 100f;

        probMessage.text =
    "Deal > Play:   " + probA.ToString("F4") + "\n" +
    "17<=X<=21:   " + probB.ToString("F4") + "\n" +
    "X > 21:   " + probC.ToString("F4");

    }

    private int CalculatePlayerSimulatedPoints(int newCardValue)
    {
        int val = 0;
        int aces = 0;
        CardHand hand = player.GetComponent<CardHand>();

        foreach (GameObject g in hand.cards)
        {
            int cVal = g.GetComponent<CardModel>().value;
            if (cVal == 11) aces++;
            else val += cVal;
        }

        if (newCardValue == 11) aces++;
        else val += newCardValue;

        for (int i = 0; i < aces; i++)
        {
            if (val + 11 <= 21) val += 11;
            else val += 1;
        }
        return val;
    }

    private int CalculateDealerSimulatedPoints(int hiddenCardValue)
    {
        int val = 0;
        int aces = 0;
        CardHand hand = dealer.GetComponent<CardHand>();

        for (int i = 1; i < hand.cards.Count; i++)
        {
            int cVal = hand.cards[i].GetComponent<CardModel>().value;
            if (cVal == 11) aces++;
            else val += cVal;
        }

        if (hiddenCardValue == 11) aces++;
        else val += hiddenCardValue;

        for (int i = 0; i < aces; i++)
        {
            if (val + 11 <= 21) val += 11;
            else val += 1;
        }
        return val;
    }

    // ========== REPARTO ==========

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        UpdatePointsUI();
        CalculateProbabilities();
    }

    // ========== BOTONES ==========

    public void Hit()
    {
        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
            finalMessage.text = "Te has pasado de 21! Pierdes!";
            ApplyBetResult(false, false);
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        if (dealer.GetComponent<CardHand>().cards.Count == 2)
            dealer.GetComponent<CardHand>().InitialToggle();

        while (dealer.GetComponent<CardHand>().points <= 16)
            PushDealer();

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        if (dealerPoints > 21)
        {
            finalMessage.text = "El dealer se pasa! Ganas!";
            ApplyBetResult(true, false);
        }
        else if (playerPoints > dealerPoints)
        {
            finalMessage.text = "Ganas! " + playerPoints + " vs " + dealerPoints;
            ApplyBetResult(true, false);
        }
        else if (dealerPoints > playerPoints)
        {
            finalMessage.text = "Pierdes! " + dealerPoints + " vs " + playerPoints;
            ApplyBetResult(false, false);
        }
        else
        {
            finalMessage.text = "Empate! Ambos con " + playerPoints + " puntos";
            ApplyBetResult(false, true);
        }

        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    public void PlayAgain()
    {
        if (credit <= 0)
        {
            finalMessage.text = "No tienes crédito! Game Over.";
            return;
        }

        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        finalMessage.text = "Elige tu apuesta y pulsa Play Again";
        probMessage.text = "";

        if (pointsMessage != null)
            pointsMessage.text = "Puntos: 0" ;

        InitBetDropdown();
        if (betDropdown != null)
            betDropdown.interactable = true;

        hitButton.interactable = false;
        stickButton.interactable = false;

        playAgainButton.onClick.RemoveAllListeners();
        playAgainButton.onClick.AddListener(Deal);
    }

    public void Deal()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";

        ShuffleCards();
        StartGame();

        playAgainButton.onClick.RemoveAllListeners();
        playAgainButton.onClick.AddListener(PlayAgain);
    }
}
