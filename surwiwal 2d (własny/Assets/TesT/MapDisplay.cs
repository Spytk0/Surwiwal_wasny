using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void DrawTexture(Texture2D texture)
    {
        // Tworzenie sprita z tekstury
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Ustawienie sprita na SpriteRenderer
        spriteRenderer.sprite = sprite;

        // Ustawienie skali obiektu, aby by³ kwadratem
        float scale = Mathf.Max(texture.width, texture.height);
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1);
    }
}
