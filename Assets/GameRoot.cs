using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public float step_timer = 0.0f; // 경과 시간을 유지

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.step_timer += Time.deltaTime; // 경과 시간을 더함
    }

    public float getPlayTime()
    {
        float time;
        time = this.step_timer;
        return (time); // 호출한 곳에 경과 시간을 알려줌
    }
}