using System.Collections.Generic;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    public GameObject card;
    public bool isDealer = false;
    public int points;
    private int coordY;    
     
    private void Awake()
    {
        points = 0;
        coordY = isDealer ? -1 : 3;
    }

    public void Clear()
    {
        points = 0;
        foreach (GameObject g in cards) Destroy(g);
        cards.Clear();
    }

    public void Push(Sprite front, int value)
    {
        GameObject cardCopy = Instantiate(card);
        cards.Add(cardCopy);

        float coordX = 1.4f * (cards.Count - 4);
        cardCopy.transform.position = new Vector3(coordX, coordY);

        CardModel model = cardCopy.GetComponent<CardModel>();
        model.front = front;
        model.value = value;

        // Si es dealer, la primera carta se queda boca abajo
        if (isDealer && cards.Count <= 1)
            model.ToggleFace(false);
        else
            model.ToggleFace(true);

        CalculatePoints();
    }

    // Nueva función para que Deck.cs pueda calcular probabilidades
    public int GetVisiblePoints()
    {
        int total = 0;
        int aces = 0;
        foreach (GameObject c in cards)
        {
            CardModel m = c.GetComponent<CardModel>();
            // Solo sumamos si la carta está boca arriba (el SpriteRenderer no es el back)
            if (m.GetComponent<SpriteRenderer>().sprite == m.front)
            {
                total += m.value;
                if (m.value == 11) aces++;
            }
        }
        while (total > 21 && aces > 0) { total -= 10; aces--; }
        return total;
    }

    private void CalculatePoints()
    {
        int val = 0;
        int aces = 0;
        foreach (GameObject f in cards)
        {
            int cVal = f.GetComponent<CardModel>().value;
            val += cVal;
            if (cVal == 11) aces++;
        }

        // Lógica robusta de Ases: si te pasas de 21, el As vale 1 (restamos 10)
        while (val > 21 && aces > 0)
        {
            val -= 10;
            aces--;
        }
        points = val;
    }


}
