using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour, IPointerClickHandler
{
    private Canvas canvas;

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene(0);
    }

    private void OnEnable() {
        canvas.sortingOrder = 10;
    }

    private void OnDisable() {
        canvas.sortingOrder = -5;
    }
}
