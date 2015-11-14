using UnityEngine;
//using System.Collections;
using ProcJammer;

[RequireComponent(typeof (AudioSource))]
public class Music : MonoBehaviour
{

    public int BeatsPerMeasure = 4;
    public int StrumsPerBeat = 2;
    public float BeatsPerMinute = 120f;
    public float UpStrumOffset = -0.125f;
    public float MissUpStrumChance = 0.25f;
    public float MissDownStrumChance = 0.05f;
    public float OffBeatVolume = 0.6f;

    public UnityEngine.Audio.AudioMixerGroup OutputTo;

    private float MeasureLength;
    private float MeasureTime;
    private int MeasureNumber;
    private bool NewMeasurePrepped = false;


    System.Collections.Generic.List<Instrument> m_Instruments = new System.Collections.Generic.List<Instrument>();


    Chord LoadChord(string name)
    {
        AudioClip[] ChordSegments = new AudioClip[2];
        ChordSegments[0] = Resources.Load<AudioClip>("sound/chords/" + name.ToLower() + "_down");
        ChordSegments[1] = Resources.Load<AudioClip>("sound/chords/" + name.ToLower() + "_up");
        return new Chord(name, ChordSegments );

    }

    void CreateInstruments()
    {
        Instrument guitar = new Instrument("rhythmGuitar", gameObject.GetComponent<AudioSource>());

        Chord[] C_Chords = new Chord[6];
        C_Chords[0] = LoadChord("C");
        C_Chords[1] = LoadChord("Dm");
        C_Chords[2] = LoadChord("Em");
        C_Chords[3] = LoadChord("F");
        C_Chords[4] = LoadChord("G");
        C_Chords[5] = LoadChord("Am");

        guitar.AddKey("C", C_Chords);

        Chord[] Am_Chords = new Chord[6];
        Am_Chords[0] = LoadChord("Am");
        Am_Chords[1] = LoadChord("C");
        Am_Chords[2] = LoadChord("Dm");
        Am_Chords[3] = LoadChord("Em");
        Am_Chords[4] = LoadChord("F");
        Am_Chords[5] = LoadChord("G");

        guitar.AddKey("Am", Am_Chords);

        SongPartSpec[] sps = new SongPartSpec[3];
        sps[0].Name = "verse";
        sps[0].MinChords = 4;
        sps[0].MaxChords = 4;
        sps[1].Name = "chorus";
        sps[1].MinChords = 3;
        sps[1].MaxChords = 5;
        sps[2].Name = "bridge";
        sps[2].MinChords = 2;
        sps[2].MaxChords = 4;
        guitar.GenerateSongParts(sps);
        guitar.InitWithSegment("verse");

        m_Instruments.Add(guitar);
    }

    void SetInstrumentValues(Instrument i)
    {
        i.m_BeatsPerMeasure = BeatsPerMeasure;
        i.m_StrumsPerBeat = StrumsPerBeat;
        i.m_BeatsPerMinute = BeatsPerMinute;
        i.m_UpStrumOffset = UpStrumOffset;
        i.m_MissUpStrumChance = MissUpStrumChance;
        i.m_MissDownStrumChance = MissDownStrumChance;
        i.m_OffBeatVolume = OffBeatVolume;
    }

    void CalcMeasureLength()
    {
        MeasureLength = 60f/BeatsPerMinute * BeatsPerMeasure;
    }

    // Use this for initialization
    void Start ()
    {
        CreateInstruments();
        CalcMeasureLength();
        MeasureNumber = 0;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    void CheckNewMeasure()
    {

        if (MeasureTime >= 1f)//MeasureLength)
        {
            MeasureTime -= 1f;// MeasureLength;
            MeasureNumber++;
            foreach (Instrument i in m_Instruments)
            {
                i.StartNextMeasure();
            }
            NewMeasurePrepped = false;
        }
    }


    void FixedUpdate()
    {
        CalcMeasureLength();
        MeasureTime += Time.fixedDeltaTime/MeasureLength;

        if (MeasureTime > 0.8f && !NewMeasurePrepped) // * MeasureLength && false == NewMeasurePrepped)
        {
            foreach (Instrument i in m_Instruments)
            {
                SetInstrumentValues(i);
                i.PrepNextMeasure("verse");
            }
            NewMeasurePrepped = true;
        }

        CheckNewMeasure();

        foreach (Instrument i in m_Instruments)
        {
            i.CheckDoStrum(MeasureTime);
        }

    }
}
