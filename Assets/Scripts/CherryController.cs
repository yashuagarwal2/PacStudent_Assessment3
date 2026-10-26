using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    
    public GameObject cherryPrefab;
    public Tweener tweener;
    public float speed = 4f;

   
    Vector3 centre = new Vector3(13.5f, -14f, 0);

    
    float distance = 25f;

    void Start()
    {
        StartCoroutine(SpawnCherries());
    }

    
    IEnumerator SpawnCherries()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(5f);

            
            float angle = Random.Range(0f, 360f);
            float radians = angle * Mathf.Deg2Rad;

            
            float x = centre.x + Mathf.Cos(radians) * distance;
            float y = centre.y + Mathf.Sin(radians) * distance;
            Vector3 start = new Vector3(x, y, 0);

            
            float endX = centre.x - Mathf.Cos(radians) * distance;
            float endY = centre.y - Mathf.Sin(radians) * distance;
            Vector3 end = new Vector3(endX, endY, 0);

            
            GameObject cherry = Instantiate(cherryPrefab, start, Quaternion.identity);
            float time = Vector3.Distance(start, end) / speed;
            tweener.AddTween(cherry.transform, start, end, time);

           
            while (cherry != null && tweener.TweenExists(cherry.transform))
            {
                yield return null;
            }

            
            if (cherry != null)
            {
                Destroy(cherry);
            }
        }
    }
}