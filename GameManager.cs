using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button twoDBtn;
    [SerializeField] private Button threeDBtn;
    [SerializeField] private Image textureImage;


    void Start()
    {
        twoDBtn.gameObject.SetActive(true);
        threeDBtn.gameObject.SetActive(false);
        textureImage.gameObject.SetActive(false);

    }
    public void TwoD()
    {
        twoDBtn.gameObject.SetActive(false);
        threeDBtn.gameObject.SetActive(true);
        textureImage.gameObject.SetActive(true);
    }
    public void ThreeD()
    {
        twoDBtn.gameObject.SetActive(true);
        threeDBtn.gameObject.SetActive(false);
        textureImage.gameObject.SetActive(false);
    }

}
