using UnityEngine;
using System.Collections;

public class LayoutCursor : MonoBehaviour {

	public Sprite[] sprites;

	// Use this for initialization
	void Start () {

		int counter = 0;
		for (int x = -6; x < 6; x++) {
			for (int y = -2; y < 2; y++) {
				Debug.Log (counter);
				if(counter < sprites.Length) {
					GameObject cursor = new GameObject();
					cursor.AddComponent<SpriteRenderer>();
					cursor.GetComponent<SpriteRenderer>().sprite = sprites[counter];
					cursor.transform.position = new Vector2 (x*1.2f, y*1.7f);	
					cursor.name = sprites [counter].name;
					counter++;

				}

			}
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
