using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string horAxisName = "Horizontal"; // �¿�(left-right)
    public string verAxisName = "Vertical"; // ����(top-bottom)
    public string jumpButtonName = "Jump"; // ���� ��ư
    public string dashButtonName = "Fire1"; // ��� ��ư, Left Ctrl
    public string glideButtonName = "Fire2"; // �۶��̵� ��ư, Left Alt
    public string runButtonName = "Fire3"; // �޸��� ��ư, Left Shift

    public float horInput { get; private set; } // �¿� �Է� ����
    public bool isHorInput { get; private set; } // �¿� �Է� �ߴ� �� ���ߴ� ��
    public float verInput { get; private set; } // ������ ���� �Է°�
    public bool isVerInput { get; private set; } // ���� �Է� �ߴ� �� ���ߴ� ��
    public bool jumpButtonDown { get; private set; } // ���� Ű �ٿ� ��
    public bool jumpButton { get; private set; } // ���� Ű �ٿ� ��
    public bool jumpButtonUp { get; private set; } // ���� Ű �� ��
    public bool dash { get; private set; } // ��� ��ư ������ �ȴ�����
    public bool glide { get; private set; } // �۶��̵� ��ư ������ ���ΰ� �ƴѰ�
    public bool run { get; private set; } // �޸��� ��ư ������ ���ΰ� �ƴѰ�

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
