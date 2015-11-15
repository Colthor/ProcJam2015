using UnityEngine;
using System.Collections.Generic;
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

    private struct SongStructure
    {
        public SongStructure(string name, int len)
        {
            section = name;
            length = len;
        }
        public string section;
        public int length;
    };

    private List<SongStructure> m_SongStructure;
    private SongPartSpec[] m_SongSpec;
    private int m_PartMeasure;
    private int m_CurrentPart;

    System.Collections.Generic.List<Instrument> m_Instruments = new System.Collections.Generic.List<Instrument>();


    Chord LoadChord(string name)
    {
        AudioClip[] ChordSegments = new AudioClip[2];
        ChordSegments[0] = Resources.Load<AudioClip>("sound/chords/" + name.ToLower() + "_down");
        ChordSegments[1] = Resources.Load<AudioClip>("sound/chords/" + name.ToLower() + "_up");
        return new Chord(name, ChordSegments );

    }

    void CreateSong()
    {

        m_SongSpec = new SongPartSpec[5];
        m_SongSpec[0].Name = "verse";
        m_SongSpec[0].MinChords = 4;
        m_SongSpec[0].MaxChords = 4;
        m_SongSpec[1].Name = "chorus";
        m_SongSpec[1].MinChords = 3;
        m_SongSpec[1].MaxChords = 5;
        m_SongSpec[2].Name = "bridge";
        m_SongSpec[2].MinChords = 2;
        m_SongSpec[2].MaxChords = 4;
        m_SongSpec[3].Name = "intro";
        m_SongSpec[3].MinChords = 2;
        m_SongSpec[3].MaxChords = 4;
        m_SongSpec[4].Name = "silence";
        m_SongSpec[4].MinChords = 2;
        m_SongSpec[4].MaxChords = 4;
        m_CurrentPart = 0;
        m_PartMeasure = 0;
        m_SongStructure = new List<SongStructure>();
        m_SongStructure.Add(new SongStructure("silence", 1));
        m_SongStructure.Add(new SongStructure("intro", 4));
        m_SongStructure.Add(new SongStructure("verse", 16));
        m_SongStructure.Add(new SongStructure("chorus", 8));
        m_SongStructure.Add(new SongStructure("verse", 16));
        m_SongStructure.Add(new SongStructure("chorus", 8));
        m_SongStructure.Add(new SongStructure("bridge", 4));
        m_SongStructure.Add(new SongStructure("verse", 16));
        m_SongStructure.Add(new SongStructure("chorus", 8));
        m_SongStructure.Add(new SongStructure("silence", 1));
        foreach (Instrument i in m_Instruments)
        { 
            i.GenerateSongParts(m_SongSpec);
            i.InitWithSegment(m_SongStructure[m_CurrentPart].section);
        }
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

    public void StartNewSong()
    {
        CreateSong();
        CalcMeasureLength();
        MeasureNumber = 0;
        MeasureTime = 0.0f;
        NewMeasurePrepped = false;

        foreach (Instrument i in m_Instruments)
        {
            SetInstrumentValues(i);
            i.PrepNextMeasure(m_SongStructure[m_CurrentPart].section);
        }
    }

    // Use this for initialization
    void Start ()
    {
        CreateInstruments();
        CreateSong();
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

    void AdvanceSong()
    {
        m_PartMeasure++;
        if(m_PartMeasure >= m_SongStructure[m_CurrentPart].length)
        {
            m_PartMeasure = 0;
            m_CurrentPart = (m_CurrentPart + 1) % (m_SongStructure.Count);
            Debug.Log("Playing " + m_SongStructure[m_CurrentPart].section + " for " + m_SongStructure[m_CurrentPart].length + " bars.");
        }
    }

    void SetNextMeasure()
    {
        foreach (Instrument i in m_Instruments)
        {
            SetInstrumentValues(i);
            i.PrepNextMeasure(m_SongStructure[m_CurrentPart].section);
        }
    }

    void FixedUpdate()
    {
        CalcMeasureLength();
        MeasureTime += Time.fixedDeltaTime/MeasureLength;

        if (MeasureTime > 0.8f && !NewMeasurePrepped)
        {
            AdvanceSong();
            SetNextMeasure();
            NewMeasurePrepped = true;
        }

        CheckNewMeasure();

        foreach (Instrument i in m_Instruments)
        {
            i.CheckDoStrum(MeasureTime);
        }

    }
}
