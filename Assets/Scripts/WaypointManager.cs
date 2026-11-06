using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    [Header("Nodos Predefinidos en Escena")]
    public Transform[] nodosIniciales = new Transform[5];

    [Header("Prefab y Generación")]
    public GameObject nodePrefab;
    public Vector2 areaGeneracion = new Vector2(8f, 4f);

    [Header("Validación de Espacio Seguro")]
    [Tooltip("Arrastra aquí los mismos obstáculos que tiene el Agente")]
    public Transform[] obstaculosEscena;
    [Tooltip("Debe ser mayor al 'Obstacle Radius' de tu Agente")]
    public float distanciaMinimaObstaculo = 2.5f; 

    public LinkedList<Transform> waypoints = new LinkedList<Transform>();

    private void Start()
    {
        foreach (Transform nodo in nodosIniciales)
        {
            if (nodo != null) waypoints.AddLast(nodo);
        }
        ShowLinkedList();
    }

    public void ShowLinkedList()
    {
        Debug.Log("");
        if (waypoints.Count == 0)
        {
            Debug.Log("");
            return;
        }

        int index = 0;
        foreach (Transform wp in waypoints)
        {
            Debug.Log($"Posición [{index}]: {wp.name}");
            index++;
        }
    }

    public void ClearLinkList()
    {
        // Opcional: Si quieres que al limpiar la lista también se destruyan TODOS los nodos físicos
        // foreach (Transform wp in waypoints) { if (wp != null) Destroy(wp.gameObject); }
        
        waypoints.Clear();
        Debug.Log("[CLEAR] Se vació la lista ligada.");
        ShowLinkedList();
    }

    public void AddNodeToLinkedList()
    {
        if (nodePrefab == null)
        {
            Debug.LogWarning("");
            return;
        }

        Vector2 randomPos = Vector2.zero;
        bool posicionSegura = false;
        int intentos = 0;
        int maxIntentos = 30; 

        while (!posicionSegura && intentos < maxIntentos)
        {
            randomPos = new Vector2(
                Random.Range(-areaGeneracion.x, areaGeneracion.x), 
                Random.Range(-areaGeneracion.y, areaGeneracion.y)
            );
            
            posicionSegura = true; 

            if (obstaculosEscena != null)
            {
                foreach (Transform obstaculo in obstaculosEscena)
                {
                    if (obstaculo != null)
                    {
                        float distancia = Vector2.Distance(randomPos, obstaculo.position);
                        if (distancia < distanciaMinimaObstaculo)
                        {
                            posicionSegura = false; 
                            break; 
                        }
                    }
                }
            }
            intentos++;
        }

        if (!posicionSegura)
        {
            Debug.LogWarning("");
            return;
        }

        GameObject newNode = Instantiate(nodePrefab, randomPos, Quaternion.identity);
        newNode.name = $"Nodo_Extra_{waypoints.Count}";

        if (waypoints.Count > 0)
        {
            waypoints.AddAfter(waypoints.First, newNode.transform);
        }
        else
        {
            waypoints.AddFirst(newNode.transform);
        }
        
        Debug.Log($"[ADD] {newNode.name} instanciado en posición segura {randomPos}.");
        ShowLinkedList();
    }

    public void RemoveNodeFromLinkedList()
    {
        if (waypoints.Count > 0)
        {
            Transform removedNode = waypoints.First.Value;
            waypoints.RemoveFirst();
            
            if (removedNode != null)
            {
                Destroy(removedNode.gameObject);
                Debug.Log($"[REMOVE] El nodo {removedNode.name} fue sacado de la lista");
            }
        }
        else
        {
            Debug.LogWarning("[REMOVE] La lista ya está vacía.");
        }
        ShowLinkedList();
    }

    public void InvertLinkList()
    {
        if (waypoints.Count <= 1) return;

        LinkedList<Transform> reversedList = new LinkedList<Transform>();
        LinkedListNode<Transform> current = waypoints.Last;

        while (current != null)
        {
            reversedList.AddLast(current.Value);
            current = current.Previous;
        }

        waypoints = reversedList;
        Debug.Log("[INVERT] El orden de los nodos se ha invertido.");
        ShowLinkedList();
    }
}