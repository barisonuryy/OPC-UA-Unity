using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    private Material objMaterial;

    // Renk değiştirme süresi
    public float duration = 2.0f;

    // Başlangıç ve hedef renkler
    public Color startColor = Color.white;
    public Color targetColor = Color.red;

    private void Start()
    {
        // Objeye atanmış materyali alıyoruz
        objMaterial = GetComponent<Renderer>().material;

        // Coroutine başlatıyoruz
      
    }

    // Coroutine ile renk geçişi
    public IEnumerator ChangeColorSmoothly()
    {
        float time = 0;

        // Belirtilen süre boyunca renk geçişi yap
        while (time < duration)
        {
            // Zamanı arttır
            time += Time.deltaTime;

            // İki renk arasında zamanla geçiş yap (0'dan 1'e)
            objMaterial.color = Color.Lerp(startColor, targetColor, time / duration);

            // Bir frame bekle
            yield return null;
        }

        // Son durumda rengi tamamen hedef renge eşitle
        objMaterial.color = targetColor;
    }
}
