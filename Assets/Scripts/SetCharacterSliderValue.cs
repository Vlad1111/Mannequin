using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetCharacterSliderValue : MonoBehaviour
{
    public string sliderName;
    [Range(-1f, 3f)]
    public float sliderValue;

    private CharacterCreation creator;
    // Start is called before the first frame update
    void Start()
    {
        creator = GetComponent<CharacterCreation>();
    }

    // Update is called once per frame
    void Update()
    {
        if(creator.poseSliderDatas.FirstOrDefault(x => x.name == sliderName) != null)
            creator.ChangeSliderValue(sliderName, sliderValue);
    }
}
