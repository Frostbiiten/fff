using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform skin;
    private Vector3 target;
    private Vector3 origin;
    [SerializeField] private AnimationCurve arc;
    private float time;
    private float totalTime;
    [SerializeField] private float radius;
    [SerializeField] private float power;
    [SerializeField] private ParticleSystem water;

    private float rot;
    public void Init(Vector3 origin, Vector3 target, float time)
    {
        this.origin = origin;
        this.target = target;
        totalTime = this.time = time;
        rot = Random.Range(1600f, 3000f) * (Random.Range(0, 1) - 0.5f);
    }
    
    void Update()
    {
        time -= Time.deltaTime;

        if (time <= 0f)
        {
            if (time + Time.deltaTime > 0f)
            {
                // explode
                GameMan.inst.camShaker.ShakeOnce(15f, 4f, 0f, 0.85f);
                skin.gameObject.SetActive(false);
                AudioManager.instance.PlaySound("SmallImpact");
                
                // Splat
                var floor = GameMan.inst.map.GetFloor(0);
                for (int x = (int)(transform.position.x - radius); x <= transform.position.x + radius; x++)
                {
                    for (int y = (int)(transform.position.y - radius); y <= transform.position.y + radius; y++)
                    {
                        Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                        float delta = (radius - (transform.position - (Vector3)p).magnitude) * power;
                        if (delta > 0f) floor.CoolTile(new Vector3Int(x, y), delta);
                    }
                }
                
                // Water
                water.Play();
                
                // Get player distance
                float playerDist = (PlayerCore.inst.transform.position - transform.position).magnitude;
                if (playerDist <= radius - 0.5f) PlayerCore.inst.ChangeHP(-1);
            }
            
            if (time < -2f)
            {
                Destroy(gameObject);
            }
            
            // water
            return;
        }
        
        skin.rotation *= Quaternion.AngleAxis(Time.deltaTime * rot, Vector3.forward);
        float t = 1f - time / totalTime;

        transform.position = Vector3.Lerp(origin, target, t);
        skin.localPosition = Vector3.up * arc.Evaluate(t);
    }
}
