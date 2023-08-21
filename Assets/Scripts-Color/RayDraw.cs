using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-1)]
public class RayDraw : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Get key number
        for(int i = 1; i <= 8; i++) if(Input.GetKeyDown(KeyCode.Alpha0+i))
        {
            DrawableBehaviour.colorIndex = (ColorIndex)(i-1);
            break;
        }
    
        // Draw with mouse
        if(Input.GetMouseButton((int)MouseButton.Left))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray.origin, ray.direction, out RaycastHit info, 20.0f))
            {
                var collider = info.collider;
                var drawable = collider.GetComponent<DrawableBehaviour>();
                if(drawable == null) return;

                // DebugExtension.DebugWireSphere(info.point, Color.red, 1, 0.5f);
                drawable.Draw(DrawableBehaviour.Colors[(int)DrawableBehaviour.colorIndex], info.textureCoord, 0.5f);
            }
        }
        else if(Input.GetMouseButton((int)MouseButton.Right))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray.origin, ray.direction, out RaycastHit info, 20.0f))
            {
                var collider = info.collider;
                var drawable = collider.GetComponent<DrawableBehaviour>();
                if(drawable == null) return;

                // DebugExtension.DebugWireSphere(info.point, Color.red, 1, 0.5f);
                drawable.Erase(info.textureCoord, 0.5f);
            }
        }

        // Update Counter
        {
            int[] colorCnt = new int[DrawableBehaviour.NumColors];
            DrawableBehaviour.ColorCountBuffer.GetData(colorCnt);

            string s = $"{DrawableBehaviour.ColorStrings[(int)DrawableBehaviour.colorIndex]}:\n";
            foreach(var cnt in colorCnt) s += $"{cnt}\n";
            FindObjectOfType<TMP_Text>().text = s;
        }
    }
}
