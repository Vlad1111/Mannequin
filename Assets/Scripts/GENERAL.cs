using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class DictionaryKeyValue<Key, Value>
{
    public Key key;
    public Value value;

    public DictionaryKeyValue(Key key, Value value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class DictionaryKeyValues<key, Value>
{
    public DictionaryKeyValue<key, Value>[] keyValues;

    public DictionaryKeyValues(DictionaryKeyValue<key, Value>[] keyValues)
    {
        this.keyValues = keyValues;
    }
}


public class GENERAL
{
    public static DictionaryKeyValue<string, float>[] GetObjectSliderValues(CharacterCreation creator, bool includePoseSliders = true)
    {
        var allSliders = creator.sliderValues.Select(x => new DictionaryKeyValue<string, float>(x.Key, x.Value));

        if (!includePoseSliders)
            return allSliders.Where(x => creator.poseSliderDatas.FirstOrDefault(y => y.name == x.key) == null).ToArray();
        return allSliders.ToArray();
    }
}
