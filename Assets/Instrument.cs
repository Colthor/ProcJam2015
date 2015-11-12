using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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

        private float m_MeasureLength;
        //private float m_MeasureTime;
        private int m_MeasureNumber;
        private int m_StrumNumber;
        private string m_CurrentKey = "C";
        private int m_CurrentChordIndex = 0;
        private Chord m_CurrentChord;

        private Dictionary<string, Chord[]> m_keys = new Dictionary<string, Chord[]>();


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
            CalcMeasureLength();
            m_thisStrum = GenerateStrumPattern(m_BeatsPerMeasure, m_BeatsPerMinute);
            m_nextStrum = null;
            m_MeasureNumber = 0;
            m_StrumNumber = 0;

        }

        public void AddKey(string keyName, Chord[] chords)
        {
            Debug.Log(m_name + " AddKey( " + keyName + " )");
            m_keys.Add(keyName, chords);
        }


        Strum[] GenerateStrumPattern(int beats, float bpm)
        {
            Debug.Log(m_name + " GenerateStrumPattern()");
            int strums = beats * m_StrumsPerBeat;
            Strum[] pattern = new Strum[strums];
            float beatLength = 60f/bpm;

            float strumLength = beatLength / m_StrumsPerBeat;
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
                        pattern[i].time += m_UpStrumOffset * (beatLength/m_StrumsPerBeat);
                        pattern[i].play = !(Random.value < m_MissUpStrumChance) || !pattern[i-1].play;
                    }
                }
               // Debug.Log(i + " volume: " + pattern[i].volume);
            }

            return pattern;
        }

        void CalcMeasureLength()
        {
            m_MeasureLength = 60f/ m_BeatsPerMinute * m_BeatsPerMeasure;
        }

        void SetChordToPlay()
        {
            m_CurrentChord = m_keys[m_CurrentKey][m_CurrentChordIndex];
        }

        public void PrepNextMeasure()
        {
            //Debug.Log(m_name + " Generating strum:");
            m_nextStrum = GenerateStrumPattern(m_BeatsPerMeasure, m_BeatsPerMinute);
            Debug.Log( m_name + " playing " + m_CurrentChord.name);
        }

        public void StartNextMeasure()
        {
            //Debug.Log(m_name + " New measure:");
            m_thisStrum = m_nextStrum;
            m_nextStrum = null;
            CalcMeasureLength();
            m_MeasureNumber++;
            m_StrumNumber = 0;
            m_CurrentChordIndex = Random.Range(0, 5);
            if (Random.value < 0.1f)
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
            }
            SetChordToPlay();
        }



        public void CheckDoStrum(float MeasureTime)
        {
            //Debug.Log(m_name + " CheckDoStrum()");

            if (m_StrumNumber > m_thisStrum.GetUpperBound(0)) return;
            if (MeasureTime >= m_thisStrum[m_StrumNumber].time)
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
