using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Text.RegularExpressions;
using static UnityEditor.Progress;
using static UnityEditor.PlayerSettings;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEditor.VersionControl;
using System.Security.Cryptography.X509Certificates;

public class DeckOfCards : MonoBehaviour
{
    #region vatiables
    public GameAccount a = new GameAccount();
    public Game game = new Game();
    [SerializeField] public string updatescore = "localhost:8000/user/updateUserbyUserId/";
    [SerializeField] public string CreateGameEndpoint = "http://localhost:8000/game";
    [SerializeField] public string EndGameEndpoint = "http://localhost:8000/game/deleteOnce";
    [SerializeField] public string searchPublicKeyURL = "http://localhost:8000/user/publickey";
    [SerializeField] public string BaseUrl = "http://localhost:8000/";
    public string code = "1234";
    public string response = "";


    public TextMeshProUGUI ruleDebugger;
    public int score;



    public Card testcards;
    public Card[] deck;
    public Player[] players;
    public Card[] gameBoard;
    public GameObject cardImagePrefab;
    public GameObject LayoutMasterEmpty; 
    public string CardInfoText;

    public static DeckOfCards deckOfCards;
    public int index;

    public float thinkTimer;
    public float timeDelay;


    public int currentPlayerIndex;
    public int currentCardIndex;

    //hearts,clubs,spades,diamonds 


    public bool canEnd;
    public bool haschosen;

    public Transform middlePileEmpty;
    public Card chosenCard;

    public Sprite[] cardImage;
    public string gameId;

    #endregion variables
    #region functions
   

    public void UpdateScore(string publicKey, int newScore)
    {
        StartCoroutine(SendScoreUpdateRequest(publicKey, newScore));
    }

