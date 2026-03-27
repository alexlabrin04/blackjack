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

        // Accedemos a la 1ª carta del dealer (índice 0) y forzamos que se ponga boca abajo (false)
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(false);

        // Si alguno tiene Blackjack en las 2 primeras cartas, gana
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
        int remaining = faces.Length - cardIndex;

        if (remaining <= 0)
        {
            probMessage.text = "No quedan cartas en la baraja.";
            return;
        }

        int playerPoints = player.GetComponent<CardHand>().points;

        // Contadores para nuestros casos de éxito
        int dealerBeats = 0;
        int safePicks = 0;
        int bustPicks = 0;

        // UN SOLO BUCLE para calcular las 3 probabilidades
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int simulatedCardValue = values[i];

            // --- CÁLCULO PROB A (Dealer supera al jugador) ---
            // Asumimos que la carta oculta es simulatedCardValue. Solo evaluamos las visibles + la simulada oculta.
            int dTotal = CalculateDealerSimulatedPoints(simulatedCardValue);
            if (dTotal > playerPoints && dTotal <= 21)
            {
                dealerBeats++;
            }

            // --- CÁLCULO PROB B y C (Jugador pide carta) ---
            int pTotal = CalculatePlayerSimulatedPoints(simulatedCardValue);

            if (pTotal >= 17 && pTotal <= 21) safePicks++; // PROB B
            if (pTotal > 21) bustPicks++;                  // PROB C
        }

        // Calculamos porcentajes
        float probA = (float)dealerBeats / remaining * 100f;
        float probB = (float)safePicks / remaining * 100f;
        float probC = (float)bustPicks / remaining * 100f;

        // Ajustamos el texto al formato exacto del PDF
        probMessage.text =
            "Dealer > Jugador: " + probA.ToString("F2") + "\n" +
            "17 <= X <= 21: " + probB.ToString("F2") + "\n" +
            "X > 21: " + probC.ToString("F2");
    }

    // --- MÉTODOS AUXILIARES PARA SIMULAR LOS ASES CORRECTAMENTE ---

    private int CalculatePlayerSimulatedPoints(int newCardValue)
    {
        int val = 0;
        int aces = 0;
        CardHand hand = player.GetComponent<CardHand>();

        // 1. Contamos las cartas que ya tiene en la mano
        foreach (GameObject g in hand.cards)
        {
            int cVal = g.GetComponent<CardModel>().value;
            if (cVal == 11) aces++;
            else val += cVal;
        }

        // 2. Añadimos la carta que estamos simulando
        if (newCardValue == 11) aces++;
        else val += newCardValue;

        // 3. Aplicamos la misma lógica de Ases que tiene CardHand.cs
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

        // 1. Contamos SOLO las cartas visibles del dealer (desde el índice 1)
        for (int i = 1; i < hand.cards.Count; i++)
        {
            int cVal = hand.cards[i].GetComponent<CardModel>().value;
            if (cVal == 11) aces++;
            else val += cVal;
        }

        // 2. Añadimos la carta oculta simulada
        if (hiddenCardValue == 11) aces++;
        else val += hiddenCardValue;

        // 3. Aplicamos la lógica de Ases
        for (int i = 0; i < aces; i++)
        {
            if (val + 11 <= 21) val += 11;
            else val += 1;
        }
        return val;
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
        PushPlayer();
        // Revelamos la carta oculta del dealer aunque el jugador haya perdido
        dealer.GetComponent<CardHand>().InitialToggle();

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
