using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour
{

    public AudioClip UpStrum;
    public AudioClip DownStrum;

    public int BeatsPerMeasure = 4;
    public int StrumsPerBeat = 2;
    public float BeatsPerMinute = 120f;
    public float UpStrumOffset = -0.125f;

    private float MeasureLength;
    private float MeasureTime;
    private int MeasureNumber;
    private int StrumNumber;

    Strum[] thisStrum;
    Strum[] nextStrum;

    //private float NextStrum;
    //private bool WasUp = true;



    private struct Strum
    {
        public bool play;
        public bool UpStrum;
        public float offset;
        public float volume;
    }

    Strum[] GenerateStrumPattern(int beats, float bpm)
    {
        int strums = beats * StrumsPerBeat;
        Strum[] pattern = new Strum[strums];
        float beatLength = 60f/bpm;

        for(int i = 0; i < strums; i++)
        {
            pattern[i].play = true;
            if( i % 2 == 0)
            {
                pattern[i].UpStrum = false;
                pattern[i].offset = 0f;
            }
            else
            {
                pattern[i].UpStrum = true;
                pattern[i].offset = UpStrumOffset * (beatLength/StrumsPerBeat);
            }
            pattern[i].volume = (i == 0) ? 1.0f:0.7f;
            Debug.Log(i + " volume: " + pattern[i].volume);
        }

        return pattern;
    }

    void CalcMeasureLength()
    {
        MeasureLength = 60f/BeatsPerMinute * (float)BeatsPerMeasure;
    }

    // Use this for initialization
    void Start ()
    {
        //NextStrum = Time.time + 0.2f;
        CalcMeasureLength();
        MeasureTime = 0f;
        thisStrum = GenerateStrumPattern(BeatsPerMeasure, BeatsPerMinute);
        nextStrum = null;
        MeasureNumber = 0;
        StrumNumber = 0;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    void CheckNewMeasure()
    {
        if(MeasureTime > 0.8f*MeasureLength && null == nextStrum )
        {
            nextStrum = GenerateStrumPattern(BeatsPerMeasure, BeatsPerMinute);
        }

        if(MeasureTime >= MeasureLength)
        {
            thisStrum = nextStrum;
            nextStrum = null;
            MeasureTime -= MeasureLength;
            CalcMeasureLength();
            MeasureNumber++;
            StrumNumber = 0;
        }
    }

    void CheckDoStrum()
    {
        if (StrumNumber > thisStrum.GetUpperBound(0)) return;
        float strumLength = MeasureLength / ((float)BeatsPerMeasure * StrumsPerBeat);
        if(MeasureTime >= StrumNumber * strumLength + thisStrum[StrumNumber].offset)
        {
            if(thisStrum[StrumNumber].play)
            {
                AudioSource audio = gameObject.GetComponent<AudioSource>();
                if (thisStrum[StrumNumber].UpStrum)
                {
                    //play upstrum
                    audio.PlayOneShot(UpStrum, thisStrum[StrumNumber].volume);
                    Debug.Log("up " + audio.isPlaying);
                }
                else
                {
                    //play downstrum
                    audio.PlayOneShot(DownStrum, thisStrum[StrumNumber].volume);
                    Debug.Log("down " + audio.isPlaying);
                }

            }
            StrumNumber++;
        }
    }

    void FixedUpdate()
    {
        MeasureTime += Time.fixedDeltaTime;

        CheckNewMeasure();
        CheckDoStrum();

        //if (Time.time > NextStrum)
        //{
        //    AudioSource audio = gameObject.GetComponent<AudioSource>();
        //    NextStrum += 0.25f;
        //    if(WasUp)
        //    {
        //        //play downstrum
        //        audio.PlayOneShot(DownStrum);
        //        Debug.Log("down " + audio.isPlaying);
        //    }
        //    else
        //    {
        //        if(Random.value < 0.5f)
        //        //play upstrum
        //        audio.PlayOneShot(UpStrum);
        //        Debug.Log("up " + audio.isPlaying);
        //    }
        //    WasUp = !WasUp;
        //}
    }
}
