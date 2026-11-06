using UnityEngine;
using System.Collections.Generic;

public class AgentController : MonoBehaviour
{
    public enum AgentState
    {
        Idle,
        StartRoute,
        ChangeRoute,
        Stop
    }

    [Header("Máquina de Estados")]
    public AgentState currentState = AgentState.Idle;
    public WaypointManager waypointManager;
    private LinkedListNode<Transform> currentNode;

    [Header("Físicas de Movimiento")]
    public float maxVelocity = 5f;
    public float maxForce = 5f;
    public float mass = 1f;
    private Vector2 velocity;

    [Header("Ajustes de Llegada (Arrival)")]
    public float slowingRadius = 4f;
    public float stoppingDistance = 0.5f;

    [Header("Evasión de Obstáculos (Collision Avoidance)")]
    public float maxSeeAhead = 4f;
    public float obstacleRadius = 1.5f;
    public Transform[] customObstacles;

    private void Update()
    {
        Vector2 position = transform.position;
        Vector2 desiredVelocity = Vector2.zero;

        switch (currentState)
        {
            case AgentState.Idle:
                velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 5f); 
                break;

            case AgentState.StartRoute:
                if (currentNode != null && currentNode.Value != null)
                {
                    Vector2 targetPos = currentNode.Value.position;
                    
                    if (currentNode.Next == null)
                    {
                        desiredVelocity = CalculateArrival(position, targetPos);
                        
                        if (Vector2.Distance(position, targetPos) <= stoppingDistance)
                        {
                            SetStateStop();
                        }
                    }
                    else
                    {
                        desiredVelocity = CalculateSeek(position, targetPos);
                        
                        if (Vector2.Distance(position, targetPos) <= stoppingDistance)
                        {
                            currentNode = currentNode.Next;
                        }
                    }
                }
                else
                {
                    SetStateChangeRoute();
                    return; 
                }
                break;

            case AgentState.ChangeRoute:
                currentNode = EncontrarNodoMasCercano();
                if (currentNode != null)
                {
                    Debug.Log("");
                    currentState = AgentState.StartRoute;
                }
                else
                {
                    SetStateStop();
                }
                return;

            case AgentState.Stop:
                desiredVelocity = Vector2.zero;
                velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 10f);
                break;
        }

        if (currentState == AgentState.StartRoute || currentState == AgentState.Stop)
        {
            Vector2 steering = desiredVelocity - velocity;

            if (currentState == AgentState.StartRoute)
            {
                steering += CalculateObstacleAvoidance(position);
            }

            steering = Vector2.ClampMagnitude(steering, maxForce);
            Vector2 acceleration = steering / mass;

            velocity = Vector2.ClampMagnitude(velocity + (acceleration * Time.deltaTime), maxVelocity);
            
            if (currentState == AgentState.Stop && velocity.magnitude < 0.1f)
            {
                velocity = Vector2.zero;
            }
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        RotateTowardsVelocity();
    }



    private Vector2 CalculateSeek(Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 desired = targetPos - currentPos;
        return desired.magnitude > 0.001f ? desired.normalized * maxVelocity : Vector2.zero;
    }

    private Vector2 CalculateArrival(Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 toTarget = targetPos - currentPos;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance) return Vector2.zero;
        else if (distance < slowingRadius)
        {
            float factor = (distance - stoppingDistance) / (slowingRadius - stoppingDistance);
            return toTarget.normalized * maxVelocity * factor;
        }
        return CalculateSeek(currentPos, targetPos);
    }

    private Vector2 CalculateObstacleAvoidance(Vector2 currentPos)
    {
        if (customObstacles == null || customObstacles.Length == 0) return Vector2.zero;

        Vector2 forward = GetForwardVector();
        float dynamicSeeAhead = (velocity.magnitude / maxVelocity) * maxSeeAhead;
        
        Vector2 ahead = currentPos + forward * dynamicSeeAhead;
        Vector2 ahead2 = currentPos + forward * (dynamicSeeAhead * 0.5f);

        Vector2 mostThreateningPos = Vector2.zero;
        bool threatFound = false;
        float minDistance = float.MaxValue;

        foreach (Transform obstacle in customObstacles)
        {
            if (obstacle == null) continue;
            Vector2 obsPos = obstacle.position;

            bool isInsideAhead = Vector2.Distance(ahead, obsPos) <= obstacleRadius;
            bool isInsideAhead2 = Vector2.Distance(ahead2, obsPos) <= obstacleRadius;
            bool isInsideCenter = Vector2.Distance(currentPos, obsPos) <= obstacleRadius;

            if (isInsideAhead || isInsideAhead2 || isInsideCenter)
            {
                float dist = Vector2.Distance(currentPos, obsPos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    mostThreateningPos = obsPos;
                    threatFound = true;
                }
            }
        }

        if (threatFound)
        {
            Vector2 avoidanceForce = ahead - mostThreateningPos;
            return avoidanceForce.normalized * maxForce;
        }
        return Vector2.zero;
    }


    private Vector2 GetForwardVector()
    {
        return velocity.sqrMagnitude > 0.001f ? velocity.normalized : (Vector2)transform.up;
    }

    private void RotateTowardsVelocity()
    {
        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }


    public void SetStateIdle()
    {
        currentState = AgentState.Idle;
    }

    public void SetStateStartRoute()
    {
        if (waypointManager != null && waypointManager.waypoints.Count > 0)
        {
            currentNode = waypointManager.waypoints.First;
            currentState = AgentState.StartRoute;
        }
        else
        {
            Debug.LogWarning("");
        }
    }

    public void SetStateChangeRoute()
    {
        currentState = AgentState.ChangeRoute;
    }

    public void SetStateStop()
    {
        currentState = AgentState.Stop;
    }

    private LinkedListNode<Transform> EncontrarNodoMasCercano()
    {
        if (waypointManager == null || waypointManager.waypoints.Count == 0) return null;

        LinkedListNode<Transform> bestNode = waypointManager.waypoints.First;
        float minDistance = float.MaxValue;
        LinkedListNode<Transform> current = waypointManager.waypoints.First;

        while (current != null)
        {
            if (current.Value != null)
            {
                float dist = Vector2.Distance(transform.position, current.Value.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestNode = current;
                }
            }
            current = current.Next;
        }

        return bestNode;
    }
}