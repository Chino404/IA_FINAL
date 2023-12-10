using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Lider liderAzul;
    public Lider liderRojo;
    public List<Secuaz> secuazAzul = new List<Secuaz>();
    public List<Secuaz> secuazRojo = new List<Secuaz>();


    public LayerMask maskWall;

    [Range(0, 5f)]
    public float weightSeparation = 4f; //El peso que va a tener cada metodo. Cual quiero que sea mas prioritario

    /// <summary>
    /// Si hay algo entre medio del Raycast
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool InLineOfSight(Vector3 start, Vector3 end)
    {
        var dir = end - start;

        return !Physics.Raycast(start, dir, dir.magnitude, maskWall); //Si no hay ningun objeto de con la layer "maskWall" entonces quiere decir que estoy viendo a mi objetico (por eso lo invierto para que me de True)
    }
}
