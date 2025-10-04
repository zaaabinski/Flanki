using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isCanDown;
    [SerializeField] private GameObject beerCan;
    [SerializeField] private GameObject drinkText;
    
    [Header("Player depended")]
    [SerializeField] private GameObject playerOneBeer;
    void Start()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void TurnOnDrinkText()
    {
        playerOneBeer.SetActive(true);
        drinkText.SetActive(true);
    }
}
