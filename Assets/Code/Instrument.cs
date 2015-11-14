using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniqueRandomItem;


namespace ProcJammer
{
    struct Strum
    {
        public bool play;
        public int Segment;
        public float time;
        public float volume;
    }

    class Chord
    {
        public Chord(string chord_name, AudioClip[] chord_segments)
        {
            name = chord_name;
            segments = chord_segments;
        }
        public string name;
        public AudioClip[] segments;
    }

    struct SongPartSpec
    {
        public string Name;
        public int MinChords;
        public int MaxChords;
    }

    struct SongPart
    {
        public SongPart(Strum[] strum_pattern, int[] chord_prog)
        {
            StrumPattern = strum_pattern;
            ChordProgression = chord_prog;
        }
        public Strum[] StrumPattern;
        public int[] ChordProgression;
    }

    class Instrument
    {
        AudioSource m_audio;

        public int m_BeatsPerMeasure = 4;
        public int m_StrumsPerBeat = 2;
        public float m_BeatsPerMinute = 120f;
        public float m_UpStrumOffset = -0.125f;
        public float m_MissUpStrumChance = 0.25f;
        public float m_MissDownStrumChance = 0.05f;
        public float m_OffBeatVolume = 0.6f;
        public int m_ChordsPerKey = 6;

        private string m_name;

        //private float m_MeasureLength;
        //private float m_MeasureTime;
        private int m_MeasureNumber;
        private int m_StrumNumber;
        private string m_CurrentKey = "C";
        private int m_CurrentChordIndex = 0;
        private Chord m_CurrentChord;
        private string m_CurrentSegment = "";
        private int[] m_ChordProgression = { 0, 4, 5, 3 };
        private int m_ChordProgIndex = 0;
        private bool m_ResetChordProg = false;

        private Dictionary<string, Chord[]> m_keys = new Dictionary<string, Chord[]>();
        private Dictionary<string, SongPart> m_songParts = new Dictionary<string, SongPart>();


        Strum[] m_thisStrum;
        Strum[] m_nextStrum;

        private Instrument()
        {

        }

        public Instrument(string instrumentName, AudioSource AudioToUse)
        {

            Debug.Log(m_name + " created");
            m_audio = AudioToUse;
            m_name = instrumentName;
            //CalcMeasureLength();
            m_thisStrum = GenerateStrumPattern(m_BeatsPerMeasure);//, m_BeatsPerMinute);
            m_nextStrum = null;
            m_MeasureNumber = 0;
            m_StrumNumber = 0;

        }

        public void GenerateSongParts(SongPartSpec[] specs)
        {
            foreach(SongPartSpec sps in specs)
            {
                Strum[] strumPattern = GenerateStrumPattern(m_BeatsPerMeasure);
                int[] chordPattern = GenerateChordPattern(sps.MinChords, sps.MaxChords);

                m_songParts.Add(sps.Name, new SongPart(strumPattern, chordPattern));
            }
        }

        public void InitWithSegment(string segment)
        {
            m_CurrentSegment = segment;
            m_CurrentChordIndex = 0;
            m_ChordProgIndex = 0;
            m_thisStrum = m_songParts[m_CurrentSegment].StrumPattern;
            m_ChordProgression = m_songParts[m_CurrentSegment].ChordProgression;
            m_CurrentChordIndex = m_ChordProgression[m_ChordProgIndex];
        }

        public void AddKey(string keyName, Chord[] chords)
        {
            Debug.Log(m_name + " AddKey( " + keyName + " )");
            m_keys.Add(keyName, chords);
        }

        int[] GenerateChordPattern(int minChords, int maxChords)
        {
            int count = Random.Range(minChords, maxChords + 1);
            int[] chords = new int[count];
            int[] ch_arr = { 0, 1, 2, 3, 4, 5 };
            UniqueRandomItem<int> pick_chords = new UniqueRandomItem<int>();
            pick_chords.AddAll(ch_arr);

            string msg = m_name + " generated chord sequence ";
            for(int i = 0; i < count; i++)
            {
                chords[i] = pick_chords.PickItem();
                msg += chords[i] + ",";
            }
            Debug.Log(msg);


            return chords;
        }

