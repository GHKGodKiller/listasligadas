using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WaypointManager : MonoBehaviour
{
    private LinkedList<string> waypoints = new LinkedList<string>();
    private int waypointCounter = 0;

    private void Start()
    {
        AddNodeToLinkedList("Inicio", 0);
        AddNodeToLinkedList("Punto A", 1);
        AddNodeToLinkedList("Destino", 2);

        MostrarInstrucciones();
        ShowLinkedList();
    }

    private void MostrarInstrucciones()
    {
        Debug.Log(
            " Controles de los Waypoints \n" +
            "   - Tecla [Espacio]: Mostrar lista\n" +
            "   - Tecla [A]: Agregar nodo\n" +
            "   - Tecla [R]: Remover nodo\n" +
            "   - Tecla [I]: Invertir lista\n" +
            "   - Tecla [C]: Limpiar lista\n"
        );
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShowLinkedList();
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            waypointCounter++;
            AddNodeToLinkedList($"Nuevo Punto #{waypointCounter}", 1);
            ShowLinkedList();
        }
        else if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RemoveNodeFromLinkedList(1);
            ShowLinkedList();
        }
        else if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            InvertLinkList();
            ShowLinkedList();
        }
        else if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            ClearLinkList();
            ShowLinkedList();
        }
    }

    public void ShowLinkedList()
    {
        Debug.Log("Estado de la lista");
        if (waypoints.Count == 0)
        {
            Debug.Log("La lista está vacía.");
            return;
        }

        int index = 0;
        foreach (string wp in waypoints)
        {
            Debug.Log($"Nodo [{index}]: {wp}");
            index++;
        }
    }

    public void ClearLinkList()
    {
        waypoints.Clear();
        Debug.Log("[C] La lista ha sido vaciada.");
    }

    public void AddNodeToLinkedList(string value, int position)
    {
        if (position <= 0 || waypoints.Count == 0)
        {
            waypoints.AddFirst(value);
            Debug.Log($"[Add] '{value}' agregado al inicio.");
            return;
        }

        LinkedListNode<string> current = waypoints.First;
        int currentIndex = 0;

        while (current != null && currentIndex < position - 1)
        {
            current = current.Next;
            currentIndex++;
        }

        if (current != null)
        {
            waypoints.AddAfter(current, value);
            Debug.Log($"[Add] '{value}' agregado en la posición {position}.");
        }
        else
        {
            waypoints.AddLast(value);
            Debug.Log($"[Add] '{value}' agregado al final.");
        }
    }

    public void RemoveNodeFromLinkedList(int position)
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("[Remove] La lista está vacía, no se puede remover nada.");
            return;
        }

        LinkedListNode<string> current = waypoints.First;
        int currentIndex = 0;

        while (current != null && currentIndex < position)
        {
            current = current.Next;
            currentIndex++;
        }

        if (current != null)
        {
            Debug.Log($"[Remove] Se eliminó el nodo '{current.Value}' de la posición {position}.");
            waypoints.Remove(current);
        }
        else
        {
            Debug.LogWarning($"[Remove] Posición {position} fuera de rango.");
        }
    }

    public void InvertLinkList()
    {
        if (waypoints.Count <= 1)
        {
            Debug.Log("[Invert] La lista tiene 1 o 0 elementos, no es necesario invertir.");
            return;
        }

        LinkedList<string> reversedList = new LinkedList<string>();
        LinkedListNode<string> current = waypoints.Last;

        while (current != null)
        {
            reversedList.AddLast(current.Value);
            current = current.Previous;
        }

        waypoints = reversedList;
        Debug.Log("[Invert] El orden de la lista ha sido invertido.");
    }
}