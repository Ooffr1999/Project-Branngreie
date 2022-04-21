using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public Transform pfHealthBar;

    private void Start()
    {
        HealthSystem healthSystem = new HealthSystem(100);

       Transform healthBarTransform = Instantiate(pfHealthBar, new Vector3(0, 10), Quaternion.identity);
        HealthBar healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem);

        Debug.Log("health: " + healthSystem.GetHealthPercent());
        healthSystem.Damage(10);
        Debug.Log("health: " + healthSystem.GetHealthPercent());

    }
}
