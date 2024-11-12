using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetLogic : MonoBehaviour
{
    public TMP_Text hitCounterText;
    private int hitCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateHitCounterText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            hitCount++;
            UpdateHitCounterText();
        }
    }

    private void UpdateHitCounterText()
    {
        if (hitCounterText != null)
        {
            hitCounterText.text = "Hits: " + hitCount;
        }
    }
}
