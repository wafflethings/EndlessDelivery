using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ULTRAKILL/Endless Enemy Data")]
public class EndlessEnemy : ScriptableObject
{
	// Token: 0x04000668 RID: 1640
	public EnemyType enemyType;

	// Token: 0x04000669 RID: 1641
	public GameObject prefab;

	// Token: 0x0400066A RID: 1642
	public int spawnCost;

	// Token: 0x0400066B RID: 1643
	public int spawnWave;

	// Token: 0x0400066C RID: 1644
	public int costIncreasePerSpawn;
}
