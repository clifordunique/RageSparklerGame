﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public void LoadSceneById(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
