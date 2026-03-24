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

    public int[] values = new int[52];
    int cardIndex = 0;

    private void Awake()
    {
        InitCardValues();
    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }

    private void InitCardValues()
    {
        // Recorremos las 52 cartas divididas en 4 palos de 13
        // cardRank 0 = As, 1-9 = cartas 2-10, 10-12 = J, Q, K
        for (int i = 0; i < 52; i++)
        {
            int cardRank = i % 13;

            if (cardRank == 0)
                values[i] = 11; // As vale 11 por defecto (CardHand lo reduce a 1 si hace falta)
            else if (cardRank >= 10)
                values[i] = 10; // J, Q, K valen 10
            else
                values[i] = cardRank + 1; // cartas 2-10
        }
    }

    private void ShuffleCards()
    {
        // Recorremos el array y para cada posicion hacemos un intercambio aleatorio
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

    void StartGame()
    {
        // Repartimos 2 cartas a cada uno alternando jugador-dealer
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        // Punto 2 del PDF: si alguno tiene Blackjack en las 2 primeras cartas, gana
        bool playerBJ = player.GetComponent<CardHand>().points == 21;
        bool dealerBJ = dealer.GetComponent<CardHand>().points == 21;

        if (playerBJ || dealerBJ)
        {
            // Revelamos la carta oculta del dealer
            dealer.GetComponent<CardHand>().InitialToggle();

            if (playerBJ && dealerBJ)
                finalMessage.text = "EMPATE - Ambos tienen Blackjack!";
            else if (playerBJ)
                finalMessage.text = "BLACKJACK! Ganas!";
            else
                finalMessage.text = "El dealer tiene Blackjack! Pierdes!";

            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    private void CalculateProbabilities()
    {
        // Cartas que quedan por repartir
        int remaining = faces.Length - cardIndex;

        if (remaining <= 0)
        {
            probMessage.text = "No quedan cartas en la baraja.";
            return;
        }

        int playerPoints = player.GetComponent<CardHand>().points;

        // Puntos visibles del dealer: saltamos la carta 0 que esta oculta
        // Solo usamos cartas desde el indice 1 en adelante
        CardHand dealerHand = dealer.GetComponent<CardHand>();
        int dealerVisiblePoints = 0;
        for (int i = 1; i < dealerHand.cards.Count; i++)
            dealerVisiblePoints += dealerHand.cards[i].GetComponent<CardModel>().value;

        // PROB A: probabilidad de que el dealer supere al jugador con la carta oculta
        // Probamos cada carta restante como posible carta oculta del dealer
        int dealerBeats = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int hiddenVal = values[i];
            int dealerTotal = dealerVisiblePoints + hiddenVal;
            if (dealerTotal > 21 && hiddenVal == 11) dealerTotal -= 10; // ajuste As
            if (dealerTotal > playerPoints && dealerTotal <= 21) dealerBeats++;
        }
        float probA = (float)dealerBeats / remaining * 100f;

        // PROB B: probabilidad de que el jugador obtenga entre 17 y 21 pidiendo carta
        int safePicks = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int newTotal = playerPoints + values[i];
            if (newTotal > 21 && values[i] == 11) newTotal -= 10; // ajuste As
            if (newTotal >= 17 && newTotal <= 21) safePicks++;
        }
        float probB = (float)safePicks / remaining * 100f;

        // PROB C: probabilidad de que el jugador se pase de 21 pidiendo carta
        int bustPicks = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int newTotal = playerPoints + values[i];
            if (newTotal > 21 && values[i] == 11) newTotal -= 10; // ajuste As
            if (newTotal > 21) bustPicks++;
        }
        float probC = (float)bustPicks / remaining * 100f;

        probMessage.text =
            "P(dealer supera al jugador): " + probA.ToString("F1") + "%\n" +
            "P(jugador 17-21 si pide carta): " + probB.ToString("F1") + "%\n" +
            "P(jugador se pasa si pide carta): " + probC.ToString("F1") + "%";
    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        // Punto 3 del PDF: el jugador pide carta de una en una
        // Si es la primera vez que pide, revelamos la carta oculta del dealer
        if (dealer.GetComponent<CardHand>().cards.Count == 2)
            dealer.GetComponent<CardHand>().InitialToggle();

        PushPlayer();

        // Punto 4 del PDF: si el jugador supera 21, pierde
        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Te has pasado de 21! Pierdes!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        // Punto 5 del PDF: cuando el jugador se planta empieza el turno del dealer
        // Revelamos la carta oculta del dealer si no se ha hecho aun
        if (dealer.GetComponent<CardHand>().cards.Count == 2)
            dealer.GetComponent<CardHand>().InitialToggle();

        // Regla fija del dealer segun el PDF:
        // - Obligado a pedir carta si tiene 16 o menos
        // - Obligado a plantarse si tiene 17 o mas
        while (dealer.GetComponent<CardHand>().points <= 16)
            PushDealer();

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        // Punto 6: si el dealer se pasa de 21, gana el jugador
        if (dealerPoints > 21)
            finalMessage.text = "El dealer se pasa! Ganas!";
        // Punto 7: si el dealer se planta, gana el que tenga mayor puntuacion
        else if (playerPoints > dealerPoints)
            finalMessage.text = "Ganas! " + playerPoints + " vs " + dealerPoints;
        else if (dealerPoints > playerPoints)
            finalMessage.text = "Pierdes! " + dealerPoints + " vs " + playerPoints;
        // Punto 8: empate si tienen la misma puntuacion
        else
            finalMessage.text = "Empate! Ambos con " + playerPoints + " puntos";

        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
}