    private IEnumerator SendScoreUpdateRequest(string publicKey, int newScore)
    {
       
            string url = updatescore + publicKey;
            string jsonData = "{\"score\": " + newScore + "}";

            using (UnityWebRequest www = UnityWebRequest.Put(url, jsonData))
            {
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Score update successful");
                }
                else
                {
                    Debug.Log("Score update failed: " + www.error);
                }
            }
        
    }

    public void CheckNFT(string name, string publicAddress)
    {
        StartCoroutine(SendCheckNFTRequest(name, publicAddress));
    }

    public void SendNFT(string destinationWallet, int transferAmount)
    {
        StartCoroutine(SendSendNFTRequest(destinationWallet, transferAmount));
    }

    private IEnumerator SendCheckNFTRequest(string name, string publicAddress)
    {
        // Create the form data
        WWWForm formData = new WWWForm();
        formData.AddField("name", name);
        formData.AddField("adresse", publicAddress);

        // Create the request
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:8000/check", formData);

        // Send the request
        yield return request.SendWebRequest();
        Debug.Log(request);
        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            string a = request.downloadHandler.text;
            Debug.Log(a);   
            // Request successful
            bool nftExists = bool.Parse(request.downloadHandler.text);
            Debug.Log(nftExists);
            Debug.Log($"NFT exists: {nftExists}");
        }
        else
        {
            // Request failed
            Debug.LogError($"Check NFT request failed: {request.error}");
        }
    }

    private IEnumerator SendSendNFTRequest(string destinationWallet, int transferAmount)
    {
        // Create the form data
        WWWForm formData = new WWWForm();
        formData.AddField("id", destinationWallet);
        formData.AddField("amount", transferAmount);

        // Create the request
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:8000/send", formData);

        // Send the request
        yield return request.SendWebRequest();

        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            // Request successful
            Debug.Log("NFT sent successfully");
        }
        else
        {
            // Request failed
            Debug.LogError($"Send NFT request failed: {request.error}");
        }
    }
    private IEnumerator SearchPublicKeyCoroutine(string code)
    {
        WWWForm form = new WWWForm();
        form.AddField("code", code);

        UnityWebRequest www = UnityWebRequest.Post(searchPublicKeyURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            response = www.downloadHandler.text;
            // Process the response here, assuming it's in JSON format
            // You can use JSON parsing libraries like Newtonsoft.Json to parse the response
            Debug.Log("Response: " + response);
        }
        else
        {
            Debug.Log("Error searching public key: " + www.error);
        }
    }

    public IEnumerator UpdateWinner(string gameId, string winnerId)
    {
        //WWWForm form = new WWWForm();
        //  form.AddField("winner", winnerId);
        var formData = new Dictionary<string, string>();
        formData["winner"] = winnerId;
        string url = "http://localhost:8000/game/put/" + gameId;
        string formDataJson = JsonUtility.ToJson(formData);

        using (UnityWebRequest www = UnityWebRequest.Put(url, formDataJson))
        // byte[] data = form.data;
       // using (UnityWebRequest www = UnityWebRequest.Put(url, data))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(gameId);
                Debug.Log(winnerId);
            }
            else
            {
                Debug.Log("Winner updated successfully.");
            }
        }
    }
    public IEnumerator DeleteGame(string gameId)
    {
        string url = "http://localhost:8000/game/delete/" + gameId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Game deleted successfully.");
            }
        }
    }
  



    public IEnumerator TryCreate()
    {
        string playerId = "645812df21c4bdae9c39ccd1";
        gameId = System.Guid.NewGuid().ToString(); // generate a new unique ID
        Debug.Log(gameId);
        WWWForm form = new WWWForm();
        form.AddField("gameId", gameId); // add the generated ID to the form data
        form.AddField("players", playerId);
        
        Debug.Log(form);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:8000/game", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("game information sent successfully.");
            }
        }
    }


        //use this for initialisation
        void Awake()
    {
        StartCoroutine(TryCreate());
        deckOfCards = this;

        string code = "1234";
        StartCoroutine(SearchPublicKeyCoroutine(code));

        UpdateScore("EX89RC941UhmJWQBtf3tAEAWmPvdLxq9GUmrhZDiLea", 40);

    }
    // Start is called before the first frame update
    
    void Start()
    {
        

        deck = new Card[52];
        for (int i = 0; i < 13; i++)
        {
            Card temp = new Card();
            temp.value = i;
            temp.suit = Card.Suit.hearts;
            deck[index] = temp;
            index++;

        }
        for (int i = 0; i < 13; i++)
        {
            Card temp = new Card();
            temp.value = i;
            temp.suit = Card.Suit.spades;
            deck[index] = temp;
            index++;

        }
        for (int i = 0; i < 13; i++)
        {
            Card temp = new Card();
            temp.value = i;
            temp.suit = Card.Suit.clubs;
            deck[index] = temp;
            index++;

        }
        for (int i = 0; i < 13; i++)
        {
            Card temp = new Card();
            temp.value = i ;
            temp.suit = Card.Suit.diamonds;
            deck[index] = temp;
            index++;
            

        }
        players = new Player[4];
        gameBoard = new Card[0];
        Card[] tempHand = new Card[0];
        for (int i = 0; i < players.Length; i++)
        {
            Player p = new Player();
            p.hand = tempHand;
            p.index = i;
            p.username = "Player" + i;
            players[i] = p;
        }
         Schuffle();
        //DealMethod1();


        //dealmethod 2


        for (int i = 0; i < players.Length; i++)
        {
            DealMethod2(players[i]);
            DealMethod2(players[i]);
            DealMethod2(players[i]);
            DealMethod2(players[i]);
            DealMethod2(players[i]);
            DealMethod2(players[i]);
            DealMethod2(players[i]);
        }
        
        int whichPlayerAmI = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].index == 0)
            {
                whichPlayerAmI = i;
            }
        }
        for (int i = 0; i < players[whichPlayerAmI].hand.Length; i++)
        {
            ShowCard(players[whichPlayerAmI].hand[i]);
        }
        //StartCoroutine(UpdateWinner(gameId, "645812df21c4bdae9c39ccd1"));
        CheckNFT("Mint Robot","J4H6yEqMioQoio4n5iz2pFHuCJ7uLHuprLX5g4gnUsef");
       // SendNFT("EX89RC941UhmJWQBtf3tAEAWmPvdLxq9GUmrhZDiLeEf", 10);
        // StartCoroutine(DeleteGame(gameId));

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    if (haschosen)
        //    {
        //         PlayCard(players[currentPlayerIndex], currentCardIndex);
        //         EndTurn();
        //    }
        //    else
        //    {
        //        Debug.Log("You need to choose a card");
        //    }
        //}

        
        if (currentPlayerIndex !=0)
        {
            thinkTimer += Time.deltaTime;
            if(thinkTimer > timeDelay)
            {
                haschosen = true;
                PlayYourChosenCard();
               
            }
        }
        
    }

    public int PlayYourChosenCard()
    {
        int ci=0;
        if(haschosen)
        {
            ci=PlayCard(players[currentPlayerIndex], currentCardIndex);
            
            EndTurn();
            AddScore(score);

            
        }
        else
        {
            Debug.Log("You need to choose a card!");
        }
        return ci;
    }

    public void Schuffle()
    {
        int replacement = UnityEngine.Random.Range(100, 1000);

        for (int i = 0; i < replacement; i++)
        {
            int A = UnityEngine.Random.Range(0, 52);
            int B = UnityEngine.Random.Range(0, 52);

            Card a = deck[A];
            Card b = deck[B];
            Card c = deck[A];


            a = b;
            b = c;

            deck[A] = a;
            deck[B] = b;
        }

     
             
    
    }
    public void DealMethod1()
    {
        //static hand sizes
        int cardsDealt = 0;
        for (int i=0;i<players.Length;i++)
        {
            Card[] dealtHand = new Card[7];
            dealtHand[0] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[1] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[2] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[3] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[4] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[5] = deck[cardsDealt];
            cardsDealt++;
            dealtHand[6] = deck[cardsDealt];
            cardsDealt++;
            players[i].hand = dealtHand;
        }

        int whichPlayerAmI = 0; 
        for (int i=0;i<players.Length;i++)
        {
            if (players[i].index == 0)
            {
                whichPlayerAmI = i;
            }
        }
        for (int i = 0; i < players[whichPlayerAmI].hand.Length;i++)
        {
            ShowCard(players[whichPlayerAmI].hand[i]);
        }
    
    
    
    
    
    }
    public void DealMethod2(Player p)
    {
        //resize deck every time a card is drawn
        //resize a hand everi time a card is dawn
        Card[] afterDraw = new Card[p.hand.Length + 1];
        p.hand.CopyTo(afterDraw, 0);
        afterDraw[p.hand.Length] = deck[0];
        p.hand = afterDraw;
        
        Card[] tempDeck = new Card[deck.Length - 1];
        for (int i = 1 ; i < deck.Length; i++)
        {
            tempDeck[i-1] = deck[i];
            
        }
        deck = tempDeck;
        Debug.Log("Player " + p.index + " just drew a card and has " + p.hand.Length + " in hand");


       

    }
    public int PlayCard(Player p, int selectedCard)
    {
        int ci = 0;
        if (p.hand.Length > selectedCard)
        {
            
            Card selection = p.hand[selectedCard];
            Card[] tempGameBoard = new Card[gameBoard.Length + 1];
            gameBoard.CopyTo(tempGameBoard, 1);
            //pick player card

            tempGameBoard[0] = selection;
            gameBoard = tempGameBoard;

         ModifiedShowCard(selection);

            // ShowCard(selection);

            Card[] tempHand = new Card[p.hand.Length - 1];
            for (int i = 0; i < p.hand.Length; i++)
            {
                if (i < selectedCard)
                {
                    tempHand[i] = p.hand[i];
                }
                if (i > selectedCard)
                {
                    tempHand[i - 1] = p.hand[i];
                }
            }
            p.hand = tempHand;
            //ShowCard(selection);
        }
        else
        {
            Debug.Log("Player is out of cards. Turn ended");
        }
        
        GameObject[] childrenOfMaster = new GameObject[LayoutMasterEmpty.transform.childCount];
        for (int i = 0; i < childrenOfMaster.Length; i++)
        {
            childrenOfMaster[i] = LayoutMasterEmpty.transform.GetChild(i).gameObject;
        }
        LayoutMasterEmpty.transform.DetachChildren();
        foreach (GameObject go in childrenOfMaster)
        {
            Destroy(go);
        }
        int whichPlayerAmI = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].index == 0)
            {
                whichPlayerAmI = i;
            }
        }
        for (int i = 0; i < players[whichPlayerAmI].hand.Length; i++)
        {
            ShowCard(players[whichPlayerAmI].hand[i]);
        }
        return ci;

    }
    public void ShowCard(Card c)
    {
      
        GameObject newestPrefab = Instantiate(cardImagePrefab);
        newestPrefab.transform.SetParent(LayoutMasterEmpty.transform);
        //Text childText = newestPrefab.GetComponentInChildren<Text>();
       //CardInfoText = c.value.ToString() +" of "+ c.suit.ToString();
        //childText.text = CardInfoText;

        Cardinfo ci = newestPrefab.GetComponent<Cardinfo>();
        ci.card = c;

        // newimage : heart, club, spade, diamond, ace to king
        int cardImageIndex = c.value * 4;
        switch(c.suit)
        {
            //case Card.Suit.hearts:
            //    cardImageIndex += 0;
            //    break;
            case Card.Suit.clubs:
                cardImageIndex += 1;
                break;
            case Card.Suit.spades:
                cardImageIndex += 2;
                break;
            case Card.Suit.diamonds:
                cardImageIndex += 3;
                break;
        }

           newestPrefab.GetComponent<Image>().sprite = cardImage[cardImageIndex];


    }
    public void ModifiedShowCard(Card c)
    {
        GameObject newestPrefab = Instantiate(cardImagePrefab);
        newestPrefab.transform.SetParent(middlePileEmpty.transform);

        switch (currentPlayerIndex)
        {
            case 0:
                newestPrefab.GetComponent<RectTransform>().localPosition = Vector3.zero + Vector3.up * 50;
                break;
            case 1:
                newestPrefab.GetComponent<RectTransform>().localPosition = Vector3.zero + Vector3.right * 50;
               
                break;
            case 2:
                newestPrefab.GetComponent<RectTransform>().localPosition = Vector3.zero + Vector3.up * -50;
                
                break;
            case 3:
                newestPrefab.GetComponent<RectTransform>().localPosition = Vector3.zero + Vector3.right * -50;
                
                break;
        }
        
        //   Text childText = newestPrefab.GetComponentInChildren<Text>();
        // CardInfoText = c.value.ToString() + " of " + c.suit.ToString();
        //childText.text = CardInfoText;

        //figure out how to set card info
        
       // Cardinfo ci = newestPrefab.GetComponent<Cardinfo>();
        //ci.card = c;
        int cardImageIndex = c.value * 4;
        switch (c.suit)
        {
            //case Card.Suit.hearts:
            //    cardImageIndex += 0;
            //    break;
            case Card.Suit.clubs:
                cardImageIndex += 1;
                break;
            case Card.Suit.spades:
                cardImageIndex += 2;
                break;
            case Card.Suit.diamonds:
                cardImageIndex += 3;
                break;
        }

        newestPrefab.GetComponent<Image>().sprite = cardImage[cardImageIndex];
        
    }
    public void CheckForHighestCard()
    {
        int currentHighest = 0;
        for( int i=0;i<players.Length;i++)
        {
           if( gameBoard[i].value>currentHighest)
            {
                currentHighest = gameBoard[i].value;
            }
        }
        Debug.Log(currentHighest);
        if (gameBoard[3].value==currentHighest)
        {   
            score++;
        }
        ruleDebugger.SetText(score.ToString());
       
    }
    public void EndTurn()
    {
        if (currentPlayerIndex == players.Length - 1)
        {
            CheckForHighestCard();
            currentPlayerIndex = 0;
        }
        else
        {
            currentPlayerIndex++;
        }
        currentCardIndex = 0;
        thinkTimer = 0;
        timeDelay = UnityEngine.Random.Range(0.3f, 4.9f);
        haschosen = false;
    }
    public void ChooseCard(Card c)
    {
        chosenCard = c;
        for(int i=0; i < players[currentPlayerIndex].hand.Length; i++)
        {
            if (c == players[currentPlayerIndex].hand[i])
            {
                currentCardIndex = i;
            }
            
        }
        haschosen = true;

    }
     public void AddScore(int score)
    { 
        Player p = new Player();
        p.playerScore += score;
        // Save the updated score to PlayerPrefs
        PlayerPrefs.SetInt("PlayerScore", p.playerScore);
        Debug.Log(p.playerScore);
        a.score=p.playerScore;
    }

    public int GetScore()
    {
        Player p = new Player();
        return p.playerScore;
    }
    #endregion functions
}
#region classes
[Serializable]
public class Card
{
    public int value;
    public enum Suit {hearts, clubs, diamonds, spades};
    public Suit suit;

}
[Serializable]
public class Player
{
    public Card[] hand;
    public int index;
    public String username;
    public const string SCORE_KEY = "PlayerScore";
    public int playerScore;

}

[Serializable]
public class CreateResponse
{
    public int code;
    public string msg;
    public GameAccount data;
}

[Serializable]
public class GameAccount
{
    public string _id;
    public string username;
    public int score;
}

[Serializable]
public class Game
{
    public string _id;
    public string[] players;
    public string[] winner;
}


#endregion  classes
