using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ISPoint : MonoBehaviour
{
    [field: SerializeField] public string IntersectionID { get; set; }
    [field: SerializeField] public List<GameObject> IncomingSplines { get; set; }
    [field: SerializeField] public List<GameObject> OutgoingSplines { get; set; }
    [field: SerializeField] public float TriggerRadius { get; set; }
    public RouteManager TheRouteManager { get; set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(IntersectionID) && gameObject.name != IntersectionID)
        {
            gameObject.name = IntersectionID;
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        // Tetikleyiciye giren nesnenin kendisinde veya ebeveynlerinde (parent)
        // BusIdentifier bileşeni var mı diye kontrol et.
        // Bu yöntem, while döngüsünden daha temiz ve performanslıdır.
        if (other.GetComponentInParent<BusIdentifier>() != null)
        {
            // BusIdentifier bileşeni bulunduysa, bu doğru nesnedir.
            // RouteManager'a bu noktaya ulaşıldığını bildir.
            // IntersectionID string'ini gönder, ISPoint objesini değil
            TheRouteManager?.PlayerReachedPoint(this.IntersectionID);
        }
    }
}