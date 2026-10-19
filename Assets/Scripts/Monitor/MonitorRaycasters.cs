using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonitorGraphicRaycaster : GraphicRaycaster
{
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (!MonitorScreen.Active)
        {
            base.Raycast(eventData, resultAppendList);
            return;
        }

        if (!MonitorScreen.OnGlass)
        {
            return;
        }

        Vector2 original = eventData.position;
        eventData.position = CameraPixels(eventCamera);
        base.Raycast(eventData, resultAppendList);
        eventData.position = original;
    }

    public static Vector2 CameraPixels(Camera cam)
    {
        if (cam == null)
        {
            return MonitorScreen.Pointer;
        }

        return new Vector2(MonitorScreen.Viewport.x * cam.pixelWidth, MonitorScreen.Viewport.y * cam.pixelHeight);
    }
}

public class MonitorPhysics2DRaycaster : Physics2DRaycaster
{
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (!MonitorScreen.Active)
        {
            base.Raycast(eventData, resultAppendList);
            return;
        }

        if (!MonitorScreen.OnGlass || eventCamera == null)
        {
            return;
        }

        Ray ray = eventCamera.ViewportPointToRay(new Vector3(MonitorScreen.Viewport.x, MonitorScreen.Viewport.y, 0f));
        float distance = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, distance, finalEventMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i].collider;
            if (collider == null)
            {
                continue;
            }

            SpriteRenderer sprite = collider.GetComponent<SpriteRenderer>();
            resultAppendList.Add(new RaycastResult
            {
                gameObject = collider.gameObject,
                module = this,
                distance = hits[i].distance,
                worldPosition = hits[i].point,
                worldNormal = hits[i].normal,
                screenPosition = eventData.position,
                index = resultAppendList.Count,
                sortingLayer = sprite != null ? sprite.sortingLayerID : 0,
                sortingOrder = sprite != null ? sprite.sortingOrder : 0
            });
        }
    }
}