        Strum[] GenerateStrumPattern(int beats)//, float bpm)
        {
            Debug.Log(m_name + " GenerateStrumPattern()");
            int strums = beats * m_StrumsPerBeat;
            Strum[] pattern = new Strum[strums];
            //float beatLength = 60f/bpm;
            float strumLength = 1f / strums;

            //float strumLength = beatLength / m_StrumsPerBeat;
            for (int i = 0; i < strums; i++)
            {
                pattern[i].play = true;
                pattern[i].time = i * strumLength;
                if (0 == i)
                {
                    pattern[i].volume = 1.0f;
                    pattern[i].Segment = 0;
                }
                else
                {
                    pattern[i].volume = m_OffBeatVolume;
                    if( i % 2 == 0)
                    {
                        pattern[i].Segment = 0;
                        pattern[i].play = !(Random.value < m_MissDownStrumChance);
                    }
                    else
                    {
                        pattern[i].Segment = 1;
                        pattern[i].time += m_UpStrumOffset * strumLength; // (beatLength/m_StrumsPerBeat);
                        pattern[i].play = !(Random.value < m_MissUpStrumChance) || !pattern[i-1].play;
                    }
                }
               // Debug.Log(i + " volume: " + pattern[i].volume);
            }

            return pattern;
        }

       /* void CalcMeasureLength()
        {
            m_MeasureLength = 60f/ m_BeatsPerMinute * m_BeatsPerMeasure;
        }*/

        void SetChordToPlay()
        {
            m_CurrentChord = m_keys[m_CurrentKey][m_CurrentChordIndex];
            Debug.Log( m_name + " playing " + m_CurrentKey + " " + m_CurrentChordIndex + " -> " + m_CurrentChord.name);
        }

        public void PrepNextMeasure(string segment)
        {
            if (m_CurrentSegment != segment)
            {
                m_CurrentSegment = segment;
                m_ResetChordProg = true;
                m_nextStrum = m_songParts[segment].StrumPattern;

            }
            else
            {
                m_nextStrum = m_thisStrum;
            }
            //Debug.Log(m_name + " Generating strum:");// GenerateStrumPattern(m_BeatsPerMeasure);//, m_BeatsPerMinute);
        }

        public void StartNextMeasure()
        {
            //Debug.Log(m_name + " New measure:");
            m_thisStrum = m_nextStrum;
            m_nextStrum = null;
            //CalcMeasureLength();
            m_MeasureNumber++;
            m_StrumNumber = 0;
            if (m_ResetChordProg)
            {
                m_ResetChordProg = false;
                m_CurrentChordIndex = 0;
                m_ChordProgression = m_songParts[m_CurrentSegment].ChordProgression;
            }
            else
            {
                m_ChordProgIndex = (m_ChordProgIndex + 1) % (m_ChordProgression.GetUpperBound(0)+1);
            }
            m_CurrentChordIndex = m_ChordProgression[m_ChordProgIndex];// Random.Range(0, 5);
           /* if (Random.value < 0.1f)
            {
                if ("C" == m_CurrentKey)
                {
                    m_CurrentKey = "Am";
                }
                else
                {
                    m_CurrentKey = "C";
                }
                Debug.Log("KEY CHANGE!!! For " + m_name + " to " + m_CurrentKey);
            }*/
            SetChordToPlay();
        }



        public void CheckDoStrum(float MeasureProportion)
        {
            //Debug.Log(m_name + " CheckDoStrum()");
            //CalcMeasureLength();
            //float MeasureProportion = MeasureTime / m_MeasureLength;

            if (m_StrumNumber > m_thisStrum.GetUpperBound(0)) return;
            if (MeasureProportion >= m_thisStrum[m_StrumNumber].time)
            {
                if (null == m_CurrentChord) SetChordToPlay();
                if (m_thisStrum[m_StrumNumber].play)
                {

                    //Debug.Log(m_name + " play " + m_CurrentChord.name + " seg " +  m_thisStrum[m_StrumNumber].Segment);
                    m_audio.PlayOneShot(m_CurrentChord.segments[m_thisStrum[m_StrumNumber].Segment], m_thisStrum[m_StrumNumber].volume);

                }
                m_StrumNumber++;
            }
        }

    }//end class Instrument

}
