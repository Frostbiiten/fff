using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        Default,
        Chase,
        Attack
    }

    private State currentState = State.Default;
    private Room _currentRoom;
    private float shakeTimer;
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private float hp;
    [SerializeField] private Transform skin;
    [SerializeField] private float shakeDist = 0.2f;
    [SerializeField] private float shakeInterval = 0.2f;
    [SerializeField] private Animator animator;
    
    [SerializeField] private GameObject attackObj;
    [SerializeField] private float attackDist = 4f;
    private float attackTimer;

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 0f)
        {
            int cur = Mathf.RoundToInt(shakeTimer / shakeInterval);
            shakeTimer -= Time.deltaTime;
            int post = Mathf.RoundToInt(shakeTimer / shakeInterval);
            if (cur != post)
            {
                skin.localPosition = -skin.localPosition + new Vector3(Random.value - 0.5f, Random.value - 0.5f) * shakeDist;
                skin.localPosition *= 0.8f;
            }

            if (hp <= 0)
            {
                Instantiate(GameMan.inst.plume, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            else
            {
                if (shakeTimer < 0f) animator.Play("EnemyDefault");
            }
        }
        else
        {
            skin.localPosition = Vector2.zero;
            
            Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            _currentRoom = GameMan.inst.map.GetFloor(0).GetRoom(pos);
            
            switch (currentState)
            {
                default:
                case State.Default:
                    if (_currentRoom != null && PlayerCore.inst.currentRoom == _currentRoom)
                    {
                        currentState = State.Chase;
                    }
                    break;
                
                case State.Attack:
                    attackTimer -= Time.deltaTime;
                    if (attackTimer < 1f && attackTimer + Time.deltaTime > 1f)
                    {
                        ThrowAttack();
                    }

                    if (attackTimer < 0f)
                    {
                        currentState = State.Default;
                    }
                    
                    break;
                
                case State.Chase:
                    break;
            }

            // check for damage
            var currentTileData = GameMan.inst.map.GetFloor(0).GetTileData(pos.x, pos.y);
            float oldHP = hp;
            hp -= currentTileData.heat * Time.deltaTime * 2.5f;
            if ((int)oldHP != (int)hp)
            {
                // hurt
                //GameMan.inst.camShaker.ShakeOnce(15f, 3f, 0f, 0.2f);
                float ang = Random.Range(0f, 360f);
                skin.localPosition = new Vector2(Mathf.Cos(ang) * shakeDist, Mathf.Sin(ang) * shakeDist);
                shakeTimer = 0.3f;
                animator.Play("EnemyHurt");
            }
        }
    }

    private void ThrowAttack()
    {
        Projectile proj = Instantiate(attackObj, transform.position, Quaternion.identity).GetComponent<Projectile>();
        proj.Init(transform.position, PlayerCore.inst.transform.position, 1f);
    }

    private void Chase()
    {
        Vector2 playerDirection = PlayerCore.inst.transform.position - transform.position;
        float playerDistance = playerDirection.magnitude;
        playerDirection /= playerDistance;

        if (playerDistance > attackDist)
        {
            rb.velocity = playerDirection * speed;
        }
        else
        {
            currentState = State.Attack;
            attackTimer = 2f;
        }
    }
    
    private void FixedUpdate()
    {
        if (shakeTimer > 0f)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            switch (currentState)
            {
                default:
                case State.Default:
                    rb.velocity = Vector2.zero;
                    break;
                
                case State.Attack:
                    rb.velocity = Vector2.zero;
                    break;

                case State.Chase:
                    Chase();
                    break;
            }
        }
    }
}
