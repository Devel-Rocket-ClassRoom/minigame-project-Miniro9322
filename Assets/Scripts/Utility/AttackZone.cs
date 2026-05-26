using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    private readonly HashSet<GameObject> hitTargets = new();

    private void OnEnable()
    {
        hitTargets.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void TryDealDamage(Collider2D collision)
    {
        if (hitTargets.Contains(collision.gameObject))
            return;

        if (collision.gameObject.CompareTag("Player") && parent.CompareTag("Boss"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
            hitTargets.Add(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Boss") && parent.CompareTag("Player"))
        {
            var damage = parent.GetComponent<IDamageable>().SetDamage();
            collision.gameObject.GetComponent<IDamageable>().GetDamage(damage);
            hitTargets.Add(collision.gameObject);
        }
    }

    public void Deactivate() => gameObject.SetActive(false);

    public void Activate()
    {
        hitTargets.Clear();
        gameObject.SetActive(true);

        // SetActive 직후 OnTriggerEnter2D가 발생 안 할 수 있어서 즉시 직접 체크
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        var results = new List<Collider2D>();
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        Physics2D.OverlapCollider(col, filter, results);
        foreach (var hit in results)
            TryDealDamage(hit);
    }
}