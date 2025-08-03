using UnityEngine;
using System.Collections.Generic;

public class ExplosionPool : Singleton<ExplosionPool>
{
    private ObjectPool<Explosion> _pool;
    public List<Explosion> ExplosionPrefabList;
    public int PoolSize = 100;

    protected override void Awake()
    {
        base.Awake();
        _pool = new ObjectPool<Explosion>(ExplosionPrefabList, PoolSize, transform);
    }

    public Explosion Get(string name)
    {
        return _pool.Get(name);
    }

    public void Return(string name, Explosion explosion)
    {
        _pool.Return(name, explosion);
    }
}
