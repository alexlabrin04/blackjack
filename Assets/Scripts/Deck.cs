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
            /*TODO:
             * Si alguno de los dos obtiene Blackjack, termina el juego y mostramos mensaje
             */
        }
    }

    private void CalculateProbabilities()
    {
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */
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
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */
        
        //Repartimos carta al jugador
        PushPlayer();

        /*TODO:
         * Comprobamos si el jugador ya ha perdido y mostramos mensaje
         */      

    }

    public void Stand()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */

        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */                
         
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
