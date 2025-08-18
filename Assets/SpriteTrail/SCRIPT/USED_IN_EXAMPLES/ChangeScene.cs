using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SpriteTrail
{
	public class ChangeScene : MonoBehaviour
	{
		public void GoToScene(string name)
		{
			SceneManager.LoadScene(name);
		}
	}
}
