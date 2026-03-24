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
        // Recorremos las 52 posiciones de la baraja
        for (int i = 0; i < 52; i++)
        {
            // El operador de módulo nos da la posición de la carta dentro de su palo (de 0 a 12)
            int cardRank = i % 13;

            // 1. Caso del As: Según las reglas, puede valer 1 u 11.
            if (cardRank == 0)
            {
                // Generalmente se inicializa como 11. 
                // La reducción a 1 cuando el jugador se pasa de 21 se suele gestionar matemáticamente en la clase CardHand al hacer el recuento.
                values[i] = 11;
            }
            // 2. Caso de las figuras (J, Q, K): Tienen una puntuación de 10 puntos.
            else if (cardRank >= 10)
            {
                values[i] = 10;
            }
            // 3. Caso de cartas numéricas (2 al 10): Equivalen al valor de su carta.
            else
            {
                // Como las posiciones en programación empiezan en 0, sumamos 1 al índice local del palo para obtener su valor real.
                // Ejemplo: El '2' está en la posición 1 (1 + 1 = 2).
                values[i] = cardRank + 1;
            }
        }
    }

    private void ShuffleCards()
    {
        // Recorremos el array de cartas (puedes usar faces.Length que es 52)
        for (int i = 0; i < faces.Length; i++)
        {
            // 1. Elegimos una posición aleatoria dentro de la baraja
            // Random.Range(0, n) devuelve un valor entre 0 y n-1, justo lo que necesitamos para los índices.
            int randomIndex = Random.Range(0, faces.Length);

            // 2. Guardamos los datos de la carta actual (i) en variables temporales
            Sprite tempFace = faces[i];
            int tempValue = values[i];

            // 3. Movemos la carta de la posición aleatoria a la posición actual (i)
            // ¡Importante hacer el cambio en AMBOS arrays a la vez!
            faces[i] = faces[randomIndex];
            values[i] = values[randomIndex];

            // 4. Ponemos la carta que guardamos en la posición aleatoria
            faces[randomIndex] = tempFace;
            values[randomIndex] = tempValue;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
            // TODO resuelto: comprobamos Blackjack tras el reparto inicial
            bool playerBlackjack = player.GetComponent<CardHand>().points == 21;
            bool dealerBlackjack = dealer.GetComponent<CardHand>().points == 21;

            if (playerBlackjack || dealerBlackjack)
            {
                // Revelamos la carta oculta del dealer
                dealer.GetComponent<CardHand>().cards[0]
                      .GetComponent<CardModel>().ToggleFace(true);

                if (playerBlackjack && dealerBlackjack)
                    finalMessage.text = "EMPATE - Ambos tienen Blackjack";
                else if (playerBlackjack)
                    finalMessage.text = "BLACKJACK - Ganas!";
                else
                    finalMessage.text = "BLACKJACK del dealer - Pierdes!";

                hitButton.interactable = false;
                stickButton.interactable = false;
            }
        }
    }

    private void CalculateProbabilities()
    {
        // Cartas que quedan por repartir en la baraja
        int remaining = faces.Length - cardIndex;

        if (remaining <= 0)
        {
            probMessage.text = "No quedan cartas en la baraja.";
            return;
        }

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerVisiblePoints = dealer.GetComponent<CardHand>().GetVisiblePoints();

        // ── Prob. A: el dealer supera al jugador con la carta oculta ──────
        // Sumamos cuántas de las cartas restantes hacen que el total del dealer
        // (visible + oculta) supere la puntuación del jugador.
        int dealerBeatsCount = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int dealerTotal = dealerVisiblePoints + values[i];
            // El As oculto puede valer 1 si con 11 se pasa
            if (dealerTotal > 21 && values[i] == 11)
                dealerTotal -= 10;

            if (dealerTotal > playerPoints && dealerTotal <= 21)
                dealerBeatsCount++;
        }
        float probDealerBeats = (float)dealerBeatsCount / remaining * 100f;

        // ── Prob. B: el jugador obtiene entre 17 y 21 pidiendo una carta ──
        int safeCount = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int newTotal = playerPoints + values[i];
            // Ajuste del As si se pasa
            if (newTotal > 21 && values[i] == 11)
                newTotal -= 10;

            if (newTotal >= 17 && newTotal <= 21)
                safeCount++;
        }
        float probSafe = (float)safeCount / remaining * 100f;

        // ── Prob. C: el jugador se pasa de 21 pidiendo una carta ──────────
        int bustCount = 0;
        for (int i = cardIndex; i < faces.Length; i++)
        {
            int newTotal = playerPoints + values[i];
            if (newTotal > 21 && values[i] == 11)
                newTotal -= 10;

            if (newTotal > 21)
                bustCount++;
        }
        float probBust = (float)bustCount / remaining * 100f;

        // ── Mostramos el resultado ─────────────────────────────────────────
        probMessage.text =
            $"Cartas restantes: {remaining}\n" +
            $"P(dealer supera al jugador): {probDealerBeats:F1}%\n" +
            $"P(jugador 17-21 si pide):    {probSafe:F1}%\n" +
            $"P(jugador se pasa si pide):  {probBust:F1}%";
    }

    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }       

    public void Hit()
    {
        // TODO resuelto: voltear carta oculta del dealer si es la mano inicial
        // La mano inicial son exactamente 2 cartas. Si el jugador pide por
        // primera vez, el dealer sigue con 2 cartas y la primera está oculta.
        if (dealer.GetComponent<CardHand>().cards.Count == 2)
        {
            dealer.GetComponent<CardHand>().cards[0]
                  .GetComponent<CardModel>().ToggleFace(true);
        }

        // Repartimos carta al jugador (ya estaba en el original)
        PushPlayer();

        // TODO resuelto: comprobamos si el jugador pierde
        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Te has pasado de 21 - Pierdes!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        // TODO resuelto: voltear carta oculta del dealer si no se ha hecho aún
        if (dealer.GetComponent<CardHand>().cards.Count >= 2)
        {
            dealer.GetComponent<CardHand>().cards[0]
                  .GetComponent<CardModel>().ToggleFace(true);
        }

        // TODO resuelto: el dealer pide carta mientras tenga 16 o menos
        while (dealer.GetComponent<CardHand>().points <= 16)
        {
            PushDealer();
        }

        // TODO resuelto: determinamos quién gana y mostramos el mensaje
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        if (dealerPoints > 21)
            finalMessage.text = "El dealer se pasa - Ganas!";
        else if (playerPoints > dealerPoints)
            finalMessage.text = "Ganas! " + playerPoints + " vs " + dealerPoints;
        else if (dealerPoints > playerPoints)
            finalMessage.text = "El dealer gana: " + dealerPoints + " vs " + playerPoints;
        else
            finalMessage.text = "Empate! " + playerPoints + " puntos";

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
