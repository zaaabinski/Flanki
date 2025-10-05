using UnityEngine;
using UnityEngine.UI;
public class PingPongThrowSlider : MonoBehaviour
{
    private Slider slider;
    public bool moveSlider;
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(moveSlider)
            slider.value = Mathf.PingPong(Time.time * 1, 1f);
    }
}
