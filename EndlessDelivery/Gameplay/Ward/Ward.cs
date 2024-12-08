using System;
using System.Collections.Generic;
using EndlessDelivery.Components;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Ward;

public class Ward : MonoBehaviour
{
    [HideInInspector] public Present Present;
    [SerializeField] private GameObject _deathEffect;
    [SerializeField] private SphereCollider _sphereCollider;
    [SerializeField] private GameObject _lineTemplate;
    private List<EnemyIdentifier> _enemies = new();
    private List<LineRenderer> _lines = new();

    private void FixedUpdate()
    {
        if (Present == null || Present.Destroyed)
        {
            CreateDeathEffect();
            Destroy(gameObject);
            return;
        }

        transform.position = Present.transform.position;

        for (int i = 0; i < _enemies.Count; i++)
        {
            EnemyIdentifier? eid = _enemies[i];
            LineRenderer line = _lines[i];
            Vector3 centre = eid.GetCenter()?.position ?? eid.transform.position;

            if (line == null)
            {
                continue;
            }

            if (eid == null || eid.dead)
            {
                line.enabled = false;
                continue;
            }

            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, centre);
        }
    }

    // public bool InsideWard(Vector3 point)
    // {
    //     return (transform.position - point).sqrMagnitude < (Mathf.Pow(_sphereCollider.radius * transform.localScale.x, 2));
    // }

    public void AddEnemy(EnemyIdentifier eid)
    {
        LineRenderer line = Instantiate(_lineTemplate).GetComponent<LineRenderer>();
        _lines.Add(line);
        _enemies.Add(eid);
        eid.Bless();
    }

    public void RemoveEnemy(EnemyIdentifier eid)
    {
        Destroy(_lines[0].gameObject);
        _lines.RemoveAt(0);
        _enemies.Remove(eid);
        eid.Unbless();
    }

    private void OnDestroy()
    {
        GameManager.Instance.AddTime(2, "<color=#6ebae6>WARD FREED</color>");
        List<EnemyIdentifier> enemiesNew = new();
        enemiesNew.AddRange(_enemies); // Prevent collection modified tbh

        foreach (EnemyIdentifier eid in enemiesNew)
        {
            RemoveEnemy(eid);
        }

        WardManager.Instance.AllWards.Remove(this);
    }

    private void CreateDeathEffect()
    {
        _deathEffect.transform.parent = null;
        _deathEffect.SetActive(true);
    }
}
