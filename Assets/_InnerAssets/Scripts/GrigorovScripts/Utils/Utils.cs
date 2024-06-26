using UnityEngine;

public static class Utils
{
    public static Vector3 RandomPointInside(this Collider collider)
    {
        Bounds spawnBounds = collider.bounds;
        return new Vector3
        {
            x = Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            y = Random.Range(spawnBounds.min.y, spawnBounds.max.y),
            z = Random.Range(spawnBounds.min.z, spawnBounds.max.z)
        };
    }
}
