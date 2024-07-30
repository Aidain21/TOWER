using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffect : MonoBehaviour
{
    public void Awake()
    {
        transform.position += new Vector3(0, 0, -3);
    }
    public IEnumerator FadeOut(float duration, Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            //right here, you can now use normalizedTime as the third parameter in any Lerp from start to end
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.clear, normalizedTime);
            yield return null;
        }
        GetComponent<SpriteRenderer>().color = Color.clear;
        Destroy(gameObject);
    }
    public IEnumerator Travel(Vector3 end, float seconds, int modifier, Sprite sprite, EnemyScript enemy = null)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
        Vector3 start = transform.position;
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / seconds));
            transform.position = new Vector3(data.x, data.y, transform.position.z);
            elapsedTime += Time.deltaTime * modifier;
            yield return new WaitForEndOfFrame();
        }
        if (enemy != null)
        {
            enemy.targetedPlayer.GetComponent<PlayerScript2D>().invManager.Damage(enemy.attack);
            enemy.EndEnemyTurn();
        }
        Destroy(gameObject);
        
    }
}
