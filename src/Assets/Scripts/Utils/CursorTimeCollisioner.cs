using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorTimeCollisioner : MonoBehaviour {

    [SerializeField]
    private OhShapeEditor _editor;  

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_editor.AudioManager.IsPlaying())
        {
            if (other.gameObject.tag != "WallToggle") return;
            WallObject wallObject = other.gameObject.GetComponentInParent<WallObject>();
            _editor.OnWallObjectClicked(wallObject);
        }
    }
}
