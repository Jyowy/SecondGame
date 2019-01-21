using UnityEngine;
using UnityEngine.UI;

public class Utils {
    
    static private void Append2DigitTimeToString(ref string timeStr, float time)
    {
        if (time < 10.0f) timeStr += "0";
        timeStr += time.ToString("F0");
    }

    static public void TimeToString(float time, ref Text text, float dsecondsSize = 1.0f)
    {
        float minutes = Mathf.Floor(time / 60.0f);
        float seconds = Mathf.Floor((time - minutes * 60.0f));
        float dseconds = Mathf.Floor((time - minutes * 60.0f - seconds) * 100.0f);

        string strTime = "";
        Append2DigitTimeToString(ref strTime, minutes);
        strTime += ":";
        Append2DigitTimeToString(ref strTime, seconds);
        strTime += "<size=" + (text.fontSize * dsecondsSize).ToString("F0") + ">:";
        Append2DigitTimeToString(ref strTime, dseconds);
        strTime += "</size>";

        text.text = strTime;
    }

}