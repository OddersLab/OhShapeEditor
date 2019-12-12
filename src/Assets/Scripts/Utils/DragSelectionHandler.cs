using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragSelectionHandler : MonoBehaviour {

    [SerializeField]
    Image SelectionBoxImage;

    public RectTransform Container;
    Vector2 startPosition;

    #region Drag Behaviour
    public void OnBeginDrag(BaseEventData eventData)
    {
        SelectionBoxImage.gameObject.SetActive(true);
        startPosition = getScreenPointToLocalPointInRectangle(eventData);
    }

    public void OnDrag(BaseEventData eventData)
    {
        Rect container = Container.rect;
        getScreenPointToLocalPointInRectangle(eventData);

        Vector2 squareSize = new Vector2(Mathf.Abs(Input.mousePosition.x) - Mathf.Abs(startPosition.x), Mathf.Abs(Input.mousePosition.y) - Mathf.Abs(startPosition.y));
        Vector2 newPosition = getScreenPointToLocalPointInRectangle(eventData);

        float x = getPositionInsideElement1D(newPosition.x, container.width);
        float y = getPositionInsideElement1D(newPosition.y, container.height);

        Vector2 endPosition = new Vector2(x, y);
        endPosition = GetPosition(endPosition, container);

        Vector2 centre = (startPosition + endPosition) / 2.0f;

        SelectionBoxImage.rectTransform.localPosition = centre;
        SelectionBoxImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(startPosition.x - endPosition.x), Mathf.Abs(startPosition.y - endPosition.y));
    }

    public List<WallObject> GetWallObjectsInsideTheSelectionBox(BaseEventData eventData, List<WallObject> wallObjects)
    {
        List<WallObject> objectsInsideTheBox = new List<WallObject>();

        if (!SelectionBoxImage.gameObject.activeSelf) return objectsInsideTheBox; // if Mouse button is not right

        if (Input.GetMouseButtonUp(1)) SelectionBoxImage.gameObject.SetActive(false);

        foreach (WallObject wallObject in wallObjects)
        {
            GameObject wall = wallObject.GetComponentInChildren<Toggle>().gameObject;

            if (isObjectInsideDragBox(SelectionBoxImage.rectTransform, wall.transform.position, wall.GetComponent<RectTransform>().rect.size, Container.GetComponentInParent<Canvas>().worldCamera))
            {
                objectsInsideTheBox.Add(wallObject);
            }

        }
        return objectsInsideTheBox;
    }
    #endregion

    #region Utils

    private Vector2 getScreenPointToLocalPointInRectangle(BaseEventData eventData)
    {
        Vector2 rectPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Container, Input.mousePosition, Container.GetComponentInParent<Canvas>().worldCamera, out rectPosition);
        return rectPosition;
    }

    private float getPositionInsideElement1D(float position, float limit)
    {
        if (Mathf.Abs(position) >= limit)
        {
            return position < 0 ? -limit : limit;
        }
        return position;
    }

    public Vector2 GetPosition(Vector2 worldPosition, Rect container)
    {
        float x = getPositionInsideElement1D(worldPosition.x, container.width / 2);
        float y = getPositionInsideElement1D(worldPosition.y, container.height / 2);
        return new Vector2(x, y);
    }

    private bool getRectangleContainsScreenPoint(RectTransform rectTransform, Vector2 position, Camera camera) {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, position, camera);
    }

    private bool isObjectInsideDragBox(RectTransform rectTransform, Vector2 position, Vector2 size, Camera camera)
    {
        return getRectangleContainsScreenPoint(rectTransform, position, camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x + size.x / 2, position.y + size.y / 2), camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x, position.y + size.y), camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x - size.x / 2, position.y + size.y), camera)
            /* Corners */
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x + size.x / 2, position.y + size.y), camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x + size.x / 2, position.y), camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x - size.x / 2, position.y + size.y), camera)
            || getRectangleContainsScreenPoint(rectTransform, new Vector2(position.x - size.x / 2, position.y), camera);
    }

    #endregion
}
