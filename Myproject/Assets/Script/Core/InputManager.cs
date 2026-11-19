using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    instance = go.AddComponent<InputManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    
    public bool IsJump { get; private set; } // Space (Jump)
    public bool IsDash { get; private set; } // LeftShift (Dash - Down)
    public bool IsRun { get; private set; } // LeftShift (Run - Hold)
    
    public bool IsFire { get; private set; } // Mouse 0 (Fire - Hold)
    public bool IsFireUp { get; private set; } // Mouse 0 (Fire - Up)
    
    public bool IsMoveKey { get; private set; } // W, A, S, D

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // GetAxis는 키를 떼도 값이 서서히 0이 되어 약 0.3~0.5초 미끄러진다. 이동은 즉시 반응하도록 Raw 값을 쓰고,
        // 애니메이션의 부드러운 전환은 Player에서 Animator 감쇠로 처리한다.
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");
        
        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");

        IsJump = Input.GetButton("Jump"); 
        
        IsDash = Input.GetKeyDown(KeyCode.LeftShift);
        IsRun = Input.GetKey(KeyCode.LeftShift);

        IsFire = Input.GetMouseButton(0);
        IsFireUp = Input.GetMouseButtonUp(0);

        IsMoveKey = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                    Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }
}
