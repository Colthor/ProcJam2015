using UnityEngine;
using System.Collections;

public class CameraMenuScript : MonoBehaviour {
	
	public GUISkin GuiSkin;
    private Music m_musicToMod;

    void Nowt()
    {
    }

    void StartNewSong()
    {
        m_musicToMod.StartNewSong();
    }

	void changeBeatsPerMeasure(int x)
	{
        int beats = x + 1;
        m_musicToMod.BeatsPerMeasure = beats;
		Debug.Log("Beats per measure now: " + beats);
	}

	void changeStrums(int x)
	{
        int strums = x + 1;
        m_musicToMod.StrumsPerBeat = strums;
		Debug.Log("Strums per beat now: " + strums);
	}

	void changeBPM(int x)
	{
        float newBPM = x * 20f + 80f;
        m_musicToMod.BeatsPerMinute = newBPM;
		Debug.Log("BPM now: " + newBPM);
	}
	void changeUpstrumChance(int x)
	{
        float chance =  ( x * .25f);
        m_musicToMod.MissUpStrumChance = chance;
		Debug.Log("Chance to miss upstrum: " + chance);
	}
	void changeDownstrumChance(int x)
	{
        float chance = ( x * .25f);
        m_musicToMod.MissDownStrumChance= chance;
		Debug.Log("Chance to miss downstrum: " + chance);
	}


	void quit()
	{
        Application.Quit();
	}

	private Menu menu;

	// Use this for initialization
	void Start () {
        string[] numberItems = { "One", "Two", "Three", "Four", "Five", "Six", "Seven" };
        string[] chanceItems = { "0%", "25%", "50%", "75%", "100%"};
		string[] bpmItems = {"0: 80bpm", "1: 100bmp", "2: 120bpm", "3: 140bpm", "4: 160bpm", "5: 180bpm", "6: 200bpm"};
        GameObject musicOb = GameObject.FindGameObjectsWithTag("MusicMaker")[0];
        m_musicToMod = musicOb.GetComponent<Music>();

        menu = this.gameObject.AddComponent<Menu>();
		menu.AddButtonItem("Up/Down: move cursor, Left/Right: change value, Space selects!", Nowt);
		menu.AddListItem("Beats per measure", numberItems, 3, changeBeatsPerMeasure);
		menu.AddListItem("Strums per beat", numberItems, 1, changeStrums);
		menu.AddListItem("No upstrum chance", chanceItems, 1, changeUpstrumChance);
		menu.AddListItem("No downstrum chance", chanceItems, 0, changeDownstrumChance);
		menu.AddButtonItem("Start new song", StartNewSong);

		menu.AddListItem("Change BPM", bpmItems, 2, changeBPM);
		menu.AddButtonItem("Quit", quit);  

		menu.GuiSkin = GuiSkin;
		
		menu.Enable();
        


	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.Escape))
		{
            //
		}
	}
}
