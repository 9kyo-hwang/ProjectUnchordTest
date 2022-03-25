using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string horAxisName = "Horizontal"; // 좌우(left-right)
    public string verAxisName = "Vertical"; // 상하(top-bottom)
    public string jumpButtonName = "Jump"; // 점프 버튼
    public string dashButtonName = "Fire1"; // 대시 버튼, Left Ctrl
    public string glideButtonName = "Fire2"; // 글라이딩 버튼, Left Alt
    public string runButtonName = "Fire3"; // 달리기 버튼, Left Shift

    public float horInput { get; private set; } // 좌우 입력 구분
    public bool isHorInput { get; private set; } // 좌우 입력 했는 지 안했는 지
    public float verInput { get; private set; } // 감지된 상하 입력값
    public bool isVerInput { get; private set; } // 상하 입력 했는 지 안했는 지
    public bool jumpButtonDown { get; private set; } // 점프 키 다운 시
    public bool jumpButton { get; private set; } // 점프 키 다운 중
    public bool jumpButtonUp { get; private set; } // 점프 키 업 시
    public bool dash { get; private set; } // 대시 버튼 눌렀나 안눌렀나
    public bool glide { get; private set; } // 글라이딩 버튼 누르는 중인가 아닌가
    public bool run { get; private set; } // 달리기 버튼 누르는 중인가 아닌가

    void Update()
    {
        horInput = Input.GetAxisRaw(horAxisName);
        isHorInput = Input.GetButton(horAxisName);
        verInput = Input.GetAxisRaw(verAxisName);
        isVerInput = Input.GetButton(verAxisName);
        jumpButtonDown = Input.GetButtonDown(jumpButtonName);
        jumpButton = Input.GetButton(jumpButtonName);
        jumpButtonUp = Input.GetButtonUp(jumpButtonName);
        dash = Input.GetButtonDown(dashButtonName);
        glide = Input.GetButton(glideButtonName);
        run = Input.GetButton(runButtonName);
    }
}
