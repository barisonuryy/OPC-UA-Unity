using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public Dictionary<string, Product> products = new Dictionary<string, Product>();

    void Start()
    {
        AddProductsWithDifferentTags();
        StartCoroutine(UpdateProductColors());
    }

    // Ürünlerin renklerini güncelleyen coroutine
    IEnumerator UpdateProductColors()
    {
        while (true)
        {
            foreach (var product in products.Values)
            {
                // Ürünün güncel değerini al (örneğin sensörden ya da bir sistemden)
                float currentValue = Random.Range(0f, 120f); // Örnek değer, sistemden gerçek değer alabilirsiniz

                // Renk değişimi için ProductColorManager kullan
                ColorChanger colorManager = product.model.GetComponent<ColorChanger>();

                if (colorManager != null)
                {
                    // Ürün değeri min veya max threshold'ları aşıyorsa, renk değiştir
                    if (currentValue < product.minThreshold || currentValue > product.maxThreshold)
                    {
                        // Uyarı durumu (renk kırmızıya değişir)
                      colorManager.StartCoroutine(colorManager.ChangeColorSmoothly());
                    }
                    else
                    {
                        // Normal durum (renk beyaza değişir)
                        colorManager.StartCoroutine(colorManager.ChangeColorSmoothly());
                    }
                }
            }

            // Sürekli güncelleme için bir süre bekle
            yield return new WaitForSeconds(2.0f);
        }
    }

    public void AddProductsWithDifferentTags()
    {
        // Tüm sahnedeki GameObject'leri bul
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Her bir GameObject için işlem yap
        foreach (GameObject obj in allObjects)
        {
            if (obj.tag != "Untagged")  // Untagged objeleri hariç tut
            {
                string productName = obj.name;      // GameObject ismi
                string productModel = obj.tag;      // GameObject tag'i

                // Örnek uyarı değerleri (min ve max threshold değerleri)
                float minThreshold = 10f;
                float maxThreshold = 100f;

                // Yeni bir Product oluştur ve dictionary'ye ekle
                Product newProduct = new Product(productName, productModel, obj, minThreshold, maxThreshold);

                if (!products.ContainsKey(productName))  // Aynı isimde ürün yoksa ekle
                {
                    products.Add(productName, newProduct);
                    Debug.Log($"Added product: {productName}, Model: {productModel}, Tag: {obj.tag}");
                }
            }
        }
    }

    public class Product
    {
        public string name;
        public string productModel;
        public GameObject model;
        public float minThreshold;  // Minimum uyarı değeri
        public float maxThreshold;  // Maksimum uyarı değeri

        public Product(string name, string productModel, GameObject model, float minThreshold, float maxThreshold)
        {
            this.name = name;
            this.productModel = productModel;
            this.model = model;
            this.minThreshold = minThreshold;
            this.maxThreshold = maxThreshold;
        }
    }
}
