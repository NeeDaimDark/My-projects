using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
public class Cardinfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Card card;
    public bool hasChosen;
    public GameObject selectedObject;
    public bool locationOrPlayable;
    public bool selectedLocation;
    //true means location ,false means playable
    public void OnPointerEnter(PointerEventData pointerEventData)
    {   if (!locationOrPlayable)
        {
            selectedObject = this.gameObject;
            selectedLocation = true;
            Debug.Log("selected");
        }
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
       
            selectedObject = null;
        if (!locationOrPlayable)
        {
            if (hasChosen)
            {
                hasChosen = false;
                transform.Translate(0, -20, 0);
            }
        }
        selectedLocation = false;
        if (locationOrPlayable && !selectedLocation)
        {

            gameObject.GetComponent<Image>().color = Color.white;

        }
        Debug.Log("exit");
    }
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (selectedObject != null)
        {
            DeckOfCards.deckOfCards.ChooseCard(card);
            Debug.Log("click");
        }

        //if(locationOrPlayable && !selectedLocation)
        //{
        //    //save location
        //    gameObject.GetComponent<Image>().color = Color.red;
        //    //play the card
        //}
        //if(locationOrPlayable && selectedLocation)
        //{

        //    gameObject.GetComponent<Image>().color = Color.white;
        //    if (DeckOfCards.deckOfCards.haschosen)
        //    {  

        //        //need to end turn(marra akahaw na3mlou select)
        //        gameObject.GetComponent<Image>().sprite = DeckOfCards.deckOfCards.cardImage[DeckOfCards.deckOfCards.PlayYourChosenCard()];

        //    }
        //}

        selectedLocation = !selectedLocation;

        if (!locationOrPlayable && hasChosen == true)
        {
            if (DeckOfCards.deckOfCards.haschosen)
            {   
                int ci= DeckOfCards.deckOfCards.PlayYourChosenCard();

            }
        }
       if (selectedObject != null && hasChosen==false && !locationOrPlayable)
        {
            DeckOfCards.deckOfCards.ChooseCard(card);
            hasChosen = true;
            transform.Translate(0, 20, 0);
        }
    }
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        Debug.Log("Terminé");
    }

    
   
}
